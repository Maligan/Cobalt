namespace Cobalt.Ecs
{
    /// Движение юнитов (контроллирумые игроками)
    public class UnitMoveSystem : IMatchSystem
    {
        public Match match;

        public void Tick(Match match, float sec)
        {
            this.match = match;

            for (int i = 0; i < match.State.units.Length; i++)
            {
                var unit = match.State.units[i];
                var unitInput = match.State.inputs[i];

                // Tick [Idle]
                if (unit.state == Unit.State.Idle)
                {
                    if (unitInput.move != Direction.None)
                    {
                        TryMoveTo(unit, unitInput.move, 0);
                    }
                }

                // Tick [Move]
                if (unit.state == Unit.State.Move)
                {
                    unit.moveProgress += unit.moveSpeed * sec;

                    // Дошли до следующей ячейки
                    if (unit.moveProgress >= 1f)
                    {
                        if (unitInput.move == unit.moveDirection)
                        {
                            TryMoveTo(unit, unit.moveDirection, unit.moveProgress % 1);
                        }
                        else if (unitInput.move != unit.moveDirection)
                        {
                            unit.state = Unit.State.Idle;
                            unit.moveProgress = 1;
                        }
                    }

                    unit.pos = Vec2f.Lerp(unit.moveFrom, unit.moveTo, unit.moveProgress);
                }
            }
        }

        public void TryMoveTo(Unit unit, Direction direction, float progress)
        {
            var from = Vec2f.Round(unit.pos);
            var to = from.GetNext(direction);

            var canPass = CanPassTo(unit, from, to);
            if (canPass)
            {
                unit.state = Unit.State.Move;
                unit.moveDirection = direction;
                unit.moveFrom = from;
                unit.moveTo = to;
                unit.moveProgress = progress;
            }
            else
            {
                unit.state = Unit.State.Idle;
                unit.moveDirection = Direction.None;
                unit.moveFrom = from;
                unit.moveTo = from;
                unit.moveProgress = 0;

                unit.pos = from;
            }
        }

        public bool CanPassTo(Unit unit, Vec2f from, Vec2f to)
        {
            var width = match.State.walls.GetLength(0);
            var height = match.State.walls.GetLength(1);
            
            // TODO: walls index vs unit pos
            var x = width/2 + (int)(to.x);
            var y = height/2 + (int)(to.y);

            if (x < 0 || x >= width) return false;
            if (y < 0 || y >= height) return false;

            return !match.State.walls[x, y];
        }
    }
    
}