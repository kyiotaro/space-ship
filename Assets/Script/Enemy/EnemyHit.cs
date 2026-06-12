using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    public float health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.gameObject.CompareTag("Bullet"))
        {
            health -= 10f;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
