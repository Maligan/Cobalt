using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb : MonoBehaviour
{
	public float Timeout;
	public float Radius;

	public void Reset()
	{
		Timeout = 2;
	}

	public void Update()
	{
		if (Timeout > 0) Timeout -= Time.deltaTime;
		else
		{
			var targets = FindObjectsOfType<Wall>()
				.Where(x => x.Type == WallType.Box)
				.Where(x => Vector2.Distance(x.transform.position, transform.position) <= Radius);

			foreach (var target in targets)
				Destroy(target.gameObject);

			Destroy(gameObject);
		}
	}
}
