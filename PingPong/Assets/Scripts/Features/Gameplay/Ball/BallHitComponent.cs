using System;
using UnityEngine;

public class BallHitComponent : MonoBehaviour
{
    public event Action<Collision2D> OnCollision;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollision?.Invoke(collision);
    }
}

public class BallHitService : IDisposable
{
    private readonly BallHitComponent _hitComponent;

    public BallHitService(BallHitComponent hitComponent)
    {
        _hitComponent = hitComponent;
        _hitComponent.OnCollision += Hit;
    }

    public void Dispose()
    {
        _hitComponent.OnCollision -= Hit;
    }

    private void Hit(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            Debug.Log("мячик отскочил");
    }
}
