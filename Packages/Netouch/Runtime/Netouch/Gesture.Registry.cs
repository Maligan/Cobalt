using System;
using System.Collections.Generic;
using Netouch.Core;

namespace Netouch
{
    public partial class Gesture
    {
        private static List<Gesture> gestures = new List<Gesture>();
        private static List<Gesture> gesturesForAdd = new List<Gesture>();
        private static List<Gesture> gesturesForRemove = new List<Gesture>();

        private static Multiset<object, Gesture> gesturesByTarget = new Multiset<object, Gesture>();
        private static Multiset<Touch, Gesture> gesturesByTouch = new Multiset<Touch, Gesture>();

        private static void Register(Gesture gesture)
        {
            if (gesturesForRemove.Contains(gesture)) gesturesForRemove.Remove(gesture);
            if (gesturesForAdd.Contains(gesture)) return;
            if (gestures.Contains(gesture)) return;
            
            if (gesture.Target != null)
            {
                var hasHitTester = false;
                
                for (var i = 0; i < hitTesters.Values.Count && !hasHitTester; i++)
                    hasHitTester = hitTesters.Values[i].CanTest(gesture.Target);

                if (hasHitTester == false)
                    throw new ArgumentException($"Can't find any '{gesture.Target.GetType().Name}' hit tester. Did you add suitable IHitTester with Gesture.Add() before?");
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
                gesture.Change -= OnGestureChange;
                gesturesByTarget.Remove(gesture.Target, gesture);
            }

            // Add
            while (gesturesForAdd.Count > 0)
            {
                var gesture = gesturesForAdd[0];
                gesturesForAdd.RemoveAt(0);
                gestures.Add(gesture);
                gesture.Change += OnGestureChange;
                gesturesByTarget.Add(gesture.Target, gesture);
            }

            // Reset (TODO: What about reseting frame?)
            foreach (var gesture in gestures)
            {
                var needReset = gesture.State == GestureState.Accepted
                             || gesture.State == GestureState.Failed
                             || gesture.State == GestureState.Ended;

                if (needReset)
                    gesture.Reset();
            }
        }

        private class Multiset<TKey, TValue>
        {
            private MultisetList _listForNull = new MultisetList();
            private Dictionary<TKey, MultisetList> _lists = new Dictionary<TKey, MultisetList>();

            public void Add(TKey key, TValue value)
            {
                var list = GetList(key);
                if (list.Contains(value) == false)                
                    list.Add(value);
            }

            public void Remove(TKey key, TValue value)
            {
                GetList(key).Remove(value);
            }

            public List<TValue> GetList(TKey key)
            {
                if (key == null)
                    return _listForNull;

                if (_lists.TryGetValue(key, out MultisetList list) == false)
                    _lists.Add(key, list = new MultisetList());
                
                return list;
            }

            private class MultisetList : List<TValue>
            {
            }
        }
    }
}