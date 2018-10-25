using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Arena
{
    private Dictionary<int, ArenaCell> cells = new Dictionary<int, ArenaCell>();

    public void Clear()
    {
        cells.Clear();
    }

    public ArenaCell GetCell(int x, int y)
    {
        var key = (x & 0xFFFF) | (y & 0xFFFF) << 16;

        if (cells.ContainsKey(key) == false)
            cells[key] = new ArenaCell() { Arena = this, X = x, Y = y };

        return cells[key];
    }
}

public class ArenaCell
{
    public Arena Arena;
    public int X;
    public int Y;
    public List<ArenaObject> Objects = new List<ArenaObject>();

    public void Add(ArenaObject obj) { Objects.Add(obj); }
    public void Remove(ArenaObject obj) { Objects.Remove(obj); }

    public Vector2 Center { get { return new Vector2(X, Y); } }
    
    public bool IsWalkable { get { return !IsDiggable; } }
    public bool IsDiggable { get { return Objects.Any(x => x.Type == ArenaObjectType.Wall); } }

    public ArenaCell GetNext(Vector2 direction) { return Arena.GetCell(X + (int)direction.x, Y + (int)direction.y); }
}

public interface ArenaObject
{
    Arena Arena { get; }
    ArenaCell Position { get; }
    ArenaObjectType Type { get; }
}

public enum ArenaObjectType
{
    Unit,
    Wall,
    Exit
}
