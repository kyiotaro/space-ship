using TMPro;
using UnityEngine;

public class UpgradePointsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private LevelSystem levelSystem;

    private void Start()
    {
        levelSystem = LevelSystem.instance;

        if (levelSystem == null)
        {
            Debug.LogError("[UpgradePointsUI] LevelSystem is not assigned or available.", this);
            enabled = false;
            return;
        }

        levelSystem.OnUpgradePointsChanged += UpdatePointsText;
        UpdatePointsText(levelSystem.GetUpgradePoints());
    }

    private void OnDestroy()
    {
        if (levelSystem != null)
            levelSystem.OnUpgradePointsChanged -= UpdatePointsText;
    }

    private void UpdatePointsText(int points)
    {
        if (pointsText != null)
            pointsText.text = $"Points: {points}";
    }
}
