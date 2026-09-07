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

    [Header("Upgrade Amounts")]
    [SerializeField] private float attackUpgrade = 2f;
    [SerializeField] private float defenseUpgrade = 1f;
    [SerializeField] private float topSpeedUpgrade = 0.5f;
    [SerializeField] private float maxHealthUpgrade = 20f;

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
            if (health <= 0f)
                Debug.LogWarning($"[PlayerStats] HP reached 0. Previous: {previous}, Current: {health}", this);
            if (!Mathf.Approximately(previous, health))
            {
                OnHealthChanged?.Invoke();
                if (previous > health)
                    OnStatsChanged?.Invoke(); // took damage
            }
            if (health <= 0f && previous > 0f)
            {
                Debug.LogWarning("[PlayerStats] OnDied event invoked.", this);
                OnDied?.Invoke();
            }
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
            Debug.LogWarning("[PlayerStats] Duplicate PlayerStats found. Removing only the duplicate component.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        Health = maxHealth;
        Debug.Log($"[PlayerStats] Initialized on {gameObject.name} with {Health}/{MaxHealth} HP.", this);
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

    public void UpgradeAttack()
    {
        if (!TrySpendUpgradePoint()) return;

        Attack += attackUpgrade;
    }

    public void UpgradeDefense()
    {
        if (!TrySpendUpgradePoint()) return;

        Defense += defenseUpgrade;
    }

    public void UpgradeTopSpeed()
    {
        if (!TrySpendUpgradePoint()) return;

        TopSpeed += topSpeedUpgrade;
    }

    public void UpgradeMaxHealth()
    {
        if (!TrySpendUpgradePoint()) return;

        maxHealth += maxHealthUpgrade;
        Health += maxHealthUpgrade;
        OnStatsChanged?.Invoke();
    }

    private bool TrySpendUpgradePoint()
    {
        return LevelSystem.instance != null && LevelSystem.instance.SpendUpgradePoint();
    }
}
