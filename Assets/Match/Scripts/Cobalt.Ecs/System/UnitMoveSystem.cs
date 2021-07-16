namespace Cobalt.Ecs
{
    /// Движение юнитов (контроллирумые игроками)
    public class UnitMoveSystem : IMatchSystem
    {
        public Match match;

        public void Update(Match match, int dt)
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
                    unit.moveProgress += unit.moveSpeed * dt / 1000;

                    // Дошли до следующей ячейки
                    if (unit.moveProgress >= 1f)
                    {
                        if (unitInput.move == unit.moveDirection)
                        {
                            TryMoveTo(unit, unit.moveDirection, unit.moveProgress % 1);
                        }
                        else if (unitInput.move != unit.moveDirection)
                        {
                            TryMoveTo(unit, unitInput.move, 0);
                        }
                    }
                }

                // Tick [Dig]
                if (unit.state == Unit.State.Dig)
                {
                    unit.digProgress += unit.digSpeed * dt / 1000;

                    // Прокопали
                    if (unit.digProgress >= 1)
                    {
                        var tx = (int)unit.moveTo.x + match.State.walls.GetLength(0)/2;
                        var ty = (int)unit.moveTo.y + match.State.walls.GetLength(1)/2;
                        match.State.walls[tx, ty] = false;
                        unit.state = Unit.State.Idle;
                    }
                }

                // Calc new position
                unit.pos = Vec2f.Lerp(unit.moveFrom, unit.moveTo, unit.moveProgress);
            }
        }

        public void TryMoveTo(Unit unit, Direction direction, float progress)
        {
            var from = Vec2f.Round(unit.pos);
            var to = from.GetNext(direction);

            if (CanPassTo(unit, to))
            {
                unit.state = Unit.State.Move;
                unit.moveDirection = direction;
                unit.moveFrom = from;
                unit.moveTo = to;
                unit.moveProgress = progress;
            }
            else if (CanDigTo(unit, to))
            {
                unit.state = Unit.State.Dig;
                unit.moveDirection = direction;
                unit.moveFrom = from;
                unit.moveTo = to;
                unit.moveProgress = 0;

                unit.digProgress = 0;
            }
            else
            {
                unit.state = Unit.State.Idle;
                unit.moveDirection = Direction.None;
                unit.moveFrom = from;
                unit.moveTo = from;
                unit.moveProgress = 0;
            }
        }

        public bool CanPassTo(Unit unit, Vec2f to)
        {
            // Walls
            var width = match.State.walls.GetLength(0);
            var height = match.State.walls.GetLength(1);
            
            var x = width/2 + (int)to.x;
            var y = height/2 + (int)to.y;

            if (x < 0 || x >= width) return false;
            if (y < 0 || y >= height) return false;

            var hasWall = match.State.walls[x, y];
            if (hasWall) return false;

            // Objects
            foreach (var u in match.State.units)
            {
                if (u != unit)
                {
                    var tx = (int)to.x;
                    var ty = (int)to.y;

                    var ufx = (int)u.moveFrom.x;
                    var ufy = (int)u.moveFrom.y;
                    if (tx == ufx && ty == ufy) return false;

                    var utx = (int)u.moveTo.x;
                    var uty = (int)u.moveTo.y;
                    if (tx == utx && ty == uty) return false;
                }
            }

            // Way is free
            return true;
        }
    
        public bool CanPushTo(Unit unit, Vec2f to)
        {
            return false;
        }

        public bool CanDigTo(Unit unit, Vec2f to)
        {
            var width = match.State.walls.GetLength(0);
            var height = match.State.walls.GetLength(1);
            
            var x = width/2 + (int)to.x;
            var y = height/2 + (int)to.y;

            if (x < 0 || x >= width) return false;
            if (y < 0 || y >= height) return false;

            return match.State.walls[x, y];
        }
    }
}