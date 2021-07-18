using System;
using System.Collections.Generic;
using UnityEngine;

public static class GUIExtensions
{
    public static void Actions(params (string, Action)[] actions)
    {
        var scale = Screen.height / 480f;

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale*Vector3.one);

        GUILayout.BeginArea(new Rect(0, 0, Screen.width/scale, Screen.height/scale));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        // GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUI.skin.textField);

        // -----------------------------------------------------------------------------

        foreach (var pair in actions)
        {
            GUI.enabled = pair.Item2 != null && Application.isPlaying;
            if (GUILayout.Button(pair.Item1))
                pair.Item2();
                
            GUI.enabled = true;
        }

        // -----------------------------------------------------------------------------

        GUILayout.EndVertical();
        // GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        // GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUI.matrix = Matrix4x4.identity;
    }
}