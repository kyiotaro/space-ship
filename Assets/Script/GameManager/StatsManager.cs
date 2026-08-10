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
            playerStats = GetComponent<PlayerStats>();
        }
    }

    private void Update()
    {
        if (playerStats == null) return;

        if (HealthText != null)
        {
            HealthText.text = "Health: " + playerStats.Health.ToString("F0") + "/" + playerStats.MaxHealth.ToString("F0");
        }

        if (AttackText != null)
        {
            AttackText.text = "Attack: " + playerStats.Attack.ToString("F0");
        }

        if (DefenseText != null)
        {
            DefenseText.text = "Defense: " + playerStats.Defense.ToString("F0");
        }

        if (SpeedText != null)
        {
            SpeedText.text = "Speed: " + playerStats.Speed.ToString("F0");
        }
    }
}
