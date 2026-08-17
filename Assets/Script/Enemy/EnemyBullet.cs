using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float damage = 10f;

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
        if (collision.gameObject.CompareTag("Player"))
        {
            // Deal damage to player
            PlayerStats.Instance?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
