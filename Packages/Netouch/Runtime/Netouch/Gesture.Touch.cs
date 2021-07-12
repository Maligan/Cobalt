using System;
using System.Collections;
using System.Collections.Generic;
using Netouch.Core;

namespace Netouch
{
    public partial class Gesture
    {
        private static void OnProcessTouch(Touch touch)
        {
            Commit();

            var gestures = gesturesByTouch.GetList(touch);

            // Refill list for this touch
            if (touch.Phase == TouchPhase.Began)
            {
                gestures.Clear();

                foreach (var target in HitTest(touch))
                    foreach (var gesture in gesturesByTarget.GetList(target))
                        gestures.Add(gesture);
            }

            // Update OnTouch()
            foreach (var gesture in gestures)
                gesture.OnTouch(touch);
        }

        private static IEnumerable HitTest(Touch touch)
        {
            foreach (var hitTester in hitTesters.Values)
            {
                var hitTest = hitTester.HitTest(touch.X, touch.Y);
                if (hitTest != null)
                {
                    var targets = hitTester.GetHierarhy(hitTest);
                    foreach (var target in targets)
                        yield return target;
                }
            }

            // NB! We allways hit to Root as "null" target at last
            yield return null;
        }

        private static void OnGestureChange(Gesture gesture)
        {
            var canPrevent =
                gesture.State == GestureState.Accepted ||
                gesture.State == GestureState.Began;

            if (canPrevent)
            {
                foreach (var other in GetHierarhyGestures(gesture.Target))
                {
                    var shouldPrevent =
                        other != gesture &&
                        other.State != GestureState.Possible &&
                        gesture.IsPrevent(other);
                    
                    if (shouldPrevent)
                        other.State = GestureState.Failed;
                }
            }
        }

        private static IEnumerable<Gesture> GetHierarhyGestures(object target)
        {
            foreach (var obj in GetHierarhy(target))
                foreach (var gesture in gesturesByTarget.GetList(obj))
                    yield return gesture;
        }

        private static IEnumerable<object> GetHierarhy(object target)
        {
            foreach (var hitTester in hitTesters.Values)
                if (hitTester.CanTest(target))
                    foreach (var obj in hitTester.GetHierarhy(target))
                        yield return obj;

            // NB! Include root as "null" target           
            yield return null;
        }
    }
}
