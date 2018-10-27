using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb : MonoBehaviour, ArenaObject
{
	public float Timeout;
	public float Radius;

    public Arena Arena { get; set; }
    public ArenaCell Position  { get; set; }
	public ArenaObjectType Type => ArenaObjectType.Bomb;

    private void Start()
	{
		Timeout = 4;
	}

	private void Update()
	{
		if (Timeout > 0) Timeout -= Time.deltaTime;
		else
		{
			foreach (var cell in Position.GetRadius(3.5f))
			{
				foreach (var obj in cell.Objects)
				{
					if (obj.Type == ArenaObjectType.Wall)
					{
						var wall = (Wall)obj;
						wall.Damage(100);
						break;
					}
				}
			}


			Position.Remove(this);
			Destroy(gameObject);

			// var targets = FindObjectsOfType<Wall>()
			// 	.Where(x => x.Type == WallType.Box)
			// 	.Where(x => Vector2.Distance(x.transform.position, transform.position) <= Radius);

			// foreach (var target in targets)
			// 	Destroy(target.gameObject);

			// Destroy(gameObject);
		}
	}
}
