using System;
using System.Collections;
using System.Collections.Generic;
using Netouch.Core;

namespace Netouch
{
    public partial class Gesture
    {
        /// Screen DPI
        public static int Dpi { get; set; } = 120;
        /// Threshold for touch movement (based on 20 pixels on a 252ppi device.)
        public static int Slop => (int)(20 * Dpi/252f + 0.5f);

        #region Gesture Register

        private static List<Gesture> gestures = new List<Gesture>();
        private static List<Gesture> gesturesForAdd = new List<Gesture>();
        private static List<Gesture> gesturesForRemove = new List<Gesture>();

        private static void Register(Gesture gesture)
        {
            if (gesturesForRemove.Contains(gesture)) gesturesForRemove.Remove(gesture);
            if (gesturesForAdd.Contains(gesture)) return;
            if (gestures.Contains(gesture)) return;
            
            if (gesture.Target != null)
            {
                var hasHitTester = false;
                
                for (var i = 0; i < hitTesters.Count && !hasHitTester; i++)
                    hasHitTester = hitTesters[i].CanTest(gesture.Target);

                if (hasHitTester == false)
                    throw new ArgumentException($"Can't find any '{gesture.Target.GetType().Name}' hit tester. Did you forget call Gesture.Add() with suitable IHitTester?");
            }

            gesturesForAdd.Add(gesture);
        }

        private static void Unregister(Gesture gesture)
        {
            if (gesturesForAdd.Contains(gesture)) gesturesForAdd.Remove(gesture);
            if (gesturesForRemove.Contains(gesture)) return;
            if (gestures.Contains(gesture) == false) return;

            gesturesForRemove.Add(gesture);
        }

        private static void Commit()
        {
            // Remove
            while (gesturesForRemove.Count > 0)
            {
                var gesture = gesturesForRemove[0];
                gesturesForRemove.RemoveAt(0);
                gestures.Remove(gesture);
                gesture.Change -= OnAnyGestureChange;
            }

            // Add
            while (gesturesForAdd.Count > 0)
            {
                var gesture = gesturesForAdd[0];
                gesturesForAdd.RemoveAt(0);
                gestures.Add(gesture);
                gesture.Change += OnAnyGestureChange;
            }

            // Reset (TODO: What about reseting farame?)
            foreach (var gesture in gestures)
            {
                var isCompleted = gesture.State == GestureState.Recognized
                               || gesture.State == GestureState.Failed
                               || gesture.State == GestureState.Ended;

                if (isCompleted)
                    gesture.Reset();
            }
        }

        private static IEnumerable<Gesture> GetGesturesFor(IEnumerable targets)
        {
            // TODO: Решение с gesture.State != IDLE в этой проверке спорное
            foreach (var target in targets)
                foreach (var gesture in gestures)
                    if (gesture.IsActive)
                        if (gesture.Target == target || gesture.State != GestureState.Idle)
                            yield return gesture;
        }

        #endregion

        #region Adapters

        private static List<IHitTester> hitTesters = new List<IHitTester>();
        private static List<ITouchInput> touchInputs = new List<ITouchInput>();

        public static void Add(IHitTester hitTester)
        {
            if (hitTester == null)
                throw new ArgumentNullException(nameof(hitTester));
            
            if (hitTesters.Contains(hitTester))
                return;

            hitTesters.Add(hitTester);
        }
        
        public static void Add(ITouchInput input) 
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (touchInputs.Contains(input))
                return;

            touchInputs.Add(input);
            input.Touch += OnInputTouch;
        }

        #endregion

        private static void OnInputTouch(Touch touch)
        {
            Commit();

            var hitObjects = HitTest(touch);
            var hitObjectsGestures = GetGesturesFor(hitObjects);

            foreach (var gesture in hitObjectsGestures)
                gesture.OnTouch(touch);
        }

        private static IEnumerable HitTest(Touch touch)
        {
            foreach (var hitTester in hitTesters)
            {
                var hitTest = hitTester.HitTest(touch.X, touch.Y);
                if (hitTest != null)
                {
                    var targets = hitTester.GetHierarhy(hitTest);
                    foreach (var target in targets)
                        yield return target;
                }
            }

            // We allways hit to Root as "null" target at last
            yield return null;
        }

        private static void OnAnyGestureChange(Gesture gesture)
        {
            // TODO: Добавить прерывание всех родительских гестур

            if (gesture.State == GestureState.Recognized)
            {
                foreach (var g in gestures)
                    if (g != gesture && g.Target == gesture.Target)
                        if (g.State == GestureState.Possible || g.State == GestureState.Began || g.state == GestureState.Changed)
                            g.State = GestureState.Failed;
            }
        }
    }
}
