using System;
using System.Collections;
using System.Collections.Generic;
using Netouch.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netouch.Unity
{
    public class UnityRaycasterHitTester : IHitTester
    {
        private BaseRaycaster raycaster;
        private PointerEventData raycastData;
        private List<RaycastResult> raycastResult;

        public UnityRaycasterHitTester(BaseRaycaster raycaster = null, EventSystem eventSystem = null)
        {
            if (eventSystem == null)
                eventSystem = EventSystem.current;

            if (eventSystem == null)
                throw new ArgumentException("Neither eventSystem argument passed nor EventSystem.current exists");

            if (raycaster == null)
            {
                raycaster = UnityEngine.Object.FindObjectOfType<BaseRaycaster>();

                if (raycaster != null)                
                    Debug.LogWarning($"The implicit found {raycaster.GetType().Name} ({raycaster.name}) will be used for HitTest(). Pass raycaster argument explicitly to avoid this warning");
            }

            if (raycaster == null)
                throw new ArgumentException("Neither raycaster argument passed nor raycaster was found with FindObjectOfType<BaseRaycaster>()");

            this.raycaster = raycaster;
            raycastData = new PointerEventData(eventSystem);
            raycastResult = new List<RaycastResult>();
        }

		public bool CanTest(object target)
        {
            return target is GameObject;
        }

        public object HitTest(float x, float y)
        {
            raycastData.position = new Vector2(x, y);
            raycastResult.Clear();
            raycaster.Raycast(raycastData, raycastResult);

            return raycastResult.Count != 0
                 ? raycastResult[0].gameObject
                 : null;
        }

        public IEnumerable GetHierarhy(object target)
        {
            var cursor = (GameObject)target;

            yield return cursor;

            while (cursor.transform.parent != null)
                yield return cursor = cursor.transform.parent.gameObject;
        }
    }
}
