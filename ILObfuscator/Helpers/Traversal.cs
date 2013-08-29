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
        /// <summary>
        /// It holds the new order of the basic blocks.
        /// </summary>
        private static List<BasicBlock> newList = new List<BasicBlock>();

        public static void ReorderBasicBlocks (Function func)
        {
            /* Compulsory cleanup before doing anything. */
            newList.Clear();

            /* 
             * We start from the first basic block which is (by convention) also
             * the first in the function's BasicBlocks list.
             */
            DealWithBasicBlock(func.BasicBlocks.First());
        }

        private static void DealWithBasicBlock(BasicBlock bb)
        {
            /* We check whether we have already dealt with this basic block. */
            if (newList.Contains(bb))
                return;

            /* We put this basic block in the new list. */
            newList.Add(bb);

            /* 
             *  We deal with this basic block's direct successor, which is:
             * 
             *  - the only one, if the basic block does not end with a jump.
             *  
             *  - the second one (the false lane) if the basic block ends with
             *    a conditional jump.
             */
            switch (bb.getSuccessors.Count)
            {
                case 0:
                    /* Fake exit block: the end of the control flow. */
                    return;

                case 1:
                    if (bb.Instructions.Last().statementType != StatementTypeType.EnumValues.eUnconditionalJump)
                    {
                        if (newList.Contains(bb.getSuccessors.First()))
                            throw new ObfuscatorException("Cannot place before direct successor.");
                        DealWithBasicBlock(bb.getSuccessors.First());
                    }
                    break;

                case 2:
                    if (newList.Contains(bb.getSuccessors.Last()))
                        throw new ObfuscatorException("Cannot place before direct successor.");
                    DealWithBasicBlock(bb.getSuccessors.Last());
                    break;

                default:
                    throw new ObfuscatorException("A basic block can only have 0, 1 or 2 successors.");
            }
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

