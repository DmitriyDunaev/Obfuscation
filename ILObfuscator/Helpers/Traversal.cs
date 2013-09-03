//#define DIMA

using Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeFormat;

namespace Obfuscator
{
    public static class Traversal
    {
        private static List<List<BasicBlock>> lists = new List<List<BasicBlock>>();

        /// <summary>
        /// Tells us whether the basic block is already contained somewhere.
        /// </summary>
        /// <param name="bb">The basic block to check (can be null).</param>
        /// <returns>The list containing it, or null if no such exists.</returns>
        private static List<BasicBlock> ContainsAlready(BasicBlock bb)
        {
            if (bb == null)
                return null;

            foreach (List<BasicBlock> l in lists)
            {
                if (l.Contains(bb))
                    return l;
            }
            return null;
        }

        /// <summary>
        /// Returns the direct predecessor of a basic block if such exists.
        /// </summary>
        /// <param name="actual">The actual basic block.</param>
        /// <returns>The direct predecessor, or null, if no such exists.</returns>
        private static BasicBlock GetDirectPredecessor(BasicBlock actual)
        {
            /* The "fake exit block" does not have any direct predecessors. */
            if (actual.getSuccessors.Count == 0)
                return null;

            foreach (BasicBlock bb in actual.getPredecessors)
            {
                switch (bb.getSuccessors.Count)
                {
                    case 1:
                        if (bb.Instructions.Last().statementType != StatementTypeType.EnumValues.eUnconditionalJump)
                            return bb;
                        break;

                    case 2:
                        if (bb.getSuccessors.Last().Equals(actual))
                            return bb;
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the direct successor of a basic block if such exists.
        /// </summary>
        /// <param name="actual">The actual basic block.</param>
        /// <returns>The direct successor, or null, if no such exists.</returns>
        private static BasicBlock GetDirectSuccessor(BasicBlock actual)
        {
            /* 
             *  A basic block has direct successor, only if it is:
             * 
             *  - the only one, and the basic block does not end with a jump,
             *    and that successor isn't the "fake exit block".
             *  
             *  - the second one (the false lane) if the basic block ends with
             *    a conditional jump (so it has 2 successors).
             */
            switch (actual.getSuccessors.Count)
            {
                case 1:
                    /* If the successor is the "fake exit block", then it's not direct. */
                    if (actual.getSuccessors.First().getSuccessors.Count == 0)
                        return null;

                    if (actual.Instructions.Last().statementType != StatementTypeType.EnumValues.eUnconditionalJump)
                        return actual.getSuccessors.First();
                    break;

                case 2:
                    return actual.getSuccessors.Last();
            }

            return null;
        }       

        /// <summary>
        /// Algorithm to reorder the basic blocks in a function.
        /// </summary>
        /// <param name="func">The function.</param>
        public static void ReorderBasicBlocks (Function func)
        {
            lists.Clear();

            /* We put all the basic blocks in a list. */
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                List<BasicBlock> succlist = ContainsAlready(GetDirectSuccessor(bb));
                if (succlist != null && !succlist.First().Equals(GetDirectSuccessor(bb)))
                    throw new ObfuscatorException("The direct successor should be the first in its list.");

                List<BasicBlock> predlist = ContainsAlready(GetDirectPredecessor(bb));
                if (predlist != null && !predlist.Last().Equals(GetDirectPredecessor(bb)))
                    throw new ObfuscatorException("The direct predecessor should be the last in its list.");

                /* We have it's direct succesor and it's direct predecessor already in the lists. */
                if (succlist != null && predlist != null)
                {
                    predlist.Add(bb);
                    predlist.AddRange(succlist);
                    lists.Remove(succlist);
                }

                /* We only have it's successor in the lists. */
                else if (succlist != null)
                    succlist.Insert(0, bb);

                /* We only have it's predecessor in the lists. */
                else if (predlist != null)
                    predlist.Add(bb);

                /* Neither it's predecessor, nor it's successor are in the lists. */
                else
                {
                    List<BasicBlock> tmp = new List<BasicBlock>();
                    tmp.Add(bb);
                    lists.Add(tmp);
                }
            }

            if (func.BasicBlocks.Count != CountAll())
                throw new ObfuscatorException("Not all the basic block are in the lists...");

            /* Now we make the reordered BasicBlocks list for the Function. */
            func.BasicBlocks.Clear();
            foreach (List<BasicBlock> bblist in lists)
                func.BasicBlocks.AddRange(bblist);
        }

        private static int CountAll ()
        {
            int sum = 0;
            foreach (List<BasicBlock> l in lists)
                sum += l.Count;
            return sum;
        }

#if DIMA        
        public delegate void ProcessBasicBlockDelegate(BasicBlock bb);


        public static void nonRecursivePostOrder(BasicBlock root, ProcessBasicBlockDelegate process)
        {
            var toVisit = new Stack<BasicBlock>();
            var visitedAncestors = new Stack<BasicBlock>();
            toVisit.Push(root);
            while (toVisit.Count > 0)
            {
                var node = toVisit.Peek();
                if (node.getSuccessors.Count > 0)
                {
                    if (visitedAncestors.PeekOrDefault() != node)
                    {
                        visitedAncestors.Push(node);
                        toVisit.PushReverse(node.getSuccessors);
                        continue;
                    }
                    visitedAncestors.Pop();
                }

                process(node);
                
                toVisit.Pop();
            }
        }

        
        public static BasicBlock PeekOrDefault(this Stack<BasicBlock> s)
        {
            return s.Count == 0 ? null : s.Peek();
        }


        public static void PushReverse(this Stack<BasicBlock> s, List<BasicBlock> list)
        {
            foreach (var l in list.ToArray().Reverse())
                s.Push(l);
        }
#endif
    }
}

