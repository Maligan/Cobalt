
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : ArenaObjectBehaviour
{
    public enum State { Idle, Move, Dig } 

    public State state;
    private Vector2 move;

    private Vector2 currMove;
    private float currMoveProgress;
    private List<ArenaObjectBehaviour> currMovePush;

    private Vector2 currDig;
    private ArenaCell currDigCell;

    public override ArenaObjectType Type => ArenaObjectType.Unit;

    public Unit()
    {
        currMovePush = new List<ArenaObjectBehaviour>();
        currMovePush.Add(this);
    }

    public override void OnRemove()
    {
        base.OnRemove();

        state = State.Idle;

        currMove = Vector2.zero;
        currMoveProgress = 0;

        currDig = Vector2.zero;
        currDigCell = null;
    }

    public void DoBomb()
    {
        if (!Position.IsPushable)
            Arena.Pool.CreateBomb(Position);
        else
            Debug.Log("Cell already is taken");
    }

    public void DoMove(Vector2 move)
    {
        this.move = move;
    }

    public void Damage(int hp)
    {
        Arena.Pool.Recycle(this);
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Move: UpdateMove(); break;
            case State.Dig:  UpdateDig();  break;
        }
    }

    private void UpdateIdle()
    {
        if (move != Vector2.zero)
        {
            TransitToMoveOrDig(0);
            // move = Vector2.zero;
        }
    }

    private void UpdateMove()
    {
        // Reverse (only if do not pushing)
        // if (currMove == -move)
        // {
        //     currMove = move;
        //     currMoveProgress = 1 - currMoveProgress;
            
        //     var tmp = currMoveFrom;
        //     currMoveFrom = currMoveTo;
        //     currMoveTo = tmp;
        // }

        // Locomotion
        if (currMoveProgress < 1)
        {
            currMoveProgress += Time.deltaTime;

            var lerpUnclamped = move == currMove && currMovePush[currMovePush.Count-1].Position.GetNext(currMove).IsWalkable;

            foreach (var obj in currMovePush)
            {
                var curr = obj.Position;
                var next = obj.Position.GetNext(currMove);

                obj.transform.localPosition = lerpUnclamped
                    ? Vector2.LerpUnclamped(curr.Center, next.Center, currMoveProgress)
                    : Vector2.Lerp(curr.Center, next.Center, currMoveProgress);
            }                
        }
        // Continue movement
        else if (move != Vector2.zero)
        {
            DetachPush();
            TransitToMoveOrDig(move == currMove ? currMoveProgress%1 : 0);
        }
        // Stay
        else
        {
            state = State.Idle;

            DetachPush();

            currMove = Vector2.zero;
            currMoveProgress = 0;
        }
    }

    private void DetachPush()
    {
        foreach (var obj in currMovePush)
        {
            var curr = obj.Position;
            var next = obj.Position.GetNext(currMove);
            obj.Position = next;
        }

        currMovePush.RemoveRange(1, currMovePush.Count-1);
    }

    private void TransitToMoveOrDig(float ratio)
    {
        var cell = Position.GetNext(move);

        if (cell.IsDiggable)
        {
            state = State.Dig;
            currDig = move;
            currDigCell = cell;
        }
        else
        {
            while (true)
            {
                if (cell.IsPushable)
                {
                    currMovePush.Add(cell.Objects[0] as ArenaObjectBehaviour);
                    cell = cell.GetNext(move);
                }
                else if (cell.IsWalkable)
                {
                    state = State.Move;
                    currMove = move;
                    currMoveProgress = ratio;
                    break;
                }
                else
                {
                    state = State.Idle;
                    currMove = Vector2.zero;
                    currMoveProgress = 0;
                    currMovePush.RemoveRange(1, currMovePush.Count-1);
                    break;
                }
            }
        }
    }

    private void UpdateDig()
    {
        if (currDig == move)
        {
            var wall = currDigCell.Objects.FirstOrDefault(x => x.Type == ArenaObjectType.Wall) as Wall;

            if (wall != null)
                wall.Dig(this);

            if (wall == null || wall.IsRemoved)
            {
                state = State.Idle;
                currDigCell = null;
            }
        }
        else
        {
            state = State.Idle;
            currDigCell = null;
        }
    }
}