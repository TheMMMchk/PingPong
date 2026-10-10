using UnityEngine;

public class MoveView : MonoBehaviour
{
    [SerializeField] private Transform trPos;

    public Vector2 Position => trPos.position;
    public float VelocityX { get; private set; }

    public void ChangePos(float x, float dt)
    {
        Vector3 previousPosition = trPos.position;
        Vector3 nextPosition = previousPosition;
        nextPosition.x = x;
        trPos.position = nextPosition;

        // Measure the actual displacement after movement limits have been applied.
        VelocityX = dt > 0f
            ? (trPos.position.x - previousPosition.x) / dt
            : 0f;
    }
}