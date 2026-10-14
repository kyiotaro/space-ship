using System;
using System.Collections.Generic;
using SkillTree.Core.Data;
using SkillTree.Core.Events;

namespace SkillTree.Core.Logic
{
    /// <summary>
    /// Primary API for managing skill tree state and operations.
    /// This is the main interface that game code will interact with.
    /// </summary>
    public interface ISkillTreeManager
    {
        /// <summary>
        /// The skill tree graph being managed.
        /// </summary>
        SkillTreeGraph Graph { get; }

        /// <summary>
        /// The current state of the skill tree.
        /// </summary>
        ISkillTreeState State { get; }

        /// <summary>
        /// Checks if a node is currently unlocked.
        /// </summary>
        bool IsNodeUnlocked(string nodeId);

        /// <summary>
        /// Checks if a node can be unlocked (prerequisites, requirements, costs are met).
        /// </summary>
        bool CanUnlockNode(string nodeId);

        /// <summary>
        /// Attempts to unlock a node. Returns true if successful.
        /// Validates prerequisites, requirements, and costs before unlocking.
        /// Publishes SkillUnlockedEvent or SkillUnlockFailedEvent.
        /// </summary>
        bool UnlockNode(string nodeId);

        /// <summary>
        /// Checks if a node can be locked (for respec functionality).
        /// </summary>
        bool CanLockNode(string nodeId);

        /// <summary>
        /// Attempts to lock a node (remove unlock). Returns true if successful.
        /// Validates that no unlocked dependents would become invalid.
        /// Publishes SkillLockedEvent if successful.
        /// </summary>
        bool LockNode(string nodeId);

        /// <summary>
        /// Resets the entire skill tree (unlocks all nodes).
        /// Publishes TreeResetEvent.
        /// </summary>
        void ResetTree();

        /// <summary>
        /// Gets a copy of the current state for serialization.
        /// </summary>
        SkillTreeState GetState();

        /// <summary>
        /// Loads state from a SkillTreeState object.
        /// Publishes StateLoadedEvent.
        /// </summary>
        void LoadState(SkillTreeState state);

        /// <summary>
        /// Gets all nodes that can currently be unlocked.
        /// </summary>
        List<SkillNode> GetUnlockableNodes();

        /// <summary>
        /// Gets a specific node by ID.
        /// </summary>
        SkillNode GetNode(string nodeId);

        /// <summary>
        /// Gets all prerequisites for a node.
        /// </summary>
        List<SkillNode> GetPrerequisites(string nodeId);

        /// <summary>
        /// Gets all nodes that depend on this node.
        /// </summary>
        List<SkillNode> GetDependents(string nodeId);

        /// <summary>
        /// Gets the current amount of a resource.
        /// </summary>
        float GetResource(string resourceType);

        /// <summary>
        /// Sets the amount of a resource.
        /// Publishes ResourceChangedEvent.
        /// </summary>
        void SetResource(string resourceType, float amount);

        /// <summary>
        /// Adds to a resource amount.
        /// Publishes ResourceChangedEvent.
        /// </summary>
        void AddResource(string resourceType, float amount);

        /// <summary>
        /// Subscribes to a specific event type.
        /// Returns an IDisposable that can be used to unsubscribe.
        /// </summary>
        IDisposable Subscribe<T>(Action<T> handler) where T : SkillTreeEvent;

        /// <summary>
        /// Gets detailed validation result for an unlock attempt.
        /// Useful for UI feedback.
        /// </summary>
        UnlockValidationResult ValidateUnlock(string nodeId);

        /// <summary>
        /// Gets detailed validation result for a lock attempt.
        /// </summary>
        LockValidationResult ValidateLock(string nodeId);
    }
}
