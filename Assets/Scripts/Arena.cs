using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Arena
{
    private Dictionary<int, ArenaCell> cellsHash = new Dictionary<int, ArenaCell>();
    private List<ArenaCell> cellsList = new List<ArenaCell>();

    public void Clear()
    {
        cellsHash.Clear();
        cellsList.Clear();
    }

    public ArenaCell GetCell(int x, int y)
    {
        var key = (x & 0xFFFF) | (y & 0xFFFF) << 16;

        if (cellsHash.ContainsKey(key) == false)
        {
            var cell = new ArenaCell() { Arena = this, X = x, Y = y };
            cellsHash.Add(key, cell);
            cellsList.Add(cell);
        }

        return cellsHash[key];
    }
    

    public IEnumerable<ArenaCell> Cells
    {
        get
        {
            return cellsList;
        }
    }
    
    public IEnumerable<ArenaObject> Objects
    {
        get
        {
            for (var i = 0; i < cellsList.Count; i++)
                for (var j = 0; j < cellsList[i].Objects.Count; j++)
                    yield return cellsList[i].Objects[j];
        }
    }
}

public class ArenaCell
{
    public static int Manhattan(ArenaCell c1, ArenaCell c2)
    {
        return Mathf.Abs(c1.X - c2.X)
             + Mathf.Abs(c1.Y - c2.Y);
    }

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
