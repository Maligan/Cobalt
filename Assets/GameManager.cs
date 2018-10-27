using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Current { get; private set; }

	// Hooks

	// Prefabs
	public GameObject Wall;
	public GameObject Unit;
	public GameObject Bomb;
	public GameObject Door;
	
	// Active objects
	public GameObject UI;

	public Arena Arena;
	public List<Unit> Units;
	public GameObject Exit;


	private void Awake()
	{
		Current = this;
	}

	private void Start()
	{
		OnPlayClick();
	}

	private void CreateArena()
	{
		Arena = new Arena();

		CreateLevel();
		StartCoroutine(InputCoroutine());
	}

	private IEnumerator InputCoroutine()
	{
		while (true)
		{
			foreach (var unit in Units)
			{
				if (Vector2.Distance(unit.transform.position, Exit.transform.position) < 1/3f)
				{
					foreach (var obj in Arena.Objects)
						Destroy(((MonoBehaviour)obj).gameObject);

					foreach (var u in Units)
						Destroy(u.gameObject);

					Units.Clear();
					Arena.Clear();
					UI.SetActive(true);
					StopAllCoroutines();
					
					break;
				}
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	public void CreateLevel()
	{
		var hw = 6;
		var hh = 4;

		for (var x = -hw; x <= hw; x++)
			for (var y = -hh; y <= hh; y++)
				if (!(x ==-hw&&y==-hh) && !(x==hw&&y==hh))
					{
						var wall = Instantiate(Wall);
						wall.name = string.Format("{0} ({1}; {2})", Wall.name, x, y);
						wall.transform.localPosition = Arena.GetCell(x, y).Center;
						wall.GetComponent<Wall>().Reset(WallType.Box);
						wall.GetComponent<Wall>().Arena = Arena;
						wall.GetComponent<Wall>().Position = Arena.GetCell(x, y);
						wall.GetComponent<Wall>().Position.Add(wall.GetComponent<Wall>());
					}

		var boxes = FindObjectsOfType<Wall>()
			.Where(x => x.Type == WallType.Box)
			.ToList();

		var box = boxes[Random.Range(0, boxes.Count)];
		var exit = Instantiate(Door, box.transform.position, Quaternion.identity).GetComponent<Exit>(); 
		exit.Position = box.Position;
		exit.Position.Add(exit);
		exit.Arena = box.Arena;
		Exit = exit.gameObject;

		Units.Clear();
		CreateUnit(-hw, -hh, false);
		CreateUnit(+hw, +hh, true);
	}

	public void CreateUnit(int x, int y, bool ai)
	{
		var unit = Instantiate(Unit);
		unit.name = Unit.name;
		unit.GetComponent<Unit>().Arena = Arena;
		unit.GetComponent<Unit>().Position = Arena.GetCell(x, y);
		unit.transform.localPosition = Arena.GetCell(x, y).Center;
		if (ai) unit.AddComponent<UnitAI>();
		else unit.AddComponent<UnitUserInput>();
		Units.Add(unit.GetComponent<Unit>());
	}


	#region Handlers 

	public void OnPlayClick()
	{
		UI.SetActive(false);
		CreateArena();
	}

	#endregion
}
