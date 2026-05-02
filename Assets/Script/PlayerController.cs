using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public GameObject projectilePrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        movement();
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
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("Shot");
            Instantiate(projectilePrefab, transform.position, transform.rotation);
        }
    }
}
