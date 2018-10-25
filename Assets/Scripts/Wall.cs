using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, ArenaObject
{
	public Material MaterialBox;
	public Material MaterialBlock;

	[HideInInspector]
	public WallType Type;

    public Arena Arena { get; set; }
    public ArenaCell Position { get; set; }
    ArenaObjectType ArenaObject.Type => ArenaObjectType.Wall;

	private Color color;

	public void Dig(ArenaObject actor)
	{
		HP -= (100 * Time.deltaTime);
		HP = Mathf.Max(HP, 0);

		GetComponentInChildren<MeshRenderer>().material.color = color * HP/100f;
		
		if (HP == 0)
		{
			IsRemoved = true;
			Destroy(gameObject);
			Position.Remove(this);
		}
	}

	public float HP { get; private set; }
	public bool IsRemoved { get; private set; }

    public void Reset(WallType type)
	{
		Type = type;
		HP = 100;

		var renderer = GetComponentInChildren<Renderer>();
		if (Type == WallType.Box) renderer.material = MaterialBox;
		if (Type == WallType.Block) renderer.material = MaterialBlock;

		color = renderer.material.color;
	}
}

public enum WallType { Box, Block }
