using UnityEngine;

public class MoveView : MonoBehaviour
{
    [SerializeField] private Transform trPos;

    public Vector2 Position => trPos.position;

    public void ChangePos(float x)
    {
        Vector2 pos = trPos.position;
        pos.x = x;
        trPos.position = pos;
    }
}