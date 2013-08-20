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
            return (Instruction.RelationalOperationType)GetSingleNumber(0, 5);
        }


        /// <summary>
        /// Randomly selects one value from many
        /// </summary>
        /// <param name="many">Values one-by-one</param>
        /// <returns>Random value, selected among the parameters</returns>
        public static object GetOneFromMany(params object[] many)
        {
            return many[GetSingleNumber(0, many.Length - 1)];
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


        /// <summary>
        /// Makes random conditional jump instruction + links basic blocks and sets RefVars
        /// </summary>
        /// <param name="nop">NoOperation instruction (will be made into ConditionalJump)</param>
        /// <param name="condition">Type of condition</param>
        /// <param name="target">Target basic block the control flow is transfered to, if the relation holds true.</param>
        public static void GenerateConditionalJumpInstruction(Instruction nop, Instruction.ConditionType condition, BasicBlock target)
        {
            if (nop.statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
                throw new RandomizerException("Only NoOperation instruction can be modified to other type!");
            if (nop.parent == null || nop.parent.parent == null)
                throw new RandomizerException("Instruction -> basic block -> function parent link is broken.");
            if (nop.parent.parent != target.parent)
                throw new RandomizerException("The instruction and the basic block should be contained in the same function.");

            Variable var = GetFakeInputParameter(nop.parent.parent);
            if (var.fixedMax.HasValue && var.fixedMax.Value > Common.GlobalMaxNumber)
                throw new RandomizerException("The fixedMax value is greated then the globally accepted maximum.");
            if (var.fixedMin.HasValue && var.fixedMin.Value < Common.GlobalMinNumber)
                throw new RandomizerException("The fixedMin value is smaller then the globally accepted minimum.");
            int right_value = 0;
            Instruction.RelationalOperationType relop = Instruction.RelationalOperationType.Equals;
            bool use_min_limit = false;
            // Here we chose to use a FixedMin or FixedMax for logical relation
            if (var.fixedMin.HasValue && var.fixedMax.HasValue)
                use_min_limit = (bool)GetOneFromMany(true, false);
            else if (var.fixedMin.HasValue)
                use_min_limit = true;
       
            if (use_min_limit)  // FixedMin will be used
            {
                right_value = GetSingleNumber(Common.GlobalMinNumber, var.fixedMin.Value);
                switch (condition)
                {
                    case Instruction.ConditionType.AlwaysTrue:
                        relop = (Instruction.RelationalOperationType)GetOneFromMany(Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals,
                                                                                    Instruction.RelationalOperationType.NotEquals);
                        break;
                    case Instruction.ConditionType.AlwaysFalse:
                        relop = (Instruction.RelationalOperationType)GetOneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                                    Instruction.RelationalOperationType.Equals);
                        break;
                    case Instruction.ConditionType.Random:
                        relop = (Instruction.RelationalOperationType)GetOneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                                    Instruction.RelationalOperationType.Equals,
                                                                                    Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals,
                                                                                    Instruction.RelationalOperationType.NotEquals);
                        break;
                    default:
                        throw new RandomizerException("Unrecognized condition type.");
                }
            }

            if (!use_min_limit)     // FixedMax will be used
            {
                right_value = GetSingleNumber(var.fixedMax.Value, Common.GlobalMaxNumber);
                switch (condition)
                {
                    case Instruction.ConditionType.AlwaysTrue:
                        relop = (Instruction.RelationalOperationType)GetOneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                                    Instruction.RelationalOperationType.Equals);
                        break;
                    case Instruction.ConditionType.AlwaysFalse:
                        relop = (Instruction.RelationalOperationType)GetOneFromMany(Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals,
                                                                                    Instruction.RelationalOperationType.NotEquals);
                        break;
                    case Instruction.ConditionType.Random:
                        relop = (Instruction.RelationalOperationType)GetOneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                                    Instruction.RelationalOperationType.Equals,
                                                                                    Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals,
                                                                                    Instruction.RelationalOperationType.NotEquals);
                        break;
                    default:
                        throw new RandomizerException("Unrecognized condition type.");
                }
            }
            nop.MakeConditionalJump(var, right_value, relop, target);
        }
    }

}