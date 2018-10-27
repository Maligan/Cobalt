using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Arena
{
    private Dictionary<int, ArenaCell> cellsHash;
    private List<ArenaCell> cellsList;

    public Arena(List<GameObject> prefabs, Transform parent)
    {
        cellsHash = new Dictionary<int, ArenaCell>();
        cellsList = new List<ArenaCell>();

        Pool = new ArenaFactory(prefabs, parent);
    }

    public void Clear()
    {
        // Recycle
        Objects.ToList().ForEach(Pool.Recycle);

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
    
    public IEnumerable<ArenaObjectBehaviour> Objects
    {
        get
        {
            for (var i = 0; i < cellsList.Count; i++)
                for (var j = 0; j < cellsList[i].Objects.Count; j++)
                    yield return (ArenaObjectBehaviour)cellsList[i].Objects[j];
        }
    }

    public ArenaFactory Pool { get; private set; }
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
    public bool IsPlaceable { get { return Objects.All(X => X.Type != ArenaObjectType.Bomb); } }

    public ArenaCell GetNext(Vector2 direction) { return Arena.GetCell(X + (int)direction.x, Y + (int)direction.y); }
    public IEnumerable<ArenaCell> GetRadius(float radius)
    {
        var fromX = Mathf.FloorToInt(X - radius);
        var fromY = Mathf.FloorToInt(Y - radius);
        var toX = Mathf.CeilToInt(X + radius);
        var toY = Mathf.CeilToInt(Y + radius);

        radius = radius*2;

        for (var x = fromX; x <= toX; x++)
        {
            for (var y = fromY; y <= toY; y++)
            {
                var dx = X - x;
                var dy = Y - y;
                var d2 = dx*dx + dy*dy;

                if (d2 <= radius)
                    yield return Arena.GetCell(x, y);
            }
        }
    }
}

public interface ArenaObject
{
    Arena Arena { get; }
    ArenaCell Position { get; set; }
    ArenaObjectType Type { get; }

    void OnCreate();
    void OnRemove();
}

public enum ArenaObjectType
{
    Unit,
    Wall,
    Exit,
    Bomb
}

public class ArenaFactory : UnityEngine.Object
{
    private Transform parent;
    private List<GameObject> prefabs;
    private Dictionary<ArenaObjectType, ArenaObjectList> objects;

    public ArenaFactory(List<GameObject> prefabs, Transform parent)
    {
        this.prefabs = prefabs;
        this.parent = parent;
        this.objects = new Dictionary<ArenaObjectType, ArenaObjectList>();
    }

    public Exit CreateExit(ArenaCell pos) { return Create<Exit>(pos, ArenaObjectType.Exit); }
    public Wall CreateWall(ArenaCell pos) { return Create<Wall>(pos, ArenaObjectType.Wall); }
    public Unit CreateUnit(ArenaCell pos) { return Create<Unit>(pos, ArenaObjectType.Unit); }
    public Bomb CreateBomb(ArenaCell pos) { return Create<Bomb>(pos, ArenaObjectType.Bomb); }

    private T Create<T>(ArenaCell pos, ArenaObjectType type) where T : ArenaObject
    {
        var obj = GetFromPoolOrInstantiate<T>(type);
        obj.Position = pos;
        obj.OnCreate();
        return obj;        
    }

    private T GetFromPoolOrInstantiate<T>(ArenaObjectType type)
    {
        var list = objects.ContainsKey(type) ? objects[type] : null;
        if (list == null)
            list = objects[type] = new ArenaObjectList();
        
        if (list.Count > 0)
        {
            var obj = list[list.Count-1];
            list.RemoveAt(list.Count-1);
            return (T)obj;
        }

        var prefab = prefabs.FirstOrDefault(x => x.GetComponent<T>() != null);
        if (prefab == null)
            throw new Exception("Prefab for '" + type + "' doesn't added");

        return Instantiate(prefab, parent).GetComponent<T>();
    }

    public void Recycle(ArenaObject obj)
    {
        obj.OnRemove();
        objects[obj.Type].Add(obj);
    }

    // For shorcut without excess <> symbols
    private class ArenaObjectList : List<ArenaObject> { }
}