using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillTree.Core.Data
{
    [CreateAssetMenu(fileName = "NewSkillTree", menuName = "Skill Tree/New Skill Tree", order = 1)]
    public class SkillTreeGraph : ScriptableObject
    {
        [SerializeField] private string _treeId;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(3, 5)] private string _description;

        [SerializeField] private List<SkillNode> _nodes = new List<SkillNode>();
        [SerializeField] private List<SkillConnection> _connections = new List<SkillConnection>();

        // Cached adjacency data for fast queries
        private Dictionary<string, SkillNode> _nodeCache;
        private Dictionary<string, List<string>> _prerequisitesCache;
        private Dictionary<string, List<string>> _dependentsCache;
        private bool _cacheValid = false;

        // Properties
        public string TreeId
        {
            get
            {
                if (string.IsNullOrEmpty(_treeId))
                {
                    _treeId = Guid.NewGuid().ToString();
                }
                return _treeId;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _treeId = value;
                }
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public List<SkillNode> Nodes => _nodes;
        public List<SkillConnection> Connections => _connections;

        public int NodeCount => _nodes?.Count ?? 0;
        public int ConnectionCount => _connections?.Count ?? 0;

        #region Node Management

        public void AddNode(SkillNode node)
        {
            if (node == null)
            {
                Debug.LogError("Cannot add null node to skill tree");
                return;
            }

            _nodes.Add(node);
            InvalidateCache();
        }

        public bool RemoveNode(string nodeId)
        {
            var node = GetNodeById(nodeId);
            if (node == null) return false;

            _nodes.Remove(node);
            _connections.RemoveAll(c => c.FromNodeId == nodeId || c.ToNodeId == nodeId);

            foreach (var otherNode in _nodes)
            {
                otherNode.PrerequisiteNodeIds.Remove(nodeId);
            }

            InvalidateCache();
            return true;
        }

        public SkillNode GetNodeById(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;

            RebuildCacheIfNeeded();
            _nodeCache.TryGetValue(nodeId, out var node);
            return node;
        }

        public bool ContainsNode(string nodeId)
        {
            return GetNodeById(nodeId) != null;
        }

        #endregion

        #region Connection Management

        public void AddConnection(SkillConnection connection)
        {
            if (connection == null || !connection.IsValid())
            {
                Debug.LogError("Cannot add invalid connection to skill tree");
                return;
            }

            if (!ContainsNode(connection.FromNodeId) || !ContainsNode(connection.ToNodeId))
            {
                Debug.LogError("Cannot add connection: one or both nodes do not exist");
                return;
            }

            if (_connections.Any(c => c.Equals(connection)))
            {
                Debug.LogWarning("Connection already exists");
                return;
            }

            _connections.Add(connection);

            var toNode = GetNodeById(connection.ToNodeId);
            if (connection.Type == ConnectionType.Prerequisite && toNode != null)
            {
                toNode.AddPrerequisite(connection.FromNodeId);
            }

            InvalidateCache();
        }

        public bool RemoveConnection(string connectionId)
        {
            var connection = _connections.FirstOrDefault(c => c.Id == connectionId);
            if (connection == null) return false;

            _connections.Remove(connection);

            var toNode = GetNodeById(connection.ToNodeId);
            if (connection.Type == ConnectionType.Prerequisite && toNode != null)
            {
                toNode.RemovePrerequisite(connection.FromNodeId);
            }

            InvalidateCache();
            return true;
        }

        public List<SkillConnection> GetConnectionsFromNode(string nodeId)
        {
            return _connections.Where(c => c.FromNodeId == nodeId).ToList();
        }

        public List<SkillConnection> GetConnectionsToNode(string nodeId)
        {
            return _connections.Where(c => c.ToNodeId == nodeId).ToList();
        }

        #endregion

        #region Graph Queries

        public List<SkillNode> GetPrerequisites(string nodeId)
        {
            RebuildCacheIfNeeded();

            if (_prerequisitesCache.TryGetValue(nodeId, out var prerequisiteIds))
            {
                return prerequisiteIds.Select(id => GetNodeById(id)).Where(n => n != null).ToList();
            }

            return new List<SkillNode>();
        }

        public List<SkillNode> GetDependents(string nodeId)
        {
            RebuildCacheIfNeeded();

            if (_dependentsCache.TryGetValue(nodeId, out var dependentIds))
            {
                return dependentIds.Select(id => GetNodeById(id)).Where(n => n != null).ToList();
            }

            return new List<SkillNode>();
        }

        public List<SkillNode> GetRootNodes()
        {
            return _nodes.Where(n => !n.HasPrerequisites()).ToList();
        }

        public bool HasPath(string fromNodeId, string toNodeId)
        {
            if (fromNodeId == toNodeId) return true;

            var visited = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(fromNodeId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == toNodeId) return true;

                if (!visited.Add(current)) continue;

                var dependents = GetDependents(current);
                foreach (var dependent in dependents)
                {
                    if (!visited.Contains(dependent.Id))
                    {
                        queue.Enqueue(dependent.Id);
                    }
                }
            }

            return false;
        }

        #endregion

        #region Validation

        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            ValidateDuplicateIds(result);
            ValidateOrphanNodes(result);
            ValidateCycles(result);
            ValidateInvalidReferences(result);

            return result;
        }

        private void ValidateDuplicateIds(ValidationResult result)
        {
            var idGroups = _nodes.GroupBy(n => n.Id).Where(g => g.Count() > 1);
            foreach (var group in idGroups)
            {
                result.AddError($"Duplicate node ID found: {group.Key}");
            }
        }

        private void ValidateOrphanNodes(ValidationResult result)
        {
            var rootNodes = GetRootNodes();
            if (rootNodes.Count == 0 && _nodes.Count > 0)
            {
                result.AddWarning("No root nodes found (all nodes have prerequisites)");
            }

            foreach (var node in _nodes)
            {
                bool hasIncomingConnections = _connections.Any(c => c.ToNodeId == node.Id);
                bool hasOutgoingConnections = _connections.Any(c => c.FromNodeId == node.Id);

                if (!hasIncomingConnections && !hasOutgoingConnections && rootNodes.Count > 1)
                {
                    result.AddWarning($"Orphan node detected: {node.DisplayName} (ID: {node.Id})");
                }
            }
        }

        private void ValidateCycles(ValidationResult result)
        {
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();

            foreach (var node in _nodes)
            {
                if (DetectCycle(node.Id, visited, recursionStack))
                {
                    result.AddError($"Cycle detected involving node: {node.DisplayName} (ID: {node.Id})");
                }
            }
        }

        private bool DetectCycle(string nodeId, HashSet<string> visited, HashSet<string> recursionStack)
        {
            if (recursionStack.Contains(nodeId))
                return true;

            if (visited.Contains(nodeId))
                return false;

            visited.Add(nodeId);
            recursionStack.Add(nodeId);

            var dependents = GetDependents(nodeId);
            foreach (var dependent in dependents)
            {
                if (DetectCycle(dependent.Id, visited, recursionStack))
                    return true;
            }

            recursionStack.Remove(nodeId);
            return false;
        }

        private void ValidateInvalidReferences(ValidationResult result)
        {
            foreach (var node in _nodes)
            {
                foreach (var prerequisiteId in node.PrerequisiteNodeIds)
                {
                    if (!ContainsNode(prerequisiteId))
                    {
                        result.AddError($"Node '{node.DisplayName}' references non-existent prerequisite: {prerequisiteId}");
                    }
                }
            }

            foreach (var connection in _connections)
            {
                if (!ContainsNode(connection.FromNodeId))
                {
                    result.AddError($"Connection references non-existent 'from' node: {connection.FromNodeId}");
                }
                if (!ContainsNode(connection.ToNodeId))
                {
                    result.AddError($"Connection references non-existent 'to' node: {connection.ToNodeId}");
                }
            }
        }

        #endregion

        #region Cache Management

        private void RebuildCacheIfNeeded()
        {
            if (_cacheValid) return;

            _nodeCache = new Dictionary<string, SkillNode>();
            _prerequisitesCache = new Dictionary<string, List<string>>();
            _dependentsCache = new Dictionary<string, List<string>>();

            foreach (var node in _nodes)
            {
                _nodeCache[node.Id] = node;
                _prerequisitesCache[node.Id] = new List<string>(node.PrerequisiteNodeIds);
                _dependentsCache[node.Id] = new List<string>();
            }

            foreach (var node in _nodes)
            {
                foreach (var prerequisiteId in node.PrerequisiteNodeIds)
                {
                    if (_dependentsCache.ContainsKey(prerequisiteId))
                    {
                        _dependentsCache[prerequisiteId].Add(node.Id);
                    }
                }
            }

            _cacheValid = true;
        }

        private void InvalidateCache()
        {
            _cacheValid = false;
        }

        private void OnValidate()
        {
            InvalidateCache();
        }

        #endregion
    }

    [Serializable]
    public class ValidationResult
    {
        [SerializeField] private List<string> _errors = new List<string>();
        [SerializeField] private List<string> _warnings = new List<string>();

        public IReadOnlyList<string> Errors => _errors;
        public IReadOnlyList<string> Warnings => _warnings;

        public bool IsValid => _errors.Count == 0;
        public bool HasWarnings => _warnings.Count > 0;

        public void AddError(string error)
        {
            if (!_errors.Contains(error))
            {
                _errors.Add(error);
            }
        }

        public void AddWarning(string warning)
        {
            if (!_warnings.Contains(warning))
            {
                _warnings.Add(warning);
            }
        }

        public void Clear()
        {
            _errors.Clear();
            _warnings.Clear();
        }

        public override string ToString()
        {
            var result = $"Validation Result: {(_errors.Count == 0 ? "PASSED" : "FAILED")}\n";

            if (_errors.Count > 0)
            {
                result += $"\nErrors ({_errors.Count}):\n";
                foreach (var error in _errors)
                {
                    result += $"  - {error}\n";
                }
            }

            if (_warnings.Count > 0)
            {
                result += $"\nWarnings ({_warnings.Count}):\n";
                foreach (var warning in _warnings)
                {
                    result += $"  - {warning}\n";
                }
            }

            return result;
        }
    }
}
