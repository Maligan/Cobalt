using System;
using System.Collections.Generic;

namespace Netouch
{
    public partial class Gesture
    {
		/// Time (in seconds)
		public static float Time { get; private set; }

        private static List<DelayedAction> queue = new List<DelayedAction>();
            
        private static void OnProcessFrame(float time)
        {
            if (time < Time)
                throw new ArgumentException($"Time value ({time}) must be greater than current time ({Time})");

            Time = time;

            while (queue.Count > 0)
            {
                var first = queue[0];
                if (first.Time > Time) break;
                queue.RemoveAt(0);
                first.Action();
            }
        }

        protected static void DelayCall(Action action, float delaySec)
        {
            DelayClear(action);
            queue.Add(new DelayedAction(action, Time + delaySec));
            queue.Sort();
        }

        protected static void DelayClear(Action action)
        {
            queue.RemoveAll(x => x.Action == action);
        }

        private struct DelayedAction : IComparable<DelayedAction>
        {
            public Action Action { get; private set; }
            public float Time { get; private set; }

            public DelayedAction(Action action, float time)
            {
                Action = action;
                Time = time;
            }

            int IComparable<DelayedAction>.CompareTo(DelayedAction other)
            {
                if (Time > other.Time) return +1;
                if (Time < other.Time) return -1;
                return 0;
            }
        }
    }
}