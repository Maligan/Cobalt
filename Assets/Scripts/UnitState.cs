using System.Collections.Generic;
using UnityEngine;

public class UnitState : MonoBehaviour
{
    public struct State
    {
        public float timestamp;
        public float x;
        public float y;

        public static implicit operator Vector2(State state)
        {
            return new Vector2(state.x, state.y);
        }
    }

    private List<State> states = new List<State>();

    private float step = 0.3f;
    private float timeout;

    private float time;
    
    
    private float clientTime;
    private float clientTimeBegin;


    public void Update()
    {
        UpdateState();

        time += Time.deltaTime;
        if (time < 2f) return; // Ждём пока буффер наполнится
        if (clientTimeBegin == 0) clientTimeBegin = Time.time;

        var clientTime = Time.time - clientTimeBegin;
        while (clientTime > states[1].timestamp) // 1 or 0 в зависимости сколько нам нужно знать в прошлом (FIXME: A while ли?)
            states.RemoveAt(0);

        var curr = new Vector2(states[0].x, states[0].y);
        var next = new Vector2(states[1].x, states[1].y);
        
        var delta = clientTime - states[0].timestamp;
        var total = states[1].timestamp - states[0].timestamp;
        
        var t = delta / total; // [0; 1)

        transform.localPosition = Vector2.LerpUnclamped(curr, next, t);
    }

    private void OnDrawGizmos()
    {
        var v2 = (Vector2)states[2];
        Gizmos.DrawSphere(v2, 0.3f);
    }

    private void UpdateState()
    {
        timeout -= Time.deltaTime;
        if (timeout > 0) return;
        timeout = step + step*Random.value;

        // Update
        var angle = Random.Range(0, 360);

        var oldPos = states.Count == 0 ? new Vector2(0, 0) : new Vector2(states[states.Count-1].x, states[states.Count-1].y);
        var newPos = oldPos + 3 * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        var state = new State
        {
            timestamp = Time.time,
            x = newPos.x,
            y = newPos.y
        };

        // Push
        states.Add(state);
    }
}