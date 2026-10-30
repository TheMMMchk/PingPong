using System;

public sealed class RunProgressionService : IDisposable
{
    private readonly BallRoundService _roundService;
    private readonly int _matchesPerRun;

    private bool _waitingForUpgrade;
    private bool _matchInProgress = true;
    private bool _runFinished;
    private bool _disposed;

    public int CompletedMatches { get; private set; }

    public event Action<int> UpgradeSelectionRequested;
    public event Action<bool> RunFinished;

    public RunProgressionService(BallRoundService roundService, int matchesPerRun = 3)
    {
        if (roundService == null)
            throw new ArgumentNullException(nameof(roundService));

        if (matchesPerRun <= 0)
            throw new ArgumentOutOfRangeException(nameof(matchesPerRun), "Matches per run must be greater than zero.");

        _roundService = roundService;
        _matchesPerRun = matchesPerRun;
        _roundService.MatchFinished += HandleMatchFinished;
    }

    public void ContinueAfterUpgrade()
    {
        if (_disposed || _runFinished || !_waitingForUpgrade)
            return;

        _waitingForUpgrade = false;
        _matchInProgress = true;
        _roundService.StartNextMatch();
    }

    private void HandleMatchFinished(bool playerWon)
    {
        if (_disposed || _runFinished || !_matchInProgress)
            return;

        _matchInProgress = false;

        if (!playerWon)
        {
            _runFinished = true;
            RunFinished?.Invoke(false);
            return;
        }

        CompletedMatches++;

        if (CompletedMatches >= _matchesPerRun)
        {
            _runFinished = true;
            RunFinished?.Invoke(true);
            return;
        }

        _waitingForUpgrade = true;
        UpgradeSelectionRequested?.Invoke(CompletedMatches);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _roundService.MatchFinished -= HandleMatchFinished;
        UpgradeSelectionRequested = null;
        RunFinished = null;
    }
}
