using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 8f;
    public GameObject projectilePrefab;
    public int ammo;
    public float reloadTime = 5f;
    public bool Reload = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        transform.position = Vector2.MoveTowards(transform.position, mousePos, speed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, angle -90f);
    }

    void Shot()
    {
        if(Input.GetMouseButtonDown(0) && ammo > 0)
        {
            Instantiate(projectilePrefab, transform.position, transform.rotation);
            ammo--;
        }
    }

    void reload()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            speed = 3f;
            Invoke("ReloadAmmo", reloadTime);
        }
    }
    void ReloadAmmo()
    {     
        ammo = 6;
        Reload = true;
        speed = 8f;
        Reload = false;
    }
}
