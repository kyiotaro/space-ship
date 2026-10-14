using SkillTree.Core.Data;
using SkillTree.Core.Logic;

namespace SkillTree.Extensions
{
    /// <summary>
    /// Interface for creating custom node types with specialized behaviors.
    /// Implement this to add custom logic when nodes are unlocked or locked.
    /// </summary>
    /// <example>
    /// public class StatBoostNodeExtension : ISkillNodeExtension
    /// {
    ///     public string NodeTypeId => "StatBoost";
    ///
    ///     public void OnNodeUnlocked(SkillNode node, ISkillTreeState state)
    ///     {
    ///         // Increase player stats based on node data
    ///         var statType = node.CustomData["StatType"];
    ///         var amount = float.Parse(node.CustomData["Amount"]);
    ///         PlayerStats.IncreaseStat(statType, amount);
    ///     }
    ///
    ///     public void OnNodeLocked(SkillNode node, ISkillTreeState state)
    ///     {
    ///         // Decrease player stats when node is locked (respec)
    ///         var statType = node.CustomData["StatType"];
    ///         var amount = float.Parse(node.CustomData["Amount"]);
    ///         PlayerStats.DecreaseStat(statType, amount);
    ///     }
    /// }
    /// </example>
    public interface ISkillNodeExtension
    {
        /// <summary>
        /// Unique identifier for this node type.
        /// This ID should match the node's CustomData["NodeExtensionType"] value.
        /// </summary>
        string NodeTypeId { get; }

        /// <summary>
        /// Called when a node is successfully unlocked.
        /// Use this to apply game effects, grant abilities, or update player stats.
        /// </summary>
        /// <param name="node">The node that was unlocked</param>
        /// <param name="state">Current skill tree state (read-only)</param>
        void OnNodeUnlocked(SkillNode node, ISkillTreeState state);

        /// <summary>
        /// Called when a node is locked (e.g., during respec).
        /// Use this to remove game effects, revoke abilities, or restore player stats.
        /// </summary>
        /// <param name="node">The node that was locked</param>
        /// <param name="state">Current skill tree state (read-only)</param>
        void OnNodeLocked(SkillNode node, ISkillTreeState state);

        /// <summary>
        /// Optional: Validate if this node can be unlocked based on custom game logic.
        /// Return null if valid, or an error message if invalid.
        /// </summary>
        /// <param name="node">The node being validated</param>
        /// <param name="state">Current skill tree state (read-only)</param>
        /// <returns>Null if valid, error message if invalid</returns>
        string ValidateUnlock(SkillNode node, ISkillTreeState state) => null;
    }
}
