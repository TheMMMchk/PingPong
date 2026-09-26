using TMPro;
using UnityEngine;

public class HealthView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;

    public void UpdateHealthText(int hp) => healthText.text = $"Здоровье: {hp}";
}
