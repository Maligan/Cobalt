using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Current { get; private set; }

	// Prefabs
	public GameObject Wall;
	public GameObject Unit;
	public GameObject Bomb;
	
	// Active objects
	public GameObject P1;
	
	private void Start()
	{
		Camera.main.transform.Translate(4.5f, 2.5f, -10);
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
	}

	public void CreateLevel()
	{
		var width = 11;
		var height = 7;

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				var wall = Instantiate(Wall);
				wall.name = string.Format("{0} ({1}; {2})", Wall.name, x, y);
				wall.transform.Translate(x, y, 0);
				wall.GetComponent<Wall>().Reset(x%2==0 || y%2==0 ? WallType.Box : WallType.Block);
			}
		}
	}

	public void CreateUnit()
	{
		P1 = Instantiate(Unit);
		P1.name = Unit.name;
		P1.transform.Translate(-1, -1, 0);
	}
}
