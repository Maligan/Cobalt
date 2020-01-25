using Netouch.Core;

namespace Netouch
{
    public class TapGesture : Gesture
    {
        public TapGesture(object target = null) : base(target) { }

        public new float Slop { get; set; } = Gesture.Slop << 2; // iOS has 45px for 132 dpi screen

        protected override void OnTouch(Touch touch)
        {
            if (State == GestureState.None && touch.Phase == TouchPhase.Began)
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
                State = GestureState.Recognized;
            
            if (State != GestureState.None && touch.Phase == TouchPhase.Canceled)
                State = GestureState.Failed;
        }
    }
}