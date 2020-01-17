using System;
using GestureKit.Input;

namespace GestureKit
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

        public Gesture(object target = null)
        {
            Target = target;
            Register(this);
        }

        ~Gesture()
        {
            Unregister(this);

            // var handlers = Change.GetInvocationList();
            // foreach (var handler in handlers)
                // Change -= (Action<Gesture>)handler;
        }

        protected abstract void OnTouch(Touch touch);

        protected virtual void Reset()
        {
            State = GestureState.Idle;
        }
    }
}