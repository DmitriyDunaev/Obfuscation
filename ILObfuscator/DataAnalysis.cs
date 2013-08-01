using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILObfuscator
{
    public static class DataAnalysis
    {
        public static void SetAllVariablesAsDead(Function func)
        {
            foreach (BasicBlock bb in func.BasicBlocks)
                foreach (Instruction inst in bb.Instructions)
                {
                    inst.DeadVariables.Clear();
                    inst.DeadVariables.AddRange(func.Variables);
                }
        }
    }
}
