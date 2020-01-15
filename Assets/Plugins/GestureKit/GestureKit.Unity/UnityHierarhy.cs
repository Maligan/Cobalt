using System;
using System.Collections;
using GestureKit.Core;
using UnityEngine;

namespace GestureKit.Unity
{
    public class UnityHierarhy : IHierarhy
    {
        public object GetParent(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var gameObject = target as GameObject;
            if (gameObject == null)
                throw new ArgumentException(nameof(target));

            var transform = gameObject.transform;
            if (transform.parent == null)
                return null;
                
            return transform.parent.gameObject;
        }

        public IEnumerable GetChildren(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var gameObject = target as GameObject;
            if (gameObject == null)
                throw new ArgumentException(nameof(target));

            var transform = gameObject.transform;
            for (var i = 0; i < transform.childCount; i++)
                yield return transform.GetChild(i).gameObject;
        }
    }
}