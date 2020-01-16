using GestureKit.Input;

namespace GestureKit
{
    public class LongPressGesture : Gesture
    {
        public float Delay { get; set; } = 0.5f;

        public LongPressGesture(object target) : base(target) { }

        protected override void OnTouch(Touch touch)
        {
            if (State == GestureState.Idle && touch.Phase == TouchPhase.Began)
                State = GestureState.Possible;

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Moved)
            {
                var dx = touch.X - touch.BeginX;
                var dy = touch.Y - touch.BeginY;
                var sqrDistance = (float)System.Math.Sqrt(dx*dx + dy*dy);
                if (sqrDistance > Slop*Slop)
                    State = GestureState.Failed;
            }

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Ended)
                State = GestureState.Failed;

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Canceled)
                State = GestureState.Failed;

            if (State == GestureState.Possible)
                if (touch.Time - touch.BeginTime > Delay)
                    State = GestureState.Recognized;
        }        
    }
}