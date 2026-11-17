using UnityEngine;

public enum ShopUpgradeStat
{
    MaxHealth,
    Attack,
    Defense,
    TopSpeed
}

[CreateAssetMenu(fileName = "ShopUpgrade", menuName = "Space Ship/Shop Upgrade")]
public class ShopUpgradeData : ScriptableObject
{
    [SerializeField] private int sortOrder;
    [SerializeField] private string category;
    [SerializeField] private string displayName;
    [TextArea(2, 4)]
    [SerializeField] private string description;
    [SerializeField] private string bonusText;
    [Min(0)]
    [SerializeField] private int creditCost;
    [SerializeField] private ShopUpgradeStat stat;
    [Min(0f)]
    [SerializeField] private float amount;

    public int SortOrder => sortOrder;
    public string Category => category;
    public string DisplayName => displayName;
    public string Description => description;
    public string BonusText => bonusText;
    public int CreditCost => creditCost;
    public ShopUpgradeStat Stat => stat;
    public float Amount => amount;

    private void OnValidate()
    {
        sortOrder = Mathf.Max(0, sortOrder);
        creditCost = Mathf.Max(0, creditCost);
        amount = Mathf.Max(0f, amount);
    }
}
