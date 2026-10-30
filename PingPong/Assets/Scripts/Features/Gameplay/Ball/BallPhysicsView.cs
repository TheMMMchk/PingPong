using UnityEngine;

public class BallPhysicsView : MonoBehaviour
{
    private const float MinSpeedMultiplier = 0.7f;
    private const float MaxSpeedMultiplier = 1f;
    private const float BaseMovementInfluence = 0.15f;
    private const float MaxMovementInfluenceBonus = 0.15f;

    [SerializeField] private Rigidbody2D _rigidbody;

    private Collider2D _ballCollider;
    private float _baseSpeed;
    private float _targetSpeed;
    private float _speedMultiplier = 1f;
    private float _attackSpeedMultiplier = 1f;

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
        _attackSpeedMultiplier = 1f;
        _targetSpeed = CalculateTargetSpeed();
        _rigidbody.linearVelocity = velocity.normalized * _targetSpeed;
    }

    public void StopAtCenter(Vector2 center)
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        _rigidbody.position = center;
        _attackSpeedMultiplier = 1f;
        _targetSpeed = CalculateTargetSpeed();
    }

    /// <summary>
    /// Applies a persistent ball-speed multiplier. Upgrade stacking can reduce
    /// the launch speed to 70% of its original value, but no further.
    /// </summary>
    public void ApplySpeedMultiplier(float multiplier)
    {
        if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier <= 0f)
            return;

        _speedMultiplier = Mathf.Clamp(_speedMultiplier * multiplier,
            MinSpeedMultiplier, MaxSpeedMultiplier);
        _targetSpeed = CalculateTargetSpeed();

        if (_rigidbody != null && _targetSpeed > 0f
            && _rigidbody.linearVelocity.sqrMagnitude > 0.0001f)
        {
            _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _targetSpeed;
        }
    }

    public bool BounceFromPaddle(Collider2D paddle, float paddleVelocityX,
        float movementInfluenceBonus = 0f)
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

        if (float.IsNaN(movementInfluenceBonus) || float.IsInfinity(movementInfluenceBonus))
            movementInfluenceBonus = 0f;

        float movementInfluence = BaseMovementInfluence
            + Mathf.Clamp(movementInfluenceBonus, 0f, MaxMovementInfluenceBonus);
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

        _attackSpeedMultiplier = 1f + 0.1f * attackQuality;
        _targetSpeed = CalculateTargetSpeed();
        _rigidbody.linearVelocity = direction * _targetSpeed;
        return attackQuality >= 0.7f;
    }

    private float CalculateTargetSpeed()
    {
        return _baseSpeed * _speedMultiplier * _attackSpeedMultiplier;
    }
}
