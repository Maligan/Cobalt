using System.Collections;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Current { get; private set; }

	// Prefabs
	public GameObject Wall;
	public GameObject Unit;
	public GameObject Bomb;
	public GameObject Door;
	
	// Active objects
	public GameObject P1;
	public GameObject Exit;

	public Arena Arena;

	private void Awake()
	{
		Current = this;
		// Camera.main.transform.Translate(6f, 4f, -10);
	}

	private void Start()
	{
		Arena = new Arena();

		CreateLevel2();
		CreateUnit();

		StartCoroutine(InputCoroutine());
	}

	private IEnumerator InputCoroutine()
	{
		while (true)
		{
			var p1 = P1.GetComponent<Unit>();
			if (Input.GetKey(KeyCode.UpArrow)) p1.Move(Vector2.up);
			if (Input.GetKey(KeyCode.DownArrow)) p1.Move(Vector2.down);
			if (Input.GetKey(KeyCode.RightArrow)) p1.Move(Vector2.right);
			if (Input.GetKey(KeyCode.LeftArrow)) p1.Move(Vector2.left);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void Update()
	{
		// Move
		// var steer = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		// var speed = 0.1f;
		// P1.transform.Translate(steer * speed);

		// Bomb
		// if (Input.GetKeyDown(KeyCode.Space))
		// {
		// 	var bomb = Instantiate(Bomb, P1.transform.position, Quaternion.identity);
		// 	bomb.name = Bomb.name;
		// 	bomb.GetComponent<Bomb>().Reset();
		// }

		// Win
		// if (Vector2.Distance(P1.transform.position, Exit.transform.position) < 1/3f)
		// {
		// 	FindObjectsOfType<Wall>().ToList().ForEach(x => Destroy(x.gameObject));
		// 	FindObjectsOfType<Bomb>().ToList().ForEach(x => Destroy(x.gameObject));
		// 	Destroy(P1);
		// 	Destroy(Exit);

		// 	Start();
		// }
	}

	public void CreateLevel2()
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
	}

	public void CreateLevel()
	{
		var width = 12;
		var height = 8;

		for (int x = 0; x <= width; x++)
		{
			for (int y = 0; y <= height; y++)
			{
				// Char Positions
				var isSpawn = (x == 1 && y == 1) || (x == width-1 && y == height-1);
				if (isSpawn)
					continue;

				var wall = Instantiate(Wall);
				wall.name = string.Format("{0} ({1}; {2})", Wall.name, x, y);
				wall.transform.Translate(x, y, 0);
				
				if (x == 0 || x == width || y == 0 || y == height)
					wall.GetComponent<Wall>().Reset(WallType.Block);
				else if (x == 1 || x == width-1 || y == 1 || y == height-1)
					wall.GetComponent<Wall>().Reset(WallType.Box);
				else
					wall.GetComponent<Wall>().Reset(x%2==1 || y%2==1 ? WallType.Box : WallType.Block);
			}
		}

		var boxes = FindObjectsOfType<Wall>()
			.Where(x => x.Type == WallType.Box)
			.Select(x => x.transform.position)
			.ToList();

		Exit = Instantiate(Door, boxes[Random.Range(0, boxes.Count)], Quaternion.identity); 
	}

	public void CreateUnit()
	{
		P1 = Instantiate(Unit);
		P1.name = Unit.name;
		P1.transform.Translate(0, 0, 0);
		P1.GetComponent<Unit>().Arena = Arena;
		P1.GetComponent<Unit>().Position = Arena.GetCell(0, 0);
	}
}
