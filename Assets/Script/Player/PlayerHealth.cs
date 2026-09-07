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
        Debug.Log($"[PlayerHealth] Started. HP: {playerStats.Health}/{playerStats.MaxHealth}, GameOver reference: {gameOver}", this);

        // Subscribe to events
        playerStats.OnHealthChanged += UpdateHealthBar;
        playerStats.OnDied += HandleDeath;

        UpdateHealthBar();
        if (playerStats.Health <= 0f)
            HandleDeath();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnDied -= HandleDeath;
        }

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

    private void Update()
    {
        if (!isDead && playerStats != null && playerStats.Health <= 0f)
            HandleDeath();
    }

    private void HandleDeath()
    {
        if (!isDead)
        {
            isDead = true;
            if (gameOver == null)
            {
                Debug.LogError("[PlayerHealth] Player died, but GameOver reference is not assigned!", this);
                return;
            }

            Debug.Log("[PlayerHealth] HandleDeath called. Activating GameOver.", this);
            gameOver.setup(true);
        }
    }

}
