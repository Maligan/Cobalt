using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BTAI;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitAI : MonoBehaviour
{
    private Unit unit;
    private Root tree;

    private IEnumerator Start()
    {
        unit = GetComponent<Unit>();

        var sides = new [] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        while (true)
        {
            var hasNeighbor = true;

            foreach (var side in sides)
            {
                var hasWall = unit.Position.GetNext(side).IsDiggable;
                if (hasWall)
                {
                    unit.SetMove(side);
                    yield return new WaitWhile(() => unit.state != Unit.State.Move);
                    unit.SetMove(Vector2.zero);
                    yield return new WaitWhile(() => unit.state != Unit.State.Idle);                    
                    break;
                }
                else if (side == sides[sides.Length-1])
                    hasNeighbor = false;

            }

            if (!hasNeighbor)
            {
                var wall = unit.Arena.Cells
                           .Where(x => x.IsDiggable)
                           .OrderBy(x => ArenaCell.Manhattan(unit.Position, x))
                           .FirstOrDefault();

                if (wall != null)
                {
                    var dx = wall.X == unit.Position.X ? 0 : Mathf.Sign(wall.X - unit.Position.X);
                    var dy = wall.Y == unit.Position.Y ? 0 :Mathf.Sign(wall.Y - unit.Position.Y);

                    if (dx != 0)
                    {
                        unit.SetMove(new Vector2(dx, 0));
                        yield return new WaitWhile(() => unit.Position.X != wall.X - dx);
                        unit.SetMove(Vector2.zero);
                    }

                    if (dy != 0)
                    {
                        unit.SetMove(new Vector2(0, dy));
                        yield return new WaitWhile(() => unit.Position.Y != wall.Y - dy);
                        unit.SetMove(Vector2.zero);
                    }
                }
            }

            yield return null;
        }


        /*
        tree = BT.Root();
        tree.OpenBranch(
            BT.If(() => unit.Position.GetNext(Vector2.up).IsDiggable).OpenBranch(
                BT.Call(() => unit.SetMove(Vector2.up)),
                BT.While(() => unit.state != Unit.State.Move),
                BT.Call(() => unit.SetMove(Vector2.zero))
            )
        );
         */
    }

    private void Update()
    {
        // tree.Tick();
    }
}
