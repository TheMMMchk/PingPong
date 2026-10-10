using UnityEngine;

public class BallPhysicsView : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;

    private Collider2D _ballCollider;
    private float _targetSpeed;

    public Vector2 Position => _rigidbody.position;
    public Vector2 Velocity => _rigidbody.linearVelocity;
    public float Radius => _ballCollider != null ? _ballCollider.bounds.extents.x : 0f;

    private void Awake()
    {
        _ballCollider = _rigidbody.GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        // Collision solving can change the velocity after a collision callback.
        // Keep the ball's speed constant while allowing its direction to change.
        Vector2 velocity = _rigidbody.linearVelocity;

        if (_targetSpeed > 0f && velocity.sqrMagnitude > 0.0001f)
            _rigidbody.linearVelocity = velocity.normalized * _targetSpeed;
    }

    public void Launch(Vector2 velocity)
    {
        _targetSpeed = velocity.magnitude;
        _rigidbody.linearVelocity = velocity.normalized * _targetSpeed;
    }

    public void StopAtCenter(Vector2 center)
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        _rigidbody.position = center;
    }

    public void BounceFromPaddle(Collider2D paddle)
    {
        Vector2 velocity = _rigidbody.linearVelocity;

        if (_targetSpeed <= 0f || velocity.sqrMagnitude < 0.0001f)
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

        _rigidbody.linearVelocity = direction * _targetSpeed;
    }
}