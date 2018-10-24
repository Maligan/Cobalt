
using System.Collections;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour, ArenaObject
{
    private enum State { Idle, Move, Dig } 

    private State state;

    private Vector2 moveNext;
    private Vector2 moveCurrent;
    private float moveProgress;
    private Vector2 moveFrom;
    private Vector2 moveTo;

    private ArenaCell dig;
    private float digProgress;

    public Arena Arena { get; set; }
    public ArenaCell Position { get; set; }
    public ArenaObjectType Type => ArenaObjectType.Unit;

    public void Move(Vector2 direction)
    {
        moveNext = direction;
    }

    private void OnDrawGizmos()
    {
        if (state == State.Move)
            Gizmos.DrawLine(moveFrom, moveTo);
        else if (state == State.Dig)
            Gizmos.DrawSphere(((Wall)dig.Objects[0]).transform.position, 1/2f);
    }

    private void Update()
    {
        if (state == State.Move)
        {   
            // Реверс текущего хода
            if (moveNext != Vector2.zero && moveNext == -moveCurrent)
            {
                var tmp = moveFrom;
                moveFrom = moveTo;
                moveTo = tmp;
                moveProgress = 1 - moveProgress;

                moveCurrent = moveNext;
                moveNext = Vector2.zero;
            }

            // Завершение хода
            if (moveProgress >= 1)
            {
                Position = Arena.GetCell(
                    Position.X + (int)moveCurrent.x,
                    Position.Y + (int)moveCurrent.y
                );

                name = string.Format("Unit ({0};{1})", Position.X, Position.Y);

                if (Position.Objects.Any(x => x.Type == ArenaObjectType.Exit))
                    Debug.Log("WIN!");

                if (moveNext == Vector2.zero)
                {
                    state = State.Idle;
                }
                else
                {

                    var next = Arena.GetCell(
                        Position.X + (int)moveNext.x,
                        Position.Y + (int)moveNext.y
                    );

                    var nextHasWall = next.Objects.Any(x => x.Type == ArenaObjectType.Wall);
                    if (nextHasWall)
                    {
                        state = State.Dig;

                        dig = next;
                        digProgress = 0;          

                        moveNext = Vector2.zero;
                    }
                    else
                    {
                        moveFrom = transform.localPosition;
                        moveTo = moveFrom + moveNext;
                        moveProgress %= 1;

                        moveCurrent = moveNext;
                        moveNext = Vector2.zero;
                    }
                }
            }

            moveProgress += Time.deltaTime*5;
            transform.localPosition = Vector2.Lerp(moveFrom, moveTo, moveProgress);
        }
        else if (state == State.Idle)
        {
            if (moveNext != Vector2.zero)
            {
                var next = Arena.GetCell(
                    Position.X + (int)moveNext.x,
                    Position.Y + (int)moveNext.y
                );

                var nextHasWall = next.Objects.Any(x => x.Type == ArenaObjectType.Wall);
                if (nextHasWall)
                {
                    state = State.Dig;

                    dig = next;
                    digProgress = 0;          

                    moveNext = Vector2.zero;          
                }
                else
                {
                    state = State.Move;

                    moveFrom = transform.localPosition;
                    moveTo = moveFrom + moveNext;
                    moveProgress = 0;

                    moveCurrent = moveNext;
                    moveNext = Vector2.zero;
                }
            }
        }
        else if (state == State.Dig)
        {
            digProgress += Time.deltaTime*5;

            if (digProgress >= 1)
            {
                state = State.Idle;

                var wall = (Wall)dig.Objects[0];
                wall.Position.Remove(wall);
                Destroy(wall.gameObject);
            }
        }
    }
}