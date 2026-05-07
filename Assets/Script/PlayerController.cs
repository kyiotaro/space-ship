using UnityEngine;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public int maxAmmo = 6;
    public float speed = 10f;
    public float reloadTime = 3f;
    public int Ammo => ammo;

    private int ammo;
    private bool isReloading;
    private float reloadTimer;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();

        ammo = maxAmmo;
    }

    void Update()
    {
        HandleAiming();
        HandleShooting();
        HandleReload();
        HandleMovement();
    }

    void HandleAiming()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                return;
        }

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    void HandleShooting()
    {
        if (isReloading) return;

        if (Input.GetButtonDown("Fire1") && ammo > 0)
        {
            Instantiate(projectilePrefab, transform.position, transform.rotation);
            ammo--;
        }
    }

    void HandleReload()
    {
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                ammo = maxAmmo;
                isReloading = false;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && ammo < maxAmmo)
        {
            isReloading = true;
            reloadTimer = reloadTime;
        }
    }

    void HandleMovement()
    {
        if(Input.GetMouseButton(1))
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }    
    }
}
