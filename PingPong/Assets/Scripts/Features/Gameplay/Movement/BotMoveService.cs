using UnityEngine;

public class BotMoveService : IMoveable
{
    private const float VerticalVelocityThreshold = 0.05f;
    private const float HorizontalDeadZone = 0.25f;

    private readonly BallPhysicsView _ball;
    private readonly MoveView _enemyPaddle;
    private readonly Collider2D _leftWall;
    private readonly Collider2D _rightWall;

    public BotMoveService(
        BallPhysicsView ball,
        MoveView enemyPaddle,
        Collider2D leftWall,
        Collider2D rightWall)
    {
        _ball = ball;
        _enemyPaddle = enemyPaddle;
        _leftWall = leftWall;
        _rightWall = rightWall;
    }

    public float GetAxisX()
    {
        Vector2 ballPosition = _ball.Position;
        Vector2 ballVelocity = _ball.Velocity;

        float targetX = GetFieldCenter();

        // The enemy only predicts the landing point when the ball is coming toward it.
        if (ballVelocity.y > VerticalVelocityThreshold &&
            ballPosition.y < _enemyPaddle.Position.y)
        {
            targetX = PredictLandingX(ballPosition, ballVelocity);
        }

        float difference = targetX - _enemyPaddle.Position.x;

        if (Mathf.Abs(difference) <= HorizontalDeadZone)
            return 0f;

        return Mathf.Sign(difference);
    }

    private float PredictLandingX(Vector2 ballPosition, Vector2 ballVelocity)
    {
        float targetY = _enemyPaddle.Position.y;
        float timeToPaddle = (targetY - ballPosition.y) / ballVelocity.y;
        float projectedX = ballPosition.x + ballVelocity.x * timeToPaddle;

        // Use the existing wall colliders instead of hard-coded field coordinates.
        float minX = _leftWall.bounds.max.x + _ball.Radius;
        float maxX = _rightWall.bounds.min.x - _ball.Radius;

        if (maxX <= minX)
            return ballPosition.x;

        // Reflect the projected position as many times as needed at the side walls.
        float fieldWidth = maxX - minX;
        float reflectedOffset = Mathf.Repeat(projectedX - minX, fieldWidth * 2f);

        if (reflectedOffset > fieldWidth)
            reflectedOffset = fieldWidth * 2f - reflectedOffset;

        return minX + reflectedOffset;
    }

    private float GetFieldCenter()
    {
        float minX = _leftWall.bounds.max.x + _ball.Radius;
        float maxX = _rightWall.bounds.min.x - _ball.Radius;
        return (minX + maxX) * 0.5f;
    }
}

public class PlayerMoveService : IMoveable
{
    public float GetAxisX() => Input.GetAxisRaw("Horizontal");
}