using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private int expValue = 20;
    private float currentHealth;
    private HitEffect hitEffect;

    void Start()
    {
        currentHealth = maxHealth;
        hitEffect = GetComponent<HitEffect>();
    }

    public void TakeDamage(float amount)
    {
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
        LevelSystem.instance?.AddExp(expValue);
        Score.instance?.AddScore(10);
        Destroy(gameObject);
    }
}
