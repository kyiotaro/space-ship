using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    private const string MaxHealthKey = "playerMaxHealth";
    private const string AttackKey = "playerAttack";
    private const string DefenseKey = "playerDefense";
    private const string TopSpeedKey = "playerTopSpeed";

    private const float DefaultMaxHealth = 100f;
    private const float DefaultAttack = 10f;
    private const float DefaultDefense = 5f;
    private const float DefaultTopSpeed = 5f;
    private int UpgradeCost;

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
            Debug.LogWarning("[PlayerStats] Duplicate PlayerStats found. Upgrade calls will use the primary instance.", this);
            return;
        }

        Instance = this;
        maxHealth = PlayerPrefs.GetFloat(MaxHealthKey, maxHealth);
        attack = PlayerPrefs.GetFloat(AttackKey, attack);
        defense = PlayerPrefs.GetFloat(DefenseKey, defense);
        topSpeed = PlayerPrefs.GetFloat(TopSpeedKey, topSpeed);
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

    [ContextMenu("Reset Stats To Defaults")]
    public void ResetStatsToDefaults()
    {
        maxHealth = DefaultMaxHealth;
        attack = DefaultAttack;
        defense = DefaultDefense;
        topSpeed = DefaultTopSpeed;
        Health = maxHealth;

        PlayerPrefs.DeleteKey(MaxHealthKey);
        PlayerPrefs.DeleteKey(AttackKey);
        PlayerPrefs.DeleteKey(DefenseKey);
        PlayerPrefs.DeleteKey(TopSpeedKey);
        PlayerPrefs.Save();

        LevelSystem levelSystem = LevelSystem.instance;
        if (levelSystem != null)
            levelSystem.ResetUpgradePoints();

        OnStatsChanged?.Invoke();
    }

    public void UpgradeAttack(float attackUpgrade)
    {
        UpgradeCost = (int)(attackUpgrade * 10); // Set the cost for upgrading attack
        if (Instance != null && Instance != this)
        {
            Instance.UpgradeAttack(attackUpgrade);
            return;
        }

        if (!TrySpendPoints(UpgradeCost)) return;

        Attack += attackUpgrade;
        SaveStats();
    }

    public void UpgradeDefense(float defenseUpgrade)
    {
        if (Instance != null && Instance != this)
        {
            Instance.UpgradeDefense(defenseUpgrade);
            return;
        }

        if (!TrySpendUpgradePoint()) return;

        Defense += defenseUpgrade;
        SaveStats();
    }

    public void UpgradeTopSpeed(float topSpeedUpgrade)
    {
        if (Instance != null && Instance != this)
        {
            Instance.UpgradeTopSpeed(topSpeedUpgrade);
            return;
        }

        if (!TrySpendUpgradePoint()) return;

        TopSpeed += topSpeedUpgrade;
        SaveStats();
    }

    public void UpgradeMaxHealth(float maxHealthUpgrade)
    {
        if (Instance != null && Instance != this)
        {
            Instance.UpgradeMaxHealth(maxHealthUpgrade);
            return;
        }

        if (!TrySpendUpgradePoint()) return;

        maxHealth += maxHealthUpgrade;
        Health += maxHealthUpgrade;
        OnStatsChanged?.Invoke();
        SaveStats();
    }

    private void SaveStats()
    {
        PlayerPrefs.SetFloat(MaxHealthKey, maxHealth);
        PlayerPrefs.SetFloat(AttackKey, attack);
        PlayerPrefs.SetFloat(DefenseKey, defense);
        PlayerPrefs.SetFloat(TopSpeedKey, topSpeed);
        PlayerPrefs.Save();
    }

    private bool TrySpendUpgradePoint()
    {
        LevelSystem levelSystem = LevelSystem.instance;
        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        if (levelSystem == null)
        {
            Debug.LogError("[PlayerStats] Cannot upgrade because no LevelSystem exists.", this);
            return false;
        }

        if (!levelSystem.SpendUpgradePoint())
        {
            Debug.LogWarning($"[PlayerStats] Cannot upgrade: no upgrade points available. Current points: {levelSystem.GetUpgradePoints()}", this);
            return false;
        }

        return true;
    }

    private bool TrySpendPoints(int points)
    {
        if (points <= 0)
        {
            Debug.LogWarning("[PlayerStats] Cannot upgrade: point cost must be greater than zero.", this);
            return false;
        }

        Point pointManager = Point.instance;
        if (pointManager == null)
            pointManager = FindFirstObjectByType<Point>();

        if (pointManager == null)
        {
            Debug.LogError("[PlayerStats] Cannot upgrade because no Point component exists.", this);
            return false;
        }

        if (!pointManager.TrySpendPoints(points))
        {
            Debug.LogWarning(
                $"[PlayerStats] Cannot upgrade: requires {points} points, but only {pointManager.Balance} are available.",
                this);
            return false;
        }

        return true;
    }
}
