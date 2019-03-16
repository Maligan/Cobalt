using System;

namespace Cobalt.Core
{
    public class MatchBuilder
    {
        public static bool[,] Random(int width, int height, int seed = 0)
        {
            var rnd = seed > 0 ? new Random(seed) : new Random();

            var result = new bool[width, height];
            var buffer = new bool[width, height];

            var ik = 0.45f;
            var numBirth = 4;
            var numDeath = 3;
            var steps = 3;

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    result[x, y] = (rnd.Next() / (float)int.MaxValue) < ik;

            for (var i = 0; i < steps; i++)
            {
                Swap(ref result, ref buffer);
                
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var nbs = GetNeighbourCount(buffer, x, y);
                        if (buffer[x, y]) result[x, y] = nbs > numDeath;
                        else result[x, y] = nbs > numBirth;
                    }
                }
            }

            return result;
        }

        private static int GetNeighbourCount(bool[,] map, int x, int y)
        {
            var result = 0;

            for (var i = -1; i <= +1; i++)
            {
                for (var j = -1; j <= +1; j++)
                {
                    var nx = x + i;
                    var ny = y + j;

                    if (i == 0 && j == 0) continue;
                    else if (nx < 0 || ny < 0 || nx >= map.GetLength(0) || ny >= map.GetLength(1)) result++;
                    else if (map[nx, ny]) result++;
                }
            }

            return result;
        }

        private static void Swap<T>(ref T v1, ref T v2)
        {
            var tmp = v1;
            v1 = v2;
            v2 = tmp;
        }
    }
}