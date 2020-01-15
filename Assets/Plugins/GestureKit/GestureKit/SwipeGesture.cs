using System;
using GestureKit.Core;

namespace GestureKit
{
    public class SwipeGesture : Gesture
    {
        public SwipeGestureDirection Direction { get; set; }

        public override void OnTouch(Touch touch)
        {
            if (State == GestureState.Idle && touch.Phase == TouchPhase.Began)
                State = GestureState.Possible;

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Moved)
            {
                var dx = touch.X-touch.PrevX;
                var dy = touch.Y-touch.PrevY;

                var byX = Math.Abs(dx) > 7;
                var byY = Math.Abs(dy) > 7;

                if (byX || byY)
                {
                    /**/ if (byX) Direction = dx > 0 ? SwipeGestureDirection.Right : SwipeGestureDirection.Left;
                    else if (byY) Direction = dy > 0 ? SwipeGestureDirection.Up : SwipeGestureDirection.Down;

                    State = GestureState.Recognized;
                }
            }

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Ended)
                State = GestureState.Failed;
        }

        public override void Reset()
        {
            Direction = SwipeGestureDirection.None;
            base.Reset();
        }
    }
}