using UnityEngine;

public class ScoutShooter : MonoBehaviour
{
    [Header("Scout Weapon")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float fireRate = 0.8f;
    [SerializeField] private float projectileSpeed = 28f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float aimTolerance = 10f;
    [SerializeField] private float reactionTime = 0.2f;
    [SerializeField] private float aimError = 5f;

    private EnemyCore core;
    private float nextShotTime;
    private float aimTime;

    private void Awake()
    {
        core = GetComponent<EnemyCore>();
    }

    private void Update()
    {
        if (core == null || !core.CanShoot || core.Target == null || bulletPrefab == null)
        {
            aimTime = 0f;
            return;
        }

        Vector2 directionToTarget = core.Target.position - transform.position;
        float angleToTarget = Vector2.Angle(transform.up, directionToTarget);

        if (angleToTarget > aimTolerance)
        {
            aimTime = 0f;
            return;
        }

        aimTime += Time.deltaTime;

        if (aimTime < reactionTime || Time.time < nextShotTime)
        {
            return;
        }

        Transform firingPoint = muzzle != null ? muzzle : transform;
        Quaternion inaccurateRotation = firingPoint.rotation * Quaternion.Euler(
            0f,
            0f,
            Random.Range(-aimError, aimError));

        GameObject spawnedBullet = Instantiate(bulletPrefab, firingPoint.position, inaccurateRotation);
        EnemyBullet enemyBullet = spawnedBullet.GetComponent<EnemyBullet>();
        if (enemyBullet != null)
        {
            enemyBullet.SetMoveSpeed(projectileSpeed * Score.DifficultyMultiplier);
            enemyBullet.SetDamage(projectileDamage * Score.DifficultyMultiplier);
        }

        nextShotTime = Time.time + (1f / Mathf.Max(0.1f, fireRate));
        aimTime = 0f;
    }
}
