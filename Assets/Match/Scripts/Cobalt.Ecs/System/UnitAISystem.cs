using System;
using System.Collections.Generic;

namespace Cobalt.Ecs
{
    public class UnitAISystem : IMatchSystem
    {
        private Match match;
        private List<Btree<Unit>> brains;

        public void Tick(Match match, float sec)
        {
            this.match = match;

            if (brains == null)
                CreateBrains();

            foreach (var brain in brains)
                brain.Tick();
        }

        private void CreateBrains()
        {
            brains = new List<Btree<Unit>>();

            for (int i = 0; i < match.State.units.Length; i++)
            {
                var unit = match.State.units[i];
                var input = GetInput(unit);

                if (input.flag == false)
                {
                    var brain = CreateBrain(unit);
                    brains.Add(brain);
                }
            }
        }

        private Btree<Unit> CreateBrain(Unit unit)
        {
            return new Btree<Unit>(unit)
                .While()
                    .If(x => x.pos.x < -3 || GetInput(x).move == Direction.None)
                        .Do(x => GetInput(x).move = Direction.Right)
                        .End()
                    .Else(x => x.pos.x > +3)
                        .Do(x => GetInput(x).move = Direction.Left)
                        .End();
        }

        private UnitInput GetInput(Unit unit)
        {
            for (var i = 0; i < match.State.units.Length; i++)
                if (match.State.units[i] == unit)
                    return match.State.inputs[i];
                
            throw new ArgumentException();
        }
    }
}