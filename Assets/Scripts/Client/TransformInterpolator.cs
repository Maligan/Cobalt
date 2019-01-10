using UnityEngine;
using Cobalt.Core;
using System.Collections.Generic;
using Cobalt.Core;

public class TransformInterpolator : MonoBehaviour
{
    public MatchTimeline Timeline { get; set; }

    private void Update()
    {
        if (Timeline == null) return;

        //-------------------------------------------------------------------
        // Первая интерполяция - начинает отсчёт времени
        if (Timeline.Time == 0)
        {
            // Выход по опережению (либо по кол-во кадров, либо по задержке)
            // if (Timeline.Length < 0.35f) return;
            if (Timeline.Count < 3) return;

            Timeline.Time = Timeline[0].timestamp;
        }

		// Ускорение по отставанию
		// TODO: ---
		// Нормальный просчёт
		Timeline.Time += Time.deltaTime;
        Timeline.Purge();

        //-------------------------------------------------------------------

        // Calculate states for interpolation
        var t0 = GetTransform(0);
        var t1 = GetTransform(1);

        // Calculate t [0; 1)
        var total = GetTimestamp(1) - GetTimestamp(0);
        var delta = Timeline.Time   - GetTimestamp(0);
        var t = delta / total;

        if (t < 0 || t > 1)
            Debug.LogWarningFormat("[TransformInterpolator] Interpolation t-factor is out of range (0; 1): {0}", t);

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

    private float GetTimestamp(int stateIndex)
    {
        return Timeline[stateIndex].timestamp;
    }

    private Vec2f GetTransform(int stateIndex)
    {
        return Timeline[stateIndex].units[0].pos;
    }
}