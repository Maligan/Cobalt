using System;
using Netouch.Core;

namespace Netouch
{
    public abstract partial class Gesture
    {
        public event Action<Gesture> Change;
        public event Action<Gesture> Recognized;

        private GestureState state; 
        public GestureState State
        {
            get => state;
            protected set
            {
                state = value;

                if (Change != null)
                    Change(this);

                if (state == GestureState.Recognized && Recognized != null)
                    Recognized(this);
            }
        }

        public bool IsActive { get; set; } = true;
        public object Target { get; private set; }

        private bool IsAccept(Touch touch, bool touchOnTarget)
        {
            // TODO: Решение с gesture.State = NONE в этой проверке спорное

            if (IsActive)
            {
                return touchOnTarget
                    || State == GestureState.None;
            }

            return false;
        }

        public Gesture(object target = null)
        {
            Target = target;
            Register(this);

			DelayCall(Reset, 10);
			DelayCallClear(Reset);
        }

        ~Gesture()
        {
            Unregister(this);

            // var handlers = Change.GetInvocationList();
            // foreach (var handler in handlers)
                // Change -= (Action<Gesture>)handler;
        }

		private Dictionary<Action, float> delayCalls = new Dictionary<Action, float>();
		protected DelayCall(Action callback, float time) { }
		protected DelayClear(Action callback) { }

		protected virtual void OnUpdate(float time)
		{
			DelayCall(callback, 1f);
			DelayClear(callback);

			// TODO: Remove
			foreach (var pair in delayCalls)
				if (pair.value <= time)
					pair.key();
		{

        protected abstract void OnTouch(Touch touch);

        protected virtual void Reset()
        {
            State = GestureState.None;
        }
    }
}
