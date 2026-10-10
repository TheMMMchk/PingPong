using System;
using UnityEngine;

public class BallHitService : IDisposable
{
    private readonly BallHitComponent _hitComponent;
    private readonly BallPhysicsView _physicsView;
    private readonly BallImpactView _impactView;
    private readonly MoveView _playerMoveView;
    private readonly MoveView _enemyMoveView;
    private readonly Collider2D _playerPaddle;
    private readonly Collider2D _enemyPaddle;
    private readonly Collider2D _leftWall;
    private readonly Collider2D _rightWall;

    public BallHitService(BallHitComponent hitComponent, BallPhysicsView physicsView, BallImpactView impactView,
        MoveView playerMoveView, MoveView enemyMoveView, Collider2D playerPaddle, Collider2D enemyPaddle,
        Collider2D leftWall, Collider2D rightWall)
    {
        _hitComponent = hitComponent;
        _physicsView = physicsView;
        _impactView = impactView;
        _playerMoveView = playerMoveView;
        _enemyMoveView = enemyMoveView;
        _playerPaddle = playerPaddle;
        _enemyPaddle = enemyPaddle;
        _leftWall = leftWall;
        _rightWall = rightWall;
        _hitComponent.OnCollision += Hit;
    }

    public void Dispose() { _hitComponent.OnCollision -= Hit; }

    private void Hit(Collision2D collision)
    {
        Collider2D other = collision.collider;
        Vector2 point = collision.contactCount > 0 ? collision.GetContact(0).point : _physicsView.Position;
        if (other == _leftWall || other == _rightWall)
        {
            _impactView.PlayWallHit(point);
            return;
        }
        if (other != _playerPaddle && other != _enemyPaddle) return;
        MoveView moveView = other == _playerPaddle ? _playerMoveView : _enemyMoveView;
        float paddleVelocity = moveView != null ? moveView.VelocityX : 0f;
        bool strong = _physicsView.BounceFromPaddle(other, paddleVelocity);
        _impactView.PlayPaddleHit(point, other, strong);
        Debug.Log(strong ? "Сильный удар по мячу!" : "мячик отскочил");
    }
}
