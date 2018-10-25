using System.Collections;
using System.Linq;
using Unity.Entities;
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
	[HideInInspector] public GameObject P1;
	[HideInInspector] public GameObject Exit;

	public Arena Arena;

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
			var p1 = P1.GetComponent<Unit>();
			// if (Input.GetKey(KeyCode.UpArrow)) p1.Move(Vector2.up);
			// if (Input.GetKey(KeyCode.DownArrow)) p1.Move(Vector2.down);
			// if (Input.GetKey(KeyCode.RightArrow)) p1.Move(Vector2.right);
			// if (Input.GetKey(KeyCode.LeftArrow)) p1.Move(Vector2.left);



			if (Vector2.Distance(P1.transform.position, Exit.transform.position) < 1/3f)
			{
				FindObjectsOfType<Wall>().ToList().ForEach(x => Destroy(x.gameObject));
				FindObjectsOfType<Bomb>().ToList().ForEach(x => Destroy(x.gameObject));
				Destroy(P1);
				Destroy(Exit);

				UI.SetActive(true);
				StopAllCoroutines();
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	public void CreateLevel()
	{
		for (var x = -6; x <= 6; x++)
			for (var y = -4; y <= 4; y++)
				if (x != 0 || y != 0)
					{
						var wall = Instantiate(Wall);
						wall.name = string.Format("{0} ({1}; {2})", Wall.name, x, y);
						wall.transform.Translate(x, y, 0);
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

		CreateUnit();
	}

	public void CreateUnit()
	{
		P1 = Instantiate(Unit);
		P1.name = Unit.name;
		P1.transform.Translate(0, 0, 0);
		P1.GetComponent<Unit>().Arena = Arena;
		P1.GetComponent<Unit>().Position = Arena.GetCell(0, 0);
	}


	#region Handlers 

	public void OnPlayClick()
	{
		UI.SetActive(false);
		CreateArena();
	}

	#endregion
}
