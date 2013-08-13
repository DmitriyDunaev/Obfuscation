using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;
using ExchangeFormat;

namespace Obfuscator
{
    public static class FakeCode
    {
        /// <summary>
        /// Describes the probability of generating a conditional jump in percents.
        /// </summary>
        private static int prob_of_cond_jump = 10;

        /// <summary>
        /// Function to change the nop's in the function to actual fake code.
        /// </summary>
        /// <param name="func">The function to work on.</param>
        public static void Generate (Function func)
        {
            /* We have to go through all the nop's in the function. */
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                foreach (Instruction ins in bb.Instructions)
                {
                    if (ins.statementType == StatementTypeType.EnumValues.eNoOperation)
                    {
                        /*
                         * Now we found a nop, so we have to decide what to do with it.
                         * 
                         * At a 10% probability we generate a Conditional Jump, which condition
                         * depends on a parameter value, and happens to be always false.
                         * We use these jumps to make the control flow irreducible.
                         * However we must not choose this if we have only ona basic block.
                         * 
                         * At the remaining 90% probability we generate instructions that are
                         * using the dead variables present at the point of the actual nop.
                         * However we must not choose this if we have no dead variables present.
                         */
                        if (func.BasicBlocks.Count < 2 && ins.DeadVariables.Count < 1)
                            throw new ObfuscatorException("We cannot really do anything with this instruction right now, sorry.");

                        if (Randomizer.GetSingleNumber(0, 99) < prob_of_cond_jump)
                        {
                            if (func.BasicBlocks.Count < 2)
                            {
                                /* Though randomizer said we should do this, we unfortunately cannot. */
                                GenFakeIns(ins);
                            }
                            /* This is where we generate a conditional jump.*/
                            GenCondJump(ins);
                        }
                        else
                        {
                            if (ins.DeadVariables.Count < 1)
                            {
                                /* Though randomizer said we should do this, we unfortunately cannot. */
                                GenCondJump(ins);
                            }
                            /* This is where we generate instructions with dead variables. */
                            GenFakeIns(ins);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function to make a Conditional Jump out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void GenCondJump(Instruction ins)
        {
            /*
             * First we want to find a basic block that is inside a loop,
             * so that we can make a jump on it, making the control flow
             * irreducible this way.
             */
            List<BasicBlock> bodies = new List<BasicBlock>();
            foreach (BasicBlock bb in ins.parent.parent.BasicBlocks)
            {
                if (DataAnalysis.isLoopBody(bb) && !bb.Equals(ins.parent))
                    bodies.Add(bb);
            }

            /* If there are such basic blocks. */
            if (bodies.Count() != 0)
            {
                int num = Randomizer.GetSingleNumber(0, bodies.Count() - 1);
                BasicBlock jumptarget = bodies[num];
            }

            /* If not: we choose a random one from all the basic blocks. */
            else
            {
                List<BasicBlock> all_bbs = new List<BasicBlock>();
                foreach (BasicBlock bb in ins.parent.parent.BasicBlocks)
                {
                    if (!bb.Equals(ins.parent))
                        all_bbs.Add(bb);
                }

                int num = Randomizer.GetSingleNumber(0, all_bbs.Count() - 1);
                BasicBlock jumptarget = all_bbs[num];
            }

            /* 
             * At this point we have a perfectly chosen jumptarget,
             * so we can finally make the conditional jump.
             */
        }

        /// <summary>
        /// Function to make an actual fake instruction out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void GenFakeIns(Instruction ins)
        {
        }
    }
}
