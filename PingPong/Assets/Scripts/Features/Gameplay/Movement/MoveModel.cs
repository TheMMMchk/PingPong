using UnityEngine;

public class MoveModel
{
    private float _speed;
    private float _dir;
    private float _pos;

    public float Speed
    {
        get => _speed;
        private set => _speed = Mathf.Clamp(value, 0,5);

    }

    public float Pos
    {
        get => _pos;
        private set => _pos = Mathf.Clamp(value, -9, 9);
    }
    public MoveModel(float speed)
    {
        Speed = speed;
    }

    public void ChangePos(float dir, float dt) => Pos = Speed * dir * dt;
}
