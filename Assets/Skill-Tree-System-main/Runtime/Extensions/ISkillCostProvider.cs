using SkillTree.Core.Data;
using SkillTree.Core.Logic;

namespace SkillTree.Extensions
{
    /// <summary>
    /// Interface for creating custom cost systems.
    /// Implement this to add specialized resource consumption logic (e.g., multiple currencies, dynamic costs).
    /// </summary>
    /// <example>
    /// public class MultiCurrencyCostProvider : ISkillCostProvider
    /// {
    ///     public string CostType => "MultiCurrency";
    ///
    ///     public bool CanAfford(SkillCost cost, ISkillTreeState state, object context)
    ///     {
    ///         int goldCost = int.Parse(cost.CustomData["Gold"]);
    ///         int gemsCost = int.Parse(cost.CustomData["Gems"]);
    ///
    ///         return state.HasResource("Gold", goldCost) &&
    ///                state.HasResource("Gems", gemsCost);
    ///     }
    ///
    ///     public void PayCost(SkillCost cost, SkillTreeState state)
    ///     {
    ///         int goldCost = int.Parse(cost.CustomData["Gold"]);
    ///         int gemsCost = int.Parse(cost.CustomData["Gems"]);
    ///
    ///         state.ConsumeResource("Gold", goldCost);
    ///         state.ConsumeResource("Gems", gemsCost);
    ///     }
    ///
    ///     public string GetCostDescription(SkillCost cost)
    ///     {
    ///         int goldCost = int.Parse(cost.CustomData["Gold"]);
    ///         int gemsCost = int.Parse(cost.CustomData["Gems"]);
    ///         return $"{goldCost} Gold, {gemsCost} Gems";
    ///     }
    /// }
    /// </example>
    public interface ISkillCostProvider
    {
        /// <summary>
        /// Unique identifier for this cost type.
        /// Set the SkillCost.CustomData["CostProvider"] to this value to use this provider.
        /// </summary>
        string CostType { get; }

        /// <summary>
        /// Checks if the player can afford this cost.
        /// </summary>
        /// <param name="cost">The cost to check</param>
        /// <param name="state">Current skill tree state (read-only)</param>
        /// <param name="context">Optional game context (e.g., Inventory, Player)</param>
        /// <returns>True if cost can be paid, false otherwise</returns>
        bool CanAfford(SkillCost cost, ISkillTreeState state, object context = null);

        /// <summary>
        /// Deducts the cost from player resources.
        /// Only called after CanAfford returns true.
        /// </summary>
        /// <param name="cost">The cost to pay</param>
        /// <param name="state">Mutable skill tree state (can modify resources)</param>
        /// <param name="context">Optional game context (e.g., Inventory, Player)</param>
        void PayCost(SkillCost cost, object state, object context = null);

        /// <summary>
        /// Gets a human-readable description of the cost.
        /// Used for displaying in UI (e.g., "50 Gold, 10 Gems").
        /// </summary>
        /// <param name="cost">The cost to describe</param>
        /// <returns>Formatted cost description</returns>
        string GetCostDescription(SkillCost cost);

        /// <summary>
        /// Optional: Refunds the cost when a node is locked (respec).
        /// Default implementation does nothing.
        /// </summary>
        /// <param name="cost">The cost to refund</param>
        /// <param name="state">Mutable skill tree state</param>
        /// <param name="context">Optional game context</param>
        void RefundCost(SkillCost cost, object state, object context = null) { }
    }
}
