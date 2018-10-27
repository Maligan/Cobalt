
using System.Collections;
using System.Linq;
using UnityEngine;

public class Unit : ArenaObjectBehaviour
{
    public enum State { Idle, Move, Dig } 

    public State state;
    private Vector2 move;

    private Vector2 currMove;
    private ArenaCell currMoveFrom;
    private ArenaCell currMoveTo;
    private float currMoveProgress;

    private Vector2 currDig;
    private ArenaCell currDigCell;

    public override ArenaObjectType Type => ArenaObjectType.Unit;

    public override void OnRemove()
    {
        base.OnRemove();

        state = State.Idle;

        currMove = Vector2.zero;
        currMoveFrom = null;
        currMoveTo = null;
        currMoveProgress = 0;

        currDig = Vector2.zero;
        currDigCell = null;
    }

    public void DoBomb()
    {
        if (Position.IsPlaceable)
        {
            Arena.Pool.CreateBomb(Position);
        }
        else
        {
            Debug.Log("Already is taken");
        }
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
        }
    }

    private void UpdateMove()
    {
        // Reverse
        if (currMove == -move)
        {
            currMove = move;
            currMoveProgress = 1 - currMoveProgress;
            
            var tmp = currMoveFrom;
            currMoveFrom = currMoveTo;
            currMoveTo = tmp;
        }

        // Locomotion
        if (currMoveProgress < 1)
        {
            currMoveProgress += Time.deltaTime;

            transform.localPosition = move == currMove && currMoveTo.GetNext(currMove).IsWalkable
                ? Vector2.LerpUnclamped(currMoveFrom.Center, currMoveTo.Center, currMoveProgress)
                : Vector2.Lerp(currMoveFrom.Center, currMoveTo.Center, currMoveProgress);
        }
        // Continue movement
        else if (move != Vector2.zero)
        {
            Position = currMoveTo;
            TransitToMoveOrDig(move == currMove ? currMoveProgress%1 : 0);
        }
        // Stay
        else
        {
            state = State.Idle;

            Position = currMoveTo;

            currMoveFrom = null;
            currMoveTo = null;
            currMove = Vector2.zero;
            currMoveProgress = 0;
        }
    }

    private void TransitToMoveOrDig(float ratio)
    {
        var cell = Arena.GetCell(Position.X + (int)move.x, Position.Y + (int)move.y);

        if (cell.IsDiggable)
        {
            state = State.Dig;

            currDig = move;
            currDigCell = cell;
        }
        else if (cell.IsWalkable)
        {
            state = State.Move;

            currMove = move;
            currMoveFrom = Position;
            currMoveTo = cell;
            currMoveProgress = ratio;
        }
        else
        {
            Debug.Log("Stuck");
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