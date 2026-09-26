using UnityEngine;

public class BotMoveService : IMoveable
{
    public float GetAxisX() => Input.GetAxisRaw("Vertical");
}

public class PlayerMoveService : IMoveable
{
    public float GetAxisX() => Input.GetAxisRaw("Horizontal");
}