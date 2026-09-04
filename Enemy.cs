using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    public int expValue = 25;

    [Header("Effects (Optional)")]
    public GameObject deathEffect;
    public AudioClip deathSound;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Call this when the player hits the enemy.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Optional: flash red, play hit animation, etc.
        }
    }

    private void Die()
    {
        // 1. Award EXP to the player
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.AddExp(expValue);
        }

        // 2. Optional visual/audio feedback
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // 3. Destroy the enemy
        Destroy(gameObject);
    }
}
