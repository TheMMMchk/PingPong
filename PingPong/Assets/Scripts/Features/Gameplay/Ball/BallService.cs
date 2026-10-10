using UnityEngine;

public class BallService
{
    private readonly BallPhysicsView _view;
    private readonly float _speed;
    private readonly Vector2 _startDirection;

    public BallService(BallPhysicsView view, float speed, Vector2 startDirection)
    {
        _view = view;
        _speed = speed;
        _startDirection = startDirection;
    }

    public void Launch()
    {
        Launch(_startDirection);
    }

    public void Launch(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = _startDirection;

        _view.Launch(direction.normalized * _speed);
    }
}
