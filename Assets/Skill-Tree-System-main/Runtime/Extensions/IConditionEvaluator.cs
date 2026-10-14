using SkillTree.Core.Data;
using SkillTree.Core.Logic;

namespace SkillTree.Extensions
{
    /// <summary>
    /// Interface for creating custom requirement conditions.
    /// Implement this to add custom validation logic for skill prerequisites.
    /// </summary>
    /// <example>
    /// public class LevelRequirementEvaluator : IConditionEvaluator
    /// {
    ///     public string ConditionType => "LevelRequirement";
    ///
    ///     public bool Evaluate(SkillRequirement requirement, ISkillTreeState state, object context)
    ///     {
    ///         if (context is Player player)
    ///         {
    ///             int requiredLevel = int.Parse(requirement.Parameters["MinLevel"]);
    ///             return player.Level >= requiredLevel;
    ///         }
    ///         return false;
    ///     }
    ///
    ///     public string GetFailureMessage(SkillRequirement requirement)
    ///     {
    ///         int requiredLevel = int.Parse(requirement.Parameters["MinLevel"]);
    ///         return $"Requires level {requiredLevel}";
    ///     }
    /// }
    /// </example>
    public interface IConditionEvaluator
    {
        /// <summary>
        /// Unique identifier for this condition type.
        /// This should match the SkillRequirement.CustomCondition value.
        /// </summary>
        string ConditionType { get; }

        /// <summary>
        /// Evaluates whether the condition is met.
        /// </summary>
        /// <param name="requirement">The requirement definition containing parameters</param>
        /// <param name="state">Current skill tree state (read-only)</param>
        /// <param name="context">Optional game context (e.g., Player, GameState)</param>
        /// <returns>True if condition is met, false otherwise</returns>
        bool Evaluate(SkillRequirement requirement, ISkillTreeState state, object context = null);

        /// <summary>
        /// Gets a human-readable message explaining why this condition failed.
        /// Used for UI feedback to the player.
        /// </summary>
        /// <param name="requirement">The requirement that failed</param>
        /// <returns>User-friendly error message</returns>
        string GetFailureMessage(SkillRequirement requirement);

        /// <summary>
        /// Optional: Gets a description of what this condition checks.
        /// Used for displaying requirements in UI.
        /// </summary>
        /// <param name="requirement">The requirement to describe</param>
        /// <returns>Description text for UI</returns>
        string GetDescription(SkillRequirement requirement) => GetFailureMessage(requirement);
    }
}
