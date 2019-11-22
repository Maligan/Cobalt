using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class DragAndDropSubAssets
{
    static DragAndDropSubAssets()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
    }

    private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
    {
        var activated = Event.current.alt;
        if (activated)
        {
            // No drag in process 
            var sources = DragAndDrop.objectReferences;
            if (sources.Length == 0) return;

            // OnGUI not for target asset
            var within = selectionRect.Contains(Event.current.mousePosition);
            if (within == false) return;

            // Drag performed on draged object
            var target = AssetDatabase.GUIDToAssetPath(guid);
            var targetInSources = DragAndDrop.paths.Contains(target);
            if (targetInSources) return;

            if (Event.current.type == EventType.DragUpdated)
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            
            if (Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                Move(sources, target);
            }
        }
    }

    private static void Move(IEnumerable<UnityEngine.Object> sources, string targetPath)
    {
        var targetIsFolder = AssetDatabase.IsValidFolder(targetPath);
        
        foreach (var source in sources)
        {
            var sourcePath = AssetDatabase.GetAssetPath(source);
            var sourceIsMain = AssetDatabase.IsMainAsset(source);
            if (sourceIsMain && targetIsFolder) continue;

            // If source is main asset in file then move it with all subassets
            var sourceAssets = new List<UnityEngine.Object>() { source };
            if (sourceIsMain)
            {
                var subassets = AssetDatabase.LoadAllAssetRepresentationsAtPath(sourcePath);
                sourceAssets.AddRange(subassets);
            }

            // Peform move assets from source file to target
            foreach (var asset in sourceAssets)
            {
                AssetDatabase.RemoveObjectFromAsset(asset);

                if (targetIsFolder)
                    AssetDatabase.CreateAsset(asset, Path.Combine(targetPath, GetExtension(asset)));
                else
                    AssetDatabase.AddObjectToAsset(asset, targetPath);
            }

            // Remove asset file if it is empty now
            if (sourceIsMain)
                AssetDatabase.DeleteAsset(sourcePath);
        }

        AssetDatabase.SaveAssets();
    }

    private static string GetExtension(UnityEngine.Object asset)
    {
        var type = asset.GetType();
        var extension = "asset";

        /**/ if (type == typeof(Material))           extension = "mat";
        else if (type == typeof(AnimationClip))      extension = "anim";
        else if (type == typeof(AnimatorController)) extension = "controller";

        return asset.name + "." + extension;
    }
}