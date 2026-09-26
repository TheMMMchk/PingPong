using TMPro;
using UnityEngine;

public class ScoreView : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public void UpdateText(int score) => scoreText.text = $"Счёт: {score}";
}

