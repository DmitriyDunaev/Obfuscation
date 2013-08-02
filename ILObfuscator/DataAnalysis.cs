using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class DataAnalysis
    {

        /// <summary>
        /// Used once by the Data Analysis Algorithm and fills all DeadVariables lists with all variables defined in the function 
        /// </summary>
        /// <param name="func">Actual Function</param>
        public static void SetAllVariablesAsDead(Function func)
        {
            foreach (BasicBlock bb in func.BasicBlocks)
                foreach (Instruction inst in bb.Instructions)
                {
                    inst.DeadVariables.Clear();
                    inst.DeadVariables.AddRange(func.Variables);
                }
        }

        /// <summary>
        /// Gets preceding instructions within a function 
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
                foreach (BasicBlock bb in instr.parent.getAllPredeccessors())
                {
                    preceding.Add(bb.Instructions[bb.Instructions.Count - 1]);
                }
                return preceding;
            }
            else return null;
        }
    }
}
