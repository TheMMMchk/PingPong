using TMPro;
using UnityEngine;

public class ScoreView : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public void UpdateText(int playerScore, int enemyScore)
    {
        if (scoreText == null)
            return;

        scoreText.text = $"Игрок: {playerScore} | Бот: {enemyScore}";
    }
}
