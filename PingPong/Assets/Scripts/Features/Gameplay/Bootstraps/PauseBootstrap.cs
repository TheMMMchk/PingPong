using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Connects pause input and UI button callbacks to the pause service.
/// UI objects are intentionally created and configured elsewhere.
/// </summary>
public sealed class PauseBootstrap : MonoBehaviour
{
    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI Integration")]
    [SerializeField] private UnityEvent<bool> pauseStateChanged = new UnityEvent<bool>();

    private PauseService _pauseService;

    public bool IsPaused => _pauseService != null && _pauseService.IsPaused;

    private void Awake()
    {
        _pauseService = new PauseService();
        _pauseService.StateChanged += HandlePauseStateChanged;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    /// <summary>Can be connected to the pause menu's Continue button.</summary>
    public void ContinueGame()
    {
        _pauseService?.Resume();
    }

    /// <summary>Can be connected to the pause menu's Settings button.</summary>
    public void OpenSettings()
    {
        Debug.LogWarning("Pause settings are not implemented yet.", this);
    }

    /// <summary>Can be connected to the pause menu's Return to Menu button.</summary>
    public void ReturnToMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("Main Menu Scene Name is empty. Set it in the PauseBootstrap Inspector.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError(
                "Cannot load scene '" + mainMenuSceneName +
                "'. Check the scene name and add the scene to Build Profiles / Build Settings.",
                this);
            return;
        }

        // Never carry a paused time scale into another scene.
        _pauseService?.Resume();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>Can be connected to a pause button or called by other game code.</summary>
    public void TogglePause()
    {
        _pauseService?.TogglePause();
    }

    private void HandlePauseStateChanged(bool paused)
    {
        pauseStateChanged?.Invoke(paused);
    }

    private void OnDestroy()
    {
        if (_pauseService == null)
            return;

        _pauseService.StateChanged -= HandlePauseStateChanged;
        _pauseService.Resume();
    }
}
