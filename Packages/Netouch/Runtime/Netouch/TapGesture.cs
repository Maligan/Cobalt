using Netouch.Core;

namespace Netouch
{
    public class TapGesture : Gesture
    {
        public TapGesture(object target = null) : base(target) { }

        public new float Slop { get; set; } = Gesture.Slop << 2; // iOS has 45px for 132 dpi screen

        public int NumTapsRequired { get; set; } = 1;
        public int NumTouchesRequired { get; set; } = 1;

        public float MaxTapDelay { get; set; } = 0.2f;
        public float MaxTapDuration { get; set; } = 1.5f;
        // public int MaxTapDistance { get; set; } = Gesture.Slop << 2;

        private int numTaps;
        private int numTouches;
        private bool numTouchesRequiredReached;

        protected override void OnTouch(Touch touch)
        {
            if (touch.Phase == TouchPhase.Began)
            {
                numTouches++;

                if (numTouches == NumTouchesRequired)
                    numTouchesRequiredReached = true;
            }

            if (touch.Phase == TouchPhase.Ended)
            {
                if (numTouchesRequiredReached == false)
                {
                    State = GestureState.Failed;
                }
                else
                {
                    numTouches--;

                    if (numTouches == 0)
                    {
                        numTaps++;

                        if (numTaps == 2)
                        {
                            var a = 2;
                        }

                        if (numTaps == NumTapsRequired)
                            State = GestureState.Accepted;

                        if (numTaps < NumTapsRequired)
                            DelayCall(OnTapTimeout, MaxTapDelay);
                    }
                }
            }
        }

        private void OnTapTimeout()
        {
            if (State == GestureState.Possible)
                State = GestureState.Failed;
        }

        protected override bool IsPrevent(Gesture other)
        {
            switch (other)
            {
                case TapGesture tap:
                    return tap.NumTapsRequired <= NumTapsRequired;

                default:
                    return true;
            }
        }

        protected override void Reset()
        {
            base.Reset();
            DelayClear(OnTapTimeout);

            numTaps = 0;
            numTouches = 0;
            numTouchesRequiredReached = false;
        }

        public override string ToString()
        {
            return GetType().Name + "#" + NumTapsRequired + " (" + HasSuppress() + ")";
        }
    }
}
