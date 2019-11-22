using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AssetDragAndDrop
{
    static AssetDragAndDrop()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
    }

    private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
    {
        var activated = Event.current.alt;
        if (activated)
        {
            // No drag in process 
            var objects = DragAndDrop.objectReferences;
            if (DragAndDrop.objectReferences.Length == 0) return;

            // OnGUI not for target asset
            var within = selectionRect.Contains(Event.current.mousePosition);
            if (within == false) return;

            // Drag performed on draged object
            var target = AssetDatabase.GUIDToAssetPath(guid);
            var targetDrag = DragAndDrop.paths.Contains(target);
            if (targetDrag) return;

            if (Event.current.type == EventType.DragUpdated)
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            
            if (Event.current.type == EventType.DragPerform)
            {
                for (var i = 0; i < objects.Length; i++)
                {
                    var source = objects[i];
                    var sourceIsMain = AssetDatabase.IsMainAsset(source);
                    var sourcePath = AssetDatabase.GetAssetPath(source);

                    AssetDatabase.RemoveObjectFromAsset(source);
                    AssetDatabase.AddObjectToAsset(source, target);
                    if (sourceIsMain) AssetDatabase.DeleteAsset(sourcePath);
                }

                AssetDatabase.SaveAssets();
            }
        }
    }
}