using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.SaveSystem
{
    /// <summary>
    /// Serializable data structure for saving/loading skill tree state.
    /// Versioned for future migrations.
    /// </summary>
    [Serializable]
    public class SkillTreeSaveData
    {
        /// <summary>
        /// Version of the save data format. Used for migrations.
        /// Current version: 1
        /// </summary>
        public int Version = 1;

        /// <summary>
        /// Timestamp when this save was created (UTC ticks)
        /// </summary>
        public long SaveTimestamp;

        /// <summary>
        /// ID of the skill tree this save data belongs to
        /// </summary>
        public string SkillTreeId;

        /// <summary>
        /// Collection of unlocked nodes
        /// </summary>
        public List<UnlockedNodeData> UnlockedNodes = new List<UnlockedNodeData>();

        /// <summary>
        /// Global resources (e.g., skill points, currencies)
        /// Stored as a list for Unity JsonUtility serialization compatibility
        /// </summary>
        public List<ResourceData> Resources = new List<ResourceData>();

        /// <summary>
        /// Custom data per skill tree (for extensions and custom implementations)
        /// Stored as a list for Unity JsonUtility serialization compatibility
        /// </summary>
        public List<CustomDataEntry> CustomData = new List<CustomDataEntry>();

        public SkillTreeSaveData()
        {
            SaveTimestamp = DateTime.UtcNow.Ticks;
        }
    }

    /// <summary>
    /// Data for a single unlocked node
    /// </summary>
    [Serializable]
    public class UnlockedNodeData
    {
        /// <summary>
        /// Unique identifier of the unlocked node
        /// </summary>
        public string NodeId;

        /// <summary>
        /// Timestamp when this node was unlocked (UTC ticks)
        /// </summary>
        public long UnlockTimestamp;

        /// <summary>
        /// Custom data specific to this node instance (for extensions)
        /// Stored as a list for Unity JsonUtility serialization compatibility
        /// </summary>
        public List<CustomDataEntry> NodeCustomData = new List<CustomDataEntry>();

        public UnlockedNodeData(string nodeId)
        {
            NodeId = nodeId;
            UnlockTimestamp = DateTime.UtcNow.Ticks;
        }

        // Parameterless constructor for serialization
        public UnlockedNodeData()
        {
        }
    }

    /// <summary>
    /// Serializable resource data entry (for Unity JsonUtility compatibility)
    /// </summary>
    [Serializable]
    public class ResourceData
    {
        public string Key;
        public float Value;

        public ResourceData()
        {
        }

        public ResourceData(string key, float value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Serializable custom data entry (for Unity JsonUtility compatibility)
    /// </summary>
    [Serializable]
    public class CustomDataEntry
    {
        public string Key;
        public string Value;

        public CustomDataEntry()
        {
        }

        public CustomDataEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Helper methods for working with save data
    /// </summary>
    public static class SaveDataExtensions
    {
        /// <summary>
        /// Checks if a node is unlocked in the save data
        /// </summary>
        public static bool IsNodeUnlocked(this SkillTreeSaveData saveData, string nodeId)
        {
            return saveData.UnlockedNodes.Exists(n => n.NodeId == nodeId);
        }

        /// <summary>
        /// Gets unlocked node data by ID
        /// </summary>
        public static UnlockedNodeData GetUnlockedNode(this SkillTreeSaveData saveData, string nodeId)
        {
            return saveData.UnlockedNodes.Find(n => n.NodeId == nodeId);
        }

        /// <summary>
        /// Adds an unlocked node to the save data
        /// </summary>
        public static void AddUnlockedNode(this SkillTreeSaveData saveData, string nodeId)
        {
            if (!saveData.IsNodeUnlocked(nodeId))
            {
                saveData.UnlockedNodes.Add(new UnlockedNodeData(nodeId));
            }
        }

        /// <summary>
        /// Removes an unlocked node from the save data
        /// </summary>
        public static void RemoveUnlockedNode(this SkillTreeSaveData saveData, string nodeId)
        {
            saveData.UnlockedNodes.RemoveAll(n => n.NodeId == nodeId);
        }

        /// <summary>
        /// Gets a resource value, or 0 if not found
        /// </summary>
        public static float GetResource(this SkillTreeSaveData saveData, string resourceName)
        {
            var resource = saveData.Resources.Find(r => r.Key == resourceName);
            return resource != null ? resource.Value : 0f;
        }

        /// <summary>
        /// Sets a resource value
        /// </summary>
        public static void SetResource(this SkillTreeSaveData saveData, string resourceName, float value)
        {
            var resource = saveData.Resources.Find(r => r.Key == resourceName);
            if (resource != null)
            {
                resource.Value = value;
            }
            else
            {
                saveData.Resources.Add(new ResourceData(resourceName, value));
            }
        }

        /// <summary>
        /// Adds to a resource value
        /// </summary>
        public static void AddResource(this SkillTreeSaveData saveData, string resourceName, float amount)
        {
            var current = saveData.GetResource(resourceName);
            saveData.SetResource(resourceName, current + amount);
        }

        /// <summary>
        /// Gets custom data, or null if not found
        /// </summary>
        public static string GetCustomData(this SkillTreeSaveData saveData, string key)
        {
            var entry = saveData.CustomData.Find(e => e.Key == key);
            return entry?.Value;
        }

        /// <summary>
        /// Sets custom data
        /// </summary>
        public static void SetCustomData(this SkillTreeSaveData saveData, string key, string jsonData)
        {
            var entry = saveData.CustomData.Find(e => e.Key == key);
            if (entry != null)
            {
                entry.Value = jsonData;
            }
            else
            {
                saveData.CustomData.Add(new CustomDataEntry(key, jsonData));
            }
        }

        /// <summary>
        /// Gets custom data for a specific node, or null if not found
        /// </summary>
        public static string GetNodeCustomData(this UnlockedNodeData nodeData, string key)
        {
            var entry = nodeData.NodeCustomData.Find(e => e.Key == key);
            return entry?.Value;
        }

        /// <summary>
        /// Sets custom data for a specific node
        /// </summary>
        public static void SetNodeCustomData(this UnlockedNodeData nodeData, string key, string jsonData)
        {
            var entry = nodeData.NodeCustomData.Find(e => e.Key == key);
            if (entry != null)
            {
                entry.Value = jsonData;
            }
            else
            {
                nodeData.NodeCustomData.Add(new CustomDataEntry(key, jsonData));
            }
        }
    }
}
