using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillTree.Core.Logic
{
    /// <summary>
    /// Mutable implementation of ISkillTreeState.
    /// Maintains the runtime state of a skill tree including unlocked nodes and resources.
    /// </summary>
    [Serializable]
    public class SkillTreeState : ISkillTreeState
    {
        [SerializeField] private string _skillTreeId;
        [SerializeField] private List<string> _unlockedNodeIds = new List<string>();
        [SerializeField] private List<NodeUnlockInfo> _nodeUnlockInfos = new List<NodeUnlockInfo>();
        [SerializeField] private List<ResourceEntry> _resources = new List<ResourceEntry>();
        [SerializeField] private List<CustomDataEntry> _customData = new List<CustomDataEntry>();
        [SerializeField] private List<NodeCustomData> _nodeCustomData = new List<NodeCustomData>();

        // Runtime cache for performance
        private HashSet<string> _unlockedNodesCache;
        private Dictionary<string, DateTime> _unlockTimeCache;
        private Dictionary<string, float> _resourceCache;
        private Dictionary<string, object> _customDataCache;
        private Dictionary<string, Dictionary<string, object>> _nodeCustomDataCache;
        private bool _cacheValid = false;

        public string SkillTreeId
        {
            get => _skillTreeId;
            set => _skillTreeId = value;
        }

        public int UnlockedNodeCount => _unlockedNodeIds.Count;

        public SkillTreeState()
        {
            _skillTreeId = string.Empty;
        }

        public SkillTreeState(string skillTreeId)
        {
            _skillTreeId = skillTreeId;
        }

        #region ISkillTreeState Implementation

        public bool IsNodeUnlocked(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return false;
            RebuildCacheIfNeeded();
            return _unlockedNodesCache.Contains(nodeId);
        }

        public DateTime? GetNodeUnlockTime(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            RebuildCacheIfNeeded();
            return _unlockTimeCache.TryGetValue(nodeId, out var time) ? time : (DateTime?)null;
        }

        public IReadOnlyList<string> GetUnlockedNodes()
        {
            return _unlockedNodeIds.AsReadOnly();
        }

        public float GetResource(string resourceType)
        {
            if (string.IsNullOrEmpty(resourceType)) return 0f;
            RebuildCacheIfNeeded();
            return _resourceCache.TryGetValue(resourceType, out var amount) ? amount : 0f;
        }

        public bool HasResource(string resourceType, float amount)
        {
            return GetResource(resourceType) >= amount;
        }

        public object GetCustomData(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            RebuildCacheIfNeeded();
            return _customDataCache.TryGetValue(key, out var value) ? value : null;
        }

        public bool HasCustomData(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            RebuildCacheIfNeeded();
            return _customDataCache.ContainsKey(key);
        }

        public object GetNodeCustomData(string nodeId, string key)
        {
            if (string.IsNullOrEmpty(nodeId) || string.IsNullOrEmpty(key)) return null;
            RebuildCacheIfNeeded();

            if (_nodeCustomDataCache.TryGetValue(nodeId, out var nodeData))
            {
                return nodeData.TryGetValue(key, out var value) ? value : null;
            }
            return null;
        }

        #endregion

        #region State Modification

        public bool UnlockNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return false;
            if (IsNodeUnlocked(nodeId)) return false;

            _unlockedNodeIds.Add(nodeId);
            _nodeUnlockInfos.Add(new NodeUnlockInfo
            {
                NodeId = nodeId,
                UnlockTime = DateTime.UtcNow
            });

            InvalidateCache();
            return true;
        }

        public bool LockNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return false;
            if (!IsNodeUnlocked(nodeId)) return false;

            _unlockedNodeIds.Remove(nodeId);
            _nodeUnlockInfos.RemoveAll(info => info.NodeId == nodeId);

            InvalidateCache();
            return true;
        }

        public void SetResource(string resourceType, float amount)
        {
            if (string.IsNullOrEmpty(resourceType)) return;

            var entry = _resources.FirstOrDefault(r => r.ResourceType == resourceType);
            if (entry != null)
            {
                entry.Amount = amount;
            }
            else
            {
                _resources.Add(new ResourceEntry { ResourceType = resourceType, Amount = amount });
            }

            InvalidateCache();
        }

        public void AddResource(string resourceType, float amount)
        {
            SetResource(resourceType, GetResource(resourceType) + amount);
        }

        public bool SpendResource(string resourceType, float amount)
        {
            if (!HasResource(resourceType, amount)) return false;
            SetResource(resourceType, GetResource(resourceType) - amount);
            return true;
        }

        public void SetCustomData(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;

            var entry = _customData.FirstOrDefault(d => d.Key == key);
            if (entry != null)
            {
                entry.Value = value?.ToString() ?? string.Empty;
            }
            else
            {
                _customData.Add(new CustomDataEntry { Key = key, Value = value?.ToString() ?? string.Empty });
            }

            InvalidateCache();
        }

        public void SetNodeCustomData(string nodeId, string key, object value)
        {
            if (string.IsNullOrEmpty(nodeId) || string.IsNullOrEmpty(key)) return;

            var nodeData = _nodeCustomData.FirstOrDefault(d => d.NodeId == nodeId);
            if (nodeData == null)
            {
                nodeData = new NodeCustomData { NodeId = nodeId };
                _nodeCustomData.Add(nodeData);
            }

            var entry = nodeData.Data.FirstOrDefault(d => d.Key == key);
            if (entry != null)
            {
                entry.Value = value?.ToString() ?? string.Empty;
            }
            else
            {
                nodeData.Data.Add(new CustomDataEntry { Key = key, Value = value?.ToString() ?? string.Empty });
            }

            InvalidateCache();
        }

        public void Reset()
        {
            _unlockedNodeIds.Clear();
            _nodeUnlockInfos.Clear();
            _resources.Clear();
            _customData.Clear();
            _nodeCustomData.Clear();
            InvalidateCache();
        }

        #endregion

        #region Cache Management

        private void RebuildCacheIfNeeded()
        {
            if (_cacheValid) return;

            // Rebuild unlocked nodes cache
            _unlockedNodesCache = new HashSet<string>(_unlockedNodeIds);

            // Rebuild unlock time cache
            _unlockTimeCache = new Dictionary<string, DateTime>();
            foreach (var info in _nodeUnlockInfos)
            {
                _unlockTimeCache[info.NodeId] = info.UnlockTime;
            }

            // Rebuild resource cache
            _resourceCache = new Dictionary<string, float>();
            foreach (var resource in _resources)
            {
                _resourceCache[resource.ResourceType] = resource.Amount;
            }

            // Rebuild custom data cache
            _customDataCache = new Dictionary<string, object>();
            foreach (var data in _customData)
            {
                _customDataCache[data.Key] = data.Value;
            }

            // Rebuild node custom data cache
            _nodeCustomDataCache = new Dictionary<string, Dictionary<string, object>>();
            foreach (var nodeData in _nodeCustomData)
            {
                var dataDict = new Dictionary<string, object>();
                foreach (var entry in nodeData.Data)
                {
                    dataDict[entry.Key] = entry.Value;
                }
                _nodeCustomDataCache[nodeData.NodeId] = dataDict;
            }

            _cacheValid = true;
        }

        private void InvalidateCache()
        {
            _cacheValid = false;
        }

        #endregion

        #region Serializable Helper Classes

        [Serializable]
        private class NodeUnlockInfo
        {
            public string NodeId;
            public DateTime UnlockTime;
        }

        [Serializable]
        private class ResourceEntry
        {
            public string ResourceType;
            public float Amount;
        }

        [Serializable]
        private class CustomDataEntry
        {
            public string Key;
            public string Value;
        }

        [Serializable]
        private class NodeCustomData
        {
            public string NodeId;
            public List<CustomDataEntry> Data = new List<CustomDataEntry>();
        }

        #endregion
    }
}
