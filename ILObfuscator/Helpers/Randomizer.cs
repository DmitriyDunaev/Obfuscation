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
        public static List<int> MultipleNumbers(int quantity, int min, int max, bool equal, bool sort_ascending)
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
        public static int SingleNumber(int min, int max)
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
        public static BasicBlock JumpableBasicBlock(Function func)
        {
            if (func.BasicBlocks.Count < 2)
                throw new RandomizerException("Function has no jumpable basic blocks.");
            int block_num = 0;
            do
            {
                block_num = SingleNumber(0, func.BasicBlocks.Count - 1);
            } while (func.BasicBlocks[block_num].getSuccessors.Count == 0);
            return func.BasicBlocks[block_num];
        }


        /// <summary>
        /// This function sets the target of a jump from a conditional jump
        /// The result can be: the original target, one of the generated
        /// polyRequired targets, or a new polyRequired target
        /// </summary>
        /// <param name="targetlist">The already existing list, with the original jump target at its first position</param>
        /// <returns>The target of the jump, based on some parameters in the Common static class</returns>
        public static BasicBlock GeneratePolyRequJumpTarget(List<BasicBlock> targetlist)
        {
            
            if (targetlist.Count() == 1) return targetlist.First();
            Common.JumpGenerationChances result = (Common.JumpGenerationChances) OneFromManyWithProbability(new int[3] { (int)Common.JumpGenerationChances.Original, (int)Common.JumpGenerationChances.Existing, (int)Common.JumpGenerationChances.New },
                                                                               Common.JumpGenerationChances.Original, Common.JumpGenerationChances.Existing, Common.JumpGenerationChances.New);
            if (result == Common.JumpGenerationChances.Original)
            {
                return targetlist.First();
            }
            else if (result == Common.JumpGenerationChances.Existing)
            {
                if (targetlist.Count() == 1) return targetlist.First();
                return targetlist[SingleNumber(1, targetlist.Count() - 1)];
            }
            else if (result == Common.JumpGenerationChances.New)
            {
                targetlist.Add(new BasicBlock(targetlist.First(), targetlist.First().getSuccessors));
                targetlist.Last().Instructions.ForEach(delegate(Instruction inst) { inst.polyRequired = true; });
                return targetlist.Last();
            }
            else
                return null;
        }


        /// <summary>
        /// Shuffles the order of elements in a list
        /// </summary>
        /// <typeparam name="T">Type of list elements</typeparam>
        /// <param name="target">The list</param>
        public static void Shuffle<T>(this IList<T> target)
        {
            SortedList<int, T> newList = new SortedList<int, T>();
            foreach (T item in target)
                newList.Add(rnd.Next(), item);
            target.Clear();
            for (int i = 0; i < newList.Count; i++)
                target.Add(newList.Values[i]);
        }
        

        /// <summary>
        /// Randomly selects one value from many
        /// </summary>
        /// <param name="many">Values one-by-one</param>
        /// <returns>Random value, selected among the parameters</returns>
        public static object OneFromMany(params object[] many)
        {
            return many[SingleNumber(0, many.Length - 1)];
        }


        /// <summary>
        /// Randomly selects one value from many with fixed values probability
        /// </summary>
        /// <param name="probabilities">Probabilities in array</param>
        /// <param name="many">Values one-by-one</param>
        /// <returns>Random value, selected according to fixed probabilities</returns>
        public static object OneFromManyWithProbability(int[] probabilities, params object[] many)
        {
            if (probabilities.Aggregate(0, (total, next) => total + next) != 100)
                throw new RandomizerException("Overall probability must be 100%.");
            if (probabilities.Count() != many.Count())
                throw new RandomizerException("Total number of parameters must be equal to number of probabilities.");
            int random_prob = SingleNumber(0, 100);
            int accumulated_prob = 0;
            for (int i = 0; i < probabilities.Count(); i++)
            {
                accumulated_prob += probabilities[i];
                if (random_prob <= accumulated_prob)
                    return many[i];
            }
            throw new RandomizerException("Internal randomizer exception occured: no value can be returned.");
        }


        /// <summary>
        /// Gets randomly one fake input parameter of a function. Throws exception if no such found.
        /// </summary>
        /// <param name="func">A function with parameters</param>
        /// <returns>One random fake input parameter of a function</returns>
        public static Variable FakeInputParameter(Function func)
        {
            if (func.LocalVariables.Count(x => x.kind == Variable.Kind.Input && x.fake) == 0)
                throw new RandomizerException("Function " + func.ID + " has no fake input variables (parameters).");
            List<Variable> fake_inputs = func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake);
            return fake_inputs[SingleNumber(0, fake_inputs.Count - 1)];
        }


        /// <summary>
        /// Gets randomly one dead variable with given states
        /// </summary>
        /// <param name="ins">An instruction, which contains dead variables</param>
        /// <param name="states">The states of dead variables, which will be collected</param>
        /// <returns>The dead variable</returns>
        public static Variable DeadVariable(Instruction ins, params Variable.State[] states)
        {
            /* First we gather the variables with the proper state. */
            List<Variable> proper_vars = new List<Variable>();
            foreach (KeyValuePair<Variable, Variable.State> dead in ins.DeadVariables)
                if (states.Contains(dead.Value))
                    proper_vars.Add(dead.Key);
            /* If no such exists, we return null. */
            if (proper_vars.Count == 0)
                return null;
            /* If there are ones that fit our needs, then we choose one randomly. */
            else
                return proper_vars[Randomizer.SingleNumber(0, proper_vars.Count - 1)];
        }



        /// <summary>
        /// Makes random conditional jump instruction + links basic blocks and sets RefVars
        /// </summary>
        /// <param name="nop">NoOperation instruction (will be made into ConditionalJump)</param>
        /// <param name="condition">Type of condition</param>
        /// <param name="target">Target basic block the control flow is transfered to, if the relation holds true.</param>
        public static void GenerateConditionalJumpInstruction(Instruction nop, Instruction.ConditionType condition, BasicBlock target)
        {
            if (nop.statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation && nop.statementType != ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
                throw new RandomizerException("Only NoOperation and UnconditionalJump instructions can be modified to ConditionalJump!");
            if (nop.parent == null || nop.parent.parent == null)
                throw new RandomizerException("Instruction -> basic block -> function parent link is broken.");
            if (nop.parent.parent != target.parent)
                throw new RandomizerException("The instruction and the basic block should be contained in the same function.");

            Variable var = FakeInputParameter(nop.parent.parent);
            if (var.fixedMax.HasValue && var.fixedMax.Value > Common.GlobalMaxNumber)
                throw new RandomizerException("The fixedMax value is greated then the globally accepted maximum.");
            if (var.fixedMin.HasValue && var.fixedMin.Value < Common.GlobalMinNumber)
                throw new RandomizerException("The fixedMin value is smaller then the globally accepted minimum.");
            int right_value = 0;
            Instruction.RelationalOperationType relop = Instruction.RelationalOperationType.Equals;
            bool use_min_limit = false;
            // Here we chose to use a FixedMin or FixedMax for logical relation
            if (var.fixedMin.HasValue && var.fixedMax.HasValue)
                use_min_limit = (bool)OneFromMany(true, false);
            else if (var.fixedMin.HasValue)
                use_min_limit = true;
       
            if (use_min_limit)  // FixedMin will be used
            {
                right_value = SingleNumber(Common.GlobalMinNumber, var.fixedMin.Value);
                switch (condition)
                {
                    case Instruction.ConditionType.AlwaysTrue:
                        relop = (Instruction.RelationalOperationType)OneFromMany(Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals,
                                                                                    Instruction.RelationalOperationType.NotEquals);
                        break;
                    case Instruction.ConditionType.AlwaysFalse:
                        relop = (Instruction.RelationalOperationType)OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                                    Instruction.RelationalOperationType.Equals);
                        break;
                    case Instruction.ConditionType.Random:
                        relop = (Instruction.RelationalOperationType)OneFromMany(Instruction.RelationalOperationType.Smaller,
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
                right_value = SingleNumber(var.fixedMax.Value, Common.GlobalMaxNumber);
                switch (condition)
                {
                    case Instruction.ConditionType.AlwaysTrue:
                        relop = (Instruction.RelationalOperationType)OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                                    Instruction.RelationalOperationType.Equals);
                        break;
                    case Instruction.ConditionType.AlwaysFalse:
                        relop = (Instruction.RelationalOperationType)OneFromMany(Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals,
                                                                                    Instruction.RelationalOperationType.NotEquals);
                        break;
                    case Instruction.ConditionType.Random:
                        relop = (Instruction.RelationalOperationType)OneFromMany(Instruction.RelationalOperationType.Smaller,
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


        /// <summary>
        /// Randomly selects unique elements from a list
        /// </summary>
        /// <param name="arr">List with elements to be selected from</param>
        /// <param name="m">Number of elements to be selected</param>
        /// <returns>List with selected elements</returns>
        public static IList<T> UniqueSelect<T>(IList<T> arr, int m)
        {
            T[] res = new T[m];
            if (m > arr.Count())
                throw new RandomizerException("Number of objects to be selected exceeds the list length.");
            
            for (int i = 0; i < arr.Count(); i++)
            {
                /* selecting m from remaining n-i */
                if ((rnd.Next() % (arr.Count() - i)) < m)
                    res[--m] = arr.ElementAt(i);
            }
            return res;
        }
    }

}