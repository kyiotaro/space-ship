using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Core Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health = 100f;
    [SerializeField] private float attack = 10f;
    [SerializeField] private float defense = 5f;
    [SerializeField] private float topSpeed = 5f;

    // Events — anything that cares about stats subscribes here
    public event Action OnHealthChanged;
    public event Action OnDied;
    public event Action OnStatsChanged;

    public float Health
    {
        get => health;
        private set
        {
            float previous = health;
            health = Mathf.Clamp(value, 0f, maxHealth);
            if (!Mathf.Approximately(previous, health))
            {
                OnHealthChanged?.Invoke();
                if (previous > health)
                    OnStatsChanged?.Invoke(); // took damage
            }
            if (health <= 0f && previous > 0f)
                OnDied?.Invoke();
        }
    }

    public float MaxHealth => maxHealth;

    public float Attack
    {
        get => attack;
        set { attack = Mathf.Max(0f, value); OnStatsChanged?.Invoke(); }
    }

    public float Defense
    {
        get => defense;
        set { defense = Mathf.Max(0f, value); OnStatsChanged?.Invoke(); }
    }

    public float TopSpeed
    {
        get => topSpeed;
        set { topSpeed = Mathf.Max(0f, value); OnStatsChanged?.Invoke(); }
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
        float reduced = Mathf.Max(0f, amount - Defense);
        Health -= reduced;
    }

    public void Heal(float amount)
    {
        Health += Mathf.Max(0f, amount);
    }

    public void ResetStats()
    {
        Health = maxHealth;
        OnStatsChanged?.Invoke();
    }
}
