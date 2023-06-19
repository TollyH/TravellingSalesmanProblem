using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace TravellingSalesmanProblem
{
    public class Solver
    {
        public List<Vector2> Cities { get; private set; }
        public List<int> LastTriedPath { get; private set; }
        public List<int> CurrentBestPath { get; private set; }
        public float CurrentBestDistance { get; private set; } = float.PositiveInfinity;

        public int TriedPaths { get; private set; } = 0;
        public int Improvements { get; private set; } = 0;

        public Stopwatch IterationStopwatch { get; private set; } = new();

        public Solver(IEnumerable<Vector2> cities)
        {
            // Create a copy of the list given as a parameter
            Cities = new List<Vector2>(cities);

            LastTriedPath = new List<int>();
            CurrentBestPath = new List<int>();
        }

        public List<int> CalculateBestPath(CancellationToken cancellationToken)
        {
            if (Cities.Count == 0)
            {
                return new List<int>();
            }

            IEnumerable<int> cityIndices = Enumerable.Range(1, Cities.Count - 1);

            IterationStopwatch.Start();
            foreach (IEnumerable<int> path in cityIndices.IteratePermutations())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                // Starting/ending city is not included in permutations as it is always first and last,
                // however it should be included in the final path result
                IEnumerable<int> fullPath = path.Prepend(0);
                fullPath = fullPath.Append(0);
                List<int> pathList = fullPath.ToList();

                LastTriedPath = pathList;
                TriedPaths++;
                float distance = CalculatePathDistance(pathList);
                if (distance < CurrentBestDistance)
                {
                    CurrentBestPath = pathList;
                    CurrentBestDistance = distance;
                    Improvements++;
                }
            }
            IterationStopwatch.Stop();

            return CurrentBestPath;
        }

        private float CalculatePathDistance(IList<int> cityPath)
        {
            List<Vector2> pathList = cityPath.Select(i => Cities[i]).ToList();
            float distance = 0;
            for (int i = 1; i < pathList.Count; i++)
            {
                distance += (pathList[i] - pathList[i - 1]).Length();
            }
            return distance;
        }
    }
}
