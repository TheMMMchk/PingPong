using UnityEngine;

public class MoveModel
{
    private const float MinUpgradeSpeed = 1f;
    private const float MaxSpeed = 5f;

    private float _speed;
    private float _dir;
    private float _pos;

    public float Speed
    {
        get => _speed;
        private set => _speed = Mathf.Clamp(value, 0f, MaxSpeed);
    }

    public float Pos
    {
        get => _pos;
        private set => _pos = Mathf.Clamp(value, -9f, 9f);
    }

    public MoveModel(float speed)
    {
        Speed = speed;
    }

    public void ChangePos(float dir, float dt) => Pos += Speed * dir * dt;

    /// <summary>
    /// Applies a speed multiplier without allowing upgrades to make movement
    /// slower than MinUpgradeSpeed or faster than MaxSpeed.
    /// </summary>
    public void ApplySpeedMultiplier(float multiplier)
    {
        if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier <= 0f)
            return;

        Speed = Mathf.Clamp(Speed * multiplier, MinUpgradeSpeed, MaxSpeed);
    }
}
