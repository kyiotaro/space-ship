using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackRange = 2f;
    public KeyCode attackKey = KeyCode.Mouse0;

    [Header("References")]
    public Transform attackOrigin; // Usually the player camera or weapon tip
    public LayerMask enemyLayer;

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        // Raycast from attack origin forward
        if (Physics.Raycast(attackOrigin.position, attackOrigin.forward, out RaycastHit hit, attackRange, enemyLayer))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log($"Hit enemy for {attackDamage} damage!");
            }
        }
    }
}
