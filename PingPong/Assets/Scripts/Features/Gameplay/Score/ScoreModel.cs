public class ScoreModel
{
    private int _score;

    public int Score
    {
        get => _score;
        private set
        {
            if (value < 0)
                return;

            _score = value;
        }
    }

    public void AddScore(int value) => Score += value;

    public void ResetScore() => Score = 0;
}
