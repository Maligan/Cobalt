using System;
using System.Collections;
using System.Collections.Generic;
using GestureKit.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GestureKit.Unity
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

        public IEnumerable HitTest(float x, float y)
        {
            raycastData.position = new Vector2(x, y);
            raycastResult.Clear();
            raycaster.Raycast(raycastData, raycastResult);

            // TODO: Sort raycastResult by depth/layers etc.
            //       Maybe GetDepth() doesn't good for 2d/3d world

            var depth = -1;
            foreach (var raycast in raycastResult)
            {
                var raycastDepth = GetDepth(raycast.gameObject);

                if (depth == -1) depth = raycastDepth;
                else if (depth >= raycastDepth) depth = raycastDepth;
                else yield break;

                yield return raycast.gameObject;
            }
        }

        private int GetDepth(GameObject gameObject)
        {
            if (gameObject.transform.parent == null)
                return 0;

            return 1 + GetDepth(gameObject.transform.parent.gameObject);
        }
    }
}