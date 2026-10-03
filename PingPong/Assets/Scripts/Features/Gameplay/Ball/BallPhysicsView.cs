using UnityEngine;

public class BallPhysicsView : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;

    public void Launch(Vector2 velocity)
    {
        _rigidbody.linearVelocity = velocity;
    }

    public void Bounce(Vector2 normal)
    {
        Vector2 velocity = Vector2.Reflect(_rigidbody.linearVelocity, normal);
        _rigidbody.linearVelocity = velocity;
    }
}
