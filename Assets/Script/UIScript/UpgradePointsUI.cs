using UnityEngine;
using UnityEngine.UIElements;

public class UpgradePointsUI : MonoBehaviour
{
    [SerializeField] private LevelSystem levelSystem;
    private Label pointsText;

    private void Start()
    {
        levelSystem = LevelSystem.instance;

        if (levelSystem == null)
        {
            Debug.LogError("[UpgradePointsUI] LevelSystem is not assigned or available.", this);
            enabled = false;
            return;
        }

        UIDocument document = FindFirstObjectByType<UIDocument>();
        if (document == null)
        {
            Debug.LogError("[UpgradePointsUI] No UI Toolkit document was found.", this);
            enabled = false;
            return;
        }

        pointsText = document.rootVisualElement.Q<Label>("upgrade-points");
        if (pointsText == null)
        {
            Debug.LogError("[UpgradePointsUI] The UI Toolkit document is missing the upgrade-points label.", this);
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
