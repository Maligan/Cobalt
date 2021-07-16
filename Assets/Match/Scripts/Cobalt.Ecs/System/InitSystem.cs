using System.Collections;

namespace Cobalt.Ecs
{
    public class InitSystem : IMatchSystem
    {
        private bool inited;

        public void Update(Match match, int dt)
        {
            if (inited) return;

            inited = true;

            // var cave = CellularAutomata.Random(21, 19, 256);
            // var caveBits = new BitArray(cave.Length);
            // var caveW = cave.GetLength(0);
            // var caveH = cave.GetLength(1);

            // for (var x = 0; x < caveW; x++)
            //     for (var y = 0; y < caveH; y++)
            //         caveBits[x * caveH + y] = cave[x, y];
            
            match.State.walls = CellularAutomata.Cave_1;
        }
    }
}