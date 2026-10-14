# Skill Tree Package - API Reference

Complete API documentation for the Unity Skill Tree Package.

---

## Table of Contents

1. [Core Interfaces](#core-interfaces)
2. [Data Models](#data-models)
3. [Runtime Logic](#runtime-logic)
4. [Event System](#event-system)
5. [Save System](#save-system)
6. [Extension Interfaces](#extension-interfaces)
7. [UI Components](#ui-components)

---

## Core Interfaces

### ISkillTreeManager

Primary interface for interacting with the skill tree system at runtime.

```csharp
public interface ISkillTreeManager
{
    // State Queries
    bool IsNodeUnlocked(string nodeId);
    bool CanUnlockNode(string nodeId);
    SkillTreeState GetState();

    // Node Operations
    bool UnlockNode(string nodeId);
    bool LockNode(string nodeId);
    void ResetTree();

    // Resource Management
    float GetResource(string resourceType);
    void SetResource(string resourceType, float amount);
    void ConsumeResource(string resourceType, float amount);

    // Event Subscription
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : ISkillTreeEvent;
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : ISkillTreeEvent;
}
```

**Usage Example:**
```csharp
[Inject] private ISkillTreeManager _manager;

void Start()
{
    _manager.Subscribe<SkillUnlockedEvent>(OnSkillUnlocked);
    _manager.SetResource("SkillPoints", 10);

    if (_manager.CanUnlockNode("skill_fireball"))
    {
        _manager.UnlockNode("skill_fireball");
    }
}

void OnSkillUnlocked(SkillUnlockedEvent evt)
{
    Debug.Log($"Unlocked: {evt.Node.DisplayName}");
}
```

### ISkillTreeState

Read-only interface for querying skill tree state.

```csharp
public interface ISkillTreeState
{
    string SkillTreeId { get; }

    bool IsNodeUnlocked(string nodeId);
    DateTime? GetNodeUnlockTime(string nodeId);
    IReadOnlyList<string> GetUnlockedNodes();

    float GetResource(string resourceType);
    bool HasResource(string resourceType, float amount);

    object GetCustomData(string key);
    bool HasCustomData(string key);
    object GetNodeCustomData(string nodeId, string key);

    int UnlockedNodeCount { get; }
}
```

### ISaveSystem<T>

Generic interface for save/load operations.

```csharp
public interface ISaveSystem<T> where T : class
{
    // Synchronous Operations
    void Save(string saveId, T data);
    T Load(string saveId);
    bool Exists(string saveId);
    void Delete(string saveId);

    // Asynchronous Operations
    Task SaveAsync(string saveId, T data);
    Task<T> LoadAsync(string saveId);

    // Backup Management
    void CreateBackup(string saveId);
    bool RestoreFromBackup(string saveId, int backupIndex = 0);
}
```

---

## Data Models

### SkillNode

Represents an individual skill in the tree.

```csharp
public class SkillNode
{
    // Identity
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public Sprite Icon { get; set; }

    // Localization
    public string DisplayNameLocalizationKey { get; set; }
    public string DescriptionLocalizationKey { get; set; }

    // Graph Structure
    public Vector2 EditorPosition { get; set; }
    public List<string> PrerequisiteNodeIds { get; set; }

    // Unlock Logic
    public SkillNodeType NodeType { get; set; }
    public List<SkillRequirement> Requirements { get; set; }
    public List<SkillCost> Costs { get; set; }

    // Extensibility
    public SerializableDictionary<string, string> CustomData { get; set; }

    // Events
    public UnityEvent OnUnlocked;
    public UnityEvent OnLocked;

    // Methods
    public bool HasPrerequisites();
    public bool HasRequirements();
    public bool HasCosts();
    public void AddPrerequisite(string nodeId);
    public void RemovePrerequisite(string nodeId);
}
```

### SkillTreeGraph

ScriptableObject container for the entire skill tree.

```csharp
public class SkillTreeGraph : ScriptableObject
{
    public string TreeId { get; set; }
    public string TreeName { get; set; }
    public string Description { get; set; }

    // Node Management
    public SkillNode GetNodeById(string nodeId);
    public IReadOnlyList<SkillNode> GetAllNodes();
    public void SetNodes(List<SkillNode> nodes);

    // Connection Management
    public IReadOnlyList<SkillConnection> GetAllConnections();
    public void SetConnections(List<SkillConnection> connections);

    // Queries
    public List<SkillNode> GetPrerequisites(string nodeId);
    public List<SkillNode> GetDependents(string nodeId);
    public bool HasPath(string fromNodeId, string toNodeId);
    public List<SkillNode> GetRootNodes();

    // Validation
    public ValidationResult Validate();
}
```

### SkillConnection

Defines relationships between nodes.

```csharp
public class SkillConnection
{
    public string FromNodeId { get; set; }
    public string ToNodeId { get; set; }
    public ConnectionType Type { get; set; }
    public Color Color { get; set; }
    public float Width { get; set; }
    public float Curve { get; set; }
}

public enum ConnectionType
{
    Prerequisite,   // Must unlock before dependent
    Soft,          // Suggested but not required
    Exclusive,     // Only one path can be chosen
    Bidirectional  // Works both ways
}
```

### SkillRequirement

Flexible prerequisite system.

```csharp
public class SkillRequirement
{
    public RequirementType Type { get; set; }
    public RequirementLogic Logic { get; set; }
    public List<string> RequiredNodeIds { get; set; }
    public int RequiredCount { get; set; }
    public string CustomCondition { get; set; }
    public SerializableDictionary<string, string> Parameters { get; set; }
}

public enum RequirementType
{
    NodeUnlocked,  // Other nodes must be unlocked
    StatCheck,     // Player stat meets threshold
    LevelCheck,    // Player level requirement
    Custom         // Use IConditionEvaluator
}

public enum RequirementLogic
{
    And,   // All requirements must be met
    Or,    // At least one requirement must be met
    Count  // Specific number of requirements must be met
}
```

### SkillCost

Resource cost for unlocking a skill.

```csharp
public class SkillCost
{
    public string ResourceType { get; set; }
    public int Amount { get; set; }
    public SerializableDictionary<string, string> CustomData { get; set; }
}
```

---

## Runtime Logic

### SkillTreeManager

Core implementation of ISkillTreeManager.

**Constructor:**
```csharp
public SkillTreeManager(
    SkillTreeGraph graph,
    ISkillTreeEventBus eventBus,
    SkillTreeState initialState = null)
```

**Key Methods:**
```csharp
public bool UnlockNode(string nodeId)
{
    // Validates prerequisites, requirements, and costs
    // Deducts costs from resources
    // Updates state
    // Publishes SkillUnlockedEvent
    // Invokes node extensions
    // Returns true if successful
}

public bool LockNode(string nodeId)
{
    // Locks a node (used for respec)
    // Checks if any dependents are unlocked
    // Refunds costs if configured
    // Publishes SkillLockedEvent
    // Returns true if successful
}

public ValidationResult ValidateUnlock(string nodeId)
{
    // Checks all requirements without modifying state
    // Returns detailed validation result
}
```

### SkillUnlockValidator

Validates whether a node can be unlocked.

```csharp
public class SkillUnlockValidator
{
    public ValidationResult Validate(string nodeId, ISkillTreeState state)
    {
        // Checks:
        // - Node exists
        // - Not already unlocked
        // - Prerequisites are met
        // - Requirements are satisfied
        // - Costs can be paid
        // - Custom validations pass

        return new ValidationResult
        {
            IsValid = true/false,
            FailureReason = "...",
            FailureMessage = "..."
        };
    }
}
```

### SkillTreeTraverser

Advanced graph traversal utilities.

```csharp
public class SkillTreeTraverser
{
    public List<SkillNode> GetAllPrerequisites(string nodeId);
    public List<SkillNode> GetAllDependents(string nodeId);
    public bool HasCycle(SkillTreeGraph graph);
    public List<SkillNode> GetOrphanNodes(SkillTreeGraph graph);
    public List<SkillNode> FindPath(string fromId, string toId);
    public int GetDepth(string nodeId);
}
```

---

## Event System

### ISkillTreeEventBus

Decoupled event communication.

```csharp
public interface ISkillTreeEventBus
{
    void Publish<TEvent>(TEvent evt) where TEvent : ISkillTreeEvent;
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : ISkillTreeEvent;
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : ISkillTreeEvent;
    void Clear();
}
```

### Event Types

**SkillUnlockedEvent**
```csharp
public class SkillUnlockedEvent : ISkillTreeEvent
{
    public SkillNode Node { get; set; }
    public DateTime UnlockTime { get; set; }
    public Dictionary<string, float> CostsPaid { get; set; }
}
```

**SkillLockedEvent**
```csharp
public class SkillLockedEvent : ISkillTreeEvent
{
    public SkillNode Node { get; set; }
    public bool CostsRefunded { get; set; }
}
```

**TreeResetEvent**
```csharp
public class TreeResetEvent : ISkillTreeEvent
{
    public string TreeId { get; set; }
    public int NodesUnlocked { get; set; }
}
```

**ResourceChangedEvent**
```csharp
public class ResourceChangedEvent : ISkillTreeEvent
{
    public string ResourceType { get; set; }
    public float OldValue { get; set; }
    public float NewValue { get; set; }
}
```

---

## Save System

### SkillTreeSaveData

Serializable save data structure.

```csharp
public class SkillTreeSaveData
{
    public int Version { get; set; }
    public string SkillTreeId { get; set; }
    public DateTime LastSaveTime { get; set; }

    // Node State
    public List<string> UnlockedNodes { get; private set; }
    public Dictionary<string, DateTime> UnlockTimes { get; private set; }

    // Resources
    public Dictionary<string, float> Resources { get; private set; }

    // Custom Data
    public Dictionary<string, string> TreeCustomData { get; private set; }
    public Dictionary<string, Dictionary<string, string>> NodeCustomData { get; private set; }

    // Methods
    public void AddUnlockedNode(string nodeId);
    public void RemoveUnlockedNode(string nodeId);
    public bool IsNodeUnlocked(string nodeId);

    public void SetResource(string resourceType, float amount);
    public float GetResource(string resourceType);
}
```

### JsonSaveSystem

JSON-based save implementation.

```csharp
public class JsonSaveSystem<T> : ISaveSystem<T> where T : class
{
    public JsonSaveSystem(SaveSystemConfig config = null);

    public void Save(string saveId, T data);
    public T Load(string saveId);
    public async Task SaveAsync(string saveId, T data);
    public async Task<T> LoadAsync(string saveId);

    // Configuration
    public class SaveSystemConfig
    {
        public string SaveDirectory { get; set; }
        public bool CreateBackups { get; set; }
        public int MaxBackups { get; set; }
        public bool PrettyPrint { get; set; }
    }
}
```

---

## Extension Interfaces

### ISkillNodeExtension

Custom node behaviors.

```csharp
public interface ISkillNodeExtension
{
    string NodeTypeId { get; }

    void OnNodeUnlocked(SkillNode node, ISkillTreeState state);
    void OnNodeLocked(SkillNode node, ISkillTreeState state);
    string ValidateUnlock(SkillNode node, ISkillTreeState state);
}
```

**Implementation Example:**
```csharp
public class AbilityGrantExtension : ISkillNodeExtension
{
    public string NodeTypeId => "GrantAbility";

    public void OnNodeUnlocked(SkillNode node, ISkillTreeState state)
    {
        string abilityId = node.CustomData["AbilityId"];
        AbilitySystem.GrantAbility(abilityId);
    }

    public void OnNodeLocked(SkillNode node, ISkillTreeState state)
    {
        string abilityId = node.CustomData["AbilityId"];
        AbilitySystem.RevokeAbility(abilityId);
    }

    public string ValidateUnlock(SkillNode node, ISkillTreeState state)
    {
        if (!node.CustomData.ContainsKey("AbilityId"))
            return "Missing AbilityId in node data";
        return null;
    }
}
```

### IConditionEvaluator

Custom requirements.

```csharp
public interface IConditionEvaluator
{
    string ConditionType { get; }

    bool Evaluate(SkillRequirement requirement, ISkillTreeState state, object context = null);
    string GetFailureMessage(SkillRequirement requirement);
    string GetDescription(SkillRequirement requirement);
}
```

### ISkillCostProvider

Custom cost systems.

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

### ISkillVisualProvider

Custom visuals.

```csharp
public interface ISkillVisualProvider
{
    string VisualProviderId { get; }
    int Priority { get; }

    void CustomizeNodeVisual(GameObject nodeUI, SkillNode node, SkillNodeVisualState state);
    void CustomizeConnectionVisual(GameObject connectionUI, SkillConnection connection);
}

public enum SkillNodeVisualState
{
    Locked,      // Cannot be unlocked yet
    Available,   // Can be unlocked
    Unlocked,    // Already unlocked
    Highlighted, // Hovered or selected
    Invalid      // Validation error
}
```

### SkillTreeExtensionRegistry

Central registry for managing extensions.

```csharp
public class SkillTreeExtensionRegistry
{
    public static SkillTreeExtensionRegistry Instance { get; }

    // Registration
    public void RegisterNodeExtension(ISkillNodeExtension extension);
    public void RegisterConditionEvaluator(IConditionEvaluator evaluator);
    public void RegisterCostProvider(ISkillCostProvider provider);
    public void RegisterVisualProvider(ISkillVisualProvider provider);

    // Retrieval
    public ISkillNodeExtension GetNodeExtension(string nodeTypeId);
    public IConditionEvaluator GetConditionEvaluator(string conditionType);
    public ISkillCostProvider GetCostProvider(string costType);
    public ISkillVisualProvider GetVisualProvider(string providerId);
    public IEnumerable<ISkillVisualProvider> GetAllVisualProviders();

    // Helpers
    public bool TryInvokeNodeUnlocked(SkillNode node, ISkillTreeState state);
    public bool TryInvokeNodeLocked(SkillNode node, ISkillTreeState state);
    public bool EvaluateCustomCondition(SkillRequirement requirement, ISkillTreeState state, object context = null);
    public bool CanAffordCustomCost(SkillCost cost, ISkillTreeState state, object context = null);
}
```

**Usage:**
```csharp
// At startup
void RegisterExtensions()
{
    var registry = SkillTreeExtensionRegistry.Instance;

    registry.RegisterNodeExtension(new StatBoostNodeExtension());
    registry.RegisterConditionEvaluator(new LevelRequirementEvaluator());
    registry.RegisterCostProvider(new MultiCurrencyCostProvider());
    registry.RegisterVisualProvider(new RarityVisualProvider());
}
```

---

## UI Components

### SkillTreeUI

Main UI controller for runtime display.

```csharp
public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private SkillTreeGraph _skillTreeGraph;
    [SerializeField] private GameObject _nodeUIPrefab;
    [SerializeField] private Transform _nodeContainer;

    [Inject] private ISkillTreeManager _manager;

    public void Initialize(SkillTreeGraph graph);
    public void RefreshAllNodes();
    public void RefreshNode(string nodeId);
    public void SetLayout(LayoutType layoutType);
}
```

### SkillNodeUI

Individual node display component.

```csharp
public class SkillNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public SkillNode Node { get; private set; }

    public void Initialize(SkillNode node, ISkillTreeManager manager);
    public void UpdateVisualState(SkillNodeVisualState state);
    public void AnimateTransition(SkillNodeVisualState fromState, SkillNodeVisualState toState);

    // Unity Events
    public UnityEvent<SkillNodeUI> OnNodeClicked;
    public UnityEvent<SkillNodeUI> OnNodeHovered;
}
```

### SkillTreeLayoutEngine

Auto-layout algorithms.

```csharp
public class SkillTreeLayoutEngine
{
    public static void ApplyLayout(SkillTreeGraph graph, LayoutType layoutType)
    {
        switch (layoutType)
        {
            case LayoutType.Manual:      // Designer-placed
            case LayoutType.Grid:        // Snap to grid
            case LayoutType.Hierarchical: // Tree structure
            case LayoutType.ForceDirected: // Physics-based
        }
    }
}
```

---

## Complete Usage Example

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using SkillTree.Core.Events;
using SkillTree.Extensions;
using SkillTree.Extensions.Samples;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SkillTreeGraph _skillTreeGraph;

    [Inject] private ISkillTreeManager _skillTreeManager;

    private void Start()
    {
        // Register extensions
        RegisterExtensions();

        // Load saved progress
        LoadProgress();

        // Subscribe to events
        _skillTreeManager.Subscribe<SkillUnlockedEvent>(OnSkillUnlocked);
        _skillTreeManager.Subscribe<ResourceChangedEvent>(OnResourceChanged);

        // Give starting resources
        _skillTreeManager.SetResource("SkillPoints", 5);
        _skillTreeManager.SetResource("Gold", 1000);
    }

    private void RegisterExtensions()
    {
        var registry = SkillTreeExtensionRegistry.Instance;

        registry.RegisterNodeExtension(new StatBoostNodeExtension());
        registry.RegisterConditionEvaluator(new LevelRequirementEvaluator());
        registry.RegisterCostProvider(new MultiCurrencyCostProvider());
        registry.RegisterVisualProvider(new RarityVisualProvider());
    }

    private void LoadProgress()
    {
        var saveSystem = new JsonSaveSystem<SkillTreeSaveData>();
        if (saveSystem.Exists("player_skills"))
        {
            var saveData = saveSystem.Load("player_skills");
            _skillTreeManager.LoadState(saveData);
        }
    }

    private void OnSkillUnlocked(SkillUnlockedEvent evt)
    {
        Debug.Log($"Unlocked: {evt.Node.DisplayName}");
        ShowNotification($"Skill Unlocked: {evt.Node.DisplayName}");

        // Auto-save after unlock
        AutoSave();
    }

    private void OnResourceChanged(ResourceChangedEvent evt)
    {
        Debug.Log($"{evt.ResourceType}: {evt.OldValue} → {evt.NewValue}");
        UpdateUI();
    }

    private void AutoSave()
    {
        var saveSystem = new JsonSaveSystem<SkillTreeSaveData>();
        var saveData = _skillTreeManager.GetState().ToSaveData();
        saveSystem.SaveAsync("player_skills", saveData);
    }
}
```

---

## Additional Resources

- **Sample Projects**: See `Samples~/` for complete examples
- **Video Tutorials**: [Link to tutorials]
- **GitHub**: [Link to repository]
- **Support**: [Link to support]

---

**Last Updated**: 2026-01-10
**Package Version**: 0.6.0
