using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb : ArenaObjectBehaviour
{
	public float Timeout;
	public float Radius;

	public override ArenaObjectType Type => ArenaObjectType.Bomb;

    public override void OnCreate()
	{
		base.OnCreate();
		Timeout = 8;
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
					else if (obj.Type == ArenaObjectType.Unit)
					{
						var unit = (Unit)obj;
						unit.Damage(100);
						break;
					}
				}
			}

			Arena.Pool.Recycle(this);
		}
	}
}
