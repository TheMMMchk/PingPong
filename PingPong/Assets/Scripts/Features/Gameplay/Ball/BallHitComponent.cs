using System;
using UnityEngine;

public class BallHitComponent : MonoBehaviour
{
    public Action OnBallHit;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            OnBallHit.Invoke();
    }
}

public class BallHitService : IDisposable
{
    private BallHitComponent hitComponent;

    public BallHitService(BallHitComponent hitComponent)
    {
        this.hitComponent = hitComponent;
        hitComponent.OnBallHit += Hit;
    }

    public void Dispose()
    {
        hitComponent.OnBallHit -= Hit;
    }

    public void Hit() 
    {
        Debug.Log("мячик отскочил");
    }
}