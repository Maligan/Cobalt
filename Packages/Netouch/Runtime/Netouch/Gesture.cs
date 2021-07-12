using System;
using System.Collections.Generic;
using Netouch.Core;

namespace Netouch
{
    public abstract partial class Gesture
    {
        private event Action<Gesture> Change;

        public bool IsActive { get; set; } = true;
        public object Target { get; private set; }

        private GestureState state;
        private GestureState stateOnUnsuppress;
        private List<Gesture> suppress = new List<Gesture>();
        
        public GestureState State
        {
            get => state;
            protected set
            {
                if (state != value)
                {
                    var valueIsSuppressable = value == GestureState.Began || value == GestureState.Accepted;
                    if (valueIsSuppressable && HasSuppress())
                    {
                        stateOnUnsuppress = value;
                    }
                    else
                    {
                        state = value;
                        stateOnUnsuppress = value;

                        Invoke(state);
                        Change?.Invoke(this);
                    }
                }
            }
        }

        public Gesture(object target = null)
        {
            Target = target;
            Register(this);
        }
        
        /// Reset internal properties after on rollback gesture state
        protected virtual void Reset() => State = GestureState.Possible;

        /// Basic touch processing
        protected abstract void OnTouch(Touch touch);

        /// Gesture should prevent another one on success
        protected virtual bool IsPrevent(Gesture other) => true;

        /// Gestrue should suppress another to get Began/Accepted state
        protected virtual bool IsSuppress(Gesture other) => state != GestureState.Failed;

        //
        // Suppressing
        //

        public void Require(Gesture toFail)
        {
            suppress.Add(toFail);
            toFail.Change += OnSuppresserChange;
        }

        private void OnSuppresserChange(Gesture toFail)
        {
            if (state != stateOnUnsuppress && !HasSuppress())
                State = stateOnUnsuppress;
        }

        protected bool HasSuppress()
        {
            foreach (var gesture in suppress)
                if (gesture.IsSuppress(this))
                    return true;

            return false;
        }

        //
        // Events (On, Once, Off)
        //

        private List<Call> calls = new List<Call>();
        private int callCounter;

        internal void Add(GestureState state, object key, Action handler, bool once)
        {
            Remove(state, key);
            calls.Add(new Call(state, handler, key, once));
        }

        internal void Remove(GestureState state, object key)
        {
            var indexOf = calls.FindIndex(x => x.Key.Equals(key) && !x.Removed);
            if (indexOf != -1)
                calls[indexOf].Remove();
        }

        private void Invoke(GestureState state)
        {
            callCounter++;

            try
            {
                var length = calls.Count;

                for (var i = 0; i < length; i++)
                {
                    var call = calls[i];
                    if (call.State == state)
                        call.Invoke();
                }
            }
            finally
            {
                callCounter--;

                if (callCounter == 0)
                    calls.RemoveAll(x => x.Removed);
            }
        }

        private class Call
        {
            public GestureState State { get; private set; }
            public object Key { get; private set; }
            public Action Handler { get; private set; }
            public bool Removed { get; private set; }
            public bool Once { get; private set; }

            public Call(GestureState state, Action handler, object key, bool once)
            {
                State = state;
                Key = key;
                Handler = handler;
                Once = once;
            }

            public void Invoke()
            {
                if (Removed) return;
                if (Once) Removed = true;
                if (Handler != null) Handler.Invoke();
            }

            public void Remove()
            {
                Removed = true;
            }
        }
    }

    // Implement as extension for strong typed handler argument
    public static class GestureExtensionEvent
    {
        public static T On<T>(this T gesture, GestureState state, Action<T> handler) where T : Gesture
        {
            gesture.Add(state, handler, () => handler(gesture), false);
            return gesture;
        }

        public static T Once<T>(this T gesture, GestureState state, Action<T> handler) where T : Gesture
        {
            gesture.Add(state, handler, () => handler(gesture), true);
            return gesture;
        }

        public static T Off<T>(this T gesture, GestureState state, Action<T> handler) where T : Gesture
        {
            gesture.Remove(state, handler);
            return gesture;
        }
    }
}