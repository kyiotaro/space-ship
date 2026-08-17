using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject projectilePrefab;
    public int maxAmmo = 6;
    public float reloadTime = 2f;

    [Header("Movement Base")]
    public float thrustForce = 25f;
    public float damping = 0.995f;

    [Header("Rotation")]
    public float rotationSpeed = 200f;

    // Private variables
    private int ammo;
    private float reloadTimer;
    private Camera mainCamera;
    private Vector2 velocity;

    // Cached reference
    private PlayerStats stats;

    // Public getter for Ammo (used by PlayerSprite.cs)
    public int Ammo => ammo;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();

        stats = PlayerStats.Instance;
        ammo = maxAmmo;
    }

    void Update()
    {
        HandleAiming();
        HandleMovement();
        HandleShooting();
        HandleReload();
    }

    void HandleAiming()
    {
        if (mainCamera == null)
        {
            Debug.LogError("mainCamera NULL!");
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        Vector3 mouseInput = Input.mousePosition;
        mouseInput.z = -mainCamera.transform.position.z;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(mouseInput);
        mousePos.z = 0f;

        Vector2 direction = mousePos - transform.position;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void HandleShooting()
    {
        if (Input.GetButtonDown("Fire1") && ammo > 0)
        {
            GameObject bullet = Instantiate(projectilePrefab, transform.position, transform.rotation);

            // Pass attack damage to the bullet
            if (bullet.TryGetComponent<PlayerBullet>(out var playerBullet))
            {
                playerBullet.SetDamage(stats != null ? stats.Attack : 10f);
            }

            ammo--;
        }
    }

    void HandleReload()
    {
        reloadTimer += Time.deltaTime;
        if (reloadTimer >= reloadTime)
        {
            ammo = maxAmmo;
            reloadTimer = 0f;
        }
    }

    void HandleMovement()
    {
        Vector2 forward = transform.up;

        if (Input.GetMouseButton(1))
        {
            velocity += forward * thrustForce * Time.deltaTime;
        }
        velocity *= damping;

        if (velocity.magnitude > PlayerStats.Instance.TopSpeed)
        {
            velocity = velocity.normalized * PlayerStats.Instance.TopSpeed;
        }

        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}
