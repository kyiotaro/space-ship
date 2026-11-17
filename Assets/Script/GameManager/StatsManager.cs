using UnityEngine;
using UnityEngine.UIElements;

public class StatsManager : MonoBehaviour
{
    private PlayerStats playerStats;
    private PlayerController playerController;
    private Label healthLabel;
    private Label attackLabel;
    private Label defenseLabel;
    private Label speedLabel;
    private Label ammoLabel;

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

        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
            playerController.OnAmmoChanged += UpdateAmmoUI;

        UIDocument document = FindFirstObjectByType<UIDocument>();
        if (document == null)
        {
            Debug.LogError("[StatsManager] No UI Toolkit document was found.", this);
            enabled = false;
            return;
        }

        VisualElement root = document.rootVisualElement;
        healthLabel = root.Q<Label>("stats-health");
        attackLabel = root.Q<Label>("stats-attack");
        defenseLabel = root.Q<Label>("stats-defense");
        speedLabel = root.Q<Label>("stats-speed");
        ammoLabel = root.Q<Label>("ammo-value");
        if (healthLabel == null || attackLabel == null || defenseLabel == null || speedLabel == null || ammoLabel == null)
        {
            Debug.LogError("[StatsManager] The UI Toolkit document is missing one or more stats labels.", this);
            enabled = false;
            return;
        }

        UpdateHealthUI();
        UpdateStatsUI();
        UpdateAmmoUI();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthUI;
            playerStats.OnStatsChanged -= UpdateStatsUI;
        }

        if (playerController != null)
            playerController.OnAmmoChanged -= UpdateAmmoUI;
    }

    private void UpdateHealthUI()
    {
        if (healthLabel != null && playerStats != null)
            healthLabel.text = $"{playerStats.Health:F0} / {playerStats.MaxHealth:F0}";
    }

    private void UpdateStatsUI()
    {
        if (playerStats == null) return;

        if (attackLabel != null)
            attackLabel.text = playerStats.Attack.ToString("F0");

        if (defenseLabel != null)
            defenseLabel.text = playerStats.Defense.ToString("F0");

        if (speedLabel != null)
            speedLabel.text = playerStats.TopSpeed.ToString("F0");
    }

    private void UpdateAmmoUI()
    {
        if (ammoLabel != null && playerController != null)
            ammoLabel.text = $"AMMO  {playerController.Ammo} / {playerController.maxAmmo}";
    }
}
