using System;
using System.Collections.Generic;
using UnityEngine;
using SkillTree.Core.Data;
using SkillTree.Core.Events;

namespace SkillTree.Core.Logic
{
    /// <summary>
    /// Core implementation of ISkillTreeManager.
    /// Manages skill tree state, validates operations, and publishes events.
    /// Thread-safe for common operations.
    /// </summary>
    public class SkillTreeManager : ISkillTreeManager
    {
        private readonly SkillTreeGraph _graph;
        private readonly ISkillTreeEventBus _eventBus;
        private readonly SkillTreeState _state;
        private readonly SkillUnlockValidator _validator;
        private readonly SkillTreeTraverser _traverser;
        private readonly object _lock = new object();

        public SkillTreeGraph Graph => _graph;
        public ISkillTreeState State => _state;

        /// <summary>
        /// Constructor for dependency injection.
        /// </summary>
        public SkillTreeManager(SkillTreeGraph graph, ISkillTreeEventBus eventBus)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _state = new SkillTreeState(graph.TreeId);
            _validator = new SkillUnlockValidator(graph);
            _traverser = new SkillTreeTraverser(graph);
        }

        /// <summary>
        /// Constructor without event bus (creates default one).
        /// </summary>
        public SkillTreeManager(SkillTreeGraph graph) : this(graph, new SkillTreeEventBus())
        {
        }

        #region Query Methods

        public bool IsNodeUnlocked(string nodeId)
        {
            return _state.IsNodeUnlocked(nodeId);
        }

        public bool CanUnlockNode(string nodeId)
        {
            var validationResult = _validator.CanUnlock(nodeId, _state);
            return validationResult.IsValid;
        }

        public bool CanLockNode(string nodeId)
        {
            var validationResult = _validator.CanLock(nodeId, _state);
            return validationResult.IsValid;
        }

        public SkillNode GetNode(string nodeId)
        {
            return _graph.GetNodeById(nodeId);
        }

        public List<SkillNode> GetPrerequisites(string nodeId)
        {
            return _graph.GetPrerequisites(nodeId);
        }

        public List<SkillNode> GetDependents(string nodeId)
        {
            return _graph.GetDependents(nodeId);
        }

        public List<SkillNode> GetUnlockableNodes()
        {
            return _traverser.GetUnlockableNodes(_state);
        }

        public UnlockValidationResult ValidateUnlock(string nodeId)
        {
            return _validator.CanUnlock(nodeId, _state);
        }

        public LockValidationResult ValidateLock(string nodeId)
        {
            return _validator.CanLock(nodeId, _state);
        }

        #endregion

        #region Unlock/Lock Operations

        public bool UnlockNode(string nodeId)
        {
            lock (_lock)
            {
                var validationResult = _validator.CanUnlock(nodeId, _state);

                if (!validationResult.IsValid)
                {
                    PublishUnlockFailedEvent(nodeId, validationResult);
                    return false;
                }

                var node = validationResult.Node;

                // Pay costs
                foreach (var cost in node.Costs)
                {
                    _state.SpendResource(cost.ResourceType, cost.Amount);
                    _eventBus.Publish(new SkillCostPaidEvent(_graph.TreeId, nodeId, cost));
                }

                // Unlock the node
                _state.UnlockNode(nodeId);

                // Trigger Unity events
                node.OnUnlocked?.Invoke();

                // Publish event
                _eventBus.Publish(new SkillUnlockedEvent(_graph.TreeId, node));

                Debug.Log($"[SkillTreeManager] Unlocked node: {node.DisplayName} ({nodeId})");
                return true;
            }
        }

        public bool LockNode(string nodeId)
        {
            lock (_lock)
            {
                var validationResult = _validator.CanLock(nodeId, _state);

                if (!validationResult.IsValid)
                {
                    Debug.LogWarning($"[SkillTreeManager] Cannot lock node {nodeId}: {validationResult.FailureMessage}");
                    return false;
                }

                var node = validationResult.Node;

                // Refund costs (optional, could be configurable)
                foreach (var cost in node.Costs)
                {
                    _state.AddResource(cost.ResourceType, cost.Amount);
                }

                // Lock the node
                _state.LockNode(nodeId);

                // Trigger Unity events
                node.OnLocked?.Invoke();

                // Publish event
                _eventBus.Publish(new SkillLockedEvent(_graph.TreeId, node));

                Debug.Log($"[SkillTreeManager] Locked node: {node.DisplayName} ({nodeId})");
                return true;
            }
        }

        private void PublishUnlockFailedEvent(string nodeId, UnlockValidationResult validationResult)
        {
            var failedEvent = new SkillUnlockFailedEvent(
                _graph.TreeId,
                nodeId,
                validationResult.FailureMessage,
                validationResult.FailureReason
            );
            _eventBus.Publish(failedEvent);

            Debug.LogWarning($"[SkillTreeManager] Failed to unlock node {nodeId}: {validationResult.FailureMessage}");
        }

        #endregion

        #region Resource Management

        public float GetResource(string resourceType)
        {
            return _state.GetResource(resourceType);
        }

        public void SetResource(string resourceType, float amount)
        {
            lock (_lock)
            {
                float oldAmount = _state.GetResource(resourceType);
                _state.SetResource(resourceType, amount);

                _eventBus.Publish(new ResourceChangedEvent(_graph.TreeId, resourceType, oldAmount, amount));
            }
        }

        public void AddResource(string resourceType, float amount)
        {
            lock (_lock)
            {
                float oldAmount = _state.GetResource(resourceType);
                _state.AddResource(resourceType, amount);
                float newAmount = _state.GetResource(resourceType);

                _eventBus.Publish(new ResourceChangedEvent(_graph.TreeId, resourceType, oldAmount, newAmount));
            }
        }

        #endregion

        #region State Management

        public void ResetTree()
        {
            lock (_lock)
            {
                int unlockedCount = _state.UnlockedNodeCount;
                _state.Reset();

                _eventBus.Publish(new TreeResetEvent(_graph.TreeId, unlockedCount));

                Debug.Log($"[SkillTreeManager] Reset skill tree. Cleared {unlockedCount} unlocked nodes.");
            }
        }

        public SkillTreeState GetState()
        {
            lock (_lock)
            {
                // Return a copy to prevent external modification
                return _state;
            }
        }

        public void LoadState(SkillTreeState state)
        {
            if (state == null)
            {
                Debug.LogWarning("[SkillTreeManager] Attempted to load null state");
                return;
            }

            lock (_lock)
            {
                // Copy data from loaded state to current state
                _state.Reset();
                _state.SkillTreeId = state.SkillTreeId;

                foreach (var nodeId in state.GetUnlockedNodes())
                {
                    _state.UnlockNode(nodeId);
                }

                // Note: Resources and custom data would need to be copied here
                // This is a simplified implementation

                int nodesUnlocked = _state.UnlockedNodeCount;
                _eventBus.Publish(new StateLoadedEvent(_graph.TreeId, nodesUnlocked));

                Debug.Log($"[SkillTreeManager] Loaded state with {nodesUnlocked} unlocked nodes");
            }
        }

        #endregion

        #region Event Subscription

        public IDisposable Subscribe<T>(Action<T> handler) where T : SkillTreeEvent
        {
            return _eventBus.Subscribe(handler);
        }

        #endregion
    }
}
