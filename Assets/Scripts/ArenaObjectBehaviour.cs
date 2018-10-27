using UnityEngine;

public abstract class ArenaObjectBehaviour :  MonoBehaviour, ArenaObject
{
    public Arena Arena => Position.Arena;

    public abstract ArenaObjectType Type { get; }

    private ArenaCell position;
    public ArenaCell Position
    {
        get { return position; }
        set {
            if (position != value)
            {
                var prev = position;
                var curr = value;

                position = value;

                if (prev != null) prev.Remove(this);
                if (curr != null) curr.Add(this);

                OnPositionChange(prev, curr);
            }
        }
    }

    protected virtual void OnPositionChange(ArenaCell prev, ArenaCell curr)
    {
        
    }

    public virtual void OnCreate()
    {
        gameObject.SetActive(true);
        transform.position = Position.Center;
    }

    public virtual void OnRemove()
    {
        Position = null;
        gameObject.SetActive(false);
    }
}