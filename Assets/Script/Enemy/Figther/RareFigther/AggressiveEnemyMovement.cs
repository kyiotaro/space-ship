using UnityEngine;

public class AggressiveEnemyMovement : MonoBehaviour
{
    [Header("Aggressive Movement")]
    [SerializeField] private float thrustForce = 25f;
    [SerializeField] private float damping = 0.99f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float rotationSpeed = 200f;

    private EnemyCore core;
    private Vector2 velocity;

    private void Awake()
    {
        core = GetComponent<EnemyCore>();
    }

    private void Update()
    {
        if (core == null || !core.CanMove || core.Target == null)
        {
            return;
        }

        Vector3 direction = core.Target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);

        velocity += (Vector2)transform.up * thrustForce * Time.deltaTime;
        velocity *= damping;

        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}
