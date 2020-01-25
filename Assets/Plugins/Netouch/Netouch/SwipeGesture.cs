using System;
using Netouch.Core;

namespace Netouch
{
    public class SwipeGesture : Gesture
    {
        public SwipeGestureDirection Direction { get; set; }

        public SwipeGesture(object target = null) : base(target) { }

        protected override void OnTouch(Touch touch)
        {
            if (State == GestureState.None && touch.Phase == TouchPhase.Began)
                State = GestureState.Possible;

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Moved)
            {
                var dx = touch.X-touch.PrevX;
                var dy = touch.Y-touch.PrevY;
                // var sqrDistance = Math.Sqrt(dx*dx + dy*dy);

                var byX = Math.Abs(dx) > Slop>>1;
                var byY = Math.Abs(dy) > Slop>>1;

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

        protected override void Reset()
        {
            Direction = SwipeGestureDirection.None;
            base.Reset();
        }
    }
}