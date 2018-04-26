using System.Linq;
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
	
	private void Start()
	{
		Camera.main.transform.Translate(6f, 4f, -10);
		Current = this;
		CreateLevel();
		CreateUnit();
	}

	private void Update()
	{
		// Move
		var steer = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		var speed = 0.1f;
		P1.transform.Translate(steer * speed);

		// Bomb
		if (Input.GetKeyDown(KeyCode.Space))
		{
			var bomb = Instantiate(Bomb, P1.transform.position, Quaternion.identity);
			bomb.name = Bomb.name;
			bomb.GetComponent<Bomb>().Reset();
		}

		// Win
		if (Vector2.Distance(P1.transform.position, Exit.transform.position) < 1/3f)
		{
			FindObjectsOfType<Wall>().ToList().ForEach(x => Destroy(x.gameObject));
			FindObjectsOfType<Bomb>().ToList().ForEach(x => Destroy(x.gameObject));
			Destroy(P1);
			Destroy(Exit);
			gameObject.SetActive(false);
		}
	}

	public void CreateLevel()
	{
		var width = 12;
		var height = 8;

		for (int x = 0; x <= width; x++)
		{
			for (int y = 0; y <= height; y++)
			{
				if (x == 1 && y == 1) continue; // Char Position

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
		P1.transform.Translate(1, 1, 0);
	}
}
