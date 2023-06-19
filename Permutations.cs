using System.Collections.Generic;
using System.Linq;

namespace TravellingSalesmanProblem
{
    public static class Permutations
    {
        public static IEnumerable<IEnumerable<T>> IteratePermutations<T>(this IEnumerable<T> sequence)
        {
            if (!sequence.Any())
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                List<T> list = sequence.ToList();
                int index = 0;
                foreach (T startItem in list)
                {
                    IEnumerable<T> remainingItems = list.Where((_, i) => i != index);
                    foreach (IEnumerable<T> permutation in remainingItems.IteratePermutations())
                    {
                        yield return permutation.Prepend(startItem);
                    }
                    index++;
                }
            }
        }

        public static int PermutationsCount(int length)
        {
            return length == 0 ? 0 : Enumerable.Range(1, length - 1).Aggregate(1, (acc, val) => acc * val);
        }
    }
}
