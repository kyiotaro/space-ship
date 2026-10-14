using System;
using System.Collections.Generic;

namespace SkillTree.Core.Logic
{
    /// <summary>
    /// Read-only interface for querying skill tree state.
    /// Used by validators, extensions, and game systems.
    /// </summary>
    public interface ISkillTreeState
    {
        /// <summary>
        /// The ID of the skill tree this state represents.
        /// </summary>
        string SkillTreeId { get; }

        /// <summary>
        /// Checks if a specific node is unlocked.
        /// </summary>
        bool IsNodeUnlocked(string nodeId);

        /// <summary>
        /// Gets the timestamp when a node was unlocked.
        /// </summary>
        DateTime? GetNodeUnlockTime(string nodeId);

        /// <summary>
        /// Gets all unlocked node IDs.
        /// </summary>
        IReadOnlyList<string> GetUnlockedNodes();

        /// <summary>
        /// Gets the current value of a resource (e.g., skill points, gold).
        /// </summary>
        float GetResource(string resourceType);

        /// <summary>
        /// Checks if the player has enough of a specific resource.
        /// </summary>
        bool HasResource(string resourceType, float amount);

        /// <summary>
        /// Gets custom data associated with the tree state.
        /// </summary>
        object GetCustomData(string key);

        /// <summary>
        /// Checks if custom data exists for a key.
        /// </summary>
        bool HasCustomData(string key);

        /// <summary>
        /// Gets custom data associated with a specific node.
        /// </summary>
        object GetNodeCustomData(string nodeId, string key);

        /// <summary>
        /// Total number of unlocked nodes.
        /// </summary>
        int UnlockedNodeCount { get; }
    }
}
