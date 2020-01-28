using Netouch.Core;

namespace Netouch
{
    public class TapGesture : Gesture
    {
        public TapGesture(object target = null) : base(target) { }

        public new float Slop { get; set; } = Gesture.Slop << 2; // iOS has 45px for 132 dpi screen

        public int NumTapsRequired { get; set; } = 1;
        // public int NumTouchesRequired { get; set; } = 1;

        public float MaxTapDelay { get; set; } = 0.3f;
        public float MaxTapDuration { get; set; } = 1.5f;
        // public int MaxTapDistance { get; set; } = Gesture.Slop << 2;

        private int numTaps;
        // private int numTouches;
        // private int numTouchesRequiredReached;

        protected override void OnTouch(Touch touch)
        {
            if (State == GestureState.None && touch.Phase == TouchPhase.Began)
                State = GestureState.Possible;

            if (State == GestureState.Possible && (touch.Time-touch.BeginTime) > MaxTapDuration)
                State = GestureState.Failed;

            if (State == GestureState.Possible && touch.GetLength() > Slop)
                State = GestureState.Failed;

            if (State == GestureState.Possible && touch.Phase == TouchPhase.Ended)
            {
				numTaps++;
                DelayClear(OnTapTimeout);

                if (numTaps < NumTapsRequired)
                    DelayCall(OnTapTimeout, MaxTapDelay);
				else if (numTaps == NumTapsRequired)
                    State = GestureState.Recognized;
            }
        }

        private void OnTapTimeout()
        {
            State = GestureState.Failed;
        }

        protected override bool CanPrevent(Gesture other)
        {
            if (other is TapGesture)
                return ((TapGesture)other).NumTapsRequired <= NumTapsRequired;

            return true;
        }

        protected override void Reset()
        {
            base.Reset();
            DelayClear(OnTapTimeout);
            numTaps = 0;
        }
    }
}
