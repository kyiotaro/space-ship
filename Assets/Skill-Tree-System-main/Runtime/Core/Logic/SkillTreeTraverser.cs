using System.Collections.Generic;
using System.Linq;
using SkillTree.Core.Data;

namespace SkillTree.Core.Logic
{
    /// <summary>
    /// Provides advanced graph traversal and query operations for skill trees.
    /// </summary>
    public class SkillTreeTraverser
    {
        private readonly SkillTreeGraph _graph;

        public SkillTreeTraverser(SkillTreeGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Gets all nodes that can be unlocked given the current state.
        /// A node is unlockable if all its prerequisites are unlocked.
        /// </summary>
        public List<SkillNode> GetUnlockableNodes(ISkillTreeState state)
        {
            var unlockable = new List<SkillNode>();

            foreach (var node in _graph.Nodes)
            {
                if (state.IsNodeUnlocked(node.Id))
                    continue; // Already unlocked

                if (ArePrerequisitesMet(node, state))
                {
                    unlockable.Add(node);
                }
            }

            return unlockable;
        }

        /// <summary>
        /// Checks if all prerequisites for a node are met.
        /// </summary>
        public bool ArePrerequisitesMet(SkillNode node, ISkillTreeState state)
        {
            if (node == null) return false;

            foreach (var prerequisiteId in node.PrerequisiteNodeIds)
            {
                if (!state.IsNodeUnlocked(prerequisiteId))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all nodes that depend on the specified node (directly or indirectly).
        /// </summary>
        public List<SkillNode> GetAllDependents(string nodeId)
        {
            var visited = new HashSet<string>();
            var dependents = new List<SkillNode>();

            TraverseDependents(nodeId, visited, dependents);

            return dependents;
        }

        private void TraverseDependents(string nodeId, HashSet<string> visited, List<SkillNode> result)
        {
            if (!visited.Add(nodeId)) return;

            var directDependents = _graph.GetDependents(nodeId);
            foreach (var dependent in directDependents)
            {
                result.Add(dependent);
                TraverseDependents(dependent.Id, visited, result);
            }
        }

        /// <summary>
        /// Gets all nodes that are prerequisites for the specified node (directly or indirectly).
        /// </summary>
        public List<SkillNode> GetAllPrerequisites(string nodeId)
        {
            var visited = new HashSet<string>();
            var prerequisites = new List<SkillNode>();

            TraversePrerequisites(nodeId, visited, prerequisites);

            return prerequisites;
        }

        private void TraversePrerequisites(string nodeId, HashSet<string> visited, List<SkillNode> result)
        {
            if (!visited.Add(nodeId)) return;

            var directPrerequisites = _graph.GetPrerequisites(nodeId);
            foreach (var prerequisite in directPrerequisites)
            {
                result.Add(prerequisite);
                TraversePrerequisites(prerequisite.Id, visited, result);
            }
        }

        /// <summary>
        /// Gets the shortest path from startNodeId to endNodeId.
        /// Returns null if no path exists.
        /// </summary>
        public List<SkillNode> GetShortestPath(string startNodeId, string endNodeId)
        {
            if (startNodeId == endNodeId)
            {
                var node = _graph.GetNodeById(startNodeId);
                return node != null ? new List<SkillNode> { node } : null;
            }

            var queue = new Queue<string>();
            var visited = new HashSet<string>();
            var parent = new Dictionary<string, string>();

            queue.Enqueue(startNodeId);
            visited.Add(startNodeId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == endNodeId)
                {
                    return ReconstructPath(parent, startNodeId, endNodeId);
                }

                var dependents = _graph.GetDependents(current);
                foreach (var dependent in dependents)
                {
                    if (!visited.Contains(dependent.Id))
                    {
                        visited.Add(dependent.Id);
                        parent[dependent.Id] = current;
                        queue.Enqueue(dependent.Id);
                    }
                }
            }

            return null; // No path found
        }

        private List<SkillNode> ReconstructPath(Dictionary<string, string> parent, string start, string end)
        {
            var path = new List<string>();
            var current = end;

            while (current != start)
            {
                path.Add(current);
                if (!parent.TryGetValue(current, out current))
                    return null;
            }

            path.Add(start);
            path.Reverse();

            return path.Select(id => _graph.GetNodeById(id)).Where(n => n != null).ToList();
        }

        /// <summary>
        /// Gets all nodes of a specific type.
        /// </summary>
        public List<SkillNode> GetNodesByType(SkillNodeType nodeType)
        {
            return _graph.Nodes.Where(n => n.NodeType == nodeType).ToList();
        }

        /// <summary>
        /// Gets the depth of a node (distance from nearest root node).
        /// </summary>
        public int GetNodeDepth(string nodeId)
        {
            var rootNodes = _graph.GetRootNodes();

            int minDepth = int.MaxValue;
            foreach (var root in rootNodes)
            {
                var path = GetShortestPath(root.Id, nodeId);
                if (path != null)
                {
                    minDepth = System.Math.Min(minDepth, path.Count - 1);
                }
            }

            return minDepth == int.MaxValue ? -1 : minDepth;
        }

        /// <summary>
        /// Gets all nodes at a specific depth level.
        /// </summary>
        public List<SkillNode> GetNodesAtDepth(int depth)
        {
            return _graph.Nodes.Where(n => GetNodeDepth(n.Id) == depth).ToList();
        }

        /// <summary>
        /// Gets the maximum depth of the tree.
        /// </summary>
        public int GetMaxDepth()
        {
            int maxDepth = 0;
            foreach (var node in _graph.Nodes)
            {
                int depth = GetNodeDepth(node.Id);
                if (depth > maxDepth)
                    maxDepth = depth;
            }
            return maxDepth;
        }

        /// <summary>
        /// Gets nodes that have no dependents (leaf nodes).
        /// </summary>
        public List<SkillNode> GetLeafNodes()
        {
            return _graph.Nodes.Where(n => _graph.GetDependents(n.Id).Count == 0).ToList();
        }

        /// <summary>
        /// Gets the total cost to unlock a node and all its prerequisites.
        /// </summary>
        public Dictionary<string, float> GetTotalCostToUnlock(string nodeId, ISkillTreeState state)
        {
            var totalCosts = new Dictionary<string, float>();
            var prerequisites = GetAllPrerequisites(nodeId);
            var targetNode = _graph.GetNodeById(nodeId);

            if (targetNode != null)
            {
                prerequisites.Add(targetNode);
            }

            foreach (var node in prerequisites)
            {
                if (state.IsNodeUnlocked(node.Id))
                    continue;

                foreach (var cost in node.Costs)
                {
                    if (!totalCosts.ContainsKey(cost.ResourceType))
                        totalCosts[cost.ResourceType] = 0;

                    totalCosts[cost.ResourceType] += cost.Amount;
                }
            }

            return totalCosts;
        }

        /// <summary>
        /// Gets nodes within a certain distance from a given node.
        /// </summary>
        public List<SkillNode> GetNodesInRadius(string centerNodeId, int radius)
        {
            var result = new List<SkillNode>();
            var visited = new HashSet<string>();
            var queue = new Queue<(string nodeId, int distance)>();

            queue.Enqueue((centerNodeId, 0));
            visited.Add(centerNodeId);

            while (queue.Count > 0)
            {
                var (currentId, distance) = queue.Dequeue();

                if (distance > 0) // Don't include the center node
                {
                    var node = _graph.GetNodeById(currentId);
                    if (node != null)
                        result.Add(node);
                }

                if (distance < radius)
                {
                    // Add prerequisites
                    var prerequisites = _graph.GetPrerequisites(currentId);
                    foreach (var prereq in prerequisites)
                    {
                        if (visited.Add(prereq.Id))
                        {
                            queue.Enqueue((prereq.Id, distance + 1));
                        }
                    }

                    // Add dependents
                    var dependents = _graph.GetDependents(currentId);
                    foreach (var dependent in dependents)
                    {
                        if (visited.Add(dependent.Id))
                        {
                            queue.Enqueue((dependent.Id, distance + 1));
                        }
                    }
                }
            }

            return result;
        }
    }
}
