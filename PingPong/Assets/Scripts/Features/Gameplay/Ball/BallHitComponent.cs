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
