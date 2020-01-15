using System.Collections.Generic;
using GestureKit;

namespace GestureKit.Core
{
    public static class GestureManager
    {
        private static List<Gesture> gestures = new List<Gesture>();

        public static void Register(Gesture gesture)
        {
            if (gestures.Contains(gesture))
                return;

            gestures.Add(gesture);
            gesture.Change += OnGestureStateChange;
        }

        public static void Unregister(Gesture gesture)
        {
            if (gestures.Contains(gesture) == false)
                return;

            gestures.Remove(gesture);
            gesture.Change -= OnGestureStateChange;
        }






        public static void AddHierarhy(IHierarhy hierarhy) { }
        public static void AddHitTester(IHitTester hitTester) { }
        public static void AddTouchInput(ITouchInput input) { input.Touch += OnTouch; }





        private static void OnTouch(Touch touch)
        {
            // TODO: Reset only on touch time change (next frame?)
            foreach (var gesture in gestures)
                if (gesture.State == GestureState.Recognized || gesture.State == GestureState.Failed)
                    gesture.Reset();

            // TODO: Sorting by depth (deeper has more priority) & recently added
            foreach (var gesture in gestures)
                gesture.OnTouch(touch);
        }

        private static void OnGestureStateChange(Gesture gesture)
        {
            if (gesture.State == GestureState.Recognized)
                OnGestureRecognized(gesture);
        }

        private static void OnGestureRecognized(Gesture gesture)
        {
            // Break other
        }
    }
}