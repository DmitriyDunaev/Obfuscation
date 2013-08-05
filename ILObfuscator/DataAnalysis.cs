using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class DataAnalysis
    {
        /* 
         * To make sure we don't deal with a basic block twice, we save the ID
         * of the basic blocks we have been to into a list.
         */
        /// <summary>
        /// Holds the ID's of the basic blocks we have already dealt with.
        /// </summary>
        private static List<string> done_ids = new List<string>();

        /// <summary>
        /// Fills the DeadVariables list of the function's instructions with proper information.
        /// </summary>
        /// <param name="func">Actual Function</param>
        public static void DeadVarsAlgortihm(Function func)
        {
            /*
             * Before the algorithm starts we ensure that all instructions have a
             * list of dead variables which is filled with all the variabes present in
             * the function. During the algorithm we will remove the variables from these
             * lists that are not really dead at the given point.
             */
            SetAllVariablesAsDead(func, Variable.State.Free);

            /*
             * We start from the point called "fake exit block", the one and only
             * ultimate endpoint of all functions ever created. Thank you Kohlmann!
             *
             * GetLastBasicBlock() - it is supposed to give the function's "fake exit block"
             */
            BasicBlock lastblock = func.GetLastBasicBlock();

            /*
             * We go through all the instructions and deal with all their
             * referenced variables. At the and of this we will have the list of the dead
             * variables for all instructions.
             */
            recursive(lastblock);
        }

        /// <summary>
        /// Recursive function to get to all the instructions of the Function.
        /// </summary>
        /// <param name="actual">Actual BasicBlock</param>
        private static void recursive(BasicBlock actual)
        {
            /*
             * We deal with every instruction in the basic block,
             * and in every instruction we deal with every referenced variable.
             * The referenced variables should be removed from the dead variables list
             * in all the instructions accesible from here.
             *
             * deal_with_var() - it will be described in detail in the forthcoming parts
             */
            foreach (Instruction ins in actual.Instructions)
            {
                foreach (Variable var in ins.RefVariables)
                    deal_with_var(var, ins);
            }

            /*
             * We have finished the task with this basic block, and we should not
             * come back here anymore, so we save its ID into the done_ids list.
             */
            done_ids.Add(actual.ID);

            /*
             * Now that this basic block is finished we should do the same things
             * with its predecessors recursively.
             * We only should deal with the basic blocks not marked as done.
             */
            foreach (BasicBlock block in actual.getPredecessors)
            {
                if ( !done_ids.Contains(block.ID) )
                    recursive(block);
            }
        }

        /// <summary>
        /// Recursive function to set a Variable as alive in the proper places.
        /// </summary>
        /// <param name="var">the Variable we are dealing with</param>
        /// <param name="ins">Actual Instruction</param>
        private static void deal_with_var(Variable var, Instruction ins)
        {
            /* 
             * This variable is used somewhere after this instruction, so it is alive here.
             * 
             * DeadVariables - it contains the dead variables at the point of the given instruction
             */
            ins.DeadVariables.Remove(var);

            /*
             * GetPrecedingInstructions() 
             *      - gives a list of the instructions followed by the actual instruction
             *      - if we are in the middle of the basic block, then it consists of only one instruction
             *      - if we are at the beginning of the basic block then it is the list of all
             *        the predecessing basic block's last instruction
             *      - if we are at the beginning of the first basic block (the one with no predecessors)
             *        then it is an empty list, meaning that we have nothing left to do here
             */
            List<Instruction> previous = GetPrecedingInstructions(ins);

            /*
             * Now we have that list of instructions, we should do the same thing we have done
             * to this instruction, assuming that it had not been done already.
             */
            foreach (Instruction i in previous)
            {
                /*
                 * If the variable is not in the instruction's dead variables list, then it indicates
                 * that we have dealt with this instruction.
                 */
                if ( i.DeadVariables.ContainsKey(var) )
                    deal_with_var(var, i);
            }
        }

        /// <summary>
        /// Used once by the Data Analysis Algorithm and fills all DeadVariables lists with all variables defined in the function 
        /// </summary>
        /// <param name="func">Actual Function</param>
        private static void SetAllVariablesAsDead(Function func, Variable.State state)
        {
            foreach (BasicBlock bb in func.BasicBlocks)
                foreach (Instruction inst in bb.Instructions)
                {
                    inst.DeadVariables.Clear();
                    foreach (Variable var in func.LocalVariables)
                        inst.DeadVariables.Add(var, state);
                }
        }

        /// <summary>
        /// Gets a list of the instructions followed by the actual instruction
        /// </summary>
        /// <param name="instr">Actual Instruction</param>
        /// <returns>A list of preceding instructions (or empty list if no such found)</returns>
        public static List<Instruction> GetPrecedingInstructions(Instruction instr)
        {
            List<Instruction> preceding = new List<Instruction>();
            int number = instr.parent.Instructions.BinarySearch(instr);
            if (number > 0)
            {
                preceding.Add(instr.parent.Instructions[number - 1]);
                return preceding;
            }
            else if (number == 0)
            {
                foreach (BasicBlock bb in instr.parent.getPredecessors)
                {
                    preceding.Add(bb.Instructions[bb.Instructions.Count - 1]);
                }
                return preceding;
            }
            else return null;
        }
    }
}
