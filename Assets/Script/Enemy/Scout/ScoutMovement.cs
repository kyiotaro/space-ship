using UnityEngine;

public class ScoutMovement : MonoBehaviour
{
    [Header("Scout Behavior")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float desiredRange = 8f;
    [SerializeField] private float stopDistance = 6f;
    [SerializeField] private float strafeStrength = 0.8f;

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

        Vector2 moveDirection = Vector2.zero;

        if (distance > desiredRange)
        {
            moveDirection = transform.up;
        }
        else if (distance < stopDistance)
        {
            moveDirection = -transform.up;
        }
        else
        {
            moveDirection = Vector2.zero;
        }

        float strafe = Mathf.Sin(Time.time * 2.5f) * strafeStrength;
        Vector2 desiredVelocity = moveDirection * moveSpeed + (Vector2)transform.right * strafe;
        velocity = Vector2.Lerp(velocity, desiredVelocity, 0.12f);

        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}
