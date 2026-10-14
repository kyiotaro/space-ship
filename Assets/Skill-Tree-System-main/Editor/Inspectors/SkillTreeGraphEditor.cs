using UnityEngine;
using UnityEditor;
using SkillTree.Core.Data;

namespace SkillTree.Editor.Inspectors
{
    [CustomEditor(typeof(SkillTreeGraph))]
    public class SkillTreeGraphEditor : UnityEditor.Editor
    {
        private SkillTreeGraph _graph;
        private ValidationResult _validationResult;
        private bool _showValidation = true;
        private bool _showNodes = true;
        private bool _showConnections = false;
        private Vector2 _nodesScrollPosition;
        private Vector2 _connectionsScrollPosition;

        private void OnEnable()
        {
            _graph = target as SkillTreeGraph;
            ValidateGraph();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawBasicInfo();
            EditorGUILayout.Space(10);

            DrawStatistics();
            EditorGUILayout.Space(10);

            DrawValidationSection();
            EditorGUILayout.Space(10);

            DrawNodesSection();
            EditorGUILayout.Space(10);

            DrawConnectionsSection();
            EditorGUILayout.Space(10);

            DrawActions();

            serializedObject.ApplyModifiedProperties();
        }

        private new void DrawHeader()
        {
            EditorGUILayout.LabelField("Skill Tree Graph", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This is the main skill tree asset. Use the Skill Tree Editor window for visual editing.\n" +
                "Window > Skill Tree Editor",
                MessageType.Info
            );
        }

        private void DrawBasicInfo()
        {
            EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            var displayNameProp = serializedObject.FindProperty("_displayName");
            var descriptionProp = serializedObject.FindProperty("_description");

            EditorGUILayout.PropertyField(displayNameProp, new GUIContent("Display Name"));
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Description"));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Tree ID", _graph.TreeId);
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawStatistics()
        {
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Total Nodes: {_graph.NodeCount}");
            EditorGUILayout.LabelField($"Total Connections: {_graph.ConnectionCount}");
            EditorGUILayout.LabelField($"Root Nodes: {_graph.GetRootNodes().Count}");
            EditorGUILayout.EndVertical();
        }

        private void DrawValidationSection()
        {
            _showValidation = EditorGUILayout.Foldout(_showValidation, "Validation", true);

            if (!_showValidation) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("Validate Graph"))
            {
                ValidateGraph();
            }

            if (_validationResult != null)
            {
                EditorGUILayout.Space(5);

                if (_validationResult.IsValid && !_validationResult.HasWarnings)
                {
                    EditorGUILayout.HelpBox("Graph is valid!", MessageType.Info);
                }
                else
                {
                    if (_validationResult.Errors.Count > 0)
                    {
                        EditorGUILayout.HelpBox($"Found {_validationResult.Errors.Count} error(s)", MessageType.Error);
                        foreach (var error in _validationResult.Errors)
                        {
                            EditorGUILayout.LabelField($"• {error}", EditorStyles.wordWrappedLabel);
                        }
                    }

                    if (_validationResult.Warnings.Count > 0)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.HelpBox($"Found {_validationResult.Warnings.Count} warning(s)", MessageType.Warning);
                        foreach (var warning in _validationResult.Warnings)
                        {
                            EditorGUILayout.LabelField($"• {warning}", EditorStyles.wordWrappedLabel);
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawNodesSection()
        {
            _showNodes = EditorGUILayout.Foldout(_showNodes, $"Nodes ({_graph.NodeCount})", true);

            if (!_showNodes) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (_graph.NodeCount == 0)
            {
                EditorGUILayout.LabelField("No nodes in graph", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                _nodesScrollPosition = EditorGUILayout.BeginScrollView(_nodesScrollPosition, GUILayout.MaxHeight(200));

                foreach (var node in _graph.Nodes)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(node.DisplayName) ? "(Unnamed)" : node.DisplayName, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"ID: {node.Id}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Type: {node.NodeType}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Prerequisites: {node.PrerequisiteNodeIds.Count}", EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();

                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = _graph;
                        EditorGUIUtility.PingObject(_graph);
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawConnectionsSection()
        {
            _showConnections = EditorGUILayout.Foldout(_showConnections, $"Connections ({_graph.ConnectionCount})", true);

            if (!_showConnections) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (_graph.ConnectionCount == 0)
            {
                EditorGUILayout.LabelField("No connections in graph", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                _connectionsScrollPosition = EditorGUILayout.BeginScrollView(_connectionsScrollPosition, GUILayout.MaxHeight(200));

                foreach (var connection in _graph.Connections)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    var fromNode = _graph.GetNodeById(connection.FromNodeId);
                    var toNode = _graph.GetNodeById(connection.ToNodeId);

                    string fromName = fromNode != null ? fromNode.DisplayName : connection.FromNodeId;
                    string toName = toNode != null ? toNode.DisplayName : connection.ToNodeId;

                    EditorGUILayout.LabelField($"{fromName} → {toName} ({connection.Type})");

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("Open in Skill Tree Editor"))
            {
                SkillTree.Editor.GraphView.SkillTreeGraphWindow.OpenWindow(_graph);
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Test Node"))
            {
                Undo.RecordObject(_graph, "Add Test Node");
                var node = new SkillNode
                {
                    DisplayName = $"Node {_graph.NodeCount + 1}",
                    Description = "Test node",
                    EditorPosition = new Vector2(100, 100 + _graph.NodeCount * 100)
                };
                _graph.AddNode(node);
                ValidateGraph();
                EditorUtility.SetDirty(_graph);
            }

            if (GUILayout.Button("Clear All"))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear Skill Tree",
                    "Are you sure you want to remove all nodes and connections?",
                    "Yes",
                    "No"))
                {
                    Undo.RecordObject(_graph, "Clear Skill Tree");
                    _graph.Nodes.Clear();
                    _graph.Connections.Clear();
                    ValidateGraph();
                    EditorUtility.SetDirty(_graph);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ValidateGraph()
        {
            _validationResult = _graph.Validate();
        }
    }
}
