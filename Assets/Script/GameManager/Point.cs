using UnityEngine;
using UnityEngine.UIElements;

public class Point : MonoBehaviour
{
    public static Point instance { get; private set; }

    [SerializeField, Min(0)] private int balance;
    private Label pointsText;
    private Label statsCreditsText;
    private Label shopCreditsText;

    public int Balance => balance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("[Point] Multiple Point managers found. Disabling duplicate.", this);
            enabled = false;
            return;
        }

        instance = this;
    }

    private void Start()
    {
        UIDocument document = FindFirstObjectByType<UIDocument>();
        if (document == null)
        {
            Debug.LogError("[Point] No UI Toolkit document was found.", this);
            return;
        }

        pointsText = document.rootVisualElement.Q<Label>("points-value");
        statsCreditsText = document.rootVisualElement.Q<Label>("stats-credits");
        shopCreditsText = document.rootVisualElement.Q<Label>("shop-credits");
        if (pointsText == null || statsCreditsText == null || shopCreditsText == null)
        {
            Debug.LogError("[Point] The UI Toolkit document is missing a credits label.", this);
            return;
        }

        UpdatePointsUI();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0)
            return;

        balance += amount;
        UpdatePointsUI();
    }

    public bool TrySpendPoints(int amount)
    {
        if (amount <= 0 || balance < amount)
            return false;

        balance -= amount;
        UpdatePointsUI();
        return true;
    }

    private void UpdatePointsUI()
    {
        if (pointsText != null)
            pointsText.text = balance.ToString();
        if (statsCreditsText != null)
            statsCreditsText.text = $"Credits: {balance}";
        if (shopCreditsText != null)
            shopCreditsText.text = balance.ToString();
    }

    private void OnValidate()
    {
        balance = Mathf.Max(0, balance);
    }
}
