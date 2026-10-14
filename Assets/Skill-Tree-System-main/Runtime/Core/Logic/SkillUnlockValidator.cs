using System;
using System.Collections.Generic;
using System.Linq;
using SkillTree.Core.Data;
using SkillTree.Core.Events;

namespace SkillTree.Core.Logic
{
    /// <summary>
    /// Validates whether a skill node can be unlocked based on prerequisites, requirements, and costs.
    /// </summary>
    public class SkillUnlockValidator
    {
        private readonly SkillTreeGraph _graph;
        private readonly SkillTreeTraverser _traverser;

        public SkillUnlockValidator(SkillTreeGraph graph)
        {
            _graph = graph;
            _traverser = new SkillTreeTraverser(graph);
        }

        /// <summary>
        /// Checks if a node can be unlocked and returns a validation result.
        /// </summary>
        public UnlockValidationResult CanUnlock(string nodeId, ISkillTreeState state)
        {
            var result = new UnlockValidationResult { NodeId = nodeId };

            // Check if node exists
            var node = _graph.GetNodeById(nodeId);
            if (node == null)
            {
                result.IsValid = false;
                result.FailureReason = UnlockFailureReason.NodeNotFound;
                result.FailureMessage = $"Node with ID '{nodeId}' does not exist in the skill tree";
                return result;
            }

            result.Node = node;

            // Check if already unlocked
            if (state.IsNodeUnlocked(nodeId))
            {
                result.IsValid = false;
                result.FailureReason = UnlockFailureReason.AlreadyUnlocked;
                result.FailureMessage = $"Node '{node.DisplayName}' is already unlocked";
                return result;
            }

            // Check prerequisites
            if (!ValidatePrerequisites(node, state, result))
            {
                result.IsValid = false;
                result.FailureReason = UnlockFailureReason.PrerequisitesNotMet;
                return result;
            }

            // Check requirements
            if (!ValidateRequirements(node, state, result))
            {
                result.IsValid = false;
                result.FailureReason = UnlockFailureReason.RequirementsNotMet;
                return result;
            }

            // Check costs
            if (!ValidateCosts(node, state, result))
            {
                result.IsValid = false;
                result.FailureReason = UnlockFailureReason.InsufficientResources;
                return result;
            }

            result.IsValid = true;
            result.FailureMessage = null;
            return result;
        }

        private bool ValidatePrerequisites(SkillNode node, ISkillTreeState state, UnlockValidationResult result)
        {
            if (!node.HasPrerequisites())
                return true;

            var unmetPrerequisites = new List<string>();

            foreach (var prerequisiteId in node.PrerequisiteNodeIds)
            {
                if (!state.IsNodeUnlocked(prerequisiteId))
                {
                    var prerequisiteNode = _graph.GetNodeById(prerequisiteId);
                    var displayName = prerequisiteNode?.DisplayName ?? prerequisiteId;
                    unmetPrerequisites.Add(displayName);
                }
            }

            if (unmetPrerequisites.Count > 0)
            {
                result.UnmetPrerequisites = unmetPrerequisites;
                result.FailureMessage = $"Prerequisites not met: {string.Join(", ", unmetPrerequisites)}";
                return false;
            }

            return true;
        }

        private bool ValidateRequirements(SkillNode node, ISkillTreeState state, UnlockValidationResult result)
        {
            if (!node.HasRequirements())
                return true;

            var unmetRequirements = new List<string>();

            foreach (var requirement in node.Requirements)
            {
                if (!EvaluateRequirement(requirement, state))
                {
                    unmetRequirements.Add(GetRequirementDescription(requirement));
                }
            }

            if (unmetRequirements.Count > 0)
            {
                result.UnmetRequirements = unmetRequirements;
                result.FailureMessage = $"Requirements not met: {string.Join(", ", unmetRequirements)}";
                return false;
            }

            return true;
        }

        private bool EvaluateRequirement(SkillRequirement requirement, ISkillTreeState state)
        {
            switch (requirement.Type)
            {
                case RequirementType.NodeUnlocked:
                    return EvaluateNodeUnlockedRequirement(requirement, state);

                case RequirementType.StatCheck:
                case RequirementType.LevelCheck:
                    // These would need to be implemented with a custom evaluator
                    // For now, we'll return true (assume met)
                    return true;

                case RequirementType.Custom:
                    // Custom requirements would be evaluated by registered evaluators
                    return true;

                default:
                    return true;
            }
        }

        private bool EvaluateNodeUnlockedRequirement(SkillRequirement requirement, ISkillTreeState state)
        {
            if (requirement.RequiredNodeIds == null || requirement.RequiredNodeIds.Count == 0)
                return true;

            switch (requirement.Logic)
            {
                case RequirementLogic.And:
                    return requirement.RequiredNodeIds.All(id => state.IsNodeUnlocked(id));

                case RequirementLogic.Or:
                    return requirement.RequiredNodeIds.Any(id => state.IsNodeUnlocked(id));

                case RequirementLogic.Count:
                    var unlockedCount = requirement.RequiredNodeIds.Count(id => state.IsNodeUnlocked(id));
                    return unlockedCount >= requirement.RequiredCount;

                default:
                    return true;
            }
        }

        private string GetRequirementDescription(SkillRequirement requirement)
        {
            switch (requirement.Type)
            {
                case RequirementType.NodeUnlocked:
                    return $"Unlock {requirement.RequiredCount} of {requirement.RequiredNodeIds.Count} related skills";

                case RequirementType.StatCheck:
                    return "Stat requirement not met";

                case RequirementType.LevelCheck:
                    return "Level requirement not met";

                case RequirementType.Custom:
                    return requirement.CustomCondition ?? "Custom requirement not met";

                default:
                    return "Unknown requirement";
            }
        }

        private bool ValidateCosts(SkillNode node, ISkillTreeState state, UnlockValidationResult result)
        {
            if (!node.HasCosts())
                return true;

            var insufficientResources = new Dictionary<string, float>();

            foreach (var cost in node.Costs)
            {
                var available = state.GetResource(cost.ResourceType);
                if (available < cost.Amount)
                {
                    insufficientResources[cost.ResourceType] = cost.Amount - available;
                }
            }

            if (insufficientResources.Count > 0)
            {
                result.InsufficientResources = insufficientResources;
                var messages = insufficientResources.Select(kvp =>
                    $"{kvp.Key}: need {kvp.Value} more");
                result.FailureMessage = $"Insufficient resources: {string.Join(", ", messages)}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a node can be locked (e.g., for respec).
        /// A node can be locked if no unlocked dependents require it.
        /// </summary>
        public LockValidationResult CanLock(string nodeId, ISkillTreeState state)
        {
            var result = new LockValidationResult { NodeId = nodeId };

            var node = _graph.GetNodeById(nodeId);
            if (node == null)
            {
                result.IsValid = false;
                result.FailureMessage = $"Node with ID '{nodeId}' does not exist";
                return result;
            }

            result.Node = node;

            if (!state.IsNodeUnlocked(nodeId))
            {
                result.IsValid = false;
                result.FailureMessage = $"Node '{node.DisplayName}' is not unlocked";
                return result;
            }

            // Check if any unlocked dependents would become invalid
            var dependents = _graph.GetDependents(nodeId);
            var blockedDependents = new List<string>();

            foreach (var dependent in dependents)
            {
                if (state.IsNodeUnlocked(dependent.Id))
                {
                    // Check if this dependent has other prerequisites besides the node being locked
                    var otherPrerequisites = dependent.PrerequisiteNodeIds
                        .Where(id => id != nodeId)
                        .ToList();

                    // If the dependent has no other prerequisites, or if its other prerequisites aren't all met,
                    // then locking this node would invalidate the dependent
                    if (otherPrerequisites.Count == 0 || !otherPrerequisites.All(id => state.IsNodeUnlocked(id)))
                    {
                        blockedDependents.Add(dependent.DisplayName);
                    }
                }
            }

            if (blockedDependents.Count > 0)
            {
                result.IsValid = false;
                result.BlockedDependents = blockedDependents;
                result.FailureMessage = $"Cannot lock: Required by {string.Join(", ", blockedDependents)}";
                return result;
            }

            result.IsValid = true;
            return result;
        }
    }

    /// <summary>
    /// Result of validating a node unlock attempt.
    /// </summary>
    public class UnlockValidationResult
    {
        public string NodeId { get; set; }
        public SkillNode Node { get; set; }
        public bool IsValid { get; set; }
        public UnlockFailureReason FailureReason { get; set; }
        public string FailureMessage { get; set; }
        public List<string> UnmetPrerequisites { get; set; } = new List<string>();
        public List<string> UnmetRequirements { get; set; } = new List<string>();
        public Dictionary<string, float> InsufficientResources { get; set; } = new Dictionary<string, float>();
    }

    /// <summary>
    /// Result of validating a node lock attempt.
    /// </summary>
    public class LockValidationResult
    {
        public string NodeId { get; set; }
        public SkillNode Node { get; set; }
        public bool IsValid { get; set; }
        public string FailureMessage { get; set; }
        public List<string> BlockedDependents { get; set; } = new List<string>();
    }
}
