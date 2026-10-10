
public class ScorePresenter
{
    private ScoreModel _scoreModel;

    private ScoreView _scoreView;

    public int Score => _scoreModel.Score;

    public ScorePresenter(ScoreModel scoreModel)
    {
        _scoreModel = scoreModel;
    }

    public ScorePresenter(ScoreModel scoreModel, ScoreView scoreView)
    {
        _scoreModel = scoreModel;
        _scoreView = scoreView;
    }

    public void AddScore(int score)
    {
        _scoreModel.AddScore(score);
        UpdateScoreText();
    }

    public void UpdateScoreText() => _scoreView?.UpdateText(_scoreModel.Score);
}

