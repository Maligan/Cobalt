using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CircleLayout : MonoBehaviour
{
    public int Radius = 100;
    [Range(0, 360)]
    public int Angle = 90;
    [Range(0, 360)]
    public int AngleDelta = 15;

    private void Update()
    {
        var children = new List<Transform>();
        for (var i = 0; i < transform.childCount; i++)
            if (transform.GetChild(i).gameObject.activeSelf)
                children.Add(transform.GetChild(i));

        if (children.Count == 1)
        {
            var child = children[0]; 

            var x = Radius * Mathf.Cos(Angle * Mathf.Deg2Rad);
            var y = Radius * Mathf.Sin(Angle * Mathf.Deg2Rad);

            child.transform.localPosition = new Vector3(x, y, child.transform.localPosition.z);
        }
        else
        {
            var count = children.Count;
            var diff = AngleDelta * (count-1);
            
            var from = Angle + diff/2;
            var to   = Angle - diff/2;

            for (var i = 0; i < count; i++)
            {
                var child = children[i];

                var angle = Mathf.LerpAngle(from, to, (float)i/(count-1));
                var x = Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                var y = Radius * Mathf.Sin(angle * Mathf.Deg2Rad);

                child.transform.localPosition = new Vector3(x, y, child.transform.localPosition.z);
            }
        }
    }
}
