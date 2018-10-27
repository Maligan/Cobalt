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
		while (true)
		{
			var units = Arena.Objects.Where(x => x.Type == ArenaObjectType.Unit);
			var exit = Arena.Objects.FirstOrDefault(x => x.Type == ArenaObjectType.Exit);

			if (exit != null)
			{
				foreach (var unit in units)
				{
					if (Vector2.Distance(unit.transform.position, exit.transform.position) < 1/3f)
					{
						Arena.Clear();
						UIRoot.SetActive(true);
						StopAllCoroutines();
						break;
					}
				}
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	public void CreateLevel()
	{
		var hw = 2;
		var hh = 2;

		for (var x = -hw; x <= hw; x++)
			for (var y = -hh; y <= hh; y++)
				if (!(x ==-hw&&y==-hh) && !(x==hw&&y==hh))
						Arena.Pool
						.CreateWall(Arena.GetCell(x, y))
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
		if (ai) unit.gameObject.AddComponent<UnitAI>();
		else unit.gameObject.AddComponent<UnitUserInput>();
	}

	#region Handlers 

	public void OnPlayClick()
	{
		UIRoot.SetActive(false);
		CreateArena();
	}

	#endregion
}
