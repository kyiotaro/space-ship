using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelSystem : MonoBehaviour
{
    // Singleton — any script can now call LevelSystem.Instance.AddExp(25)
    public static LevelSystem Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Drag your Slider here (EXP Bar)")]
    public Slider expSlider;

    [Tooltip("Drag your TextMeshPro - Text (UI) here (Level Display)")]
    public TMP_Text levelText;

    [Header("Level Settings")]
    [Tooltip("Base EXP required for Level 1 → 2")]
    public int baseExpRequired = 100;

    [Tooltip("Multiplied each level. 1.5 = 50% more EXP needed per level")]
    public float expCurveMultiplier = 1.5f;

    [Header("Current Stats (Runtime Only)")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int expToNextLevel = 100;

    public event Action<int> OnLevelUp;
    public event Action<int, int> OnExpChanged;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (expSlider == null)
            Debug.LogError("[LevelSystem] EXP Slider is not assigned!", this);

        if (levelText == null)
            Debug.LogError("[LevelSystem] Level Text (TMP) is not assigned!", this);

        UpdateUI();
    }

    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        currentExp += amount;

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel);
        UpdateUI();
    }

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

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);
        RecalculateExpToNextLevel();
        UpdateUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        RecalculateExpToNextLevel();
        OnLevelUp?.Invoke(currentLevel);
        Debug.Log($"🎉 Level Up! You are now Level {currentLevel}");
    }

    private void RecalculateExpToNextLevel()
    {
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

    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentExp() => currentExp;
    public int GetExpToNextLevel() => expToNextLevel;
    public float GetExpPercent() => expToNextLevel > 0 ? (float)currentExp / expToNextLevel : 0f;
}
