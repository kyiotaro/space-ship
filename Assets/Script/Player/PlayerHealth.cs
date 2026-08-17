using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("References")]
    public Slider healthBar;
    public GameOver gameOver;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 3f;

    private float respawnTimer;
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

        UpdateHealthBar();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnDied -= HandleDeath;
        }
    }

    private void Update()
    {
        if (isDead)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                Respawn();
            }
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

    private void HandleDeath()
    {
        if (!isDead)
        {
            isDead = true;
            gameOver?.setup(true);
        }
    }

    private void Respawn()
    {
        respawnTimer = 0f;
        isDead = false;
        playerStats?.ResetStats();
        gameOver?.setup(false);
    }
}
