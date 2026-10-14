# Skill Tree Extension Guide

Complete guide to extending the Skill Tree system with custom behaviors, requirements, costs, and visuals.

---

## Overview

The Skill Tree system is built with extensibility as a core principle. Four main extension points allow you to customize behavior without modifying core code:

1. **ISkillNodeExtension** - Custom node behaviors (stat boosts, ability unlocks, etc.)
2. **IConditionEvaluator** - Custom requirements (level checks, quest completion, etc.)
3. **ISkillCostProvider** - Custom cost systems (multiple currencies, items, etc.)
4. **ISkillVisualProvider** - Custom visuals (coloring, effects, animations, etc.)

---

## Extension Point 1: Custom Node Behaviors

### When to Use

Use `ISkillNodeExtension` when you need to execute custom logic when a skill is unlocked or locked:
- Granting/revoking abilities
- Modifying player stats
- Unlocking game content
- Triggering cutscenes or events
- Updating quest progress

### Interface

```csharp
public interface ISkillNodeExtension
{
    string NodeTypeId { get; }
    void OnNodeUnlocked(SkillNode node, ISkillTreeState state);
    void OnNodeLocked(SkillNode node, ISkillTreeState state);
    string ValidateUnlock(SkillNode node, ISkillTreeState state);
}
```

### Example: Granting Abilities

```csharp
using SkillTree.Extensions;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;

public class AbilityUnlockExtension : ISkillNodeExtension
{
    public string NodeTypeId => "AbilityUnlock";

    private IAbilitySystem _abilitySystem;

    public AbilityUnlockExtension(IAbilitySystem abilitySystem)
    {
        _abilitySystem = abilitySystem;
    }

    public void OnNodeUnlocked(SkillNode node, ISkillTreeState state)
    {
        if (node.CustomData.TryGetValue("AbilityId", out string abilityId))
        {
            _abilitySystem.GrantAbility(abilityId);
            Debug.Log($"Granted ability: {abilityId}");

            // Optional: Grant multiple abilities
            if (node.CustomData.TryGetValue("BonusAbilityId", out string bonusId))
            {
                _abilitySystem.GrantAbility(bonusId);
            }
        }
    }

    public void OnNodeLocked(SkillNode node, ISkillTreeState state)
    {
        if (node.CustomData.TryGetValue("AbilityId", out string abilityId))
        {
            _abilitySystem.RevokeAbility(abilityId);
            Debug.Log($"Revoked ability: {abilityId}");
        }
    }

    public string ValidateUnlock(SkillNode node, ISkillTreeState state)
    {
        // Validate node has required data
        if (!node.CustomData.ContainsKey("AbilityId"))
            return "Missing 'AbilityId' in node custom data";

        // Optional: Check if ability can be granted
        string abilityId = node.CustomData["AbilityId"];
        if (_abilitySystem.IsAbilityLocked(abilityId))
            return $"Ability {abilityId} is currently locked by game state";

        return null; // Valid
    }
}
```

### Setting Up Node Data

In the Unity Editor:
1. Select your SkillNode
2. In CustomData, add key `NodeExtensionType` with value `"AbilityUnlock"`
3. Add key `AbilityId` with the ID of the ability to grant
4. (Optional) Add `BonusAbilityId` for additional abilities

### Registration

```csharp
void RegisterExtensions()
{
    var abilitySystem = GetComponent<IAbilitySystem>();
    var extension = new AbilityUnlockExtension(abilitySystem);
    SkillTreeExtensionRegistry.Instance.RegisterNodeExtension(extension);
}
```

---

## Extension Point 2: Custom Requirements

### When to Use

Use `IConditionEvaluator` when you need custom prerequisites beyond node unlocks:
- Player level requirements
- Quest completion
- Achievement checks
- Time-based unlocks
- Territory control
- Faction reputation

### Interface

```csharp
public interface IConditionEvaluator
{
    string ConditionType { get; }
    bool Evaluate(SkillRequirement requirement, ISkillTreeState state, object context = null);
    string GetFailureMessage(SkillRequirement requirement);
    string GetDescription(SkillRequirement requirement);
}
```

### Example: Quest Completion Requirement

```csharp
public class QuestRequirementEvaluator : IConditionEvaluator
{
    public string ConditionType => "QuestComplete";

    private IQuestSystem _questSystem;

    public QuestRequirementEvaluator(IQuestSystem questSystem)
    {
        _questSystem = questSystem;
    }

    public bool Evaluate(SkillRequirement requirement, ISkillTreeState state, object context = null)
    {
        if (!requirement.Parameters.TryGetValue("QuestId", out string questId))
        {
            Debug.LogWarning("Missing 'QuestId' parameter in QuestComplete requirement");
            return false;
        }

        bool isComplete = _questSystem.IsQuestComplete(questId);

        Debug.Log($"Quest {questId} completion check: {isComplete}");
        return isComplete;
    }

    public string GetFailureMessage(SkillRequirement requirement)
    {
        string questName = requirement.Parameters.GetValueOrDefault("QuestName", "Unknown Quest");
        return $"Complete quest: {questName}";
    }

    public string GetDescription(SkillRequirement requirement)
    {
        string questName = requirement.Parameters.GetValueOrDefault("QuestName", "Unknown Quest");
        return $"Requires: {questName}";
    }
}
```

### Setting Up Requirement

```csharp
var requirement = new SkillRequirement
{
    Type = RequirementType.Custom,
    CustomCondition = "QuestComplete"
};
requirement.Parameters["QuestId"] = "main_quest_01";
requirement.Parameters["QuestName"] = "The Hero's Journey";

node.Requirements.Add(requirement);
```

---

## Extension Point 3: Custom Costs

### When to Use

Use `ISkillCostProvider` for specialized resource systems:
- Multiple currencies (gold, gems, tokens)
- Item requirements (consume items)
- Experience points
- Karma/honor systems
- Dynamic costs (scale with level)

### Interface

```csharp
public interface ISkillCostProvider
{
    string CostType { get; }
    bool CanAfford(SkillCost cost, ISkillTreeState state, object context = null);
    void PayCost(SkillCost cost, object state, object context = null);
    string GetCostDescription(SkillCost cost);
    void RefundCost(SkillCost cost, object state, object context = null);
}
```

### Example: Item Cost Provider

```csharp
public class ItemCostProvider : ISkillCostProvider
{
    public string CostType => "ItemCost";

    private IInventorySystem _inventory;

    public ItemCostProvider(IInventorySystem inventory)
    {
        _inventory = inventory;
    }

    public bool CanAfford(SkillCost cost, ISkillTreeState state, object context = null)
    {
        var items = GetRequiredItems(cost);

        foreach (var item in items)
        {
            if (!_inventory.HasItem(item.Key, item.Value))
            {
                Debug.Log($"Missing {item.Value}x {item.Key}");
                return false;
            }
        }

        return true;
    }

    public void PayCost(SkillCost cost, object state, object context = null)
    {
        var items = GetRequiredItems(cost);

        foreach (var item in items)
        {
            _inventory.RemoveItem(item.Key, item.Value);
            Debug.Log($"Consumed {item.Value}x {item.Key}");
        }
    }

    public string GetCostDescription(SkillCost cost)
    {
        var items = GetRequiredItems(cost);
        var descriptions = items.Select(kvp => $"{kvp.Value}x {GetItemName(kvp.Key)}");
        return string.Join(", ", descriptions);
    }

    public void RefundCost(SkillCost cost, object state, object context = null)
    {
        var items = GetRequiredItems(cost);

        foreach (var item in items)
        {
            _inventory.AddItem(item.Key, item.Value);
            Debug.Log($"Refunded {item.Value}x {item.Key}");
        }
    }

    private Dictionary<string, int> GetRequiredItems(SkillCost cost)
    {
        var items = new Dictionary<string, int>();

        // Parse item requirements from CustomData
        foreach (var key in cost.CustomData.Keys)
        {
            if (key.StartsWith("Item_"))
            {
                string itemId = key.Substring(5); // Remove "Item_" prefix
                if (int.TryParse(cost.CustomData[key], out int amount))
                {
                    items[itemId] = amount;
                }
            }
        }

        return items;
    }

    private string GetItemName(string itemId)
    {
        return _inventory.GetItemData(itemId)?.DisplayName ?? itemId;
    }
}
```

### Setting Up Cost

```csharp
var cost = new SkillCost
{
    ResourceType = "Items"
};
cost.CustomData["CostProvider"] = "ItemCost";
cost.CustomData["Item_rare_crystal"] = "5";
cost.CustomData["Item_ancient_scroll"] = "1";

node.Costs.Add(cost);
```

---

## Extension Point 4: Custom Visuals

### When to Use

Use `ISkillVisualProvider` to customize node appearance:
- Color coding by rarity, class, or category
- Visual effects (glow, particles, animations)
- Dynamic icons based on state
- Border styles and thicknesses
- Connection line customization

### Interface

```csharp
public interface ISkillVisualProvider
{
    string VisualProviderId { get; }
    int Priority { get; }
    void CustomizeNodeVisual(GameObject nodeUI, SkillNode node, SkillNodeVisualState state);
    void CustomizeConnectionVisual(GameObject connectionUI, SkillConnection connection);
}
```

### Example: School of Magic Visual Provider

```csharp
public class MagicSchoolVisualProvider : ISkillVisualProvider
{
    public string VisualProviderId => "MagicSchool";
    public int Priority => 10;

    public void CustomizeNodeVisual(GameObject nodeUI, SkillNode node, SkillNodeVisualState state)
    {
        if (!node.CustomData.TryGetValue("School", out string school))
            return;

        // Get color for magic school
        Color schoolColor = GetSchoolColor(school);

        // Apply to node background
        var background = nodeUI.transform.Find("Background")?.GetComponent<Image>();
        if (background != null)
        {
            background.color = schoolColor * GetStateMultiplier(state);
        }

        // Add glow effect for unlocked legendary nodes
        if (state == SkillNodeVisualState.Unlocked && node.NodeType == SkillNodeType.Capstone)
        {
            AddGlowEffect(nodeUI, schoolColor);
        }

        // Update border
        UpdateBorder(nodeUI, school, state);
    }

    public void CustomizeConnectionVisual(GameObject connectionUI, SkillConnection connection)
    {
        // Match connection color to school (would need to look up nodes)
        var lineRenderer = connectionUI.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = connection.Type == ConnectionType.Prerequisite ? 2f : 1f;
        }
    }

    private Color GetSchoolColor(string school) => school switch
    {
        "Fire" => new Color(1.0f, 0.3f, 0.2f),
        "Ice" => new Color(0.2f, 0.6f, 1.0f),
        "Lightning" => new Color(0.9f, 0.9f, 0.2f),
        "Earth" => new Color(0.5f, 0.4f, 0.2f),
        "Arcane" => new Color(0.8f, 0.2f, 0.8f),
        _ => Color.white
    };

    private float GetStateMultiplier(SkillNodeVisualState state) => state switch
    {
        SkillNodeVisualState.Locked => 0.3f,
        SkillNodeVisualState.Available => 0.7f,
        SkillNodeVisualState.Unlocked => 1.0f,
        SkillNodeVisualState.Highlighted => 1.2f,
        _ => 1.0f
    };

    private void AddGlowEffect(GameObject nodeUI, Color color)
    {
        var glow = nodeUI.transform.Find("Glow");
        if (glow != null)
        {
            var glowImage = glow.GetComponent<Image>();
            if (glowImage != null)
            {
                glowImage.color = color;
                glowImage.enabled = true;
            }
        }
    }

    private void UpdateBorder(GameObject nodeUI, string school, SkillNodeVisualState state)
    {
        var border = nodeUI.transform.Find("Border")?.GetComponent<Image>();
        if (border != null)
        {
            Color schoolColor = GetSchoolColor(school);
            border.color = schoolColor;

            // Animate border when highlighted
            if (state == SkillNodeVisualState.Highlighted)
            {
                // Use PrimeTween for smooth animation
                //PrimeTween.Tween.Scale(border.transform, 1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo);
            }
        }
    }
}
```

---

## Best Practices

### 1. Dependency Injection

Use constructor injection for dependencies:

```csharp
public class MyExtension : ISkillNodeExtension
{
    private readonly IPlayerStats _stats;
    private readonly IAchievementSystem _achievements;

    public MyExtension(IPlayerStats stats, IAchievementSystem achievements)
    {
        _stats = stats;
        _achievements = achievements;
    }
}
```

### 2. Error Handling

Always validate data and handle errors gracefully:

```csharp
public void OnNodeUnlocked(SkillNode node, ISkillTreeState state)
{
    try
    {
        if (!node.CustomData.TryGetValue("RequiredKey", out string value))
        {
            Debug.LogWarning($"Node {node.Id} missing RequiredKey");
            return;
        }

        // Process node...
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error in extension for node {node.Id}: {ex.Message}");
    }
}
```

### 3. Clear Data Contracts

Document what CustomData keys your extension expects:

```csharp
/// <summary>
/// Unlocks weapons when nodes are activated.
///
/// Required CustomData keys:
/// - "WeaponId" (string): The ID of the weapon to unlock
/// - "WeaponTier" (int): The tier level (1-5)
///
/// Optional CustomData keys:
/// - "GrantAmmo" (bool): Whether to grant starting ammo (default: true)
/// - "AmmoAmount" (int): Amount of ammo to grant (default: 100)
/// </summary>
public class WeaponUnlockExtension : ISkillNodeExtension
{
    // Implementation...
}
```

### 4. Registration at Startup

Create a centralized bootstrap script:

```csharp
public class SkillTreeBootstrap : MonoBehaviour
{
    [SerializeField] private bool _registerExtensions = true;

    private void Awake()
    {
        if (_registerExtensions)
        {
            RegisterAllExtensions();
        }
    }

    private void RegisterAllExtensions()
    {
        var registry = SkillTreeExtensionRegistry.Instance;

        // Node Extensions
        registry.RegisterNodeExtension(new StatBoostNodeExtension());
        registry.RegisterNodeExtension(new AbilityUnlockExtension(GetAbilitySystem()));
        registry.RegisterNodeExtension(new WeaponUnlockExtension(GetWeaponSystem()));

        // Condition Evaluators
        registry.RegisterConditionEvaluator(new LevelRequirementEvaluator());
        registry.RegisterConditionEvaluator(new QuestRequirementEvaluator(GetQuestSystem()));

        // Cost Providers
        registry.RegisterCostProvider(new MultiCurrencyCostProvider());
        registry.RegisterCostProvider(new ItemCostProvider(GetInventory()));

        // Visual Providers
        registry.RegisterVisualProvider(new RarityVisualProvider());
        registry.RegisterVisualProvider(new MagicSchoolVisualProvider());
    }
}
```

### 5. Testing Extensions

Test extensions in isolation:

```csharp
[Test]
public void AbilityExtension_UnlocksAbility_WhenNodeUnlocked()
{
    // Arrange
    var mockAbilitySystem = new MockAbilitySystem();
    var extension = new AbilityUnlockExtension(mockAbilitySystem);

    var node = new SkillNode("test_node");
    node.CustomData["AbilityId"] = "fireball";

    var mockState = new MockSkillTreeState();

    // Act
    extension.OnNodeUnlocked(node, mockState);

    // Assert
    Assert.IsTrue(mockAbilitySystem.HasAbility("fireball"));
}
```

---

## Advanced Patterns

### Composite Extensions

Combine multiple behaviors:

```csharp
public class CompositeNodeExtension : ISkillNodeExtension
{
    private readonly List<ISkillNodeExtension> _extensions = new();

    public string NodeTypeId => "Composite";

    public void AddExtension(ISkillNodeExtension extension)
    {
        _extensions.Add(extension);
    }

    public void OnNodeUnlocked(SkillNode node, ISkillTreeState state)
    {
        foreach (var extension in _extensions)
        {
            extension.OnNodeUnlocked(node, state);
        }
    }

    public void OnNodeLocked(SkillNode node, ISkillTreeState state)
    {
        foreach (var extension in _extensions)
        {
            extension.OnNodeLocked(node, state);
        }
    }
}
```

### Dynamic Costs

Scale costs based on progression:

```csharp
public class DynamicCostProvider : ISkillCostProvider
{
    public string CostType => "Dynamic";

    public bool CanAfford(SkillCost cost, ISkillTreeState state, object context = null)
    {
        float baseCost = float.Parse(cost.CustomData["BaseCost"]);
        float scalingFactor = float.Parse(cost.CustomData.GetValueOrDefault("ScalingFactor", "1.5"));

        // Scale cost based on number of unlocked nodes
        int unlockedCount = state.UnlockedNodeCount;
        float actualCost = baseCost * Mathf.Pow(scalingFactor, unlockedCount / 5);

        return state.HasResource("SkillPoints", actualCost);
    }

    // ... implement other methods
}
```

---

## Troubleshooting

### Extension Not Being Called

1. Check registration: `SkillTreeExtensionRegistry.Instance.GetNodeExtension(nodeTypeId)`
2. Verify CustomData key: `node.CustomData["NodeExtensionType"]` matches `NodeTypeId`
3. Check logs for errors during extension execution

### Validation Failing

1. Check `ValidateUnlock()` return value
2. Ensure all required CustomData keys are present
3. Verify requirement parameters are set correctly

### Visual Changes Not Appearing

1. Check `Priority` - higher values override lower
2. Verify GameObject hierarchy matches what you're searching for
3. Ensure visual provider is registered before UI initialization

---

## Additional Resources

- **API Reference**: See `API_Reference.md` for complete interface documentation
- **Sample Extensions**: Check `Runtime/Extensions/Samples/` for working examples
- **Sample Skill Trees**: Use menu `Tools > Skill Tree > Create Example` to see extensions in action

---

**Last Updated**: 2026-01-10
