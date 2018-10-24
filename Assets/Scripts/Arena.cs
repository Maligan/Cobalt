using System.Collections.Generic;

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
            cells[key] = new ArenaCell() { X = x, Y = y };

        return cells[key];
    }
}

public class ArenaCell
{
    public int X;
    public int Y;
    public List<ArenaObject> Objects = new List<ArenaObject>();

    public void Add(ArenaObject obj) { Objects.Add(obj); }
    public void Remove(ArenaObject obj) { Objects.Remove(obj); }
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
