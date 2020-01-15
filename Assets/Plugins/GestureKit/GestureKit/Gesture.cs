using System;
using GestureKit.Core;

namespace GestureKit
{
    public abstract class Gesture : IDisposable
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

        public Gesture()
        {
            GestureManager.Register(this);
        }

        ~Gesture()
        {
            GestureManager.Unregister(this);
        }

        public abstract void OnTouch(Touch touch);

        public virtual void Reset()
        {
            State = GestureState.Idle;
        }

        public void Dispose()
        {
            GestureManager.Unregister(this);

            var handlers = Change.GetInvocationList();
            foreach (var handler in handlers)
                Change -= (Action<Gesture>)handler;
        }
    }
}