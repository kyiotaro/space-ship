using UnityEngine;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using SkillTree.Core.Events;

namespace SkillTree.Samples
{
    /// <summary>
    /// Example script demonstrating how to use the Skill Tree system.
    /// This shows both manual usage and event subscription patterns.
    /// </summary>
    public class SkillTreeExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkillTreeGraph _skillTreeGraph;

        [Header("Test Configuration")]
        [SerializeField] private string _nodeIdToUnlock = "";
        [SerializeField] private string _resourceType = "SkillPoints";
        [SerializeField] private float _initialResourceAmount = 100f;

        private ISkillTreeManager _manager;
        private ISkillTreeEventBus _eventBus;

        private void Start()
        {
            InitializeSkillTree();
            SetupEventListeners();
            InitializeResources();
            PrintSkillTreeInfo();
        }

        private void InitializeSkillTree()
        {
            if (_skillTreeGraph == null)
            {
                Debug.LogError("No SkillTreeGraph assigned!");
                return;
            }

            // Create event bus and manager
            _eventBus = new SkillTreeEventBus();
            _manager = new SkillTreeManager(_skillTreeGraph, _eventBus);

            Debug.Log($"Initialized Skill Tree: {_skillTreeGraph.DisplayName}");
        }

        private void SetupEventListeners()
        {
            // Subscribe to skill unlocked events
            _manager.Subscribe<SkillUnlockedEvent>(OnSkillUnlocked);

            // Subscribe to unlock failed events
            _manager.Subscribe<SkillUnlockFailedEvent>(OnSkillUnlockFailed);

            // Subscribe to resource changed events
            _manager.Subscribe<ResourceChangedEvent>(OnResourceChanged);

            // Subscribe to tree reset events
            _manager.Subscribe<TreeResetEvent>(OnTreeReset);
        }

        private void InitializeResources()
        {
            // Give the player some starting resources
            _manager.SetResource(_resourceType, _initialResourceAmount);
            Debug.Log($"Initialized {_resourceType}: {_initialResourceAmount}");
        }

        private void PrintSkillTreeInfo()
        {
            Debug.Log($"=== Skill Tree Info ===");
            Debug.Log($"Tree ID: {_skillTreeGraph.TreeId}");
            Debug.Log($"Total Nodes: {_skillTreeGraph.NodeCount}");
            Debug.Log($"Root Nodes: {_skillTreeGraph.GetRootNodes().Count}");

            // Print root nodes
            Debug.Log("Root Nodes:");
            foreach (var rootNode in _skillTreeGraph.GetRootNodes())
            {
                Debug.Log($"  - {rootNode.DisplayName} ({rootNode.Id})");
            }

            // Print unlockable nodes
            var unlockableNodes = _manager.GetUnlockableNodes();
            Debug.Log($"Currently Unlockable Nodes: {unlockableNodes.Count}");
            foreach (var node in unlockableNodes)
            {
                Debug.Log($"  - {node.DisplayName} ({node.Id})");
            }
        }

        #region Event Handlers

        private void OnSkillUnlocked(SkillUnlockedEvent evt)
        {
            Debug.Log($"<color=green>✓ Unlocked Skill: {evt.Node.DisplayName}</color>");
            Debug.Log($"  Description: {evt.Node.Description}");
            Debug.Log($"  Unlocked at: {evt.Timestamp}");

            // Check what's now unlockable
            var nowUnlockable = _manager.GetUnlockableNodes();
            if (nowUnlockable.Count > 0)
            {
                Debug.Log($"  {nowUnlockable.Count} new skills are now unlockable");
            }
        }

        private void OnSkillUnlockFailed(SkillUnlockFailedEvent evt)
        {
            Debug.LogWarning($"<color=red>✗ Failed to unlock skill: {evt.NodeId}</color>");
            Debug.LogWarning($"  Reason: {evt.Reason}");
            Debug.LogWarning($"  Failure Type: {evt.FailureReason}");
        }

        private void OnResourceChanged(ResourceChangedEvent evt)
        {
            Debug.Log($"Resource Changed: {evt.ResourceType}");
            Debug.Log($"  Old Amount: {evt.OldAmount}");
            Debug.Log($"  New Amount: {evt.NewAmount}");
            Debug.Log($"  Delta: {evt.Delta:+0;-0}");
        }

        private void OnTreeReset(TreeResetEvent evt)
        {
            Debug.Log($"<color=yellow>Tree Reset!</color>");
            Debug.Log($"  {evt.NodesUnlocked} nodes were cleared");
        }

        #endregion

        #region Public Methods (for testing)

        /// <summary>
        /// Attempts to unlock a skill by ID.
        /// Call this from inspector or other scripts.
        /// </summary>
        public void UnlockSkill(string nodeId)
        {
            if (_manager == null)
            {
                Debug.LogError("Manager not initialized!");
                return;
            }

            Debug.Log($"Attempting to unlock: {nodeId}");

            // Get validation info first
            var validation = _manager.ValidateUnlock(nodeId);
            if (!validation.IsValid)
            {
                Debug.LogWarning($"Cannot unlock: {validation.FailureMessage}");

                if (validation.UnmetPrerequisites.Count > 0)
                {
                    Debug.LogWarning($"  Missing prerequisites: {string.Join(", ", validation.UnmetPrerequisites)}");
                }

                if (validation.InsufficientResources.Count > 0)
                {
                    Debug.LogWarning("  Insufficient resources:");
                    foreach (var kvp in validation.InsufficientResources)
                    {
                        Debug.LogWarning($"    - {kvp.Key}: need {kvp.Value} more");
                    }
                }
            }

            // Attempt unlock
            bool success = _manager.UnlockNode(nodeId);
            Debug.Log($"Unlock result: {(success ? "SUCCESS" : "FAILED")}");
        }

        /// <summary>
        /// Unlocks the first root node in the tree.
        /// </summary>
        public void UnlockFirstRootNode()
        {
            var rootNodes = _skillTreeGraph.GetRootNodes();
            if (rootNodes.Count > 0)
            {
                UnlockSkill(rootNodes[0].Id);
            }
            else
            {
                Debug.LogWarning("No root nodes found!");
            }
        }

        /// <summary>
        /// Unlocks all currently unlockable nodes.
        /// </summary>
        public void UnlockAllAvailable()
        {
            var unlockableNodes = _manager.GetUnlockableNodes();
            Debug.Log($"Unlocking {unlockableNodes.Count} available nodes...");

            foreach (var node in unlockableNodes)
            {
                _manager.UnlockNode(node.Id);
            }
        }

        /// <summary>
        /// Resets the entire skill tree.
        /// </summary>
        public void ResetTree()
        {
            if (_manager == null) return;

            Debug.Log("Resetting tree...");
            _manager.ResetTree();
            InitializeResources();
        }

        /// <summary>
        /// Adds resources for testing.
        /// </summary>
        public void AddResources(float amount)
        {
            if (_manager == null) return;

            _manager.AddResource(_resourceType, amount);
            Debug.Log($"Added {amount} {_resourceType}");
        }

        /// <summary>
        /// Prints current state info.
        /// </summary>
        public void PrintCurrentState()
        {
            if (_manager == null) return;

            var state = _manager.State;
            Debug.Log($"=== Current State ===");
            Debug.Log($"Unlocked Nodes: {state.UnlockedNodeCount}");
            Debug.Log($"{_resourceType}: {_manager.GetResource(_resourceType)}");

            var unlocked = state.GetUnlockedNodes();
            if (unlocked.Count > 0)
            {
                Debug.Log("Unlocked Skills:");
                foreach (var nodeId in unlocked)
                {
                    var node = _manager.GetNode(nodeId);
                    if (node != null)
                    {
                        Debug.Log($"  - {node.DisplayName}");
                    }
                }
            }
        }

        #endregion

        #region Unity Editor Test Methods

        [ContextMenu("Test: Unlock Configured Node")]
        private void TestUnlockConfiguredNode()
        {
            if (!string.IsNullOrEmpty(_nodeIdToUnlock))
            {
                UnlockSkill(_nodeIdToUnlock);
            }
            else
            {
                Debug.LogWarning("No node ID configured for testing");
            }
        }

        [ContextMenu("Test: Unlock First Root")]
        private void TestUnlockFirstRoot()
        {
            UnlockFirstRootNode();
        }

        [ContextMenu("Test: Print State")]
        private void TestPrintState()
        {
            PrintCurrentState();
        }

        [ContextMenu("Test: Reset Tree")]
        private void TestResetTree()
        {
            ResetTree();
        }

        [ContextMenu("Test: Add 50 Resources")]
        private void TestAddResources()
        {
            AddResources(50f);
        }

        #endregion
    }
}
