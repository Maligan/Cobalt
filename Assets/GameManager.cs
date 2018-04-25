using UnityEngine;

public class GameManager : MonoBehaviour
{
	// Prefabs
	public GameObject Wall;
	public GameObject Unit;
	public GameObject Bomb;
	
	// Active objects
	public GameObject P1;
	
	private void Start()
	{
		CreateLevel();
		CreateUnit();
	}

	public void CreateLevel()
	{
		var width = 5;
		var height = 3;

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				var wall = Instantiate(Wall);
				wall.name = string.Format("{0} ({1}; {2})", Wall.name, x, y);
				wall.transform.Translate(x*2, y*2, 0);
			}
		}
	}

	public void CreateUnit()
	{
		P1 = Instantiate(Unit);
		P1.transform.Translate(-1, -1, 0);
	}
}
