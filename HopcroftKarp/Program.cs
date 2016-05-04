using System;
using System.Collections.Generic;
using System.Linq;

namespace HopcroftKarp
{
    internal static class Program
    {
        private static void Main()
        {
            var lefts = new[] { "U0", "U1", "U2", "U3", "U4" };
            var rights = new[] { "V0", "V1", "V2", "V3", "V4" };

            var edges = new Dictionary<string, string[]>
            {
                ["U0"] = new[] { "V0", "V1" },
                ["U1"] = new[] { "V0", "V4" },
                ["U2"] = new[] { "V2", "V3" },
                ["U3"] = new[] { "V0", "V4" },
                ["U4"] = new[] { "V1", "V3" }
            };

            Console.WriteLine(HopcroftKarp(lefts, rights, edges));
        }

        private static bool Bfs(IEnumerable<string> lefts,
                IReadOnlyDictionary<string, string[]> edges,
                IReadOnlyDictionary<string, string> toMatchedRight,
                IReadOnlyDictionary<string, string> toMatchedLeft,
                IDictionary<string, long> distances,
                Queue<string> unmatchedLefts)
        {
            foreach (var left in lefts)
            {
                if (toMatchedRight[left] == "")
                {
                    distances[left] = 0;
                    unmatchedLefts.Enqueue(left);
                }
                else
                {
                    distances[left] = long.MaxValue;
                }
            }

            distances[""] = long.MaxValue;

            while (0 < unmatchedLefts.Count)
            {
                var left = unmatchedLefts.Dequeue();

                if (distances[left] < distances[""])
                {
                    foreach (var right in edges[left])
                    {
                        var left2 = toMatchedLeft[right];
                        if (distances[left2] == long.MaxValue)
                        {
                            // Reach here if the right is not matched with left.
                            distances[left2] = distances[left] + 1;
                            unmatchedLefts.Enqueue(left2);
                        }
                    }
                }
            }

            return distances[""] != long.MaxValue;
        }

        private static bool Dfs(string left,
                                IReadOnlyDictionary<string, string[]> edges,
                                IDictionary<string, string> toMatchedRight,
                                IDictionary<string, string> toMatchedLeft,
                                IDictionary<string, long> distances)
        {
            if (left == "")
            {
                return true;
            }

            foreach (var right in edges[left])
            {
                var left2 = toMatchedLeft[right];
                if (distances[left2] == distances[left] + 1)
                {
                    if (Dfs(left2, edges, toMatchedRight, toMatchedLeft, distances))
                    {
                        toMatchedLeft[right] = left;
                        toMatchedRight[left] = right;
                        return true;
                    }
                }
            }

            distances[left] = long.MaxValue;

            return false;
        }

        private static long HopcroftKarp(string[] lefts,
                                          IEnumerable<string> rights,
                                          IReadOnlyDictionary<string, string[]> edges)
        {
            // "distance" is from a starting left to another left when zig-zaging left, right, left, right, left in DFS.

            // Take the following for example:
            // left1 -> (unmatched edge) -> right1 -> (matched edge) -> left2 -> (unmatched edge) -> right2 -> (matched edge) -> left3
            // distance can be as follows.
            // distances[left1] = 0 (Starting left is distance 0.)
            // distances[left2] = distances[left1] + 1 = 1
            // distances[left3] = distances[left2] + 1 = 2

            // Note
            // Both a starting left and an ending left are unmatched with right.
            // Moving from left to right uses a unmatched edge.
            // Moving from right to left uses a matched edge.

            var distances = new Dictionary<string, long>();

            var unmatchedLefts = new Queue<string>();

            // All lefts are unmatched at first
            var toMatchedRight = lefts.ToDictionary(s => s, s => "");

            // All rights are unmatched at first
            var toMatchedLeft = rights.ToDictionary(s => s, s => "");

            // Note
            // toMatchedRight and toMatchedLeft are the same thing and inverse to each other.
            // Using either of them works enough but is inefficient
            // because a dictionary cannot be straightforwardly looked up bi-directionally.

            var matchingCount = 0L;

            while (Bfs(lefts, edges, toMatchedRight, toMatchedLeft, distances, unmatchedLefts))
            {
                matchingCount += lefts.Where(left => toMatchedRight[left] == "")
                                      .LongCount(left => Dfs(left, edges, toMatchedRight, toMatchedLeft, distances));
            }

            return matchingCount;
        }
    }
}
