using Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class Traversal
    {

        
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

    }
}

