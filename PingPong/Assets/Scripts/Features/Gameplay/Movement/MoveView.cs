using UnityEngine;

public class MoveView : MonoBehaviour
{
    [SerializeField] private Transform trPos;
    public void ChangePos(float x)
    {
        Vector2 pos = trPos.position;
        pos.x = x;
        trPos.position = pos;
    } 
}
