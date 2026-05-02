using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    public GameObject laserPrefab;
    public GameObject player;
    int jumlahSegmen = 10;
    float space = 1.6f;
    float start = 1f;
    bool IsMovementEnabled = true;
    float speed = 6f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IsMovementEnabled = false;
            Invoke("Laser", 0.5f);
          
        }

        if (IsMovementEnabled == true)
        {
            Movement();
        }
    }

    void Laser()
    {   
        for (int i = 0; i < jumlahSegmen; i++)
        {
            Vector3 pos = transform.position + transform.up * (start +(i * space));
            pos.z = 0.1f;
            Instantiate(laserPrefab, pos, transform.rotation);
        }
        Invoke("DestroyLaser", 0.2f);
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
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        //move towards the player
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }
}
