using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthBar;

    public float maxHealth;
    public float currentHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
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
            if (currentHealth <= 0)
            {
                Debug.Log("Game Over!");
            }
        }
    }
}
