using Netouch.Core;

namespace Netouch
{
    public class TapGesture : Gesture
    {
        public TapGesture(object target = null) : base(target) { }

        public new float Slop { get; set; } = Gesture.Slop << 2; // iOS has 45px for 132 dpi screen

        public int NumTapsRequired { get; set; } = 1;
        // public int NumTouchesRequired { get; set; } = 1;

        public float MaxTapDelay { get; set; } = 0.4f;
        // public int MaxTapDuration { get; set; } = 1500;
        // public int MaxTapDistance { get; set; } = Gesture.Slop << 2;

        private int numTaps;
        // private int numTouches;
        // private int numTouchesRequiredReached;

        protected override void OnTouch(Touch touch)
        {
            if (State == GestureState.None && touch.Phase == TouchPhase.Began)
                State = GestureState.Possible;

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Ended)
            {
                if (++numTaps == NumTapsRequired)
                    State = GestureState.Recognized;
                else
                    DelayCall(OnTapTimeout, MaxTapDelay);
            }

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Moved)
            {
                var dx = touch.X - touch.BeginX;
                var dy = touch.Y - touch.BeginY;
                var sqrDistance = (float)System.Math.Sqrt(dx*dx + dy*dy);
                if (sqrDistance > Slop*Slop)
                    State = GestureState.Failed;
            }
        }

        private void OnTapTimeout()
        {
            State = GestureState.Failed;
        }

        protected override void Reset()
        {
            base.Reset();
            DelayClear(OnTapTimeout);
            numTaps = 0;
        }
    }
}