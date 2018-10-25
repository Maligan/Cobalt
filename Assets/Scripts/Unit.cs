
using System.Collections;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour, ArenaObject
{
    private enum State { Idle, Move, Dig } 

    private State state;
    private Vector2 move;

    private Vector2 currMove;
    private ArenaCell currMoveFrom;
    private ArenaCell currMoveTo;
    private float currMoveProgress;

    private Vector2 currDig;
    private Wall currDigObj;

    public Arena Arena { get; set; }
    public ArenaCell Position { get; set; }
    public ArenaObjectType Type => ArenaObjectType.Unit;

    private void Update()
    {
        UpdateInput();

        switch (state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Move: UpdateMove(); break;
            case State.Dig:  UpdateDig();  break;
        }
    }

    private void UpdateInput()
    {
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        if (h > 0) move = Vector2.right;
        else if (h < 0) move = Vector2.left;
        else if (v > 0) move = Vector2.up;
        else if (v < 0) move = Vector2.down;
        else move = Vector2.zero;
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
            currDigObj = cell.Objects.First(x => x.Type == ArenaObjectType.Wall) as Wall;
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
            currDigObj.Dig(this);

            if (currDigObj.IsRemoved)
            {
                state = State.Idle;
                currDigObj = null;
            }
        }
        else
        {
            state = State.Idle;
            currDigObj = null;
        }
    }
}