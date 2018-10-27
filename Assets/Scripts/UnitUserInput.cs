using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitUserInput : MonoBehaviour
{
    private Unit unit;

    private void Start()
    {
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        // [Move]

        Vector2 move;

        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        if (h > 0) move = Vector2.right;
        else if (h < 0) move = Vector2.left;
        else if (v > 0) move = Vector2.up;
        else if (v < 0) move = Vector2.down;
        else move = Vector2.zero;

        unit.DoMove(move);

        // [Place]

        if (Input.GetButtonDown("Jump"))
            unit.DoBomb();
    }
}
