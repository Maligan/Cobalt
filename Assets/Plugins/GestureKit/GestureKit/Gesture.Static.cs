using System;
using System.Collections;
using System.Collections.Generic;
using GestureKit.Input;

namespace GestureKit
{
    public partial class Gesture
    {
        #region Gesture Register

        private static List<Gesture> gestures = new List<Gesture>();

        private static void Register(Gesture gesture)
        {
            if (gestures.Contains(gesture))
                return;
            
            if (gesture.Target != null)
            {
                var hasHitTester = hitTester != null && hitTester.Type.IsInstanceOfType(gesture.Target);
                if (hasHitTester == false)
                    throw new ArgumentException($"HitTester for type '{gesture.Target.GetType().Name}' doesn't added");
            }

            gestures.Add(gesture);
            gesture.Change += OnGestureStateChange;
        }

        private static void Unregister(Gesture gesture)
        {
            if (gestures.Contains(gesture) == false)
                return;

            gestures.Remove(gesture);
            gesture.Change -= OnGestureStateChange;
        }

        private static IEnumerable<Gesture> GetGesturesFor(IEnumerable targets)
        {
            // TODO: Может быть вынести в явном виде null в список целей
            //       Решение с gesture.State != IDLE в этой проверке тоже спорное
            foreach (var target in targets)
                foreach (var gesture in gestures)
                    if (gesture.IsActive)
                        if (gesture.Target == target || gesture.Target == null || gesture.State != GestureState.Idle)
                            yield return gesture;
            }

        #endregion

        #region Adapters

        private static IHitTester hitTester;

        public static void Add(IHitTester hitTester) { Gesture.hitTester = hitTester; }
        public static void Add(ITouchInput input) { input.Touch += OnInputTouch; }

        /// Default DPI for PC screen
        public static int Dpi { get; set; } = 120;
        /// Threshold for touch movement (based on 20 pixels on a 252ppi device.)
        public static int Slop => (int)(20 * Dpi/252f + 0.5f);

        #endregion

        private static void OnInputTouch(Touch touch)
        {
            // TODO: Can be catch gestures on root
            if (hitTester == null)
                return;

            // TODO: Reset only on touch time change (next frame?)
            foreach (var gesture in gestures)
                if (gesture.State == GestureState.Recognized || gesture.State == GestureState.Failed)
                    gesture.Reset();

            var hitTargets = hitTester.HitTest(touch.X, touch.Y);
            var hitGestures = GetGesturesFor(hitTargets);
            foreach (var gesture in hitGestures)
                gesture.OnTouch(touch);
        }

        private static void OnGestureStateChange(Gesture gesture)
        {
            if (gesture.State == GestureState.Recognized)
                OnGestureRecognized(gesture);
        }

        // TODO: Может быть вызывать это нужно ДО Recognized
        // TODO: Непонятно из BEGAN и CHANGED можно ли в Fail Пересылать, или нужно в END
        private static void OnGestureRecognized(Gesture gesture)
        {
            foreach (var g in gestures)
                if (g != gesture && g.Target == gesture.Target)
                    if (g.State == GestureState.Possible || g.State == GestureState.Began || g.state == GestureState.Changed)
                        g.State = GestureState.Failed;
        }
    }
}