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

        public Type Type => typeof(GameObject);

        public UnityRaycasterHitTester(BaseRaycaster raycaster = null, EventSystem eventSystem = null)
        {
            if (eventSystem == null)
                eventSystem = EventSystem.current;

            if (raycaster == null)
                raycaster = UnityEngine.Object.FindObjectOfType<BaseRaycaster>();

            this.raycaster = raycaster;
            raycastData = new PointerEventData(eventSystem);
            raycastResult = new List<RaycastResult>();
        }

        public object HitTest(float x, float y)
        {
            raycastData.position = new Vector2(x, y);
            raycastResult.Clear();
            raycaster.Raycast(raycastData, raycastResult);

            if (raycastResult.Count == 0)
                return null;
            
            return raycastResult[0].gameObject;
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
