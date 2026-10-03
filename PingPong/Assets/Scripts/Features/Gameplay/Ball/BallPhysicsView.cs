using UnityEngine;

public class BallPhysicsView : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;

    public void Launch(Vector2 velocity)
    {
        _rigidbody.linearVelocity = velocity;
    }

    public void BounceFromPlayer()
    {
        Vector2 velocity = _rigidbody.linearVelocity;
        float speed = velocity.magnitude;
        float horizontal = velocity.x;
        float vertical = Mathf.Abs(velocity.y);

        if (vertical < 0.01f)
            vertical = speed;

        _rigidbody.linearVelocity = new Vector2(horizontal, vertical).normalized * speed;
    }
}
