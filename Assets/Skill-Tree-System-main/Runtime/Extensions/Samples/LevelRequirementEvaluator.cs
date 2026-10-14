using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using UnityEngine;

namespace SkillTree.Extensions.Samples
{
    /// <summary>
    /// Sample condition evaluator that checks player level requirements.
    /// This is a reference implementation showing how to create custom requirement conditions.
    /// </summary>
    /// <remarks>
    /// To use this evaluator:
    /// 1. Register it: SkillTreeExtensionRegistry.Instance.RegisterConditionEvaluator(new LevelRequirementEvaluator());
    /// 2. Create a SkillRequirement with Type = Custom
    /// 3. Set requirement.CustomCondition = "LevelRequirement"
    /// 4. Set requirement.Parameters["MinLevel"] = "5" (required level)
    /// 5. Pass your Player object as context when checking unlock: manager.CanUnlockNode(nodeId, playerContext)
    /// </remarks>
    public class LevelRequirementEvaluator : IConditionEvaluator
    {
        public string ConditionType => "LevelRequirement";

        private IPlayerLevelSystem _levelSystem;

        public LevelRequirementEvaluator(IPlayerLevelSystem levelSystem = null)
        {
            _levelSystem = levelSystem ?? new DefaultPlayerLevelSystem();
        }

        public bool Evaluate(SkillRequirement requirement, ISkillTreeState state, object context = null)
        {
            if (!requirement.Parameters.TryGetValue("MinLevel", out string minLevelStr))
            {
                Debug.LogWarning("[LevelRequirement] Missing 'MinLevel' parameter");
                return false;
            }

            if (!int.TryParse(minLevelStr, out int minLevel))
            {
                Debug.LogWarning($"[LevelRequirement] Invalid 'MinLevel' value: {minLevelStr}");
                return false;
            }

            int playerLevel = _levelSystem.GetPlayerLevel();

            bool result = playerLevel >= minLevel;

            if (!result)
            {
                Debug.Log($"[LevelRequirement] Player level {playerLevel} < required {minLevel}");
            }

            return result;
        }

        public string GetFailureMessage(SkillRequirement requirement)
        {
            if (requirement.Parameters.TryGetValue("MinLevel", out string minLevelStr))
            {
                return $"Requires level {minLevelStr}";
            }
            return "Level requirement not met";
        }

        public string GetDescription(SkillRequirement requirement)
        {
            return GetFailureMessage(requirement);
        }
    }

    /// <summary>
    /// Interface for your game's level system.
    /// Implement this to integrate with your actual player level tracking.
    /// </summary>
    public interface IPlayerLevelSystem
    {
        int GetPlayerLevel();
    }

    /// <summary>
    /// Default implementation that returns a fixed level.
    /// Replace with your actual level system.
    /// </summary>
    public class DefaultPlayerLevelSystem : IPlayerLevelSystem
    {
        private int _currentLevel = 1;

        public int GetPlayerLevel()
        {
            return _currentLevel;
        }

        public void SetLevel(int level)
        {
            _currentLevel = level;
            Debug.Log($"[Level] Player level set to {level}");
        }
    }
}
