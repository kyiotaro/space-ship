using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using SkillTree.Core.Data;

namespace SkillTree.Editor.GraphView
{
    /// <summary>
    /// GraphView implementation for visual skill tree editing
    /// </summary>
    public class SkillTreeGraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private SkillTreeGraph _graph;
        private SkillTreeGraphWindow _window;
        private Dictionary<string, SkillNodeView> _nodeViews = new Dictionary<string, SkillNodeView>();
        private SkillNodeSearchWindow _searchWindow;

        private const float GRID_SPACING = 20f;
        private const float NODE_SPACING_X = 300f;
        private const float NODE_SPACING_Y = 150f;

        public SkillTreeGraphView(SkillTreeGraphWindow window)
        {
            _window = window;

            // Setup graph view style and features
            LoadStyleSheet();

            // Add grid background
            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            // Setup manipulators for interaction
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Setup zoom
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Register callbacks
            graphViewChanged += OnGraphViewChanged;
            viewTransformChanged += OnViewTransformChanged;

            // Enable undo/redo
            Undo.undoRedoPerformed += OnUndoRedo;

            // Initialize search window
            _searchWindow = ScriptableObject.CreateInstance<SkillNodeSearchWindow>();
            _searchWindow.Initialize(this);
            nodeCreationRequest = OnNodeCreationRequest;
        }

        ~SkillTreeGraphView()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void LoadStyleSheet()
        {
            // Try to load the USS stylesheet from the package
            var guids = AssetDatabase.FindAssets("SkillTreeGraphViewStyles t:StyleSheet");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (styleSheet != null)
                {
                    styleSheets.Add(styleSheet);
                }
            }
        }

        #region Graph Population

        public void PopulateView(SkillTreeGraph graph)
        {
            _graph = graph;

            // Clear existing view
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            _nodeViews.Clear();

            if (_graph == null)
                return;

            // Create node views
            foreach (var node in _graph.Nodes)
            {
                CreateNodeView(node);
            }

            // Create connections/edges
            foreach (var connection in _graph.Connections)
            {
                CreateEdgeView(connection);
            }
        }

        private SkillNodeView CreateNodeView(SkillNode node)
        {
            var nodeView = new SkillNodeView(node, this);
            nodeView.SetPosition(new Rect(node.EditorPosition, Vector2.zero));

            _nodeViews[node.Id] = nodeView;
            AddElement(nodeView);

            return nodeView;
        }

        private void CreateEdgeView(SkillConnection connection)
        {
            if (!_nodeViews.TryGetValue(connection.FromNodeId, out var fromNodeView))
                return;

            if (!_nodeViews.TryGetValue(connection.ToNodeId, out var toNodeView))
                return;

            var outputPort = fromNodeView.OutputPort;
            var inputPort = toNodeView.InputPort;

            if (outputPort == null || inputPort == null)
                return;

            var edge = new SkillConnectionView
            {
                output = outputPort,
                input = inputPort
            };

            edge.SetConnection(connection);

            AddElement(edge);
            outputPort.Connect(edge);
            inputPort.Connect(edge);
        }

        #endregion

        #region Node Creation

        public void CreateNode(Vector2 position)
        {
            if (_graph == null)
                return;

            Undo.RecordObject(_graph, "Add Skill Node");

            var node = new SkillNode
            {
                DisplayName = $"Node {_graph.NodeCount + 1}",
                Description = "New skill node",
                EditorPosition = position
            };

            _graph.AddNode(node);
            CreateNodeView(node);

            EditorUtility.SetDirty(_graph);
        }

        #endregion

        #region GraphView Overrides

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                // Don't connect to self
                if (startPort.node == port.node)
                    return;

                // Don't connect same direction
                if (startPort.direction == port.direction)
                    return;

                // Check if connection would create a cycle
                if (WouldCreateCycle(startPort, port))
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private bool WouldCreateCycle(Port startPort, Port endPort)
        {
            if (_graph == null)
                return false;

            // Determine direction
            var fromNode = startPort.direction == Direction.Output ? startPort.node : endPort.node;
            var toNode = startPort.direction == Direction.Output ? endPort.node : startPort.node;

            if (fromNode.userData is not SkillNode from || toNode.userData is not SkillNode to)
                return false;

            // Check if creating this connection would create a cycle
            return _graph.HasPath(to.Id, from.Id);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (_graph == null)
                return;

            // Get mouse position in graph space
            var mousePosition = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Create Node (Search)", a => OpenSearchWindow(evt.mousePosition));
            evt.menu.AppendAction("Add Node (Quick)", a => CreateNode(mousePosition));

            if (evt.target is SkillNodeView nodeView)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Delete Node", a => DeleteNode(nodeView));
                evt.menu.AppendAction("Duplicate Node", a => DuplicateNode(nodeView));
            }
        }

        private void OnNodeCreationRequest(NodeCreationContext context)
        {
            if (_searchWindow != null)
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
            }
        }

        private void OpenSearchWindow(Vector2 mousePosition)
        {
            if (_searchWindow != null)
            {
                SearchWindow.Open(new SearchWindowContext(mousePosition), _searchWindow);
            }
        }

        #endregion

        #region Graph Changes

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            if (_graph == null)
                return changes;

            // Handle edge creation (connections)
            if (changes.edgesToCreate != null)
            {
                foreach (var edge in changes.edgesToCreate)
                {
                    CreateConnection(edge);
                }
            }

            // Handle element removal
            if (changes.elementsToRemove != null)
            {
                foreach (var element in changes.elementsToRemove)
                {
                    if (element is SkillNodeView nodeView)
                    {
                        DeleteNodeFromGraph(nodeView);
                    }
                    else if (element is Edge edge)
                    {
                        DeleteConnection(edge);
                    }
                }
            }

            // Handle moved elements
            if (changes.movedElements != null)
            {
                foreach (var element in changes.movedElements)
                {
                    if (element is SkillNodeView nodeView)
                    {
                        UpdateNodePosition(nodeView);
                    }
                }
            }

            return changes;
        }

        private void CreateConnection(Edge edge)
        {
            if (edge.output?.node is not SkillNodeView fromNodeView ||
                edge.input?.node is not SkillNodeView toNodeView)
                return;

            var fromNode = fromNodeView.Node;
            var toNode = toNodeView.Node;

            Undo.RecordObject(_graph, "Create Connection");

            var connection = new SkillConnection
            {
                FromNodeId = fromNode.Id,
                ToNodeId = toNode.Id,
                Type = ConnectionType.Prerequisite
            };

            _graph.AddConnection(connection);
            edge.userData = connection;

            EditorUtility.SetDirty(_graph);
        }

        private void DeleteConnection(Edge edge)
        {
            SkillConnection connection = null;

            if (edge is SkillConnectionView connectionView)
            {
                connection = connectionView.Connection;
            }
            else if (edge.userData is SkillConnection conn)
            {
                connection = conn;
            }

            if (connection == null)
                return;

            Undo.RecordObject(_graph, "Delete Connection");

            _graph.RemoveConnection(connection.Id);

            EditorUtility.SetDirty(_graph);
        }

        private void DeleteNodeFromGraph(SkillNodeView nodeView)
        {
            Undo.RecordObject(_graph, "Delete Node");

            _graph.RemoveNode(nodeView.Node.Id);
            _nodeViews.Remove(nodeView.Node.Id);

            EditorUtility.SetDirty(_graph);
        }

        private void UpdateNodePosition(SkillNodeView nodeView)
        {
            Undo.RecordObject(_graph, "Move Node");

            nodeView.Node.EditorPosition = nodeView.GetPosition().position;

            EditorUtility.SetDirty(_graph);
        }

        #endregion

        #region Node Operations

        private void DeleteNode(SkillNodeView nodeView)
        {
            if (nodeView == null)
                return;

            DeleteElements(new[] { nodeView });
        }

        private void DuplicateNode(SkillNodeView nodeView)
        {
            if (nodeView == null || _graph == null)
                return;

            Undo.RecordObject(_graph, "Duplicate Node");

            var originalNode = nodeView.Node;
            var newNode = new SkillNode
            {
                DisplayName = originalNode.DisplayName + " (Copy)",
                Description = originalNode.Description,
                Icon = originalNode.Icon,
                NodeType = originalNode.NodeType,
                EditorPosition = originalNode.EditorPosition + new Vector2(50, 50),
                // Note: Don't copy prerequisites - let user connect manually
            };

            // Copy costs and requirements
            foreach (var cost in originalNode.Costs)
            {
                newNode.Costs.Add(new SkillCost
                {
                    ResourceType = cost.ResourceType,
                    Amount = cost.Amount
                });
            }

            foreach (var requirement in originalNode.Requirements)
            {
                newNode.Requirements.Add(new SkillRequirement
                {
                    Type = requirement.Type,
                    Logic = requirement.Logic,
                    RequiredCount = requirement.RequiredCount,
                    CustomCondition = requirement.CustomCondition
                });
            }

            _graph.AddNode(newNode);
            CreateNodeView(newNode);

            EditorUtility.SetDirty(_graph);
        }

        #endregion

        #region Layout Algorithms

        public void ApplyHierarchicalLayout()
        {
            if (_graph == null || _graph.NodeCount == 0)
                return;

            Undo.RecordObject(_graph, "Apply Hierarchical Layout");

            var rootNodes = _graph.GetRootNodes();
            var visited = new HashSet<string>();
            var layerPositions = new Dictionary<string, (int layer, int indexInLayer)>();

            // Calculate layers using BFS
            var queue = new Queue<(SkillNode node, int layer)>();
            var layerCounts = new Dictionary<int, int>();

            foreach (var root in rootNodes)
            {
                queue.Enqueue((root, 0));
            }

            while (queue.Count > 0)
            {
                var (node, layer) = queue.Dequeue();

                if (!visited.Add(node.Id))
                    continue;

                if (!layerCounts.ContainsKey(layer))
                    layerCounts[layer] = 0;

                int indexInLayer = layerCounts[layer]++;
                layerPositions[node.Id] = (layer, indexInLayer);

                var dependents = _graph.GetDependents(node.Id);
                foreach (var dependent in dependents)
                {
                    queue.Enqueue((dependent, layer + 1));
                }
            }

            // Position nodes
            foreach (var node in _graph.Nodes)
            {
                if (layerPositions.TryGetValue(node.Id, out var position))
                {
                    float x = position.indexInLayer * NODE_SPACING_X;
                    float y = position.layer * NODE_SPACING_Y;
                    node.EditorPosition = new Vector2(x, y);

                    if (_nodeViews.TryGetValue(node.Id, out var nodeView))
                    {
                        nodeView.SetPosition(new Rect(node.EditorPosition, Vector2.zero));
                    }
                }
            }

            EditorUtility.SetDirty(_graph);
        }

        public void ApplyGridLayout()
        {
            if (_graph == null || _graph.NodeCount == 0)
                return;

            Undo.RecordObject(_graph, "Apply Grid Layout");

            int columns = Mathf.CeilToInt(Mathf.Sqrt(_graph.NodeCount));
            int index = 0;

            foreach (var node in _graph.Nodes)
            {
                int row = index / columns;
                int col = index % columns;

                float x = col * NODE_SPACING_X;
                float y = row * NODE_SPACING_Y;

                node.EditorPosition = new Vector2(x, y);

                if (_nodeViews.TryGetValue(node.Id, out var nodeView))
                {
                    nodeView.SetPosition(new Rect(node.EditorPosition, Vector2.zero));
                }

                index++;
            }

            EditorUtility.SetDirty(_graph);
        }

        #endregion

        #region Save/Load

        public void SaveGraphData()
        {
            if (_graph == null)
                return;

            // Sync node positions
            foreach (var kvp in _nodeViews)
            {
                var nodeView = kvp.Value;
                nodeView.Node.EditorPosition = nodeView.GetPosition().position;
            }

            EditorUtility.SetDirty(_graph);
        }

        #endregion

        #region Callbacks

        private void OnViewTransformChanged(UnityEditor.Experimental.GraphView.GraphView graphView)
        {
            // Could save view position/zoom here if needed
        }

        private void OnUndoRedo()
        {
            // Refresh the view when undo/redo is performed
            if (_graph != null)
            {
                PopulateView(_graph);
            }
        }

        #endregion

        public SkillTreeGraph Graph => _graph;
        public EditorWindow window => _window;
    }
}
