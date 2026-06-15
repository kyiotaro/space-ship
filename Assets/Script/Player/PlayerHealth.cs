using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthBar;
    public GameOver gameOver;
    public float maxHealth;
    public float currentHealth;
    private float respawnTime = 3f;
    private float respawnTimer;

    private HitEffect hitEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        hitEffect = GetComponent<HitEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = currentHealth;
        healthBar.maxValue = maxHealth;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.gameObject.CompareTag("Enemy_Bullet"))
        {
            currentHealth -= 10f;
            
            if (hitEffect != null)
            {
                hitEffect.Play();
            }

            if (currentHealth <= 0)
            {
                gameOver.setup(true);
                respawnTimer += Time.deltaTime;
                if (respawnTimer >= respawnTime)
                {
                    respawnTimer = 0f;
                }
            }
        }
    }
}
