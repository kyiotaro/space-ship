using UnityEngine;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using SkillTree.Core.Events;

namespace SkillTree.Integration
{
    /// <summary>
    /// Simple MonoBehaviour controller for the skill tree system.
    /// Use this if you're not using VContainer or want manual control.
    ///
    /// This creates and manages a SkillTreeManager instance.
    /// Subscribe to events and call methods to interact with the skill tree.
    /// </summary>
    public class SkillTreeController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("The skill tree graph to manage")]
        private SkillTreeGraph _skillTreeGraph;

        [Header("Debug")]
        [SerializeField, Tooltip("Enable debug logging")]
        private bool _debugMode = false;

        private ISkillTreeManager _manager;
        private ISkillTreeEventBus _eventBus;

        public ISkillTreeManager Manager => _manager;
        public SkillTreeGraph Graph => _skillTreeGraph;

        private void Awake()
        {
            if (_skillTreeGraph == null)
            {
                Debug.LogError("[SkillTreeController] No SkillTreeGraph assigned!");
                return;
            }

            InitializeManager();
        }

        private void InitializeManager()
        {
            _eventBus = new SkillTreeEventBus();
            _manager = new SkillTreeManager(_skillTreeGraph, _eventBus);

            if (_debugMode)
            {
                SubscribeToDebugEvents();
            }

            Debug.Log($"[SkillTreeController] Initialized manager for: {_skillTreeGraph.DisplayName}");
        }

        private void SubscribeToDebugEvents()
        {
            _manager.Subscribe<SkillUnlockedEvent>(OnSkillUnlocked);
            _manager.Subscribe<SkillLockedEvent>(OnSkillLocked);
            _manager.Subscribe<SkillUnlockFailedEvent>(OnSkillUnlockFailed);
            _manager.Subscribe<SkillCostPaidEvent>(OnSkillCostPaid);
            _manager.Subscribe<TreeResetEvent>(OnTreeReset);
            _manager.Subscribe<ResourceChangedEvent>(OnResourceChanged);
        }

        #region Debug Event Handlers

        private void OnSkillUnlocked(SkillUnlockedEvent evt)
        {
            Debug.Log($"[Debug] Skill Unlocked: {evt.Node.DisplayName}");
        }

        private void OnSkillLocked(SkillLockedEvent evt)
        {
            Debug.Log($"[Debug] Skill Locked: {evt.Node.DisplayName}");
        }

        private void OnSkillUnlockFailed(SkillUnlockFailedEvent evt)
        {
            Debug.LogWarning($"[Debug] Unlock Failed: {evt.Reason}");
        }

        private void OnSkillCostPaid(SkillCostPaidEvent evt)
        {
            Debug.Log($"[Debug] Cost Paid: {evt.Cost.Amount} {evt.Cost.ResourceType}");
        }

        private void OnTreeReset(TreeResetEvent evt)
        {
            Debug.Log($"[Debug] Tree Reset: {evt.NodesUnlocked} nodes were unlocked");
        }

        private void OnResourceChanged(ResourceChangedEvent evt)
        {
            Debug.Log($"[Debug] Resource Changed: {evt.ResourceType} ({evt.OldAmount} -> {evt.NewAmount})");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Attempts to unlock a skill node.
        /// </summary>
        public bool TryUnlockSkill(string nodeId)
        {
            if (_manager == null)
            {
                Debug.LogError("[SkillTreeController] Manager not initialized!");
                return false;
            }

            return _manager.UnlockNode(nodeId);
        }

        /// <summary>
        /// Attempts to lock a skill node (for respec).
        /// </summary>
        public bool TryLockSkill(string nodeId)
        {
            if (_manager == null)
            {
                Debug.LogError("[SkillTreeController] Manager not initialized!");
                return false;
            }

            return _manager.LockNode(nodeId);
        }

        /// <summary>
        /// Checks if a skill is unlocked.
        /// </summary>
        public bool IsSkillUnlocked(string nodeId)
        {
            return _manager?.IsNodeUnlocked(nodeId) ?? false;
        }

        /// <summary>
        /// Gets the validation result for unlocking a skill.
        /// Useful for UI to show why a skill can't be unlocked.
        /// </summary>
        public UnlockValidationResult GetUnlockValidation(string nodeId)
        {
            if (_manager == null)
            {
                return new UnlockValidationResult
                {
                    NodeId = nodeId,
                    IsValid = false,
                    FailureMessage = "Manager not initialized"
                };
            }

            return _manager.ValidateUnlock(nodeId);
        }

        /// <summary>
        /// Resets the entire skill tree.
        /// </summary>
        public void ResetTree()
        {
            _manager?.ResetTree();
        }

        /// <summary>
        /// Sets a resource amount.
        /// </summary>
        public void SetResource(string resourceType, float amount)
        {
            _manager?.SetResource(resourceType, amount);
        }

        /// <summary>
        /// Adds to a resource amount.
        /// </summary>
        public void AddResource(string resourceType, float amount)
        {
            _manager?.AddResource(resourceType, amount);
        }

        /// <summary>
        /// Gets a resource amount.
        /// </summary>
        public float GetResource(string resourceType)
        {
            return _manager?.GetResource(resourceType) ?? 0f;
        }

        #endregion
    }
}
