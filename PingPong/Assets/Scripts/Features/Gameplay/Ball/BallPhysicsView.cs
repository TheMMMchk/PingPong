using UnityEngine;

public class BallPhysicsView : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;

    private Collider2D _ballCollider;

    public Vector2 Position => _rigidbody.position;
    public Vector2 Velocity => _rigidbody.linearVelocity;
    public float Radius => _ballCollider != null ? _ballCollider.bounds.extents.x : 0f;

    private void Awake()
    {
        _ballCollider = _rigidbody.GetComponent<Collider2D>();
    }

    public void Launch(Vector2 velocity)
    {
        _rigidbody.linearVelocity = velocity.normalized * velocity.magnitude;
    }

    public void BounceFromWall()
    {
        Vector2 velocity = _rigidbody.linearVelocity;
        float speed = velocity.magnitude;

        velocity.x = -velocity.x;
        _rigidbody.linearVelocity = velocity.normalized * speed;
    }

    public void BounceFromPaddle(Collider2D paddle)
    {
        Vector2 velocity = _rigidbody.linearVelocity;
        float speed = velocity.magnitude;

        if (speed < 0.01f)
            return;

        float paddleHalfWidth = paddle.bounds.extents.x;
        float hitOffset = 0f;

        if (paddleHalfWidth > 0.01f)
        {
            hitOffset = (transform.position.x - paddle.bounds.center.x) / paddleHalfWidth;
            hitOffset = Mathf.Clamp(hitOffset, -1f, 1f);
        }

        const float maxHorizontal = 0.9f;
        const float minVertical = 0.25f;

        float horizontal = hitOffset * maxHorizontal;
        float vertical = Mathf.Sqrt(Mathf.Max(0f, 1f - horizontal * horizontal));
        vertical = Mathf.Max(vertical, minVertical);

        float directionY = transform.position.y >= paddle.bounds.center.y ? 1f : -1f;

        Vector2 direction = new Vector2(horizontal, vertical * directionY).normalized;

        if (Mathf.Abs(direction.y) < minVertical)
        {
            direction.y = Mathf.Sign(direction.y) * minVertical;
            direction.Normalize();
        }

        // Preserve the ball's current speed after changing its direction.
        _rigidbody.linearVelocity = direction.normalized * speed;
    }
}