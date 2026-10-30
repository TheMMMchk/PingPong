using System;
using UnityEngine;

public class BallRoundService
{
    private readonly BallPhysicsView _ballView;
    private readonly BallService _ballService;
    private readonly Collider2D _playerPaddle;
    private readonly Collider2D _enemyPaddle;
    private readonly ScorePresenter _scorePresenter;
    private readonly Vector2 _center;
    private readonly int _pointsToWin;

    private Vector2 _nextServeDirection;
    private float _serveDelay;
    private bool _waitingForServe;
    private bool _matchFinished;

    private const float ServeDelay = 1f;

    public event Action<bool> MatchFinished;

    public BallRoundService(
        BallPhysicsView ballView,
        BallService ballService,
        Collider2D playerPaddle,
        Collider2D enemyPaddle,
        ScorePresenter scorePresenter,
        Vector2 initialServeDirection,
        Vector2 center,
        int pointsToWin = 3)
    {
        if (pointsToWin <= 0)
            throw new ArgumentOutOfRangeException(nameof(pointsToWin), "Points to win must be greater than zero.");

        _ballView = ballView;
        _ballService = ballService;
        _playerPaddle = playerPaddle;
        _enemyPaddle = enemyPaddle;
        _scorePresenter = scorePresenter;
        _nextServeDirection = initialServeDirection.normalized;
        _center = center;
        _pointsToWin = pointsToWin;

        Debug.Log($"Счёт — Игрок: {_scorePresenter.PlayerScore} | Бот: {_scorePresenter.EnemyScore}");
    }

    public void Tick(float deltaTime)
    {
        if (_matchFinished)
            return;

        if (_waitingForServe)
        {
            _serveDelay -= deltaTime;

            if (_serveDelay <= 0f)
            {
                _waitingForServe = false;
                _ballService.Launch(_nextServeDirection);
            }

            return;
        }

        float ballY = _ballView.Position.y;
        float bottomLimit = _playerPaddle.bounds.min.y - _ballView.Radius;
        float topLimit = _enemyPaddle.bounds.max.y + _ballView.Radius;

        if (ballY < bottomLimit)
        {
            AwardPointToEnemy();
            return;
        }

        if (ballY > topLimit)
            AwardPointToPlayer();
    }

    private void AwardPointToEnemy()
    {
        if (_matchFinished)
            return;

        _scorePresenter.AddEnemyScore(1);
        LogScore("Бот");
        AdvanceServeDirection();

        if (_scorePresenter.EnemyScore >= _pointsToWin)
        {
            FinishMatch(false);
            return;
        }

        PrepareNextServe();
    }

    private void AwardPointToPlayer()
    {
        if (_matchFinished)
            return;

        _scorePresenter.AddPlayerScore(1);
        LogScore("Игрок");
        AdvanceServeDirection();

        if (_scorePresenter.PlayerScore >= _pointsToWin)
        {
            FinishMatch(true);
            return;
        }

        PrepareNextServe();
    }

    private void LogScore(string pointWinner)
    {
        Debug.Log($"Очко получает: {pointWinner}. Счёт — Игрок: {_scorePresenter.PlayerScore} | Бот: {_scorePresenter.EnemyScore}");
    }

    private void AdvanceServeDirection()
    {
        // Preserve the existing alternating serve direction after every point, including a match point.
        _nextServeDirection.y = -_nextServeDirection.y;
    }

    private void PrepareNextServe()
    {
        _ballView.StopAtCenter(_center);
        _serveDelay = ServeDelay;
        _waitingForServe = true;
    }

    private void FinishMatch(bool playerWon)
    {
        if (_matchFinished)
            return;

        _matchFinished = true;
        _waitingForServe = false;
        _serveDelay = 0f;
        _ballView.StopAtCenter(_center);

        MatchFinished?.Invoke(playerWon);
    }

    public void StartNextMatch()
    {
        if (!_matchFinished)
            return;

        _scorePresenter.ResetScores();
        _ballView.StopAtCenter(_center);
        _matchFinished = false;
        _serveDelay = ServeDelay;
        _waitingForServe = true;
    }
}
