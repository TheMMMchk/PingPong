using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Performs Unity-specific navigation and application-exit operations.
/// </summary>
public sealed class MainMenuNavigationService
{
    private const string GameplaySceneName = "SampleScene";

    public void OpenGameplay()
    {
        if (!Application.CanStreamedLevelBeLoaded(GameplaySceneName))
        {
            Debug.LogError(
                $"Cannot open scene '{GameplaySceneName}'. " +
                "Run the PingPong menu scene generator and make sure SampleScene is enabled in Build Settings.");
            return;
        }

        SceneManager.LoadScene(GameplaySceneName);
    }

    public void ExitApplication()
    {
        // Application.Quit closes a built player. Unity Editor itself remains open.
        Application.Quit();
    }
}
