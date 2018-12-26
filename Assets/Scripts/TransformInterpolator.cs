using UnityEngine;
using Cobalt.Core;
using System.Collections.Generic;

public class TransformInterpolator : MonoBehaviour
{
    public MatchTimeline Timeline { get; set; }

    private void Update()
    {
        if (Timeline == null) return;

        //-------------------------------------------------------------------
		// Выход по опережению
		if (Timeline.Count < 3) return;
        if (Timeline.Time == 0) Timeline.Time = Timeline[0].timestamp;
		// Ускорение по отставани
		// TODO: ---
		// Нормальный просчёт
		Timeline.Time += Time.deltaTime;
		while (Timeline.Count > 2 && Timeline.Time > Timeline[1].timestamp)
            Timeline.States.RemoveAt(0);
        //-------------------------------------------------------------------

        if (Timeline.Count < 2) return;

        // Calculate states for interpolation
        var t0 = GetTransform(0);
        var t1 = GetTransform(1);

        // Calculate t [0; 1)
        var total = GetTimestamp(1) - GetTimestamp(0);
        var delta = GetTime()       - GetTimestamp(0);
        var t = delta / total;

        // Interpolate
        transform.position = Vector2.LerpUnclamped(
            new Vector2(t0.x, t0.y),
            new Vector2(t1.x, t1.y),
            t
        );
    }

    private void OnDrawGizmos()
    {
        if (Timeline != null)
        {
            for (int i = 0; i < Timeline.Count; i++)
            {
                var state = GetTransform(i);
                var vector = new Vector2(state.x, state.y);
        		Gizmos.color = Color.magenta;
                Gizmos.DrawCube(vector, Vector3.one * 1/10f);
            }
        }
    }

    private float GetTime()
    {
        return Timeline.Time;
    }

    private float GetTimestamp(int stateIndex)
    {
        return Timeline[stateIndex].timestamp;
    }

    private ITransform GetTransform(int stateIndex)
    {
        return Timeline[stateIndex].units[0];
    }
}