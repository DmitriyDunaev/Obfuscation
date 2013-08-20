using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;

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

        private static List<Variable> unsafe_vars = new List<Variable>();

        /*
         * This is a wrapper function around the real algorithm.
         * We have to find the dead variables in two steps: the FREE ones in one,
         * and the NOT_INITIALIZED ones in the other.
         * The order is not important, as I see it's fully changeable, and it does
         * not influence the result.
         */
        /// <summary>
        /// Fills the DeadVariables list of the function's instructions with proper information.
        /// </summary>
        /// <param name="func">Actual Function</param>
        public static void DeadVarsAlgortihm(Function func)
        {
            /* 
             * We get all the unsafe variables in one list, so in the end we can
             * remove them from the DeadVariables and DeadPointers lists.
             */
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                foreach (Instruction ins in bb.Instructions)
                    unsafe_vars.AddRange(ins.GetUnsafeVariables());
            }
            /* Remove duplications. */
            unsafe_vars = unsafe_vars.Distinct().ToList();

            /* First we clear all the DeadVariables lists. */
            ClearDeadVarsLists(func);

            /* Then we search for the FREE and the NOT_INITIALIZED dead variables. */
            _DeadVarsAlgortihm(func, Variable.State.Free);
            _DeadVarsAlgortihm(func, Variable.State.Not_Initialized);

            /*
             * Now we have the list of the dead variables, we must separate the dead
             * pointers from the other dead variables, for future use.
             */
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                foreach (Instruction ins in bb.Instructions)
                {
                    /* First we remove the unsafe variables. */
                    foreach (Variable var in unsafe_vars)
                    {
                        if (ins.DeadVariables.ContainsKey(var) && ins.DeadVariables[var] != Variable.State.Not_Initialized)
                            ins.DeadVariables.Remove(var);
                    }

                    /* Then we separate pointers from non-pointers. */
                    foreach (Variable var in ins.DeadVariables.Keys)
                    {
                        if (var.pointer)
                            ins.DeadPointers.Add(var, new PointerData(ins.DeadVariables[var], null));
                    }
                    foreach (Variable var in ins.DeadPointers.Keys)
                        ins.DeadVariables.Remove(var);
                }
            }
        }

        private static void _DeadVarsAlgortihm(Function func, Variable.State state)
        {
            /*
             * Before the algorithm starts we ensure that all instructions have a
             * list of dead variables which is filled with all the variables present in
             * the function. During the algorithm we will remove the variables from these
             * lists that are not really dead at the given point.
             */
             SetAllVariablesAsDead(func, state);

             /*
              * If we are going upwards (looking for FREE dead variables), we start from
              * the point called "fake exit block", the one and only ultimate endpoint
              * of all functions ever created. Thank you Kohlmann!
              * 
              * If going downwards (looking for NOT_INITIALIZED dead variables),
              * we start from the very first basic block (func.BasicBlocks[0]).
              */
             BasicBlock block = (state == Variable.State.Free) ? func.GetFakeExitBasicBlock() : func.GetEntranceBasicBlock();

            /*
             * We go through all the instructions and deal with all their
             * referenced variables. At the and of this we will have the list of the dead
             * variables for all instructions.
             */
            recursive(block, state);

            /*
             * In the end we clear the done_ids list, so it won't influence the algorithm's
             * future runs (if these will exist).
             */
            done_ids.Clear();
        }

        /// <summary>
        /// Recursive function to get to all the instructions of the Function.
        /// </summary>
        /// <param name="actual">Actual BasicBlock</param>
        private static void recursive(BasicBlock actual, Variable.State state)
        {
            /*
             * We deal with every original(1) instruction in the basic block,
             * and in every instruction we deal with every referenced variable.
             * The referenced variables should be removed from the dead variables list
             * in all the instructions accesible from here.
             * 
             * (1): Only the original instructions determine whether a variable is dead
             *      or not, because the fake instructions work with dead variables only.
             *
             * deal_with_var() - it will be described in detail in the forthcoming parts
             */
            foreach (Instruction ins in actual.Instructions)
            {
                if (/*ins.isFake == false*/ true) 
                {
                    foreach (Variable var in ins.RefVariables)
                        deal_with_var(var, ins, state);
                }
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
            List<BasicBlock> bblist = (state == Variable.State.Free) ? actual.getPredecessors : actual.getSuccessors;
            foreach (BasicBlock block in bblist)
            {
                if ( !done_ids.Contains(block.ID) )
                    recursive(block, state);
            }
        }

        /// <summary>
        /// Recursive function to set a Variable as alive in the proper places.
        /// </summary>
        /// <param name="var">the Variable we are dealing with</param>
        /// <param name="ins">Actual Instruction</param>
        private static void deal_with_var(Variable var, Instruction ins, Variable.State state)
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
            List<Instruction> inslist = (state == Variable.State.Free) ? ins.GetPrecedingInstructions() : ins.GetFollowingInstructions();

            /*
             * Now we have that list of instructions, we should do the same thing we have done
             * to this instruction, assuming that it had not been done already.
             */
            foreach (Instruction i in inslist)
            {
                /*
                 * If the variable is not in the instruction's dead variables list, then it indicates
                 * that we have dealt with this instruction.
                 */
                if ( i.DeadVariables.ContainsKey(var) && i.DeadVariables[var] == state )
                    deal_with_var(var, i, state);
            }
        }

        /// <summary>
        /// Clears all the DeadVariables lists of the function's instructions.
        /// </summary>
        /// <param name="func">Actual Function</param>
        private static void ClearDeadVarsLists(Function func)
        {
            foreach (BasicBlock bb in func.BasicBlocks)
                foreach (Instruction inst in bb.Instructions)
                    inst.DeadVariables.Clear();
        }

        /// <summary>
        /// Fills the DeadVariables lists with the variables that aren't already in them.
        /// </summary>
        /// <param name="func">Actual Function</param>
        private static void SetAllVariablesAsDead(Function func, Variable.State state)
        {
            foreach (BasicBlock bb in func.BasicBlocks)
                foreach (Instruction inst in bb.Instructions)
                {
                    foreach (Variable var in func.LocalVariables)
                    {
                        /* 
                         * QUESTION: do we want the variables used for constant recalculation
                         * to be used as dead variables as the original ones?
                         */
                        if (/*!var.fake && */!inst.DeadVariables.ContainsKey(var))
                            inst.DeadVariables.Add(var, state);
                    }
                }
        }

        /* ---------- isLoopBody, isMainRoute algorithms start ----------- */

        /// <summary>
        /// List to hold the id's of the basic blocks we already reached.
        /// Used by the isLoopBody function.
        /// </summary>
        private static List<string> found_ids = new List<string>();

        /// <summary>
        /// Used in the
        /// </summary>
        private static string id = string.Empty;

        /// <summary>
        /// Function to find out whether a basic block is in a loop body, or not.
        /// </summary>
        /// <param name="actual">The questioned basic block.</param>
        /// <returns>True if the basic block is in a loop, False if not.</returns>
        public static bool isLoopBody(BasicBlock bb)
        {
            return StartFromBB(bb, true);
        }

        /// <summary>
        /// Function to find out whether a basic block is in the main Control Flow, or not.
        /// </summary>
        /// <param name="bb">The questioned basic block.</param>
        /// <returns>True if the basic block is in the main Control Flow, False if not.</returns>
        public static bool isMainRoute(BasicBlock bb)
        {
            return !StartFromBB(bb, false);
        }

        private static bool StartFromBB(BasicBlock bb, bool directed)
        {
            /*
             * We clear the former found_ids list, because we don't want the previous run of
             * the algorithm to influence the present one.
             */
            found_ids.Clear();

            if (!directed)
                id = bb.ID;

            foreach (BasicBlock item in bb.getSuccessors)
                reachable_from(item, directed);

            /*
             * If and only if we have got to this basic block during the algorithm,
             * then it is inside a loop.
             */
            if (found_ids.Contains(bb.ID))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Adds this basic block to the found_ids list, and calls itself for all the BB's successors.
        /// </summary>
        /// <param name="actual">Actual BasicBlock</param>
        private static void reachable_from(BasicBlock actual, bool directed)
        {
            /*
             * First we check whether the actual basic block has been already
             * found. If yes, we have nothing to do left.
             */
            if (!found_ids.Contains(actual.ID))
            {
                /* We add the actual basic block's ID to the found_ids list. */
                found_ids.Add(actual.ID);

                /* Then we continue with all the basic blocks reachable from here. */
                List<BasicBlock> reachable = new List<BasicBlock>();
                foreach (BasicBlock item in actual.getSuccessors)
                    reachable.Add(item);
                if (!directed)
                {
                    foreach (BasicBlock item in actual.getPredecessors)
                    {
                        if (item.ID != id)
                            reachable.Add(item);
                    }
                }
                foreach (BasicBlock item in reachable)
                    reachable_from(item, directed);
            }
        }

        /* ----------- isLoopBody, isMainRoute algorithms end ------------ */
    }
}
