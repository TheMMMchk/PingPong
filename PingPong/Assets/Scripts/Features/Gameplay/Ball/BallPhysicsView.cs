using UnityEngine;

public class BallPhysicsView : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;

    public void Launch(Vector2 velocity)
    {
        _rigidbody.linearVelocity = velocity;
    }
}
