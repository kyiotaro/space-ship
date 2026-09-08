using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shotsPerSecond = 1.5f;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float aimTolerance = 12f;
    [SerializeField] private float aimReactionTime = 0.4f;
    [SerializeField] private float aimError = 8f;

    private EnemyCore core;
    private float nextShotTime;
    private float targetInAimTime;

    private void Awake()
    {
        core = GetComponent<EnemyCore>();
    }

    public void SetBulletPrefab(GameObject prefab)
    {
        bulletPrefab = prefab;
    }

    private void Update()
    {
        if (core == null || !core.CanShoot || core.Target == null || bulletPrefab == null)
        {
            targetInAimTime = 0f;
            return;
        }

        Vector2 directionToTarget = core.Target.position - transform.position;
        float angleToTarget = Vector2.Angle(transform.up, directionToTarget);

        if (angleToTarget > aimTolerance)
        {
            targetInAimTime = 0f;
            return;
        }

        targetInAimTime += Time.deltaTime;

        if (targetInAimTime < aimReactionTime || Time.time < nextShotTime)
        {
            return;
        }

        Transform firingPoint = muzzle != null ? muzzle : transform;
        Quaternion inaccurateRotation = firingPoint.rotation * Quaternion.Euler(
            0f,
            0f,
            Random.Range(-aimError, aimError));

        Instantiate(bulletPrefab, firingPoint.position, inaccurateRotation);
        nextShotTime = Time.time + GetShotInterval();
        targetInAimTime = 0f;
    }

    private float GetShotInterval()
    {
        return shotsPerSecond > 0f ? 1f / shotsPerSecond : float.PositiveInfinity;
    }
}
