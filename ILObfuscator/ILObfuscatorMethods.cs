#define WORKING_IN_PROGRESS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public partial class Routine
    {
    }


    public partial class Function
    {
        /// <summary>
        /// Gets the last basic block, which is a "fake end block"
        /// </summary>
        /// <returns>The last basic block in a function</returns>
        public BasicBlock GetLastBasicBlock()
        {
            foreach (BasicBlock bb in BasicBlocks)
                if (bb.getSuccessors.Count.Equals(0))
                    return bb;
            return null;
        }
    }


    public partial class BasicBlock
    {
        /// <summary>
        /// Inserts an instruction to the beginning of a basic block
        /// </summary>
        /// <param name="instruction">Instruction to be inserted</param>
        public void InsertFirstInstruction(Instruction instruction)
        {
            Instructions.Insert(0, instruction);
            instruction.parent = this;
        }

    //    public BasicBlock InsertAfter(BasicBlock bbTarget)
    //    {
    //    }
    }


    public partial class Instruction
    {
        /// <summary>
        /// Gets a list of the instructions followed by this instruction
        /// </summary>
        /// <returns>A list of preceding instructions (or empty list if no such found)</returns>
        public List<Instruction> GetPrecedingInstructions()
        {
            List<Instruction> preceding = new List<Instruction>();
            int number = parent.Instructions.BinarySearch(this);
            if (number > 0)
            {
                preceding.Add(parent.Instructions[number - 1]);
                return preceding;
            }
            else if (number == 0)
            {
                foreach (BasicBlock bb in parent.getPredecessors)
                {
                    preceding.Add(bb.Instructions[bb.Instructions.Count - 1]);
                }
                return preceding;
            }
            else return null;
        }

        /// <summary>
        /// Gets a list of the instructions following this instruction
        /// </summary>
        /// <returns>A list of following instructions (or empty list if no such found)</returns>
        public List<Instruction> GetNextInstructions()
        {
            List<Instruction> next = new List<Instruction>();
            int number = parent.Instructions.BinarySearch(this);
            if (number < parent.Instructions.Count - 1)
            {
                next.Add(parent.Instructions[number + 1]);
                return next;
            }
            else if (number == parent.Instructions.Count - 1)
            {
                foreach (BasicBlock bb in parent.getSuccessors)
                {
                    next.Add(bb.Instructions[0]);
                }
                return next;
            }
            else return null;
        }



#if !WORKING_IN_PROGRESS

        /*
         * When we modify a fake instruction, we change the states in it, so we
         * have to update the dead variables in the following instructions.
         */
        /// <summary>
        /// Refreshes the state of all following instructions' dead variables.
        /// </summary>
        public void RefreshNext()
        {
            /*
             * We should be able to determine from the TAC instruction that which
             * dead variables' ( <- RefVariables ) state changes to what.
             */
              setStates();

            /*
             * For every used dead variable in the instruction we should push it's
             * (changed) state through all the following instructions, so it will get
             * the appropriate state everywhere.
             */
            foreach (Variable var in RefVariables)
            {
                if (DeadVariables.ContainsKey(var))
                {
                    foreach (Instruction ins in GetNextInstructions())
                        ins.RefreshNext(var, DeadVariables[var]);
                }
            }
        }

        /*
         * This function is called when we encounter a change in a dead variable's state.
         * So we want to push this change through all the instructions, and deal with
         * this variable only. That's why we don't need to check this instruction
         * if it changes the state, because the change comes from above.
         * 
         * (the statements above may not be true. requires some more thinking...)
         */
        private void RefreshNext(Variable var, Variable.State state)
        {
            if (DeadVariables[var] != state)
            {
                DeadVariables[var] = state;
                foreach (Instruction ins in GetNextInstructions())
                    ins.RefreshNext(var, state);
            }
        }
#endif

    }


    public partial class Variable
    {
    }
}
