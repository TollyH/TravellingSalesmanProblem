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
                int index = 0;
                foreach (T startItem in sequence)
                {
                    IEnumerable<T> remainingItems = sequence.Where((_, i) => i != index);
                    foreach (IEnumerable<T> permutation in remainingItems.IteratePermutations())
                    {
                        yield return permutation.Prepend(startItem);
                    }
                    index++;
                }
            }
        }
    }
}
