using UnityEngine;
using Cobalt.Ecs;

namespace Cobalt.Unity
{
    public class MatchUnitSync : MonoBehaviour
    {
        public MatchTimeline Timeline { get; set; }
        public Color[] Colors;
        public int UnitIndex;

        private void Update()
        {
            if (!Timeline.IsStarted) return;

            GetComponent<SpriteRenderer>().color = Colors[UnitIndex];
            transform.localPosition = Timeline.GetPosition(UnitIndex);
        }

        private void OnDrawGizmos()
        {
            if (!Timeline.IsStarted) return;

            var poses = Timeline.GetPositions(UnitIndex);

            for (int i = 0; i < poses.Length; i++)
            {
                var position = transform.parent.TransformPoint(poses[i]);

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(position, 1/10f);
            }
        }
    }
}