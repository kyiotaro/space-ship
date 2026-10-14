using System;
using System.Collections.Generic;
using UnityEngine;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using SkillTree.Core.Events;

namespace SkillTree.UI
{
    /// <summary>
    /// Main UI controller for the skill tree system.
    /// Renders the skill tree, manages node and connection UI instances,
    /// and handles visual updates in response to state changes.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkillTreeGraph _skillTreeGraph;
        [SerializeField] private ISkillTreeManager _skillTreeManager;

        [Header("Prefabs")]
        [SerializeField] private SkillNodeUI _nodeUIPrefab;
        [SerializeField] private SkillConnectionUI _connectionUIPrefab;

        [Header("Containers")]
        [SerializeField] private RectTransform _nodesContainer;
        [SerializeField] private RectTransform _connectionsContainer;

        [Header("Layout")]
        [SerializeField] private LayoutType _layoutType = LayoutType.Manual;
        [SerializeField] private Vector2 _nodeSpacing = new Vector2(200f, 150f);
        [SerializeField] private float _gridCellSize = 100f;

        [Header("Zoom and Pan")]
        [SerializeField] private float _minZoom = 0.5f;
        [SerializeField] private float _maxZoom = 2f;
        [SerializeField] private float _zoomSpeed = 0.1f;

        // Node and connection UI instances
        private Dictionary<string, SkillNodeUI> _nodeUIInstances = new Dictionary<string, SkillNodeUI>();
        private List<SkillConnectionUI> _connectionUIInstances = new List<SkillConnectionUI>();

        // Layout engine
        private SkillTreeLayoutEngine _layoutEngine;
        private SkillTreeInputHandler _inputHandler;

        // Event subscriptions
        private List<IDisposable> _eventSubscriptions = new List<IDisposable>();

        // Properties
        public SkillTreeGraph Graph => _skillTreeGraph;
        public ISkillTreeManager Manager => _skillTreeManager;
        public RectTransform NodesContainer => _nodesContainer;
        public RectTransform ConnectionsContainer => _connectionsContainer;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeContainers();
            _layoutEngine = new SkillTreeLayoutEngine(this);
            _inputHandler = gameObject.AddComponent<SkillTreeInputHandler>();
            _inputHandler.Initialize(this, _minZoom, _maxZoom, _zoomSpeed);
        }

        private void Start()
        {
            if (_skillTreeGraph != null)
            {
                RenderSkillTree();
            }

            if (_skillTreeManager != null)
            {
                SubscribeToEvents();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        private void InitializeContainers()
        {
            if (_nodesContainer == null)
            {
                var nodesObj = new GameObject("Nodes");
                _nodesContainer = nodesObj.AddComponent<RectTransform>();
                _nodesContainer.SetParent(transform, false);
                _nodesContainer.anchorMin = Vector2.zero;
                _nodesContainer.anchorMax = Vector2.one;
                _nodesContainer.sizeDelta = Vector2.zero;
            }

            if (_connectionsContainer == null)
            {
                var connectionsObj = new GameObject("Connections");
                _connectionsContainer = connectionsObj.AddComponent<RectTransform>();
                _connectionsContainer.SetParent(transform, false);
                _connectionsContainer.anchorMin = Vector2.zero;
                _connectionsContainer.anchorMax = Vector2.one;
                _connectionsContainer.sizeDelta = Vector2.zero;

                // Connections should be rendered behind nodes
                _connectionsContainer.SetAsFirstSibling();
            }
        }

        /// <summary>
        /// Sets the skill tree graph to render.
        /// </summary>
        public void SetSkillTree(SkillTreeGraph graph)
        {
            if (_skillTreeGraph == graph) return;

            _skillTreeGraph = graph;
            RenderSkillTree();
        }

        /// <summary>
        /// Sets the skill tree manager for state queries and operations.
        /// </summary>
        public void SetManager(ISkillTreeManager manager)
        {
            if (_skillTreeManager == manager) return;

            UnsubscribeFromEvents();
            _skillTreeManager = manager;
            SubscribeToEvents();

            // Refresh all node states
            RefreshAllNodes();
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Renders the entire skill tree from scratch.
        /// Clears existing UI elements and creates new ones.
        /// </summary>
        public void RenderSkillTree()
        {
            if (_skillTreeGraph == null)
            {
                Debug.LogWarning("Cannot render skill tree: graph is null");
                return;
            }

            // Clear existing UI
            ClearUI();

            // Apply layout
            ApplyLayout();

            // Create node UI elements
            foreach (var node in _skillTreeGraph.Nodes)
            {
                CreateNodeUI(node);
            }

            // Create connection UI elements
            foreach (var connection in _skillTreeGraph.Connections)
            {
                CreateConnectionUI(connection);
            }

            // Update initial states
            RefreshAllNodes();
        }

        private void CreateNodeUI(SkillNode node)
        {
            if (_nodeUIPrefab == null)
            {
                Debug.LogError("Node UI prefab is not assigned");
                return;
            }

            var nodeUI = Instantiate(_nodeUIPrefab, _nodesContainer);
            nodeUI.Initialize(node, this);
            nodeUI.SetPosition(node.EditorPosition);

            _nodeUIInstances[node.Id] = nodeUI;
        }

        private void CreateConnectionUI(SkillConnection connection)
        {
            if (_connectionUIPrefab == null)
            {
                Debug.LogError("Connection UI prefab is not assigned");
                return;
            }

            if (!_nodeUIInstances.TryGetValue(connection.FromNodeId, out var fromNodeUI))
            {
                Debug.LogWarning($"Cannot create connection: from node '{connection.FromNodeId}' not found");
                return;
            }

            if (!_nodeUIInstances.TryGetValue(connection.ToNodeId, out var toNodeUI))
            {
                Debug.LogWarning($"Cannot create connection: to node '{connection.ToNodeId}' not found");
                return;
            }

            var connectionUI = Instantiate(_connectionUIPrefab, _connectionsContainer);
            connectionUI.Initialize(connection, fromNodeUI, toNodeUI);

            _connectionUIInstances.Add(connectionUI);
        }

        private void ClearUI()
        {
            // Destroy all node UI instances
            foreach (var nodeUI in _nodeUIInstances.Values)
            {
                if (nodeUI != null)
                {
                    Destroy(nodeUI.gameObject);
                }
            }
            _nodeUIInstances.Clear();

            // Destroy all connection UI instances
            foreach (var connectionUI in _connectionUIInstances)
            {
                if (connectionUI != null)
                {
                    Destroy(connectionUI.gameObject);
                }
            }
            _connectionUIInstances.Clear();
        }

        #endregion

        #region Layout

        private void ApplyLayout()
        {
            if (_layoutEngine == null) return;

            switch (_layoutType)
            {
                case LayoutType.Manual:
                    // Use editor positions as-is
                    break;
                case LayoutType.Grid:
                    _layoutEngine.ApplyGridLayout(_gridCellSize);
                    break;
                case LayoutType.Hierarchical:
                    _layoutEngine.ApplyHierarchicalLayout(_nodeSpacing);
                    break;
                case LayoutType.ForceDirected:
                    _layoutEngine.ApplyForceDirectedLayout(_nodeSpacing);
                    break;
            }
        }

        /// <summary>
        /// Changes the layout type and re-applies it.
        /// </summary>
        public void SetLayoutType(LayoutType layoutType)
        {
            _layoutType = layoutType;
            ApplyLayout();

            // Update node positions
            foreach (var kvp in _nodeUIInstances)
            {
                var node = _skillTreeGraph.GetNodeById(kvp.Key);
                if (node != null)
                {
                    kvp.Value.SetPosition(node.EditorPosition);
                }
            }

            // Update connection visuals
            foreach (var connectionUI in _connectionUIInstances)
            {
                connectionUI.UpdateVisual();
            }
        }

        #endregion

        #region State Updates

        /// <summary>
        /// Refreshes the visual state of all nodes based on current manager state.
        /// </summary>
        public void RefreshAllNodes()
        {
            if (_skillTreeManager == null) return;

            foreach (var kvp in _nodeUIInstances)
            {
                RefreshNodeState(kvp.Key);
            }

            foreach (var connectionUI in _connectionUIInstances)
            {
                connectionUI.UpdateVisual();
            }
        }

        /// <summary>
        /// Refreshes the visual state of a specific node.
        /// </summary>
        public void RefreshNodeState(string nodeId)
        {
            if (_skillTreeManager == null) return;

            if (!_nodeUIInstances.TryGetValue(nodeId, out var nodeUI))
            {
                return;
            }

            bool isUnlocked = _skillTreeManager.IsNodeUnlocked(nodeId);
            bool canUnlock = _skillTreeManager.CanUnlockNode(nodeId);

            SkillNodeState nodeState;
            if (isUnlocked)
            {
                nodeState = SkillNodeState.Unlocked;
            }
            else if (canUnlock)
            {
                nodeState = SkillNodeState.Available;
            }
            else
            {
                nodeState = SkillNodeState.Locked;
            }

            nodeUI.SetState(nodeState);
        }

        #endregion

        #region Event Handling

        private void SubscribeToEvents()
        {
            if (_skillTreeManager == null) return;

            _eventSubscriptions.Add(_skillTreeManager.Subscribe<SkillUnlockedEvent>(OnSkillUnlocked));
            _eventSubscriptions.Add(_skillTreeManager.Subscribe<SkillLockedEvent>(OnSkillLocked));
            _eventSubscriptions.Add(_skillTreeManager.Subscribe<TreeResetEvent>(OnTreeReset));
            _eventSubscriptions.Add(_skillTreeManager.Subscribe<StateLoadedEvent>(OnStateLoaded));
        }

        private void UnsubscribeFromEvents()
        {
            foreach (var subscription in _eventSubscriptions)
            {
                subscription?.Dispose();
            }
            _eventSubscriptions.Clear();
        }

        private void OnSkillUnlocked(SkillUnlockedEvent evt)
        {
            RefreshNodeState(evt.NodeId);

            // Also refresh dependents as they may become available
            if (_skillTreeGraph != null)
            {
                var dependents = _skillTreeGraph.GetDependents(evt.NodeId);
                foreach (var dependent in dependents)
                {
                    RefreshNodeState(dependent.Id);
                }
            }
        }

        private void OnSkillLocked(SkillLockedEvent evt)
        {
            RefreshNodeState(evt.NodeId);

            // Also refresh dependents
            if (_skillTreeGraph != null)
            {
                var dependents = _skillTreeGraph.GetDependents(evt.NodeId);
                foreach (var dependent in dependents)
                {
                    RefreshNodeState(dependent.Id);
                }
            }
        }

        private void OnTreeReset(TreeResetEvent evt)
        {
            RefreshAllNodes();
        }

        private void OnStateLoaded(StateLoadedEvent evt)
        {
            RefreshAllNodes();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the UI instance for a specific node.
        /// </summary>
        public SkillNodeUI GetNodeUI(string nodeId)
        {
            _nodeUIInstances.TryGetValue(nodeId, out var nodeUI);
            return nodeUI;
        }

        /// <summary>
        /// Gets all node UI instances.
        /// </summary>
        public IEnumerable<SkillNodeUI> GetAllNodeUIs()
        {
            return _nodeUIInstances.Values;
        }

        /// <summary>
        /// Gets all connection UI instances.
        /// </summary>
        public IEnumerable<SkillConnectionUI> GetAllConnectionUIs()
        {
            return _connectionUIInstances;
        }

        #endregion
    }

    /// <summary>
    /// Layout types for skill tree arrangement.
    /// </summary>
    public enum LayoutType
    {
        Manual,         // Use designer-placed positions
        Grid,           // Snap to grid
        Hierarchical,   // Root nodes at top, dependencies flow down
        ForceDirected   // Physics-based layout
    }

    /// <summary>
    /// Visual states for skill nodes.
    /// </summary>
    public enum SkillNodeState
    {
        Locked,      // Cannot be unlocked (prerequisites not met)
        Available,   // Can be unlocked
        Unlocked     // Already unlocked
    }
}
