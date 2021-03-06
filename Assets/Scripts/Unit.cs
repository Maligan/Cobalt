
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
    [SerializeField] private bool currMoveHalf;
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
        move = Vector2.zero;

        currMove = Vector2.zero;
        currMoveProgress = 0;
        currMoveHalf = false;

        currDig = Vector2.zero;
        currDigCell = null;

        GetComponent<UnitAI>().enabled = false;
        GetComponent<UnitUserInput>().enabled = false;
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

            var moveNext = currMoveHalf == false && currMoveProgress > 1/2f;
            if (moveNext) currMoveHalf = true;

            var lerpUnclamped = move == currMove && false; // currMovePush[currMovePush.Count-1].Position.GetNext(currMove).IsWalkable;

            for (var i = 0; i < currMovePush.Count; i++)
            {
                // If pushed object was removed
                var obj = currMovePush[i];
                if (obj.gameObject.activeSelf == false)
                {
                    currMovePush.RemoveAt(i--);
                    continue;
                }

                if (moveNext)
                    obj.Position = obj.Position.GetNext(currMove);

                var curr = currMoveHalf ? obj.Position.GetNext(-currMove) : obj.Position;
                var next = currMoveHalf ? obj.Position                    : obj.Position.GetNext(currMove);

                obj.transform.localPosition = lerpUnclamped
                    ? Vector2.LerpUnclamped(curr.Center, next.Center, currMoveProgress)
                    : Vector2.Lerp(curr.Center, next.Center, currMoveProgress);
            }                
        }
        // Continue movement
        else if (move != Vector2.zero)
        {
            currMovePush.RemoveRange(1, currMovePush.Count-1);

            TransitToMoveOrDig(move == currMove ? currMoveProgress%1 : 0); // FIXME: in case progress > 1.5 ?!
        }
        // Stay
        else
        {
            currMovePush.RemoveRange(1, currMovePush.Count-1);

            state = State.Idle;
            currMove = Vector2.zero;
            currMoveProgress = 0;
            currMoveHalf = false;
        }
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
                    currMoveHalf = false;
                    break;
                }
                else
                {
                    state = State.Idle;
                    currMove = Vector2.zero;
                    currMoveProgress = 0;
                    currMoveHalf = false;
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