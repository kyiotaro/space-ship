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

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (expSlider == null)
            Debug.LogError("[LevelSystem] EXP Slider is not assigned!", this);

        if (levelText == null)
            Debug.LogError("[LevelSystem] Level Text (TMP) is not assigned!", this);

        UpdateUI();
    }

    /// <summary>
    /// Call this when the player earns EXP.
    /// </summary>
    public void AddExp(int amount)
    {
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
        currentLevel = Mathf.Max(1, level);
        RecalculateExpToNextLevel();
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

    // --- Getters for external scripts ---

    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentExp() => currentExp;
    public int GetExpToNextLevel() => expToNextLevel;
    public int GetUpgradePoints() => upgradePoints;
    public float GetExpPercent() => expToNextLevel > 0 ? (float)currentExp / expToNextLevel : 0f;

    public bool SpendUpgradePoint()
    {
        if (upgradePoints <= 0) return false;

        upgradePoints--;
        OnUpgradePointsChanged?.Invoke(upgradePoints);
        return true;
    }
}
