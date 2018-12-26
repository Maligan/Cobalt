using UnityEngine;
using Cobalt.Shard;
using System.Collections.Generic;

public class TransformInterpolator : MonoBehaviour
{
    public MatchTimeline Timeline { get; set; }

    private void Update()
    {
        if (Timeline.Count < 2) return;

        // Calculate states for interpolation
        var t0 = GetTransform(0);
        var t1 = GetTransform(1);

        // Calculate t [0; 1)
        var total = GetTimestamp(1) - GetTimestamp(0);
        var delta = GetTime()       - GetTimestamp(0);
        var t = delta / total;

        // Interpolate
        transform.position = Vector2.Lerp(
            new Vector2(t0.x, t0.y),
            new Vector2(t1.x, t1.y),
            t
        );
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