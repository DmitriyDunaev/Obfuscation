#define RESERVED_FOR_FUTURE

#if !RESERVED_FOR_FUTURE

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
         * To make sure we don't deal with an instruction twice, we save the ID
         * of the instructions we have been to into a list.
         */
        /// <summary>
        /// Holds the ID's of the instructions we have already dealt with.
        /// </summary>
        private static List<string> done_ids = new List<string>();

        /// <summary>
        /// Holds the dead variables list.
        /// </summary>
        private static Dictionary<Variable, Variable.State> DeadVariables = new Dictionary<Variable, Variable.State>();

        /// <summary>
        /// Makes the proper dead variables list of an instruction.
        /// </summary>
        /// <param name="ins">You will get the dead variables list of this instruction.</param>
        /// <returns>The dead variables list.</returns>
        public static Dictionary<Variable, Variable.State> DeadVarsAlgortihm (Instruction ins)
        {
            /*
             * Before the algorithm starts we ensure that the instruction has a
             * list of dead variables which is filled with all the variabes present in
             * the function. During the algorithm we will remove the variables from these
             * lists that are not really dead at the given point.
             */
            SetAllVariablesAsDead(ins, Variable.State.Free);

            /*
             * We clear the former done_ids list,, because we don't want the previous run of
             * the algorithm to influence the present one.
             */
            done_ids.Clear();

            /*
             * We go through all the instructions accessible from here and deal with all their
             * referenced variables. At the and of this we will have the list of the dead
             * variables of this instruction.
             */
            recursive(ins);

            /*
             * Now we have the dead variables list filled with proper information,
             * we can return it to the caller.
             */
            return DeadVariables;
        }

        /// <summary>
        /// Recursive function to get to all the instructions accessible from this one.
        /// </summary>
        /// <param name="actual">Actual Instruction</param>
        private static void recursive(Instruction actual)
        {
            /*
             * We deal with every referenced variable in the instruction.
             * The referenced variables should be removed from the dead variables list.
             */
             foreach (Variable var in actual.RefVariables)
                DeadVariables.Remove(var);

            /*
             * We have finished the task with this instruction, and we should not
             * come back here anymore, so we save its ID into the done_ids list.
             */
            done_ids.Add(actual.ID);

            /*
             * Now that this instruction is finished we should do the same things
             * with the instructions following it.
             * We only should deal with the instructions not marked as done.
             */
            foreach (Instruction ins in GetNextInstructions(actual))
            {
                if ( !done_ids.Contains(ins.ID) )
                    recursive(ins);
            }
        }

        /// <summary>
        /// Used once by the Data Analysis Algorithm and fills all DeadVariables lists with all variables defined in the function 
        /// </summary>
        /// <param name="func">Actual Function</param>
        private static void SetAllVariablesAsDead(Instruction ins, Variable.State state)
        {
            DeadVariables.Clear();
            foreach (Variable var in ins.parent.parent.LocalVariables)
                DeadVariables.Add(var, state);
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

        /// <summary>
        /// Gets a list of the instructions following the actual instruction
        /// </summary>
        /// <param name="instr">Actual Instruction</param>
        /// <returns>A list of following instructions (or empty list if no such found)</returns>
        public static List<Instruction> GetNextInstructions(Instruction instr)
        {
            List<Instruction> next = new List<Instruction>();
            int number = instr.parent.Instructions.BinarySearch(instr);
            if (number < instr.parent.Instructions.Count - 1)
            {
                next.Add(instr.parent.Instructions[number + 1]);
                return next;
            }
            else if (number == instr.parent.Instructions.Count - 1)
            {
                foreach (BasicBlock bb in instr.parent.getSuccessors)
                {
                    next.Add(bb.Instructions[0]);
                }
                return next;
            }
            else return null;
        }
    }
}

#endif
