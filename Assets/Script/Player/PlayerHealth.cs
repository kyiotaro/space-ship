using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("References")]
    public Slider healthBar;
    public GameOver gameOver;

    private bool isDead;
    private HitEffect hitEffect;
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        if (playerStats == null)
        {
            Debug.LogError("PlayerHealth: PlayerStats.Instance is null!");
            enabled = false;
            return;
        }

        hitEffect = GetComponent<HitEffect>();

        // Subscribe to events
        playerStats.OnHealthChanged += UpdateHealthBar;
        playerStats.OnDied += HandleDeath;
        if (LevelSystem.instance != null)
            LevelSystem.instance.OnLevelUp += HandleLevelUp;

        UpdateHealthBar();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnDied -= HandleDeath;
        }

        if (LevelSystem.instance != null)
            LevelSystem.instance.OnLevelUp -= HandleLevelUp;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null && playerStats != null)
        {
            healthBar.maxValue = playerStats.MaxHealth;
            healthBar.value = playerStats.Health;
        }

        // Play hit effect whenever health changes (damage taken)
        if (hitEffect != null && playerStats != null && playerStats.Health > 0f)
        {
            hitEffect.Play();
        }
    }

    private void HandleDeath()
    {
        if (!isDead)
        {
            isDead = true;
            gameOver?.setup(true);
        }
    }

    private void HandleLevelUp(int newLevel)
    {
        playerStats?.Heal(playerStats.MaxHealth * 0.25f);
    }

}
