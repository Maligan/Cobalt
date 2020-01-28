using Netouch.Core;

namespace Netouch
{
    public class LongPressGesture : Gesture
    {
        public float Delay { get; set; } = 0.5f;

        public LongPressGesture(object target) : base(target) { }

        protected override void OnTouch(Touch touch)
        {
            if (State == GestureState.None && touch.Phase == TouchPhase.Began)
                State = GestureState.Possible;

            if (State == GestureState.Possible && touch.GetLength() > Slop)
                State = GestureState.Failed;

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