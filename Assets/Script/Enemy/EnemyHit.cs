using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    public float maxhealth;
    public float currenthealth;
    private HitEffect hitEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currenthealth = maxhealth;
        hitEffect = GetComponent<HitEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.gameObject.CompareTag("Player_Bullet"))
        {
            currenthealth -= 10f;
            
            if (hitEffect != null)
            {
                hitEffect.Play();
            }

            if (currenthealth <= 0)
            {
                Score.instance.AddScore(10);
                Destroy(gameObject);
            }
        }
    }
}
