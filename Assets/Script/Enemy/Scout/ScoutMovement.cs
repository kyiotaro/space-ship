using UnityEngine;

public class ScoutMovement : MonoBehaviour
{
    [Header("Scout Behavior")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float desiredRange = 8f;

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

        Vector3 toTarget = core.Target.position - transform.position;
        float distance = toTarget.magnitude;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f;
        Quaternion desiredRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime);

        Vector2 desiredVelocity = distance > desiredRange
            ? (Vector2)transform.up * (moveSpeed * Score.DifficultyMultiplier)
            : Vector2.zero;

        velocity = Vector2.Lerp(velocity, desiredVelocity, 0.12f);

        if (distance <= desiredRange)
        {
            velocity = Vector2.zero;
        }

        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}
