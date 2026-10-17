using System;

/// <summary>
/// Connects main-menu interaction events to the actions they request.
/// This is a plain C# class; the Bootstrap owns its lifetime.
/// </summary>
public sealed class MainMenuController : IDisposable
{
    private readonly MainMenuView _view;
    private readonly MainMenuNavigationService _navigation;
    private bool _disposed;

    public MainMenuController(MainMenuView view, MainMenuNavigationService navigation)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

        _view.PlayRequested += HandlePlayRequested;
        _view.SettingsRequested += HandleSettingsRequested;
        _view.ExitRequested += HandleExitRequested;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _view.PlayRequested -= HandlePlayRequested;
        _view.SettingsRequested -= HandleSettingsRequested;
        _view.ExitRequested -= HandleExitRequested;
        _disposed = true;
    }

    private void HandlePlayRequested() => _navigation.OpenGameplay();
    
    private void HandleSettingsRequested()
    {
        // Intentionally empty. The Settings button is present and clickable,
        // but the settings screen is not part of this task.
    }

    private void HandleExitRequested() => _navigation.ExitApplication();
}
