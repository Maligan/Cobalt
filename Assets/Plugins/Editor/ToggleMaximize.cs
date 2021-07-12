using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class ToggleMaximize
{
    static ToggleMaximize()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (EditorApplication.isPlaying && ShouldToggleMaximize())
            EditorWindow.focusedWindow.maximized = !EditorWindow.focusedWindow.maximized;
    }

    private static bool ShouldToggleMaximize()
    {
        return Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift);
    }
}
