using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    AudioManager AudioManager;
    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private int expValue = 20;
    [SerializeField] private AnimatedSprite deathAnimation;
    private float currentHealth;
    private HitEffect hitEffect;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
        hitEffect = GetComponent<HitEffect>();

        if (deathAnimation == null)
        {
            deathAnimation = GetComponent<AnimatedSprite>();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            return;
        }

        AudioManager.playSFX(AudioManager.HitSFX);
        currentHealth -= amount;

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
        AudioManager.playSFX(AudioManager.DieSFX);
        LevelSystem.instance?.AddExp(expValue);

        if (deathAnimation != null)
        {
            Collider2D enemyCollider = GetComponent<Collider2D>();
            if (enemyCollider != null)
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
}
