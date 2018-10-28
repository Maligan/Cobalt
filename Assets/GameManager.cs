using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Current { get; private set; }

	// Hooks

	// Prefabs
	public List<GameObject> Prefabs;
	public Arena Arena;

	public GameObject ArenaRoot;
	public GameObject UIRoot;

	private void Awake()
	{
		Current = this;
	}

	private void Start()
	{
		Arena = new Arena(Prefabs, ArenaRoot.transform);
		OnPlayClick();
	}

	private void CreateArena()
	{
		CreateLevel();
		StartCoroutine(InputCoroutine());
	}

	private IEnumerator InputCoroutine()
	{
		var unitCount = Arena.Objects.Count(x => x.Type == ArenaObjectType.Unit);

		while (true)
		{
			var units = Arena.Objects.Where(x => x.Type == ArenaObjectType.Unit);
			var exit = Arena.Objects.FirstOrDefault(x => x.Type == ArenaObjectType.Exit);
			var win = unitCount != units.Count();

			if (exit != null)
			{
				foreach (var unit in units)
				{
					if (Vector2.Distance(unit.transform.position, exit.transform.position) < 1/3f)
					{
						win = true;
						break;
					}
				}
			}
			

			if (win)
			{
				Arena.Clear();
				UIRoot.SetActive(true);
				StopAllCoroutines();
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	public void CreateLevel()
	{
		var hw = 20;
		var hh = 10;

		var result = ArenaGenerator.Create(hw*2, hh*2);
		result[0, 0] = false;
		result[hw*2-1, hh*2-1] = false;

		for (var x = 0; x < hw*2; x++)
			for (var y = 0; y < hh*2; y++)
				if (result[x, y])
						Arena.Pool.CreateWall(Arena.GetCell(x-hw, y-hh))
						.name = string.Format("Wall ({0}; {1})", x, y);

		var poses = FindObjectsOfType<Wall>().Where(x => x.WallType == WallType.Box).Select(x => x.Position).ToList();
		var pos = poses[Random.Range(0, poses.Count)];
		Arena.Pool.CreateExit(pos);

		CreateUnit(-hw, -hh, false);
		CreateUnit(+hw, +hh, true);
	}

	public void CreateUnit(int x, int y, bool ai)
	{
		var unit = Arena.Pool.CreateUnit(Arena.GetCell(x, y));

		if (ai) unit.GetComponent<UnitAI>().enabled = true;
        else unit.GetComponent<UnitUserInput>().enabled = true;
	}

	#region Handlers 

	public void OnPlayClick()
	{
		UIRoot.SetActive(false);
		CreateArena();
	}

	#endregion
}
