using Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class Randomizer
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);


        /// <summary>
        /// Provides a list of random numbers within specified boundaries
        /// </summary>
        /// <param name="quantity">Amount of numbers needed</param>
        /// <param name="min">Nonnegative inclusive lower bound</param>
        /// <param name="max">Nonnegative inclusive upper bound</param>
        /// <param name="equal">If true, generated random numbers can be equal to each other</param>
        /// <param name="sort_ascending">If true, the returned list will be sorted in ascending order</param>
        /// <returns>A list of random numbers within specified boundaries</returns>
        public static List<int> GetMultipleNumbers(int quantity, int min, int max, bool equal, bool sort_ascending)
        {
            if (min > max || min < 0 || max < 0)
                throw new RandomizerException("Random numbers cannot be generated. Wrong conditions passed.");
            if (!equal && max - min + 1 < quantity)
                throw new RandomizerException("Random numbers cannot be generated. Wrong conditions passed.");
            List<int> randoms = new List<int>();
            for (int i = 0; i < quantity; i++)
            {
                int num;
                do
                    num = rnd.Next(min, max + 1);
                while (!equal && randoms.Contains(num));
                randoms.Add(num);
            }
            if (sort_ascending)
                randoms.Sort();
            return randoms;
        }


        /// <summary>
        /// Generates a random number within specified boundaries
        /// </summary>
        /// <param name="min">Nonnegative inclusive lower bound</param>
        /// <param name="max">Nonnegative inclusive upper bound</param>
        /// <returns>A single random number within specified boundaries</returns>
        public static int GetSingleNumber(int min, int max)
        {
            if (max < min || max < 0 || min < 0)
                throw new RandomizerException("Random numbers cannot be generated. Wrong conditions passed.");
            return rnd.Next(min, max + 1);
        }


        /// <summary>
        /// Gets random basic block within a function that we can jump to. It can be any basic block, but not a "fake exit block".
        /// </summary>
        /// <param name="func">A parent function</param>
        /// <returns>Random basic block</returns>
        public static BasicBlock GetJumpableBasicBlock(Function func)
        {
            if (func.BasicBlocks.Count < 2)
                throw new RandomizerException("Function has no jumpable basic blocks.");
            int block_num = 0;
            do
            {
                block_num = GetSingleNumber(0, func.BasicBlocks.Count - 1);
            } while (func.BasicBlocks[block_num].getSuccessors.Count == 0);
            return func.BasicBlocks[block_num];
        }


        /// <summary>
        /// Gets a random relational operation
        /// </summary>
        /// <returns>Random relational operator</returns>
        public static Instruction.RelationalOperationType GetRelop()
        {
            return (Instruction.RelationalOperationType) GetSingleNumber(0, 5);
        }


        /// <summary>
        /// Gets randomely one fake input parameter of a function. Throws exception if no such found.
        /// </summary>
        /// <param name="func">A function with parameters</param>
        /// <returns>One random fake input parameter of a function</returns>
        public static Variable GetFakeInputParameter(Function func)
        {
            if (func.LocalVariables.Count(x => x.kind == Variable.Kind.Input && x.fake) == 0)
                throw new RandomizerException("Function " + func.ID + " has no fake input variables (parameters).");
            List<Variable> fake_inputs = func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake);
            return fake_inputs[GetSingleNumber(0, fake_inputs.Count - 1)];
        }


        public static void MakeConditionalJumpInstruction(Instruction nop, Instruction.ConditionType condition)
        {
            //if(nop.statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
            //    throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            //if(condition == Instruction.ConditionType.AlwaysTrue)

        }
    }
}
