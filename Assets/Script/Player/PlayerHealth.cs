using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("References")]
    public Slider healthBar;
    public GameOver gameOver;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 3f;
    [SerializeField] private float damageAmount = 10f;

    private float respawnTimer;
    private bool isDead;
    private HitEffect hitEffect;
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.ResetStats();
        }

        hitEffect = GetComponent<HitEffect>();
    }

    private void Update()
    {
        if (healthBar != null && playerStats != null)
        {
            healthBar.maxValue = playerStats.MaxHealth;
            healthBar.value = playerStats.Health;
        }

        if (isDead)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                respawnTimer = 0f;
                isDead = false;
                if (playerStats != null)
                {
                    playerStats.ResetStats();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy_Bullet")) return;

        if (playerStats != null)
        {
            playerStats.TakeDamage(damageAmount);
        }

        if (hitEffect != null)
        {
            hitEffect.Play();
        }

        if (playerStats != null && playerStats.Health <= 0f && !isDead)
        {
            isDead = true;
            gameOver?.setup(true);
        }
    }
}
