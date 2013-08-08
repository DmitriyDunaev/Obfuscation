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

        /// <summary>
        /// Gets the first basic block.
        /// </summary>
        /// <returns>The first basic block in a function.</returns>
        public BasicBlock GetFirstBasicBlock()
        {
            /* 
             * The first block in the BasicBlocks list is always
             * the first basic block of the function.
             */
            return BasicBlocks[0];
        }

        /// <summary>
        /// This function returns all basicblocks from the function,
        /// which has an unconditional jump at the end
        /// </summary>
        /// <returns>A list of basicblocks, fufilling the condition written above</returns>
        public List<BasicBlock> GetUnconditionalJumps()
        {
            List<BasicBlock> list = new List<BasicBlock>();
            foreach (BasicBlock bb in BasicBlocks)
            {
                if (bb.LastInstruction().statementType == ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump )
                {
                    list.Add(bb);
                }
            }
            return list;
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

        /// <summary>
        /// This is the dumb version of the InsertAfter, which iserts a BasicBlock
        /// after an unconditional jump
        /// </summary>
        /// <param name="bbTarget">The target of the unconditional jump</param>
        /// <returns>The created block</returns>
        public BasicBlock InsertAfter(BasicBlock bbTarget)
        {
            Instruction nop = new Instruction(ExchangeFormat.StatementTypeType.EnumValues.eNoOperation);
            BasicBlock newblock = new BasicBlock(parent, nop);
            newblock.Successors.Add(bbTarget);
            bbTarget.Predecessors.Remove(this);
            bbTarget.Predecessors.Add(newblock);
            Successors.Remove(bbTarget);
            Successors.Add(newblock);
            newblock.Predecessors.Add(this);

            return newblock;

            // TODO: Get this done. Creating a basic block is not this simple.
        }

        public Instruction LastInstruction()
        {
            return Instructions[Instructions.Count() - 1];
        }
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

        /// <summary>
        /// Returns the state of the instruction's used dead variable.
        /// </summary>
        /// <param name="var">A dead variable used by the instruction.</param>
        /// <returns>The state of the dead variable.</returns>
        private Variable.State setState(Variable var)
        {
            /*
             * TODO: Write this function.
             * 
             * As I see, it can return two values:
             *  - FILLED: if the variable is a leftvalue in the instruction
             *  - FREE: if the variable is not a leftvalue (so it is ONLY a rightvalue and not both)
             */
        }

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
             * For every used dead variable in the instruction we should first determine
             * its new state, then we could push it's (changed) state through all the
             * following instructions, so it will get the appropriate state everywhere.
             */
            foreach (Variable var in RefVariables)
            {
                /*
                 * We will only use this function on fake instructions, and these may
                 * only use dead variables, which fact makes this condition unnecessary.
                 * Despite this, I leave it here, because maybe we could use alive variables
                 * in fake instructions, for example copying their values to dead variables.
                 *
                 * So QUESTION:
                 * Are we going to use only dead variables in fake instructions?
                 */
                if (DeadVariables.ContainsKey(var))
                {
                    /* First we set the state of that used dead variable. */ 
                    DeadVariables[var] = setState(var);

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction ins in GetNextInstructions())
                        ins.RefreshNext(var, DeadVariables[var]);
                }
            }
        }

        /*
         * This function is called when we encounter a change in a dead variable's state.
         * So we want to push this change through all the instructions, and deal with
         * this variable only.
         * 
         * However we can't just overwrite the statement without any check...
         */
        private void RefreshNext(Variable var, Variable.State state)
        {
            /*
             * We have to do anything only if the variable is in this instruction's
             * DeadVariables list, and it's state differs from the new state.
             * But if this instruction uses this variable as well, then its state must
             * not be changed, because it's perfect as it is right now.
             * That makes this condition necessary.
             */
            if (DeadVariables.ContainsKey(var) && DeadVariables[var] != state && !RefVariables.Contains(var))
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
