using TMPro;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI DefenseText;
    public TextMeshProUGUI SpeedText;

    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        if (playerStats == null)
        {
            Debug.LogError("StatsManager: PlayerStats.Instance is null!");
            enabled = false;
            return;
        }

        playerStats.OnHealthChanged += UpdateHealthUI;
        playerStats.OnStatsChanged += UpdateStatsUI;

        // Initial update
        UpdateHealthUI();
        UpdateStatsUI();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthUI;
            playerStats.OnStatsChanged -= UpdateStatsUI;
        }
    }

    private void UpdateHealthUI()
    {
        if (HealthText != null && playerStats != null)
        {
            HealthText.text = $"Health: {playerStats.Health:F0}/{playerStats.MaxHealth:F0}";
        }
    }

    private void UpdateStatsUI()
    {
        if (playerStats == null) return;

        if (AttackText != null)
            AttackText.text = $"Attack: {playerStats.Attack:F0}";

        if (DefenseText != null)
            DefenseText.text = $"Defense: {playerStats.Defense:F0}";

        if (SpeedText != null)
            SpeedText.text = $"Speed: {playerStats.TopSpeed:F0}";
    }
}
