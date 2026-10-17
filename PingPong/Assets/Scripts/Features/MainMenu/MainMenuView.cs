using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presents the main menu and translates Unity button clicks into UI events.
/// It contains no scene-navigation or gameplay rules.
/// </summary>
public sealed class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    public event Action PlayRequested;
    public event Action SettingsRequested;
    public event Action ExitRequested;

    private void OnEnable()
    {
        if (playButton != null)
            playButton.onClick.AddListener(RaisePlayRequested);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(RaiseSettingsRequested);

        if (exitButton != null)
            exitButton.onClick.AddListener(RaiseExitRequested);
    }

    private void OnDisable()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(RaisePlayRequested);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(RaiseSettingsRequested);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(RaiseExitRequested);
    }

    private void RaisePlayRequested() => PlayRequested?.Invoke();
    private void RaiseSettingsRequested() => SettingsRequested?.Invoke();
    private void RaiseExitRequested() => ExitRequested?.Invoke();
}
