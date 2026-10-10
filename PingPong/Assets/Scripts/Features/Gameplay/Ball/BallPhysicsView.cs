using UnityEngine;

public class BallPhysicsView : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;

    private Collider2D _ballCollider;
    private float _baseSpeed;
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
        Vector2 velocity = _rigidbody.linearVelocity;
        if (_targetSpeed > 0f && velocity.sqrMagnitude > 0.0001f)
            _rigidbody.linearVelocity = velocity.normalized * _targetSpeed;
    }

    public void Launch(Vector2 velocity)
    {
        _baseSpeed = velocity.magnitude;
        _targetSpeed = _baseSpeed;
        _rigidbody.linearVelocity = velocity.normalized * _targetSpeed;
    }

    public void StopAtCenter(Vector2 center)
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        _rigidbody.position = center;
        _targetSpeed = _baseSpeed;
    }

    public bool BounceFromPaddle(Collider2D paddle, float paddleVelocityX)
    {
        Vector2 velocity = _rigidbody.linearVelocity;
        if (_baseSpeed <= 0f || velocity.sqrMagnitude < 0.0001f)
            return false;

        float paddleHalfWidth = paddle.bounds.extents.x;
        float hitOffset = 0f;
        if (paddleHalfWidth > 0.01f)
        {
            hitOffset = (_rigidbody.position.x - paddle.bounds.center.x) / paddleHalfWidth;
            hitOffset = Mathf.Clamp(hitOffset, -1f, 1f);
        }

        const float maxHorizontal = 0.9f;
        const float minVertical = 0.25f;
        const float maxPaddleSpeed = 4f;
        const float movementInfluence = 0.15f;

        float movementFactor = Mathf.Clamp(paddleVelocityX / maxPaddleSpeed, -1f, 1f);
        float horizontal = Mathf.Clamp(hitOffset * maxHorizontal + movementFactor * movementInfluence,
            -maxHorizontal, maxHorizontal);
        float vertical = Mathf.Max(Mathf.Sqrt(Mathf.Max(0f, 1f - horizontal * horizontal)), minVertical);
        float directionY = _rigidbody.position.y >= paddle.bounds.center.y ? 1f : -1f;
        Vector2 direction = new Vector2(horizontal, vertical * directionY).normalized;

        if (Mathf.Abs(direction.y) < minVertical)
        {
            direction.y = Mathf.Sign(direction.y) * minVertical;
            direction.Normalize();
        }

        float edgePrecision = Mathf.InverseLerp(0.8f, 1f, Mathf.Abs(hitOffset));
        float movementPrecision = Mathf.InverseLerp(1.5f, maxPaddleSpeed, Mathf.Abs(paddleVelocityX));
        bool movingTowardShot = Mathf.Abs(paddleVelocityX) > 0.01f
            && Mathf.Sign(paddleVelocityX) == Mathf.Sign(hitOffset);
        float attackQuality = movingTowardShot ? edgePrecision * movementPrecision : 0f;

        _targetSpeed = _baseSpeed * (1f + 0.1f * attackQuality);
        _rigidbody.linearVelocity = direction * _targetSpeed;
        return attackQuality >= 0.7f;
    }
}