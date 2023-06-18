using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TravellingSalesmanProblem
{
    public class Solver
    {
        public List<Vector2> Cities { get; private set; }
        public IEnumerable<int> LastTriedPath { get; private set; }
        public IEnumerable<int> CurrentBestPath { get; private set; }
        public float CurrentBestDistance { get; private set; } = float.PositiveInfinity;

        public Solver(IEnumerable<Vector2> cities)
        {
            // Create a copy of the list given as a parameter
            Cities = new List<Vector2>(cities);

            LastTriedPath = new List<int>();
            CurrentBestPath = new List<int>();
        }

        public IEnumerable<int> CalculateBestPath()
        {
            IEnumerable<int> cityIndices = Enumerable.Range(1, Cities.Count);

            foreach (IEnumerable<int> path in cityIndices.IteratePermutations())
            {
                // Starting/ending city is not included in permutations as it is always first and last,
                // however it should be included in the final path result
                IEnumerable<int> fullPath = path.Prepend(0);
                fullPath = fullPath.Append(0);

                LastTriedPath = fullPath;
                float distance = CalculatePathDistance(fullPath);
                if (distance < CurrentBestDistance)
                {
                    CurrentBestPath = fullPath;
                    CurrentBestDistance = distance;
                }
            }

            return CurrentBestPath;
        }

        private float CalculatePathDistance(IEnumerable<int> cityPath)
        {
            List<Vector2> pathList = cityPath.Select(i => Cities[i]).ToList();
            float distance = 0;
            for (int i = 1; i < pathList.Count; i++)
            {
                // .LengthSquared is faster than .Length whilst still being enough to find the shortest path
                distance += (pathList[i] - pathList[i - 1]).LengthSquared();
            }
            return distance;
        }
    }
}
