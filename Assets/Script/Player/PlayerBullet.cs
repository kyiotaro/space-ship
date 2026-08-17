using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    private float damage = 10f;

    public void SetDamage(float amount)
    {
        damage = amount;
    }

    void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Deal damage to enemy
            if (collision.TryGetComponent<EnemyHit>(out var enemy))
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
