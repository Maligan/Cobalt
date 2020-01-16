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

            foreach (var result in raycastResult)
                yield return result.gameObject;
        }
    }
}