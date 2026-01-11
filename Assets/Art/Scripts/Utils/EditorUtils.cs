#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class EditorUtils
{
    [MenuItem("Tools/WS26-VFX/Add All \"Art\" Scenes To Build")]
    public static void AddAllScenesToBuild()
    {
        var allScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Art" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => new EditorBuildSettingsScene(path, true))
            .ToArray();

        EditorBuildSettings.scenes = allScenes;
        Debug.Log($"Added {allScenes.Length} scenes to build settings.");
    }
}
#endif
