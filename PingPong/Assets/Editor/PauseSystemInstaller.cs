using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds only the pause-system component to the currently open scene.
/// Does not create or modify any UI.
/// </summary>
public static class PauseSystemInstaller
{
    [MenuItem("Tools/PingPong/Install Pause System In Open Scene")]
    private static void InstallPauseSystem()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog(
                "Pause System",
                "Open the gameplay scene first, then run this command again.",
                "OK");
            return;
        }

        PauseBootstrap existingBootstrap = FindPauseBootstrap(scene);
        if (existingBootstrap != null)
        {
            Selection.activeGameObject = existingBootstrap.gameObject;
            EditorUtility.DisplayDialog(
                "Pause System",
                "The pause system is already installed in this scene.",
                "OK");
            return;
        }

        GameObject pauseSystem = new GameObject("PauseSystem");
        Undo.RegisterCreatedObjectUndo(pauseSystem, "Install Pause System");
        Undo.AddComponent<PauseBootstrap>(pauseSystem);

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = pauseSystem;

        Debug.Log(
            "Pause system added to scene '" + scene.name +
            "'. Review PauseBootstrap settings, connect the UI in its own branch, and save the scene.");
    }

    private static PauseBootstrap FindPauseBootstrap(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            PauseBootstrap bootstrap = root.GetComponentInChildren<PauseBootstrap>(true);
            if (bootstrap != null)
                return bootstrap;
        }

        return null;
    }
}
