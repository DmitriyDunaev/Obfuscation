#define WORKING_IN_PROGRESS

/*
 * WARNING FOR FUTURE (1):
 * We assume that a dead pointer can only point to a non-pointer dead variable.
 * We are setting them like this, and using them like this, so be aware of this fact.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            throw new ObfuscatorException("No 'fake end block' is found in function " + ID);
        }

        /// <summary>
        /// Gets the first basic block.
        /// </summary>
        /// <returns>The first basic block in a function.</returns>
        public BasicBlock GetFirstBasicBlock()
        {
            if (BasicBlocks.Count == 0)
                throw new ObfuscatorException("A function must have at least one basic block");
            // The first item in the BasicBlocks list is always the first basic block of the function.
            return BasicBlocks[0];
        }

        /// <summary>
        /// This function returns all basicblocks of a function, which last instruction is UnconditionalJump
        /// </summary>
        /// <returns>A list of basicblocks, which last instruction is UnconditionalJump </returns>
        public List<BasicBlock> GetUnconditionalJumps()
        {
            List<BasicBlock> list = new List<BasicBlock>();
            foreach (BasicBlock bb in BasicBlocks)
                if (bb.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
                    list.Add(bb);
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
        /// This is the dumb version of the InsertAfter, which inserts a BasicBlock after an unconditional jump
        /// </summary>
        /// <param name="bbTarget">The target of the unconditional jump</param>
        /// <returns>The created block</returns>
        public BasicBlock InsertAfter(BasicBlock target)
        {
            if (Successors.Count > 1) // two successors
                throw new ObfuscatorException("You cannot insert new basic block after the basic block with two successors.");

            BasicBlock newblock = new BasicBlock(parent, new Instruction(ExchangeFormat.StatementTypeType.EnumValues.eNoOperation));

            if (Successors.Count == 1)  // one successor
            {
                Successors.Clear();
                Successors.Add(newblock);
                target.Predecessors.Remove(this);
                target.Predecessors.Add(newblock);
                newblock.Predecessors.Add(this);
                newblock.Successors.Add(target);
            }
            else // no successors
            {
                Successors.Add(newblock);
                newblock.Predecessors.Add(this);
            }

            // The last instruction is modified if it was an unconditional 'goto' to match new target.ID
            this.RetargetLastUnconditionalGoto(newblock.ID);
            //var t = new Tuple<Variable, Variable.State>(parent.LocalVariables[0], Variable.State.Free);
            //var t = Tuple.Create(parent.LocalVariables[0], Variable.State.Free);

            return newblock;

        }


        /// <summary>
        /// Retargets (sets new ID_GUID) last 'goto' instruction of a basic block
        /// </summary>
        /// <param name="targetID">ID_'GUID' to be retargeted to</param>
        /// <returns>True if 'goto' has been retargeted successfully; false if the last instruction is not 'goto'</returns>
        public bool RetargetLastUnconditionalGoto(string targetID)
        {
            if (this.Instructions.Last().statementType != ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
                return false;
            else
            {
                string resultString = null;
                resultString = Regex.Match(Instructions.Last().TACtext, @"\bID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b").Value;
                Instructions.Last().TACtext = Instructions.Last().TACtext.Replace(resultString, targetID);
                return true;
            }
        }

    }


    public partial class Instruction
    {
        /// <summary>
        /// Gets a list of the instructions followed by the given instruction in a control flow (directly preceding in CFG)
        /// </summary>
        /// <returns>A list of instructions (or an empty list if no such found)</returns>
        public List<Instruction> GetPrecedingInstructions()
        {
            List<Instruction> preceding = new List<Instruction>();
            if(parent == null)
                throw new ObfuscatorException("Instruction is not contained in a basic block. Instruction's 'parent' propery is null.");
            if (parent.Instructions.Contains(this))
            {
                if (parent.Instructions.First().Equals(this))
                    foreach (BasicBlock bb in parent.getPredecessors)
                    {
                        if (bb.Instructions.Count == 0)
                            throw new ObfuscatorException("No instructions found in basic block" + bb.ID);
                        preceding.Add(bb.Instructions.Last());
                    }
                else
                    preceding.Add(parent.Instructions[parent.Instructions.BinarySearch(this) - 1]);
            }
            else
                throw new ObfuscatorException("The instruction is not properly linked to its parent. It is not contained in 'parent.Instructions' list.");
            return preceding;
        }

        /// <summary>
        /// Gets a list of the instructions following the given instruction in a control flow (directly following in CFG)
        /// </summary>
        /// <returns>A list of instructions (or empty list if no such found)</returns>
        public List<Instruction> GetFollowingInstructions()
        {
            List<Instruction> following = new List<Instruction>();
            if (parent == null)
                throw new ObfuscatorException("Instruction is not contained in a basic block. Instruction's 'parent' propery is null.");
            if (parent.Instructions.Contains(this))
            {
                if (parent.Instructions.Last().Equals(this))
                    foreach (BasicBlock bb in parent.getSuccessors)
                    {
                        if (bb.Instructions.Count == 0)
                            throw new ObfuscatorException("No instructions found in basic block" + bb.ID);
                        following.Add(bb.Instructions.First());
                    }
                else
                    following.Add(parent.Instructions[parent.Instructions.BinarySearch(this) + 1]);
            }
            else
                throw new ObfuscatorException("The instruction is not properly linked to its parent. It is not contained in 'parent.Instructions' list.");
            return following;
        }

        internal void ReplaceGoto(string newID)
        {
            // TODO
        }

        public List<Variable> GetUnsafeVariables()
        {
            List<Variable> unsafeVar = new List<Variable>();
            //if (statementType == ExchangeFormat.StatementTypeType.EnumValues.ePointerAssignment)
            //{
            //    string resultString = null;
            //    bool found = false;
            //    resultString = Regex.Match(TACtext, @"\bID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b").Value;
            //}
            return unsafeVar;
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
                    foreach (Instruction ins in GetFollowingInstructions())
                        ins.RefreshNext(var, DeadVariables[var]);
                }
                else if (DeadPointers.ContainsKey(var))
                {
                    /* First we set the state of that used dead pointer. */
                    DeadPointers[var].State = setState(var);

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction ins in GetFollowingInstructions())
                        ins.RefreshNext(var, DeadPointers[var].State);

                    /* 
                     * We set the state of the dead variable pointed by the dead pointer.
                     * 
                     * WARNING FOR FUTURE: (1)
                     */ 
                    DeadVariables[DeadPointers[var].PointsTo] = setState(var);

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction ins in GetFollowingInstructions())
                        ins.RefreshNext(DeadPointers[var].PointsTo, DeadVariables[DeadPointers[var].PointsTo]);
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
             * If this instruction uses this variable as well, or this instruction
             * uses a pointer that points to this variable then its state must not
             * be changed, because it's perfect as it is right now.
             */
            if (RefVariables.Contains(var))
            {
                /* This instruction uses this variable. */
                return;
            }

            /*
             * We have to do anything only if the variable is in this instruction's
             * DeadVariables list, and it's state differs from the new state.
             */
            if (DeadVariables.ContainsKey(var)      // This variable is dead.
                && DeadVariables[var] != state)     // The states differ.
            {
                foreach (Variable v in RefVariables)
                {
                    if (DeadPointers.ContainsKey(v) && DeadPointers[v].PointsTo.Equals(var))
                    {
                        /* The instruction uses a pointer that points to this variable. */
                        return;
                    }
                }
                DeadVariables[var] = state;
                foreach (Instruction ins in GetFollowingInstructions())
                    ins.RefreshNext(var, state);
            }

            /*
             * Another case for doing something when we pass a dead pointer with changed state.
             * In this case we do not have to look for variables pointing to that one.
             * 
             * WARNING FOR FUTURE: (1)
             */
            if (DeadPointers.ContainsKey(var)           // This is a dead pointer.
                && DeadPointers[var].State != state)    // The states differ.
            {
                DeadPointers[var].State = state;
                foreach (Instruction ins in GetFollowingInstructions())
                    ins.RefreshNext(var, state);
            }
        }
#endif

    }


    public partial class Variable
    {
    }
}
