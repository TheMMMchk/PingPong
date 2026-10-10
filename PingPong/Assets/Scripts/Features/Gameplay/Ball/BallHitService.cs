using System;
using UnityEngine;

public class BallHitService : IDisposable
{
    private readonly BallHitComponent _hitComponent;
    private readonly BallPhysicsView _physicsView;
    private readonly Collider2D _playerPaddle;
    private readonly Collider2D _enemyPaddle;
    private readonly Collider2D _leftWall;
    private readonly Collider2D _rightWall;

    public BallHitService(
        BallHitComponent hitComponent,
        BallPhysicsView physicsView,
        Collider2D playerPaddle,
        Collider2D enemyPaddle,
        Collider2D leftWall,
        Collider2D rightWall)
    {
        _hitComponent = hitComponent;
        _physicsView = physicsView;
        _playerPaddle = playerPaddle;
        _enemyPaddle = enemyPaddle;
        _leftWall = leftWall;
        _rightWall = rightWall;

        _hitComponent.OnCollision += Hit;
    }

    public void Dispose()
    {
        _hitComponent.OnCollision -= Hit;
    }

    private void Hit(Collision2D collision)
    {
        Collider2D otherCollider = collision.collider;

        // Side-wall bounce is handled by the ball's PhysicsMaterial2D.
        // Do not reverse the velocity here as well, or physics and code
        // will both resolve the same collision.
        if (otherCollider == _leftWall || otherCollider == _rightWall)
            return;

        if (otherCollider == _playerPaddle || otherCollider == _enemyPaddle)
        {
            _physicsView.BounceFromPaddle(otherCollider);
            Debug.Log("мячик отскочил");
        }
    }
}