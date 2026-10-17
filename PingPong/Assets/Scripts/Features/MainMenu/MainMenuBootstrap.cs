using UnityEngine;

/// <summary>
/// Composition root for the main-menu feature.
/// Creates plain C# logic and passes its scene dependencies explicitly.
/// </summary>
public sealed class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private MainMenuView view;

    private MainMenuController _controller;

    private void Awake()
    {
        if (view == null)
        {
            Debug.LogError("MainMenuBootstrap is missing its MainMenuView reference.", this);
            enabled = false;
            return;
        }

        _controller = new MainMenuController(view, new MainMenuNavigationService());
    }

    private void OnDestroy()
    {
        _controller?.Dispose();
    }
}
