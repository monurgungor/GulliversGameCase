using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Turns off Raycast Target on UI graphics that nothing can click.
///
/// Every graphic with the flag set is another rectangle the event system tests
/// on each touch, and decorative panels, shadows and labels make up most of a
/// casual game's canvas. Re-run this after a round of UI work; new graphics come
/// in with the flag on.
/// </summary>
public static class RaycastTargetCleaner
{
    [MenuItem("Tools/Word Game/Clear Unused Raycast Targets")]
    public static void Clean()
    {
        int cleared = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);

            int changed = CleanHierarchy(prefab);
            if (changed > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                cleared += changed;
            }

            PrefabUtility.UnloadPrefabContents(prefab);
        }

        foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
        {
            Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
            int changed = 0;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                changed += CleanHierarchy(root);
            }

            if (changed > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                cleared += changed;
            }
        }

        Debug.Log($"RaycastTargetCleaner: cleared {cleared} graphics.");
    }

    private static int CleanHierarchy(GameObject root)
    {
        int cleared = 0;

        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
        {
            if (!graphic.raycastTarget || NeedsRaycast(graphic))
            {
                continue;
            }

            Undo.RecordObject(graphic, "Clear Raycast Target");
            graphic.raycastTarget = false;
            EditorUtility.SetDirty(graphic);
            cleared++;
        }

        return cleared;
    }

    /// <summary>
    /// A graphic earns its raycast when something in its own branch reacts to
    /// input: a button it is part of, a handler on it, or the viewport a scroll
    /// view drags against.
    /// </summary>
    private static bool NeedsRaycast(Graphic graphic)
    {
        if (graphic.GetComponentInParent<Selectable>(true) != null)
        {
            return true;
        }

        if (graphic.GetComponent<IEventSystemHandler>() != null)
        {
            return true;
        }

        var scrollRect = graphic.GetComponentInParent<ScrollRect>(true);
        return scrollRect != null && IsViewportOf(scrollRect, graphic);
    }

    private static bool IsViewportOf(ScrollRect scrollRect, Graphic graphic)
    {
        RectTransform viewport = scrollRect.viewport != null
            ? scrollRect.viewport
            : scrollRect.transform as RectTransform;

        return viewport != null && viewport == graphic.rectTransform;
    }
}
