using UnityEngine;

public class MoveController
{
    private MoveModel _model;
    private MoveView _view;

    private IMoveable _moveable;

    public MoveController(MoveModel model, MoveView view, IMoveable moveable)
    {
        _model = model;
        _view = view;
        _moveable = moveable;
    }

    public void Tick(float dt)
    {
        float dirX = _moveable.GetAxisX();
        _model.ChangePos(dirX, dt);
        _view.ChangePos(_model.Pos, dt);
    }
}