using UnityEngine;

public class BallRoundService
{
    private readonly BallPhysicsView _ballView;
    private readonly BallService _ballService;
    private readonly Collider2D _playerPaddle;
    private readonly Collider2D _enemyPaddle;
    private readonly ScorePresenter _scorePresenter;
    private readonly Vector2 _center;

    private Vector2 _nextServeDirection;
    private float _serveDelay;
    private bool _waitingForServe;

    private const float ServeDelay = 1f;

    public BallRoundService(
        BallPhysicsView ballView,
        BallService ballService,
        Collider2D playerPaddle,
        Collider2D enemyPaddle,
        ScorePresenter scorePresenter,
        Vector2 initialServeDirection,
        Vector2 center)
    {
        _ballView = ballView;
        _ballService = ballService;
        _playerPaddle = playerPaddle;
        _enemyPaddle = enemyPaddle;
        _scorePresenter = scorePresenter;
        _nextServeDirection = initialServeDirection.normalized;
        _center = center;

        Debug.Log($"Счёт — Игрок: {_scorePresenter.PlayerScore} | Бот: {_scorePresenter.EnemyScore}");
    }

    public void Tick(float deltaTime)
    {
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
        _scorePresenter.AddEnemyScore(1);
        LogScore("Бот");
        PrepareNextServe();
    }

    private void AwardPointToPlayer()
    {
        _scorePresenter.AddPlayerScore(1);
        LogScore("Игрок");
        PrepareNextServe();
    }

    private void LogScore(string pointWinner)
    {
        Debug.Log($"Очко получает: {pointWinner}. Счёт — Игрок: {_scorePresenter.PlayerScore} | Бот: {_scorePresenter.EnemyScore}");
    }

    private void PrepareNextServe()
    {
        _ballView.StopAtCenter(_center);

        // Alternate the serving side after every point.
        _nextServeDirection.y = -_nextServeDirection.y;

        _serveDelay = ServeDelay;
        _waitingForServe = true;
    }
}
