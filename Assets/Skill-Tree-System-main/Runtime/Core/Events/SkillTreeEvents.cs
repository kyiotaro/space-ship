using System;
using SkillTree.Core.Data;

namespace SkillTree.Core.Events
{
    /// <summary>
    /// Base class for all skill tree events.
    /// </summary>
    public abstract class SkillTreeEvent
    {
        public string SkillTreeId { get; set; }
        public DateTime Timestamp { get; set; }

        protected SkillTreeEvent(string skillTreeId)
        {
            SkillTreeId = skillTreeId;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Event fired when a skill node is successfully unlocked.
    /// </summary>
    public class SkillUnlockedEvent : SkillTreeEvent
    {
        public SkillNode Node { get; set; }
        public string NodeId { get; set; }

        public SkillUnlockedEvent(string skillTreeId, SkillNode node) : base(skillTreeId)
        {
            Node = node;
            NodeId = node?.Id;
        }
    }

    /// <summary>
    /// Event fired when a skill node is locked (e.g., due to respec).
    /// </summary>
    public class SkillLockedEvent : SkillTreeEvent
    {
        public SkillNode Node { get; set; }
        public string NodeId { get; set; }

        public SkillLockedEvent(string skillTreeId, SkillNode node) : base(skillTreeId)
        {
            Node = node;
            NodeId = node?.Id;
        }
    }

    /// <summary>
    /// Event fired when resources are spent to unlock a skill.
    /// </summary>
    public class SkillCostPaidEvent : SkillTreeEvent
    {
        public string NodeId { get; set; }
        public SkillCost Cost { get; set; }

        public SkillCostPaidEvent(string skillTreeId, string nodeId, SkillCost cost) : base(skillTreeId)
        {
            NodeId = nodeId;
            Cost = cost;
        }
    }

    /// <summary>
    /// Event fired when an unlock attempt fails.
    /// </summary>
    public class SkillUnlockFailedEvent : SkillTreeEvent
    {
        public string NodeId { get; set; }
        public string Reason { get; set; }
        public UnlockFailureReason FailureReason { get; set; }

        public SkillUnlockFailedEvent(string skillTreeId, string nodeId, string reason, UnlockFailureReason failureReason) : base(skillTreeId)
        {
            NodeId = nodeId;
            Reason = reason;
            FailureReason = failureReason;
        }
    }

    /// <summary>
    /// Event fired when the skill tree is reset.
    /// </summary>
    public class TreeResetEvent : SkillTreeEvent
    {
        public int NodesUnlocked { get; set; }

        public TreeResetEvent(string skillTreeId, int nodesUnlocked) : base(skillTreeId)
        {
            NodesUnlocked = nodesUnlocked;
        }
    }

    /// <summary>
    /// Event fired when resources are added to the state.
    /// </summary>
    public class ResourceChangedEvent : SkillTreeEvent
    {
        public string ResourceType { get; set; }
        public float OldAmount { get; set; }
        public float NewAmount { get; set; }
        public float Delta { get; set; }

        public ResourceChangedEvent(string skillTreeId, string resourceType, float oldAmount, float newAmount) : base(skillTreeId)
        {
            ResourceType = resourceType;
            OldAmount = oldAmount;
            NewAmount = newAmount;
            Delta = newAmount - oldAmount;
        }
    }

    /// <summary>
    /// Event fired when state is loaded from save data.
    /// </summary>
    public class StateLoadedEvent : SkillTreeEvent
    {
        public int NodesUnlocked { get; set; }

        public StateLoadedEvent(string skillTreeId, int nodesUnlocked) : base(skillTreeId)
        {
            NodesUnlocked = nodesUnlocked;
        }
    }

    /// <summary>
    /// Event fired when state is saved.
    /// </summary>
    public class StateSavedEvent : SkillTreeEvent
    {
        public string SaveKey { get; set; }

        public StateSavedEvent(string skillTreeId, string saveKey) : base(skillTreeId)
        {
            SaveKey = saveKey;
        }
    }

    /// <summary>
    /// Reasons why a skill unlock might fail.
    /// </summary>
    public enum UnlockFailureReason
    {
        AlreadyUnlocked,
        PrerequisitesNotMet,
        RequirementsNotMet,
        InsufficientResources,
        NodeNotFound,
        Custom
    }
}
