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
        Vector2 direction = _startDirection.normalized;
        _view.Launch(direction * _speed);
    }
}
