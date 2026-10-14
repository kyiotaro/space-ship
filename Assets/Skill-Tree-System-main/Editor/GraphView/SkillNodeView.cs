using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using SkillTree.Core.Data;

namespace SkillTree.Editor.GraphView
{
    /// <summary>
    /// Visual representation of a SkillNode in the GraphView
    /// </summary>
    public class SkillNodeView : Node
    {
        private SkillNode _node;
        private SkillTreeGraphView _graphView;
        private Port _inputPort;
        private Port _outputPort;

        private Label _titleLabel;
        private Label _typeLabel;
        private VisualElement _iconContainer;
        private Image _iconImage;
        private VisualElement _infoContainer;
        private Label _descriptionLabel;
        private Label _prereqLabel;
        private Label _costLabel;

        public SkillNode Node => _node;
        public Port InputPort => _inputPort;
        public Port OutputPort => _outputPort;

        public SkillNodeView(SkillNode node, SkillTreeGraphView graphView)
        {
            _node = node;
            _graphView = graphView;

            userData = node;

            // Set node capabilities
            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable | Capabilities.Copiable;

            // Setup node UI
            ConstructNodeUI();
            CreatePorts();
            RefreshNodeData();

            // Register callbacks
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void ConstructNodeUI()
        {
            // Set title
            title = _node.DisplayName;

            // Add node type badge
            _typeLabel = new Label(GetNodeTypeDisplayName());
            _typeLabel.AddToClassList("node-type-label");
            titleContainer.Add(_typeLabel);

            // Add icon display
            _iconContainer = new VisualElement();
            _iconContainer.AddToClassList("node-icon-container");

            _iconImage = new Image();
            _iconImage.AddToClassList("node-icon");
            if (_node.Icon != null)
            {
                _iconImage.image = _node.Icon.texture;
            }
            _iconContainer.Add(_iconImage);
            mainContainer.Insert(0, _iconContainer);

            // Add node info section
            _infoContainer = new VisualElement();
            _infoContainer.AddToClassList("node-info-container");

            // Description preview (truncated)
            _descriptionLabel = new Label(TruncateText(_node.Description, 50));
            _descriptionLabel.AddToClassList("node-description");
            _infoContainer.Add(_descriptionLabel);

            // Prerequisites count
            _prereqLabel = new Label($"Prerequisites: {_node.PrerequisiteNodeIds.Count}");
            _prereqLabel.AddToClassList("node-stat");
            _infoContainer.Add(_prereqLabel);

            // Costs display
            _costLabel = new Label($"Costs: {_node.Costs.Count}");
            _costLabel.AddToClassList("node-stat");
            _infoContainer.Add(_costLabel);

            mainContainer.Add(_infoContainer);

            // Add edit button
            var editButton = new Button(() => OpenInspector())
            {
                text = "Edit"
            };
            editButton.AddToClassList("node-edit-button");
            mainContainer.Add(editButton);

            // Style based on node type
            ApplyNodeTypeStyle();
        }

        private void CreatePorts()
        {
            // Input port (for prerequisites coming in)
            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            _inputPort.portName = "Prerequisites";
            inputContainer.Add(_inputPort);

            // Output port (for this node to be a prerequisite for others)
            _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            _outputPort.portName = "Unlocks";
            outputContainer.Add(_outputPort);
        }

        public void RefreshNodeData()
        {
            title = _node.DisplayName;
            _typeLabel.text = GetNodeTypeDisplayName();

            if (_node.Icon != null)
            {
                _iconImage.image = _node.Icon.texture;
                _iconContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                _iconContainer.style.display = DisplayStyle.None;
            }

            // Update description
            if (_descriptionLabel != null)
            {
                _descriptionLabel.text = TruncateText(_node.Description, 50);
                _descriptionLabel.style.display = string.IsNullOrEmpty(_node.Description) ? DisplayStyle.None : DisplayStyle.Flex;
            }

            // Update prerequisites count
            if (_prereqLabel != null)
            {
                _prereqLabel.text = $"Prerequisites: {_node.PrerequisiteNodeIds.Count}";
                _prereqLabel.style.display = _node.HasPrerequisites() ? DisplayStyle.Flex : DisplayStyle.None;
            }

            // Update costs
            if (_costLabel != null)
            {
                _costLabel.text = $"Costs: {_node.Costs.Count}";
                _costLabel.style.display = _node.HasCosts() ? DisplayStyle.Flex : DisplayStyle.None;
            }

            ApplyNodeTypeStyle();
        }

        private void ApplyNodeTypeStyle()
        {
            // Remove all type classes
            RemoveFromClassList("node-standard");
            RemoveFromClassList("node-root");
            RemoveFromClassList("node-milestone");
            RemoveFromClassList("node-capstone");
            RemoveFromClassList("node-special");

            // Add appropriate class based on node type
            switch (_node.NodeType)
            {
                case SkillNodeType.Root:
                    AddToClassList("node-root");
                    break;
                case SkillNodeType.Milestone:
                    AddToClassList("node-milestone");
                    break;
                case SkillNodeType.Capstone:
                    AddToClassList("node-capstone");
                    break;
                case SkillNodeType.Special:
                    AddToClassList("node-special");
                    break;
                default:
                    AddToClassList("node-standard");
                    break;
            }
        }

        private string GetNodeTypeDisplayName()
        {
            return _node.NodeType switch
            {
                SkillNodeType.Root => "ROOT",
                SkillNodeType.Milestone => "MILESTONE",
                SkillNodeType.Capstone => "CAPSTONE",
                SkillNodeType.Special => "SPECIAL",
                _ => "STANDARD"
            };
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        private void OpenInspector()
        {
            // Create a popup window or inspector for detailed editing
            SkillNodeInspectorWindow.OpenWindow(_node, _graphView, this);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                // Double-click to edit
                OpenInspector();
                evt.StopPropagation();
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            // Update the node data
            if (_node != null)
            {
                _node.EditorPosition = newPos.position;
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Edit Properties", a => OpenInspector());
            evt.menu.AppendSeparator();

            // Node type submenu
            evt.menu.AppendAction($"Set Type/Standard",
                a => SetNodeType(SkillNodeType.Standard),
                a => _node.NodeType == SkillNodeType.Standard ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            evt.menu.AppendAction($"Set Type/Root",
                a => SetNodeType(SkillNodeType.Root),
                a => _node.NodeType == SkillNodeType.Root ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            evt.menu.AppendAction($"Set Type/Milestone",
                a => SetNodeType(SkillNodeType.Milestone),
                a => _node.NodeType == SkillNodeType.Milestone ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            evt.menu.AppendAction($"Set Type/Capstone",
                a => SetNodeType(SkillNodeType.Capstone),
                a => _node.NodeType == SkillNodeType.Capstone ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            evt.menu.AppendAction($"Set Type/Special",
                a => SetNodeType(SkillNodeType.Special),
                a => _node.NodeType == SkillNodeType.Special ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
        }

        private void SetNodeType(SkillNodeType nodeType)
        {
            if (_graphView?.Graph == null)
                return;

            Undo.RecordObject(_graphView.Graph, "Change Node Type");
            _node.NodeType = nodeType;
            RefreshNodeData();
            EditorUtility.SetDirty(_graphView.Graph);
        }
    }

    /// <summary>
    /// Simple inspector window for detailed node editing
    /// </summary>
    public class SkillNodeInspectorWindow : EditorWindow
    {
        private SkillNode _node;
        private SkillTreeGraphView _graphView;
        private SkillNodeView _nodeView;
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPosition;

        public static void OpenWindow(SkillNode node, SkillTreeGraphView graphView, SkillNodeView nodeView)
        {
            var window = GetWindow<SkillNodeInspectorWindow>("Edit Skill Node");
            window.minSize = new Vector2(400, 500);
            window._node = node;
            window._graphView = graphView;
            window._nodeView = nodeView;
            window.Show();
        }

        private void OnGUI()
        {
            if (_node == null)
            {
                EditorGUILayout.HelpBox("No node selected", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUI.BeginChangeCheck();

            // Basic properties
            EditorGUILayout.LabelField("Basic Properties", EditorStyles.boldLabel);
            _node.DisplayName = EditorGUILayout.TextField("Display Name", _node.DisplayName);
            _node.Description = EditorGUILayout.TextArea(_node.Description, GUILayout.Height(60));
            _node.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", _node.Icon, typeof(Sprite), false);

            EditorGUILayout.Space(10);

            // Localization
            EditorGUILayout.LabelField("Localization", EditorStyles.boldLabel);
            _node.DisplayNameLocalizationKey = EditorGUILayout.TextField("Name Key", _node.DisplayNameLocalizationKey);
            _node.DescriptionLocalizationKey = EditorGUILayout.TextField("Description Key", _node.DescriptionLocalizationKey);

            EditorGUILayout.Space(10);

            // Node type
            EditorGUILayout.LabelField("Node Type", EditorStyles.boldLabel);
            _node.NodeType = (SkillNodeType)EditorGUILayout.EnumPopup("Type", _node.NodeType);

            EditorGUILayout.Space(10);

            // Read-only info
            EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Node ID", _node.Id);
            EditorGUILayout.IntField("Prerequisites Count", _node.PrerequisiteNodeIds.Count);
            EditorGUILayout.Vector2Field("Editor Position", _node.EditorPosition);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            // Costs
            EditorGUILayout.LabelField($"Costs ({_node.Costs.Count})", EditorStyles.boldLabel);
            int costToRemove = -1;
            for (int i = 0; i < _node.Costs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                _node.Costs[i].ResourceType = EditorGUILayout.TextField("Type", _node.Costs[i].ResourceType);
                _node.Costs[i].Amount = EditorGUILayout.IntField("Amount", _node.Costs[i].Amount);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    costToRemove = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (costToRemove >= 0)
            {
                _node.Costs.RemoveAt(costToRemove);
            }

            if (GUILayout.Button("Add Cost"))
            {
                _node.Costs.Add(new SkillCost { ResourceType = "Currency", Amount = 100 });
            }

            EditorGUILayout.Space(10);

            // Requirements
            EditorGUILayout.LabelField($"Requirements ({_node.Requirements.Count})", EditorStyles.boldLabel);
            int requirementToRemove = -1;
            for (int i = 0; i < _node.Requirements.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _node.Requirements[i].Type = (RequirementType)EditorGUILayout.EnumPopup("Type", _node.Requirements[i].Type);
                _node.Requirements[i].Logic = (RequirementLogic)EditorGUILayout.EnumPopup("Logic", _node.Requirements[i].Logic);

                if (_node.Requirements[i].Type == RequirementType.Custom)
                {
                    _node.Requirements[i].CustomCondition = EditorGUILayout.TextField("Condition", _node.Requirements[i].CustomCondition);
                }

                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    requirementToRemove = i;
                }
                EditorGUILayout.EndVertical();
            }

            if (requirementToRemove >= 0)
            {
                _node.Requirements.RemoveAt(requirementToRemove);
            }

            if (GUILayout.Button("Add Requirement"))
            {
                _node.Requirements.Add(new SkillRequirement { Type = RequirementType.NodeUnlocked, Logic = RequirementLogic.And });
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (_graphView?.Graph != null)
                {
                    EditorUtility.SetDirty(_graphView.Graph);
                }

                // Refresh the node view to reflect changes immediately
                if (_nodeView != null)
                {
                    _nodeView.RefreshNodeData();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
