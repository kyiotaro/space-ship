using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1f)] private float maxHealth = 30f;
    [SerializeField, Min(0f)] private float defense = 0f;
    [SerializeField, Min(0)] private int expValue = 20;
    [SerializeField, Min(0)] private int scoreValue = 20;

    [Header("Death")]
    [Tooltip("Optional animation played before this enemy is destroyed.")]
    [SerializeField] private AnimatedSprite deathAnimation;

    private float currentHealth;
    private bool isDead;

    private AudioManager audioManager;
    private HitEffect hitEffect;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        hitEffect = GetComponent<HitEffect>();

        if (deathAnimation == null)
        {
            deathAnimation = GetComponent<AnimatedSprite>();
        }
    }

    private void Start()
    {
        ResetHealth();
    }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth * Score.DifficultyMultiplier;
    public float Defense => defense * Score.DifficultyMultiplier;
    public bool IsDead => isDead;

    public void ResetHealth()
    {
        currentHealth = MaxHealth;
        isDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            return;
        }

        float damage = Mathf.Max(0f, amount - Defense);
        if (audioManager != null)
            audioManager.playSFX(audioManager.HitSFX);
        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (hitEffect != null)
        {
            hitEffect.Play();
        }

        if (currentHealth <= 0f)
        { 
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        if (audioManager != null)
            audioManager.playSFX(audioManager.DieSFX);
        LevelSystem.instance?.AddExp(expValue);
        Score.instance?.AddScore(scoreValue);

        if (deathAnimation != null)
        {
            Collider2D[] enemyColliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D enemyCollider in enemyColliders)
            {
                enemyCollider.enabled = false;
            }

            EnemyCore enemyCore = GetComponent<EnemyCore>();
            if (enemyCore != null)
            {
                enemyCore.enabled = false;
            }

            deathAnimation.PlayOnce();
            return;
        }

        Destroy(gameObject);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        defense = Mathf.Max(0f, defense);
        expValue = Mathf.Max(0, expValue);
        scoreValue = Mathf.Max(0, scoreValue);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }
}
