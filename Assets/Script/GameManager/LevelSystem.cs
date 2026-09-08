using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem instance;

    public Slider expSlider;
    public TMP_Text levelText;
    public int baseExpRequired = 100;
    public float expCurveMultiplier = 1.5f;
    [Range(0f, 1f)] public float levelUpHealPercent = 0.25f;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int expToNextLevel = 100;
    [SerializeField] private int upgradePoints = 0;

    // Events you can hook into from other scripts
    public event Action<int> OnLevelUp;      // Fires with new level
    public event Action<int, int> OnExpChanged; // Fires with (currentExp, expToNextLevel)
    public event Action<int> OnUpgradePointsChanged;

    private bool isPrimary;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            isPrimary = true;
        }
    }

    private void Start()
    {
        if (!isPrimary)
        {
            if (instance != null)
            {
                instance.OnExpChanged += MirrorPrimaryUI;
                MirrorPrimaryUI(instance.currentExp, instance.expToNextLevel);
            }
            return;
        }

        if (expSlider == null)
            Debug.LogError("[LevelSystem] EXP Slider is not assigned!", this);

        if (levelText == null)
            Debug.LogError("[LevelSystem] Level Text (TMP) is not assigned!", this);

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (!isPrimary && instance != null)
            instance.OnExpChanged -= MirrorPrimaryUI;

        if (instance == this)
            instance = null;
    }

    /// <summary>
    /// Call this when the player earns EXP.
    /// </summary>
    public void AddExp(int amount)
    {
        if (!isPrimary)
        {
            instance?.AddExp(amount);
            return;
        }

        if (amount <= 0) return;

        currentExp += amount;

        // Handle multiple level-ups in one go
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel);
        UpdateUI();
    }

    /// <summary>
    /// Sets current EXP directly (e.g. loading saved data).
    /// </summary>
    public void SetExp(int exp)
    {
        if (!isPrimary)
        {
            instance?.SetExp(exp);
            return;
        }

        currentExp = Mathf.Max(0, exp);

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel);
        UpdateUI();
    }

    /// <summary>
    /// Sets level directly (e.g. loading saved data).
    /// </summary>
    public void SetLevel(int level)
    {
        if (!isPrimary)
        {
            instance?.SetLevel(level);
            return;
        }

        currentLevel = Mathf.Max(1, level);
        RecalculateExpToNextLevel();
        OnExpChanged?.Invoke(currentExp, expToNextLevel);
        UpdateUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        upgradePoints++;
        RecalculateExpToNextLevel();
        PlayerStats.Instance?.Heal(PlayerStats.Instance.MaxHealth * levelUpHealPercent);
        OnLevelUp?.Invoke(currentLevel);
        OnUpgradePointsChanged?.Invoke(upgradePoints);
    }

    private void RecalculateExpToNextLevel()
    {
        // EXP formula: base * multiplier^(level-1)
        expToNextLevel = Mathf.RoundToInt(baseExpRequired * Mathf.Pow(expCurveMultiplier, currentLevel - 1));
    }

    private void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Lv. {currentLevel}";

        if (expSlider != null)
        {
            expSlider.maxValue = expToNextLevel;
            expSlider.value = currentExp;
        }
    }

    private void MirrorPrimaryUI(int exp, int expRequired)
    {
        if (instance == null) return;

        if (levelText != null)
            levelText.text = $"Lv. {instance.currentLevel}";

        if (expSlider != null)
        {
            expSlider.maxValue = expRequired;
            expSlider.value = exp;
        }
    }

    // --- Getters for external scripts ---

    public int GetCurrentLevel() => isPrimary ? currentLevel : instance != null ? instance.currentLevel : currentLevel;
    public int GetCurrentExp() => isPrimary ? currentExp : instance != null ? instance.currentExp : currentExp;
    public int GetExpToNextLevel() => isPrimary ? expToNextLevel : instance != null ? instance.expToNextLevel : expToNextLevel;
    public int GetUpgradePoints() => isPrimary ? upgradePoints : instance != null ? instance.upgradePoints : upgradePoints;
    public float GetExpPercent()
    {
        int required = GetExpToNextLevel();
        return required > 0 ? (float)GetCurrentExp() / required : 0f;
    }

    public bool SpendUpgradePoint()
    {
        if (!isPrimary)
            return instance != null && instance.SpendUpgradePoint();

        if (upgradePoints <= 0) return false;

        upgradePoints--;
        OnUpgradePointsChanged?.Invoke(upgradePoints);
        return true;
    }
}
