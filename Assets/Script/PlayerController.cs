using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float speed;
    public GameObject projectilePrefab;
    public int ammo;
    public float reloadTime = 3f;
    
    private bool isReloading = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = 10f;
        ammo = 6;
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        Shot();
        reload();
        Debug.Log("Ammo: " + ammo);
    }

    void movement()
    {   
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
       
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        float forwardInput = Input.GetAxis("Vertical");
        transform.Translate(Vector2.up * forwardInput * speed * Time.deltaTime);
    }

    void Shot()
    {
        if(Input.GetMouseButtonDown(0) && ammo > 0 && !isReloading)
        {
            Instantiate(projectilePrefab, transform.position, transform.rotation);
            ammo--;
        }
    }

    void reload()
    {
        if(Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            isReloading = true;
            speed = 5f;
            Invoke("ReloadAmmo", reloadTime);
        }
    }

    void ReloadAmmo()
    {     
        ammo = 6;
        speed = 10f;
        isReloading = false;
    }
}
