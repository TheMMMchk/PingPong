using System;
using UnityEngine;

public class BallHitService : IDisposable
{
    private readonly BallHitComponent _hitComponent;
    private readonly BallPhysicsView _physicsView;

    public BallHitService(BallHitComponent hitComponent, BallPhysicsView physicsView)
    {
        _hitComponent = hitComponent;
        _physicsView = physicsView;
        _hitComponent.OnCollision += Hit;
    }

    public void Dispose()
    {
        _hitComponent.OnCollision -= Hit;
    }

    private void Hit(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        _physicsView.Bounce(collision.GetContact(0).normal);
        Debug.Log("мячик отскочил");
    }
}
