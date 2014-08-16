using Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
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
        /// Randomly selects one value from many with fixed values probability using uniform distribution
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
        /// Randomly selects one value from one set using uniform distribution. The sets are chosen by given probability, the choice of element in the set is equiprobable.
        /// </summary>
        /// <param name="probabilities">Probabilities of chosing the respective sets</param>
        /// <param name="sets">The sets with values</param>
        /// <returns>One value from a set</returns>
        public static object OneValueFromManySetsWithProbability(int[] probabilities, params IEnumerable<object>[] sets)
        {
            IEnumerable selected_set = (IEnumerable)OneFromManyWithProbability(probabilities, sets);
            return OneFromMany(selected_set.Cast<object>().ToArray());
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
        /// Selects one number between MostProbable and ZeroProbable based on f(x)=1/x function
        /// </summary>
        /// <param name="mostProbableNumber">The most probable number (inclusive)</param>
        /// <param name="zeroProbableNumber">The zero probable number (exclusive)</param>
        /// <returns>A number based on probability distribution</returns>
        public static int OneFromSectionWithDescendingProbability(int mostProbableNumber, int zeroProbableNumber)
        {
            if (mostProbableNumber == zeroProbableNumber)
                return mostProbableNumber;
            int numbers = Math.Abs(zeroProbableNumber - mostProbableNumber);
            List<int> weights = new List<int>(numbers);
            for (int i = 0; i < numbers; i++)
                weights.Add(Convert.ToInt32(Math.Round((1 / (0.5 + i))*1000000)));
            double divisor = weights.Sum() / 1000000;
            for (int i = 1; i < numbers; i++)
                weights[i] = Convert.ToInt32(Math.Round(weights[i] / divisor));
            weights[0] = 0;
            weights[0] = 1000000 - weights.Sum();
            Dictionary<int, int> weighted_numbers = new Dictionary<int, int>();
            for (int i = 0; i < numbers; i++)
            {
                int aggregated_weight = 0;
                for (int j = 0; j <= i; j++)
                    aggregated_weight = aggregated_weight + Convert.ToInt32(weights[j]);
                if (mostProbableNumber < zeroProbableNumber)
                    weighted_numbers.Add(mostProbableNumber + i, aggregated_weight);
                else
                    weighted_numbers.Add(mostProbableNumber - i, aggregated_weight);
            }
            int equiprobable_random = rnd.Next(0, 1000000);
            foreach (KeyValuePair<int,int> candidate in weighted_numbers)
            {
                if (candidate.Value > equiprobable_random)
                    return candidate.Key;
            }
            throw new RandomizerException("Unexpected behaviour in Randomizer with descending probability. Return candidate not found.");
        }


        /// <summary>
        /// Randomly (using uniform distribution) selects unique elements from a list
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