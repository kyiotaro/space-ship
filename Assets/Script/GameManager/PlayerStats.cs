using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Core Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health = 100f;
    [SerializeField] private float attack = 10f;
    [SerializeField] private float defense = 5f;
    [SerializeField] private float speed = 5f;

    public float Health
    {
        get => health;
        private set => health = Mathf.Clamp(value, 0f, maxHealth);
    }

    public float MaxHealth => maxHealth;

    public float Attack
    {
        get => attack;
        set => attack = Mathf.Max(0f, value);
    }

    public float Defense
    {
        get => defense;
        set => defense = Mathf.Max(0f, value);
    }

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        Health -= Mathf.Max(0f, amount);
    }

    public void Heal(float amount)
    {
        Health += Mathf.Max(0f, amount);
    }

    public void ResetStats()
    {
        Health = maxHealth;
    }
}
