using UnityEngine;

public class Exit : MonoBehaviour, ArenaObject
{
    public Arena Arena { get; set; }
    public ArenaCell Position { get; set; }
    public ArenaObjectType Type => ArenaObjectType.Exit;
}
