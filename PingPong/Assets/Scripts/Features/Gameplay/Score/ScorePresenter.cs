public class ScorePresenter
{
    private readonly ScoreModel _playerScoreModel;
    private readonly ScoreModel _enemyScoreModel;
    private readonly ScoreView _scoreView;

    public int PlayerScore => _playerScoreModel.Score;
    public int EnemyScore => _enemyScoreModel.Score;

    public ScorePresenter(
        ScoreModel playerScoreModel,
        ScoreModel enemyScoreModel,
        ScoreView scoreView)
    {
        _playerScoreModel = playerScoreModel;
        _enemyScoreModel = enemyScoreModel;
        _scoreView = scoreView;

        UpdateScoreText();
    }

    public void AddPlayerScore(int score)
    {
        _playerScoreModel.AddScore(score);
        UpdateScoreText();
    }

    public void AddEnemyScore(int score)
    {
        _enemyScoreModel.AddScore(score);
        UpdateScoreText();
    }

    public void ResetScores()
    {
        _playerScoreModel.ResetScore();
        _enemyScoreModel.ResetScore();
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        _scoreView.UpdateText(PlayerScore, EnemyScore);
    }
}
