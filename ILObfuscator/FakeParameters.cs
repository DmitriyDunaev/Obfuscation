using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objects;
using Services;

namespace Obfuscator
{
    public static class FakeParameters
    {

        /// <summary>
        /// Creates fake input parameters in all functions on the routine
        /// </summary>
        /// <param name="routine">A routine with functions that will receive fake input parameters</param>
        public static void CreateFakeParameters(Routine routine)
        {
            for (int i = 0; i < Common.NumFakeInputParam; i++)
            {
                //Creating an interval
                List<int> borders = Randomizer.MultipleNumbers(2, Common.GlobalMinValue + Common.LoopConditionalJumpMaxRange,
                        Common.GlobalMaxValue - Common.LoopConditionalJumpMaxRange, false, true);
                //Creating the fake input parameter in all functions
                foreach (Function func in routine.Functions)
                    func.NewFakeInputParameter(borders.First(), borders.Last());
            }

            //Updating all CALLs according to the new number of parameters
            foreach (Function func in routine.Functions)
            {
                UpdateAllFunctionCalls(func);
            }
        }


        /// <summary>
        /// Gets randomly (using uniform distribution) one fake input parameter of a function. Throws exception if no such found.
        /// </summary>
        /// <param name="func">A function with parameters</param>
        /// <returns>One random fake input parameter of a function</returns>
        public static Variable GetRandom(Function func)
        {
            if (func.LocalVariables.Count(x => x.kind == Variable.Kind.Input && x.fake) == 0)
                throw new ObfuscatorException("Function " + func.ID + " has no fake input variables (parameters).");
            List<Variable> fake_inputs = func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake);
            return fake_inputs[Randomizer.SingleNumber(0, fake_inputs.Count - 1)];
        }

        
        /// <summary>
        /// Updates all CALLs according to the number of fake and original parameters of the called functions
        /// </summary>
        public static void UpdateAllFunctionCalls(Function func)
        {
            //We use a dictionary to store the "call" instruction and its index inside the basic block
            Dictionary<Instruction, int> allCalls = new Dictionary<Instruction, int>();
            List<Instruction> bbCalls = new List<Instruction>();
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                //We collect all "call" instructions in this basic block
                
                bbCalls = bb.Instructions.FindAll(x => x.TACtext.Contains("call") && !x.TACtext.Contains("printf")
                && !x.TACtext.Contains("scanf"));
                foreach (Instruction ins in bbCalls)
                {
                    //Storing all "call" instructions in the actual basic block with their indexes
                    allCalls.Add(ins, bb.Instructions.IndexOf(ins));
                }
            }

            //We check wheter we have any "call" instruction in this function
            if (allCalls.Count > 0)
            {
                foreach (Instruction call in allCalls.Keys)
                {
                    //Splitting the call in tokens - tokens[0] = "call", tokens[1] = "function_ID", tokens[2] = "numParams"
                    string[] tokens = call.TACtext.Split(' ');
                    //Checking wheter if the called function is present in our routine (internal function)
                    if (func.parent.Functions.Exists(x => x.ID == tokens[1]))
                    {
                        //Getting the number of parameters to update it later
                        int numParams = int.Parse(tokens[2]);
                        //Fetching fake input parameters
                        List<Variable> fakeInputsParams = func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake == true);
                        //Number of lines that are shifted during the injection of fake "param" instructions
                        int shift = 0;
                        foreach (Variable fakeInputVar in fakeInputsParams)
                        {

                            Instruction param = new Instruction(call.parent);
                            MakeInstruction.ProceduralParam(param, fakeInputVar, null);
                            //Inserting the "param" instruction right before the "call" instruction
                            call.parent.Instructions.Insert(allCalls[call] + shift, param);
                            shift++;
                            //Updating the number of parameters for this call
                            numParams++;
                        }

                        //Updating the TAC text of the call with the new number of parameters
                        call.parent.Instructions[allCalls[call] + shift].TACtext = string.Join(" ", "call", tokens[1],
                            numParams);
                    }
                }
            }
        }
    }
}
