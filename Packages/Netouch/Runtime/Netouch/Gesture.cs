using System;
using System.Collections.Generic;
using Netouch.Core;

namespace Netouch
{
    public abstract partial class Gesture
    {
        public event Action<Gesture> Change;
        public event Action<Gesture> Recognized;

        public bool IsActive { get; set; } = true;
        public object Target { get; private set; }

        private GestureState state;
        private GestureState stateOnUnsuppress;
        private List<Gesture> stateSuppressers = new List<Gesture>();
        
        public GestureState State
        {
            get => state;
            protected set
            {
                if (state != value)
                {
                    var valueIsSuppressable = value == GestureState.Began || value == GestureState.Recognized;
                    if (valueIsSuppressable && HasSuppress())
                    {
                        stateOnUnsuppress = value;
                    }
                    else
                    {
                        state = value;
                        stateOnUnsuppress = value;

                        if (Change != null)
                            Change(this);

                        if (state == GestureState.Recognized && Recognized != null)
                            Recognized(this);
                    }
                }
            }
        }

        public Gesture(object target = null)
        {
            Target = target;
            Register(this);
        }

        public void Require(Gesture toFail)
        {
            stateSuppressers.Add(toFail);
            toFail.Change += OnSuppresserChange;
        }

        private void OnSuppresserChange(Gesture toFail)
        {
            if (state != stateOnUnsuppress && !HasSuppress())
                State = stateOnUnsuppress;
        }

        private bool HasSuppress()
        {
            foreach (var gesture in stateSuppressers)
                if (gesture.state != GestureState.None && gesture.state != GestureState.Failed)
                    return true;

            return false;
        }

        protected abstract void OnTouch(Touch touch);

        protected virtual bool CanPrevent(Gesture other) => true;

        protected virtual void Reset()
        {
            State = GestureState.None;
        }
    }
}
