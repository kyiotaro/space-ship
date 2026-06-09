using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    public GameObject bulletPrefab;
    private GameObject player;
    public int segment = 10;
    public float space = 1.6f;
    public float start = 1f;
    public bool IsMovementEnabled = true;
    public bool IsShootingEnabled = true;
    public float speed;
    public float rotationSpeed = 200f;
    private Quaternion targetRotation;
    private Renderer rend;
    private Vector2 velocity;
    public float thrustForce = 7f;
    public float damping = 0.99f;
    public float maxSpeed = 8f;
    private float shootTimer = 0.3f;
    private float nextShootTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rend = GetComponent<Renderer>();
        speed = 4f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!rend.isVisible)
        {
            IsShootingEnabled = false;
        }
        else
        {
            IsMovementEnabled = true;
            IsShootingEnabled = true;
        }
        if (IsShootingEnabled)
        {
            if (transform.rotation == targetRotation && Time.time >= nextShootTime)
            {
                Shoot();
                nextShootTime = Time.time + shootTimer;
            }
        }

        if (IsMovementEnabled == true)
        {
            Movement();
        }
    }

    void Shoot()
    {   
        Instantiate(bulletPrefab, transform.position, transform.rotation);
    }

    void EnableMovement()
    {
        IsMovementEnabled = true;
    }

    void Movement()
    {   
        //face the player
        Vector3 direction = player.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);


        //move towards the player
        Vector2 forward = transform.up;
        
        velocity += forward * thrustForce * Time.deltaTime;
        velocity *= damping;

        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
        
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(gameObject);
            Debug.Log("Hit!");
        }
     }
}
