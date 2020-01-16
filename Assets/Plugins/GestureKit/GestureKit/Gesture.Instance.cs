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

        public object Target { get; private set; }

        public Gesture(object target = null)
        {
            Register(this);
            Target = target;
        }

        ~Gesture()
        {
            Unregister(this);

            // var handlers = Change.GetInvocationList();
            // foreach (var handler in handlers)
                // Change -= (Action<Gesture>)handler;
        }

        public abstract void OnTouch(Touch touch);

        public virtual void Reset()
        {
            State = GestureState.Idle;
        }
    }
}