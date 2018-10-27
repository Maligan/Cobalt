using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : ArenaObjectBehaviour
{
	public Material MaterialBox;
	public Material MaterialBlock;

	public WallType WallType;

    public override ArenaObjectType Type => ArenaObjectType.Wall;

	private Color color;

	public override void OnCreate()
	{
		base.OnCreate();

		HP = 100;
		GetComponentInChildren<Renderer>().material.color = Color.gray;
	}


	public void Dig(ArenaObject actor)
	{
		Damage(Mathf.RoundToInt(100 * Time.deltaTime));
	}

	public void Damage(int hp)
	{
		HP = Mathf.Max(HP-hp, 0);

		GetComponentInChildren<Renderer>().material.color = color * HP/100f;
		
		if (HP == 0)
		{
			IsRemoved = true;
			Arena.Pool.Recycle(this);
		}
	}

	public float HP { get; private set; }
	public bool IsRemoved { get; private set; }

    public void Reset(WallType type)
	{
		WallType = type;
		HP = 100;

		var renderer = GetComponentInChildren<Renderer>();
		if (WallType == WallType.Box) renderer.material = MaterialBox;
		if (WallType == WallType.Block) renderer.material = MaterialBlock;

		color = renderer.material.color;
	}
}

public enum WallType { Box, Block }
