using UnityEngine;
using Cobalt.Ecs;

namespace Cobalt.Unity
{
    public class TransformInterpolator : MonoBehaviour
    {
        public MatchTimeline Timeline { get; set; }
        public int UnitIndex;

        private void Update()
        {
            if (Timeline == null || Timeline.IsEmpty) return;

            transform.localPosition = Timeline.GetPosition(UnitIndex);
        }

        private void OnDrawGizmos()
        {
            // if (Timeline != null)
            // {
            //     for (int i = 0; i < Timeline.Count; i++)
            //     {
            //         var state = GetTransform(i);
            //         var localPos = new Vector2(state.x, state.y);
            //         var globalPos = transform.parent.TransformPoint(localPos);
            //         Gizmos.color = Color.magenta;
            //         Gizmos.DrawCube(globalPos, Vector3.one * 1/10f);
            //     }
            // }
        }
    }
}