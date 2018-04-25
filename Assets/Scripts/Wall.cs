using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
	public Material MaterialBox;
	public Material MaterialBlock;

	[HideInInspector]
	public WallType Type;

	public void Reset(WallType type)
	{
		Type = type;

		var renderer = GetComponentInChildren<Renderer>();
		if (Type == WallType.Box) renderer.material = MaterialBox;
		if (Type == WallType.Block) renderer.material = MaterialBlock;
	}
}

public enum WallType { Box, Block }
