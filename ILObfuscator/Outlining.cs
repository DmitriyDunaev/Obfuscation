using ExchangeFormat;
using Objects;
using Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class Outlining
    {
        public static void generateFunctions(Routine routine)
        {
 
            List<Function> functions = new List<Function>();

            foreach (Function originalFnction in routine.Functions)
            {
                foreach (BasicBlock originalBB in originalFnction.BasicBlocks)
                {
                    foreach(List<Instruction> instructions in getInstructions(originalBB))
                    {
                        Function function = getNewFunction(originalBB, instructions);
                        if (function != null)
                        {
                            functions.Add(function);
                        }
                    }
                }
            }
            routine.Functions.InsertRange(0, functions);
        }

        private static Function getNewFunction(BasicBlock originalBB, List<Instruction> instructionsToOutline)
        {
            //There is no instructions to outline so we do nothing.
            if (instructionsToOutline.Count == 0)
            {
                return null;
            }

            //Create new Function, and it's first BasicBlock.
            Function newfunction = new Function(originalBB.parent.parent);
            BasicBlock newBB = BasicBlock.getBasicBlock(newfunction);

            //The variable we are going to return in, is going to be the new function's last instruction's first variable.
            Variable variableToReturn = instructionsToOutline.Last().GetVarFromCondition();

            //Link the variable's olds Ids to the new Ids.
            Dictionary<string, string> oldToNew = new Dictionary<string, string>();
            Dictionary<string, string> newToOld = new Dictionary<string, string>();

            //The index of the old instruction in its old BasicBlock.
            int index = 0;

            foreach (Instruction instruction in instructionsToOutline)
            {
                index = originalBB.Instructions.IndexOf(instruction);
                originalBB.Instructions.Remove(instruction);

                //We are going through the instructions variables, and generate the "param " instructions, to call the new function.
                List<string> instructionParams = new List<string>();
                foreach (Variable variable in instruction.RefVariables)
                {
                    //If the variable in the "oldToNew" list, it was already processed in a previous instruction.
                    //If the variable in the "functionParams" list, it was already processed in the current instruction.
                    //In these cases we do not create a "param " instruction because it would be a duplication.
                    if (!oldToNew.ContainsKey(variable.ID) && !instructionParams.Contains(variable.ID))
                    {
                        instructionParams.Add(variable.ID); //Set the current instruction's variables to prevent duplications.
                        Instruction paramInstruction = new Instruction(originalBB, "param " + variable.name, true);
                        paramInstruction.RefVariables.Add(variable);
                        originalBB.insertInstruction(ref index, paramInstruction);
                    }
                }

                instruction.renewVariableIds(oldToNew, newToOld);
                instruction.setVariables();
                instruction.parent = newBB;
                newBB.Instructions.Add(instruction);
            }

            //In every function, the return variable is going to be the last instruction's, first variable. (At this point the instructions has new variable objects, not the same as the point at "variableToReturn = ...")
            Variable variableReturnWith = instructionsToOutline.Last().GetVarFromCondition();
            //We make a copy instruction to return with the correct value, in a different variable.
            Variable copyVariable = new Variable(Variable.Kind.Output, Variable.Purpose.Original);
            copyVariable.fake = false;
            Instruction copyInstruction = new Instruction(newBB, copyVariable.name + " := " + variableReturnWith.name, true, Objects.Common.StatementType.Copy);
            copyInstruction.RefVariables.Add(copyVariable);
            copyInstruction.RefVariables.Add(variableReturnWith);
            newBB.Instructions.Add(copyInstruction);
            //Create the "return" statement in the new BB.
            Instruction returnInstruction = new Instruction(newBB, "return " + copyVariable.name, true);
            returnInstruction.RefVariables.Add(copyVariable);
            //Add the instruction to the end of the new BB.
            newBB.Instructions.Add(returnInstruction);

            //Set the new variables.
            newfunction.AddVariables();

            //Create the "call" and "retrive" instructions in the old BB.
            originalBB.insertInstruction(ref index, new Instruction(originalBB, "call " + newfunction.ID + " " + newfunction.getVariableCount(), true));
            Instruction retriveIns = new Instruction(originalBB, "retrieve " + variableToReturn.name, true);
            retriveIns.RefVariables.Add(variableToReturn);
            originalBB.insertInstruction(ref index, retriveIns);


            //If the new BB has no LastBasicBlock, we create one.
            BasicBlock lastBlock = newfunction.GetLastBasicBlock();
            if (!lastBlock.isFakeExitBlock())
            {
                //Create the falseReturnBB and link the successor/predecessor.
                BasicBlock falseRet = BasicBlock.getBasicBlock(newfunction);
                lastBlock.LinkToSuccessor(falseRet);
                //Add the "false return instruction" to the "falseReturnBB". 
                falseRet.Instructions.Add(new Instruction(falseRet, "return"));
            }

            return newfunction;
        }

        /// <summary>
        /// Return a list of consecutive instructions that can be outline.
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        private static List<List<Instruction>> getInstructions(BasicBlock bb)
        {
            //Lists of proper insturcitons to obfuscate.
            List<List<Instruction>> instructionsList = getProperInstructionsList(bb);
            List<List<Instruction>> returnList = new List<List<Instruction>>();

            foreach (List<Instruction> insturctions in instructionsList)
            {
                if (Randomizer.SingleNumber(0, 100) >= Common.OutlingingProbability)
                {
                    continue;
                }

                //We need at least two instructions per function and least one instruction next to the call of the new funtcion.
                int instructionsInFunction = Randomizer.SingleNumber(2, Math.Min(Common.MaxInstructionsPerFunctions, insturctions.Count - 1));
                int start = Randomizer.SingleNumber(0, insturctions.Count - instructionsInFunction);
                returnList.Add(insturctions.GetRange(start, instructionsInFunction));
            }

            return returnList;
        }

        private static Boolean functionCallInstructionSet = false;

        private static List<List<Instruction>> getProperInstructionsList(BasicBlock bb)
        {
            List<List<Instruction>> instructionsList = new List<List<Instruction>>();
            List<Instruction> instructions = new List<Instruction>();
            bool proper = false;
            
            foreach (Instruction instruction in bb.Instructions)
            {
                if (instruction.TACtext.StartsWith("param"))
                {
                    functionCallInstructionSet = true;
                }
                else if (instruction.TACtext.StartsWith("retrieve") || instruction.TACtext.StartsWith("return"))
                {
                    functionCallInstructionSet = false;
                }

                //These are the supported instructions type.
                switch (instruction.statementType)
                {
                    case Objects.Common.StatementType.FullAssignment:
                    case Objects.Common.StatementType.UnaryAssignment:
                    case Objects.Common.StatementType.Copy:
                        proper = true; break;
                    default: proper = false; break;
                }
                proper = true;

                //Not fake instructions are not supported.
                //We cant break a function call with our function call
                proper = instruction.isFake && !functionCallInstructionSet && instruction.wasNop ? proper : false;

               /* if (instruction.statementType.Equals(Objects.Common.StatementType.FullAssignment))
                {
                    if (instruction.RefVariables.Count != 3)
                    {
                        proper = false;
                    }
                }*/

                if (proper)
                {
                    instructions.Add(instruction);
                }
                else if (instructions.Count >= 5) //We need at least two instructions in a function, and more next to the function call.
                {
                    instructions.Remove(instructions.First());
                    instructions.Remove(instructions.Last());
                    instructionsList.Add(instructions);
                    instructions = new List<Instruction>();
                }
            }

            return instructionsList;
        }
    }
}
