using System;
using UnityEngine;

public class BallHitComponent : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private Vector2 startDirection = new Vector2(1f, 0.35f);

    private Rigidbody2D _rigidbody;
    private PhysicsMaterial2D _bounceMaterial;

    public Action OnBallHit;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        ConfigurePhysics();
        Launch();
    }

    private void ConfigurePhysics()
    {
        _rigidbody.gravityScale = 0f;
        _rigidbody.linearDamping = 0f;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        _bounceMaterial = new PhysicsMaterial2D("BallBounce")
        {
            friction = 0f,
            bounciness = 1f,
            frictionCombine = PhysicsMaterialCombine2D.Minimum,
            bounceCombine = PhysicsMaterialCombine2D.Maximum
        };

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        collider.sharedMaterial = _bounceMaterial;
    }

    private void Launch()
    {
        Vector2 direction = startDirection.normalized;
        _rigidbody.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            OnBallHit?.Invoke();
    }

    private void OnDestroy()
    {
        if (_bounceMaterial != null)
            Destroy(_bounceMaterial);
    }
}

public class BallHitService : IDisposable
{
    private readonly BallHitComponent _hitComponent;

    public BallHitService(BallHitComponent hitComponent)
    {
        _hitComponent = hitComponent;
        _hitComponent.OnBallHit += Hit;
    }

    public void Dispose()
    {
        _hitComponent.OnBallHit -= Hit;
    }

    public void Hit()
    {
        Debug.Log("мячик отскочил");
    }
}
