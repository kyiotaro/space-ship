using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SkillTree.Core.Data;

namespace SkillTree.UI
{
    /// <summary>
    /// Provides various layout algorithms for arranging skill tree nodes.
    /// Supports manual, grid, hierarchical, and force-directed layouts.
    /// </summary>
    public class SkillTreeLayoutEngine
    {
        private SkillTreeUI _treeUI;

        public SkillTreeLayoutEngine(SkillTreeUI treeUI)
        {
            _treeUI = treeUI ?? throw new ArgumentNullException(nameof(treeUI));
        }

        #region Manual Layout

        /// <summary>
        /// Uses the designer-placed positions from EditorPosition.
        /// This is the default layout that doesn't modify positions.
        /// </summary>
        public void ApplyManualLayout()
        {
            // No changes needed - nodes already use their EditorPosition
        }

        #endregion

        #region Grid Layout

        /// <summary>
        /// Snaps all nodes to a grid with specified cell size.
        /// </summary>
        public void ApplyGridLayout(float cellSize)
        {
            if (_treeUI?.Graph == null) return;

            foreach (var node in _treeUI.Graph.Nodes)
            {
                Vector2 snappedPosition = SnapToGrid(node.EditorPosition, cellSize);
                node.EditorPosition = snappedPosition;
            }
        }

        private Vector2 SnapToGrid(Vector2 position, float cellSize)
        {
            float x = Mathf.Round(position.x / cellSize) * cellSize;
            float y = Mathf.Round(position.y / cellSize) * cellSize;
            return new Vector2(x, y);
        }

        #endregion

        #region Hierarchical Layout

        /// <summary>
        /// Arranges nodes in a hierarchical tree structure.
        /// Root nodes at the top, dependencies flow downward.
        /// </summary>
        public void ApplyHierarchicalLayout(Vector2 spacing)
        {
            if (_treeUI?.Graph == null) return;

            var graph = _treeUI.Graph;
            var rootNodes = graph.GetRootNodes();

            if (rootNodes.Count == 0)
            {
                Debug.LogWarning("No root nodes found for hierarchical layout");
                return;
            }

            // Assign levels to all nodes (BFS traversal)
            var nodeLevels = AssignNodeLevels(graph);

            // Group nodes by level
            var levelGroups = nodeLevels.GroupBy(kvp => kvp.Value)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Key).ToList());

            // Position nodes level by level
            float currentY = 0f;
            foreach (var level in levelGroups.Keys.OrderBy(k => k))
            {
                var nodesInLevel = levelGroups[level];
                PositionNodesInRow(nodesInLevel, currentY, spacing.x);
                currentY -= spacing.y;
            }
        }

        private Dictionary<SkillNode, int> AssignNodeLevels(SkillTreeGraph graph)
        {
            var levels = new Dictionary<SkillNode, int>();
            var visited = new HashSet<string>();
            var queue = new Queue<(SkillNode node, int level)>();

            // Start with root nodes at level 0
            foreach (var root in graph.GetRootNodes())
            {
                queue.Enqueue((root, 0));
            }

            while (queue.Count > 0)
            {
                var (node, level) = queue.Dequeue();

                if (visited.Contains(node.Id))
                {
                    // Update level to the maximum depth encountered
                    if (levels[node] < level)
                    {
                        levels[node] = level;
                    }
                    continue;
                }

                visited.Add(node.Id);
                levels[node] = level;

                // Add dependents to queue
                var dependents = graph.GetDependents(node.Id);
                foreach (var dependent in dependents)
                {
                    queue.Enqueue((dependent, level + 1));
                }
            }

            return levels;
        }

        private void PositionNodesInRow(List<SkillNode> nodes, float yPosition, float horizontalSpacing)
        {
            if (nodes.Count == 0) return;

            float totalWidth = (nodes.Count - 1) * horizontalSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < nodes.Count; i++)
            {
                float x = startX + i * horizontalSpacing;
                nodes[i].EditorPosition = new Vector2(x, yPosition);
            }
        }

        #endregion

        #region Force-Directed Layout

        /// <summary>
        /// Physics-based layout that minimizes edge crossings.
        /// Nodes repel each other while connections act as springs.
        /// </summary>
        public void ApplyForceDirectedLayout(Vector2 spacing, int iterations = 50)
        {
            if (_treeUI?.Graph == null) return;

            var graph = _treeUI.Graph;
            var nodes = graph.Nodes;

            if (nodes.Count == 0) return;

            // Initialize positions randomly if they're all at origin
            InitializeRandomPositionsIfNeeded(nodes);

            // Physical simulation parameters
            float repulsionStrength = spacing.magnitude * 100f;
            float attractionStrength = 0.01f;
            float damping = 0.9f;

            // Node velocities
            var velocities = new Dictionary<string, Vector2>();
            foreach (var node in nodes)
            {
                velocities[node.Id] = Vector2.zero;
            }

            // Run simulation
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                var forces = new Dictionary<string, Vector2>();

                // Initialize forces
                foreach (var node in nodes)
                {
                    forces[node.Id] = Vector2.zero;
                }

                // Apply repulsion between all node pairs
                for (int i = 0; i < nodes.Count; i++)
                {
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        var nodeA = nodes[i];
                        var nodeB = nodes[j];

                        Vector2 delta = nodeB.EditorPosition - nodeA.EditorPosition;
                        float distance = delta.magnitude;

                        if (distance < 0.1f) distance = 0.1f; // Avoid division by zero

                        float repulsion = repulsionStrength / (distance * distance);
                        Vector2 force = delta.normalized * repulsion;

                        forces[nodeA.Id] -= force;
                        forces[nodeB.Id] += force;
                    }
                }

                // Apply attraction along connections
                foreach (var connection in graph.Connections)
                {
                    var fromNode = graph.GetNodeById(connection.FromNodeId);
                    var toNode = graph.GetNodeById(connection.ToNodeId);

                    if (fromNode == null || toNode == null) continue;

                    Vector2 delta = toNode.EditorPosition - fromNode.EditorPosition;
                    float distance = delta.magnitude;

                    float attraction = distance * attractionStrength;
                    Vector2 force = delta.normalized * attraction;

                    forces[fromNode.Id] += force;
                    forces[toNode.Id] -= force;
                }

                // Update velocities and positions
                foreach (var node in nodes)
                {
                    velocities[node.Id] = (velocities[node.Id] + forces[node.Id]) * damping;
                    node.EditorPosition += velocities[node.Id];
                }
            }

            // Center the layout
            CenterLayout(nodes);
        }

        private void InitializeRandomPositionsIfNeeded(List<SkillNode> nodes)
        {
            // Check if all nodes are at origin
            bool allAtOrigin = nodes.All(n => n.EditorPosition.magnitude < 0.1f);

            if (allAtOrigin)
            {
                // Spread nodes randomly
                float spread = 500f;
                foreach (var node in nodes)
                {
                    float x = UnityEngine.Random.Range(-spread, spread);
                    float y = UnityEngine.Random.Range(-spread, spread);
                    node.EditorPosition = new Vector2(x, y);
                }
            }
        }

        private void CenterLayout(List<SkillNode> nodes)
        {
            if (nodes.Count == 0) return;

            // Calculate centroid
            Vector2 centroid = Vector2.zero;
            foreach (var node in nodes)
            {
                centroid += node.EditorPosition;
            }
            centroid /= nodes.Count;

            // Shift all nodes so centroid is at origin
            foreach (var node in nodes)
            {
                node.EditorPosition -= centroid;
            }
        }

        #endregion

        #region Circular Layout

        /// <summary>
        /// Arranges nodes in a circular pattern.
        /// Useful for trees with no clear hierarchy.
        /// </summary>
        public void ApplyCircularLayout(float radius)
        {
            if (_treeUI?.Graph == null) return;

            var nodes = _treeUI.Graph.Nodes;
            if (nodes.Count == 0) return;

            float angleStep = 360f / nodes.Count;

            for (int i = 0; i < nodes.Count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                nodes[i].EditorPosition = new Vector2(x, y);
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Calculates the bounding box of all nodes.
        /// </summary>
        public Rect GetBoundingBox()
        {
            if (_treeUI?.Graph == null || _treeUI.Graph.Nodes.Count == 0)
            {
                return new Rect(0, 0, 100, 100);
            }

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var node in _treeUI.Graph.Nodes)
            {
                minX = Mathf.Min(minX, node.EditorPosition.x);
                minY = Mathf.Min(minY, node.EditorPosition.y);
                maxX = Mathf.Max(maxX, node.EditorPosition.x);
                maxY = Mathf.Max(maxY, node.EditorPosition.y);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Scales the entire layout by a factor.
        /// </summary>
        public void ScaleLayout(float scaleFactor)
        {
            if (_treeUI?.Graph == null) return;

            foreach (var node in _treeUI.Graph.Nodes)
            {
                node.EditorPosition *= scaleFactor;
            }
        }

        #endregion
    }
}
