using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    public GameObject laserPrefab;
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
    private float shootTimer;
    private Vector2 velocity;
    public float thrustForce = 7f;
    public float damping = 0.99f;
    public float maxSpeed = 8f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rend = GetComponent<Renderer>();
        speed = 4f;
        shootTimer = Random.Range(3f, 5f);
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
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                Laser();
                shootTimer = Random.Range(3f, 5f);
            }
        }

        if (IsMovementEnabled == true)
        {
            Movement();
        }
    }

    void Laser()
    {   
        IsMovementEnabled = false;
        for (int i = 0; i < segment; i++)
        {
            Vector3 pos = transform.position + transform.up * (start +(i * space));
            pos.z = 0.1f;
            Instantiate(laserPrefab, pos, transform.rotation);
        }
        Invoke("DestroyLaser", 0.2f);
        Invoke("EnableMovement", 0.3f); // Delay enabling movement slightly
    }

    void EnableMovement()
    {
        IsMovementEnabled = true;
    }

    void DestroyLaser()
    {
        GameObject[] lassers = GameObject.FindGameObjectsWithTag("Lasser");
        foreach (GameObject lasser in lassers)
        {
            Destroy(lasser);
        }
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
}
