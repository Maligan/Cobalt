using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class GameViewMaximize
{
    static GameViewMaximize()
    {
        var callbackInfo = typeof(EditorApplication).GetField ("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var callback = (EditorApplication.CallbackFunction)callbackInfo.GetValue(null);

        callback += OnEvent;
        callbackInfo.SetValue (null, callback);
    }

    private static void OnEvent()
    {
        var needToggle = EditorApplication.isPlaying
                    && Event.current.type == EventType.KeyDown
                    && Event.current.shift
                    && Event.current.keyCode == KeyCode.Space
                    && EditorWindow.focusedWindow != null
                    && EditorWindow.focusedWindow.GetType().Name == "GameView";

        if (needToggle)
            EditorWindow.focusedWindow.maximized = !EditorWindow.focusedWindow.maximized;
    }
}