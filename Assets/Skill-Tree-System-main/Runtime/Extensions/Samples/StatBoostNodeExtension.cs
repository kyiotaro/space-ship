using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using UnityEngine;

namespace SkillTree.Extensions.Samples
{
    /// <summary>
    /// Sample node extension that increases player stats when unlocked.
    /// This is a reference implementation showing how to create custom node behaviors.
    /// </summary>
    /// <remarks>
    /// To use this extension:
    /// 1. Register it: SkillTreeExtensionRegistry.Instance.RegisterNodeExtension(new StatBoostNodeExtension());
    /// 2. Set node's CustomData["NodeExtensionType"] = "StatBoost"
    /// 3. Set node's CustomData["StatType"] = "Health" (or "Damage", "Defense", etc.)
    /// 4. Set node's CustomData["Amount"] = "10" (the boost amount)
    /// </remarks>
    public class StatBoostNodeExtension : ISkillNodeExtension
    {
        public string NodeTypeId => "StatBoost";

        // In a real implementation, you'd inject or reference your game's stat system
        private IPlayerStatsSystem _statsSystem;

        public StatBoostNodeExtension(IPlayerStatsSystem statsSystem = null)
        {
            _statsSystem = statsSystem ?? new DefaultPlayerStatsSystem();
        }

        public void OnNodeUnlocked(SkillNode node, ISkillTreeState state)
        {
            if (TryGetStatData(node, out string statType, out float amount))
            {
                _statsSystem.IncreaseStat(statType, amount);
                Debug.Log($"[StatBoost] Increased {statType} by {amount} (Node: {node.DisplayName})");
            }
            else
            {
                Debug.LogWarning($"[StatBoost] Invalid stat data for node {node.Id}");
            }
        }

        public void OnNodeLocked(SkillNode node, ISkillTreeState state)
        {
            if (TryGetStatData(node, out string statType, out float amount))
            {
                _statsSystem.DecreaseStat(statType, amount);
                Debug.Log($"[StatBoost] Decreased {statType} by {amount} (Node: {node.DisplayName})");
            }
        }

        public string ValidateUnlock(SkillNode node, ISkillTreeState state)
        {
            // Validate that the node has the required data
            if (!node.CustomData.ContainsKey("StatType"))
                return "Missing 'StatType' in node custom data";

            if (!node.CustomData.ContainsKey("Amount"))
                return "Missing 'Amount' in node custom data";

            // Validate that amount is a valid number
            if (!float.TryParse(node.CustomData["Amount"], out _))
                return "Invalid 'Amount' value - must be a number";

            return null; // Valid
        }

        private bool TryGetStatData(SkillNode node, out string statType, out float amount)
        {
            statType = null;
            amount = 0;

            if (!node.CustomData.TryGetValue("StatType", out statType))
                return false;

            if (!node.CustomData.TryGetValue("Amount", out string amountStr))
                return false;

            return float.TryParse(amountStr, out amount);
        }
    }

    /// <summary>
    /// Interface for your game's stat system.
    /// Implement this to integrate with your actual player stats.
    /// </summary>
    public interface IPlayerStatsSystem
    {
        void IncreaseStat(string statType, float amount);
        void DecreaseStat(string statType, float amount);
        float GetStat(string statType);
    }

    /// <summary>
    /// Default implementation that just logs stat changes.
    /// Replace with your actual stat system.
    /// </summary>
    public class DefaultPlayerStatsSystem : IPlayerStatsSystem
    {
        private readonly System.Collections.Generic.Dictionary<string, float> _stats =
            new System.Collections.Generic.Dictionary<string, float>();

        public void IncreaseStat(string statType, float amount)
        {
            if (_stats.ContainsKey(statType))
                _stats[statType] += amount;
            else
                _stats[statType] = amount;

            Debug.Log($"[Stats] {statType} is now {_stats[statType]}");
        }

        public void DecreaseStat(string statType, float amount)
        {
            if (_stats.ContainsKey(statType))
                _stats[statType] -= amount;
            else
                _stats[statType] = -amount;

            Debug.Log($"[Stats] {statType} is now {_stats[statType]}");
        }

        public float GetStat(string statType)
        {
            return _stats.TryGetValue(statType, out float value) ? value : 0f;
        }
    }
}
