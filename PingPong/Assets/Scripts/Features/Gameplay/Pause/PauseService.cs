using System;
using UnityEngine;

/// <summary>
/// Owns the paused state and controls Unity's global time scale.
/// Contains no scene objects or UI references.
/// </summary>
public sealed class PauseService
{
    private bool _isPaused;
    private float _timeScaleBeforePause = 1f;

    public bool IsPaused => _isPaused;

    public event Action<bool> StateChanged;

    public void TogglePause()
    {
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (_isPaused)
            return;

        _timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        _isPaused = true;

        StateChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (!_isPaused)
            return;

        Time.timeScale = _timeScaleBeforePause;
        _isPaused = false;

        StateChanged?.Invoke(false);
    }
}
