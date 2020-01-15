using GestureKit.Core;

namespace GestureKit
{
    public class TapGesture : Gesture
    {
        public float Slop { get; set; }

        public override void OnTouch(Touch touch)
        {
            if (State == GestureState.Idle && touch.Phase == TouchPhase.Began)
                State = GestureState.Possible;

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Moved)
                State = GestureState.Failed;
            
            if (State == GestureState.Possible && touch.Phase == TouchPhase.Ended)
                State = GestureState.Recognized;
        }
    }
}