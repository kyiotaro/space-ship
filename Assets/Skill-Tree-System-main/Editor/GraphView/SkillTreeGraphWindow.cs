using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using SkillTree.Core.Data;

namespace SkillTree.Editor.GraphView
{
    /// <summary>
    /// Main editor window for visual skill tree editing using GraphView
    /// </summary>
    public class SkillTreeGraphWindow : EditorWindow
    {
        private SkillTreeGraph _currentGraph;
        private SkillTreeGraphView _graphView;
        private ToolbarMenu _fileMenu;
        private ToolbarButton _saveButton;
        private ToolbarButton _validateButton;
        private Label _statusLabel;
        private Label _graphNameLabel;

        private const string WINDOW_TITLE = "Skill Tree Editor";
        private const string LAST_OPENED_GRAPH_KEY = "SkillTreeEditor_LastGraph";

        [MenuItem("Window/Skill Tree Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<SkillTreeGraphWindow>();
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.minSize = new Vector2(800, 600);
        }

        /// <summary>
        /// Opens the window with a specific skill tree graph
        /// </summary>
        public static void OpenWindow(SkillTreeGraph graph)
        {
            var window = GetWindow<SkillTreeGraphWindow>();
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.minSize = new Vector2(800, 600);
            window.LoadGraph(graph);
        }

        private void OnEnable()
        {
            ConstructUI();
            TryLoadLastGraph();
        }

        private void OnDisable()
        {
            if (_currentGraph != null)
            {
                SaveLastOpenedGraph();
            }
        }

        private void ConstructUI()
        {
            // Create root visual element
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            // Create toolbar
            var toolbar = new Toolbar();
            root.Add(toolbar);

            // File menu
            _fileMenu = new ToolbarMenu { text = "File" };
            _fileMenu.menu.AppendAction("New Skill Tree", a => CreateNewGraph());
            _fileMenu.menu.AppendAction("Open Skill Tree", a => OpenGraphDialog());
            _fileMenu.menu.AppendSeparator();
            _fileMenu.menu.AppendAction("Save", a => SaveGraph(), a =>
                _currentGraph != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            _fileMenu.menu.AppendAction("Save As...", a => SaveGraphAs(), a =>
                _currentGraph != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            toolbar.Add(_fileMenu);

            // Edit menu
            var editMenu = new ToolbarMenu { text = "Edit" };
            editMenu.menu.AppendAction("Add Node", a => AddNode(), a =>
                _currentGraph != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            editMenu.menu.AppendSeparator();
            editMenu.menu.AppendAction("Validate Graph", a => ValidateGraph(), a =>
                _currentGraph != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            toolbar.Add(editMenu);

            // Layout menu
            var layoutMenu = new ToolbarMenu { text = "Layout" };
            layoutMenu.menu.AppendAction("Auto Layout - Hierarchical", a => AutoLayoutHierarchical(), a =>
                _currentGraph != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            layoutMenu.menu.AppendAction("Auto Layout - Grid", a => AutoLayoutGrid(), a =>
                _currentGraph != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            toolbar.Add(layoutMenu);

            // Spacer
            var spacer = new ToolbarSpacer { flex = true };
            toolbar.Add(spacer);

            // Graph name label
            _graphNameLabel = new Label("No graph loaded");
            _graphNameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _graphNameLabel.style.paddingLeft = 10;
            _graphNameLabel.style.paddingRight = 10;
            toolbar.Add(_graphNameLabel);

            // Status label
            _statusLabel = new Label("Ready");
            _statusLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            _statusLabel.style.paddingRight = 10;
            toolbar.Add(_statusLabel);

            // Create graph view container
            var graphContainer = new VisualElement();
            graphContainer.style.flexGrow = 1;
            root.Add(graphContainer);

            // Create the graph view (will be initialized when a graph is loaded)
            CreateGraphView(graphContainer);
        }

        private void CreateGraphView(VisualElement container)
        {
            if (_graphView != null)
            {
                container.Remove(_graphView);
            }

            _graphView = new SkillTreeGraphView(this)
            {
                style = { flexGrow = 1 }
            };

            container.Add(_graphView);

            if (_currentGraph != null)
            {
                _graphView.PopulateView(_currentGraph);
            }
        }

        #region Graph Loading/Saving

        private void CreateNewGraph()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Skill Tree",
                "NewSkillTree",
                "asset",
                "Enter a name for the new skill tree");

            if (string.IsNullOrEmpty(path))
                return;

            var newGraph = CreateInstance<SkillTreeGraph>();
            newGraph.DisplayName = System.IO.Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(newGraph, path);
            AssetDatabase.SaveAssets();

            LoadGraph(newGraph);
            UpdateStatus("Created new skill tree");
        }

        private void OpenGraphDialog()
        {
            string path = EditorUtility.OpenFilePanel(
                "Open Skill Tree",
                "Assets",
                "asset");

            if (string.IsNullOrEmpty(path))
                return;

            // Convert absolute path to relative path
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var graph = AssetDatabase.LoadAssetAtPath<SkillTreeGraph>(path);
            if (graph != null)
            {
                LoadGraph(graph);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to load skill tree graph", "OK");
            }
        }

        public void LoadGraph(SkillTreeGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("Attempted to load null graph");
                return;
            }

            _currentGraph = graph;
            _graphView?.PopulateView(graph);

            UpdateGraphNameLabel();
            UpdateStatus($"Loaded: {graph.DisplayName}");
        }

        private void SaveGraph()
        {
            if (_currentGraph == null)
            {
                UpdateStatus("No graph to save");
                return;
            }

            _graphView?.SaveGraphData();

            EditorUtility.SetDirty(_currentGraph);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UpdateStatus($"Saved: {_currentGraph.DisplayName}");
        }

        private void SaveGraphAs()
        {
            if (_currentGraph == null)
                return;

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Skill Tree As",
                _currentGraph.DisplayName,
                "asset",
                "Enter a name for the skill tree");

            if (string.IsNullOrEmpty(path))
                return;

            var newGraph = Instantiate(_currentGraph);
            newGraph.DisplayName = System.IO.Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(newGraph, path);
            AssetDatabase.SaveAssets();

            LoadGraph(newGraph);
            UpdateStatus($"Saved as: {newGraph.DisplayName}");
        }

        private void TryLoadLastGraph()
        {
            string lastGraphPath = EditorPrefs.GetString(LAST_OPENED_GRAPH_KEY, "");
            if (!string.IsNullOrEmpty(lastGraphPath))
            {
                var graph = AssetDatabase.LoadAssetAtPath<SkillTreeGraph>(lastGraphPath);
                if (graph != null)
                {
                    LoadGraph(graph);
                }
            }
        }

        private void SaveLastOpenedGraph()
        {
            if (_currentGraph != null)
            {
                string path = AssetDatabase.GetAssetPath(_currentGraph);
                EditorPrefs.SetString(LAST_OPENED_GRAPH_KEY, path);
            }
        }

        #endregion

        #region Actions

        private void AddNode()
        {
            if (_currentGraph == null || _graphView == null)
                return;

            _graphView.CreateNode(Vector2.zero);
            UpdateStatus("Added new node");
        }

        private void ValidateGraph()
        {
            if (_currentGraph == null)
                return;

            var result = _currentGraph.Validate();

            if (result.IsValid && !result.HasWarnings)
            {
                EditorUtility.DisplayDialog("Validation Success", "Graph is valid!", "OK");
                UpdateStatus("Validation passed");
            }
            else
            {
                string message = result.ToString();
                EditorUtility.DisplayDialog("Validation Result", message, "OK");
                UpdateStatus($"Validation: {result.Errors.Count} errors, {result.Warnings.Count} warnings");
            }
        }

        private void AutoLayoutHierarchical()
        {
            if (_currentGraph == null || _graphView == null)
                return;

            _graphView.ApplyHierarchicalLayout();
            UpdateStatus("Applied hierarchical layout");
        }

        private void AutoLayoutGrid()
        {
            if (_currentGraph == null || _graphView == null)
                return;

            _graphView.ApplyGridLayout();
            UpdateStatus("Applied grid layout");
        }

        #endregion

        #region UI Updates

        private void UpdateStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
            }
        }

        private void UpdateGraphNameLabel()
        {
            if (_graphNameLabel != null && _currentGraph != null)
            {
                _graphNameLabel.text = string.IsNullOrEmpty(_currentGraph.DisplayName)
                    ? "Unnamed Graph"
                    : _currentGraph.DisplayName;
            }
        }

        #endregion

        public SkillTreeGraph CurrentGraph => _currentGraph;
    }
}
