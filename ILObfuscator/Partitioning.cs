using ExchangeFormat;
using Objects;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class Partitioning
    {
        public static void generateFunctions(Routine routine)
        {
            int funcID = 98700000;
            int bbID = 65400000;
            List<Function> functions = new List<Function>();
            
            foreach (Function func in routine.Functions)
            {
                foreach (BasicBlock oldBB in func.BasicBlocks)
                {

                    List<Instruction> instructions = getInstructions(oldBB);

                    if (instructions.Count == 0)
                    {
                        continue;
                    }

                    //Create new Function, and it's first BasicBlock.
                    Function newfunction = new Function(routine, getID(funcID++));
                    BasicBlock newBB = BasicBlock.getBasicBlock(newfunction, getID(bbID++));

                    Variable myReturn =  null;
                    Variable myReturnOld = null;

                    //Link the variable's olds Ids to the new Ids.
                    Dictionary<String, String> oldToNew = new Dictionary<String, String>();
                    Dictionary<String, String> newToOld = new Dictionary<String, String>();
                    int index = 0; //The index of the old instruction in its old BasicBlock.
                    foreach (Instruction instruction in instructions)
                    {
                        index = oldBB.Instructions.IndexOf(instruction);
                        oldBB.Instructions.Remove(instruction);

                        //We are going through the instructions variables, and generate the "param " instructions, to call the new function.
                        List<string> functionParams = new List<string>();
                        foreach (Variable variable in instruction.RefVariables)
                        {
                            //If the variable in the "oldToNew" list, it was already processed in a previous instruction.
                            //If the variable in the "functionParams" list, it was already processed in the current instruction.
                            if (!oldToNew.ContainsKey(variable.ID) && !functionParams.Contains(variable.ID))
                            {
                                functionParams.Add(variable.ID); //Set the current instructions variables to prevent duplicate.
                                Instruction paramInstruction = Instruction.getInstruction(oldBB, (new IDManager()).ToString(), "param " + variable.name, true);
                                paramInstruction.RefVariables.Add(variable);
                                oldBB.Instructions.Insert(index++, paramInstruction);

                                variable.kind = Variable.Kind.Input; //TODO
                            }
                        }

                        myReturnOld = instruction.RefVariables.First(); //In every function, the return variable is going to be the last instructions, first variable.
                        instruction.renewVariableIds(oldToNew, newToOld);
                        instruction.parent = newBB;
                        instruction.mySetID(new IDManager());
                        newBB.Instructions.Add(instruction);
                        myReturn = new Variable(instruction.RefVariables.First());
                        ///myReturn.kind = Variable.Kind.Output; //TODO
                    }
                    //Set the new variables.
                    newfunction.AddVariables();
                    //Create the "call" and "retrive" instructions in the old BB.
                    oldBB.Instructions[index++] = Instruction.getInstruction(oldBB, (new IDManager()).ToString(), "call " + newfunction.ID + " " + newfunction.LocalVariables.Count, true);
                    Instruction retriveIns = Instruction.getInstruction(oldBB, (new IDManager()).ToString(), "retrieve " + myReturnOld.name, true);
                    retriveIns.RefVariables.Add(myReturnOld);
                    oldBB.Instructions[index++] = retriveIns;

                    //Create the "return" statement in the new BB.
                    Instruction myRetIns = Instruction.getInstruction(newBB, (new IDManager()).ToString(), "return " + myReturn.name, true);
                    myRetIns.RefVariables.Add(myReturn);
                    //Add the instruction to the end of the new BB.
                    newBB.Instructions.Add(myRetIns);

                    //If the new BB has no LastBasicBlock, we create one.
                    BasicBlock lastBlock = newfunction.GetLastBasicBlock();
                    if (!lastBlock.isFakeExitBlock())
                    { 
                        //Create the falseReturnBB and link the successor/predecessor.
                        BasicBlock falseRet = BasicBlock.getBasicBlock(newfunction, getID(bbID++));
                        falseRet.getPredecessors.Add(lastBlock);
                        lastBlock.getSuccessors.Add(falseRet);
                        //Add the "false return instruction" to the "falseReturnBB". 
                        falseRet.Instructions.Add(Instruction.getInstruction(falseRet, (new IDManager()).ToString()));
                    }
                    functions.Add(newfunction); //We add the funtions to the routine later, because of the foreach loop.
                }
            }
            routine.Functions.AddRange(functions);
        }


        private static String getID(int num)
        {
            return "ID_"+num+"-43C3-464F-A363-485CE6CC25F7";
        }

        /// <summary>
        /// Returns the list of instructions that are going to be in a new function.
        /// </summary>
        /// <param name="bb">The BasicBlock that contains the instructions.</param>
        /// <returns>The chosen instructions.</returns>
        private static List<Instruction> getInstructions(BasicBlock bb)
        {
            List<Instruction> instructions = new List<Instruction>();
            foreach (Instruction inst in  bb.Instructions)
            {
                if (!inst.isFake) //If the instructions has origian instructions, we dont use the method.
                {
                    return instructions;
                }
            }

            //TODO early logic. Just for the development of correct code generating.
            if (bb.Instructions.Count > 10)
            {
                for (int i = 0; i < bb.Instructions.Count - 5; i++)
                {
                    instructions.Add(bb.Instructions.ElementAt(i));
                }
            }

            return instructions;

        }
    }
}
