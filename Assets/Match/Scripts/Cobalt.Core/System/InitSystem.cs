using System.Collections;

namespace Cobalt.Core
{
    public class InitSystem : IMatchSystem
    {
        private bool inited;

        public void Tick(Match match, float sec)
        {
            if (inited) return;

            inited = true;

            var cave = MatchBuilder.Random(21, 19);
            var caveBits = new BitArray(cave.Length);
            var caveW = cave.GetLength(0);
            var caveH = cave.GetLength(1);

            for (var x = 0; x < caveW; x++)
                for (var y = 0; y < caveH; y++)
                    caveBits[x * caveH + y] = cave[x, y];
        }
    }
}