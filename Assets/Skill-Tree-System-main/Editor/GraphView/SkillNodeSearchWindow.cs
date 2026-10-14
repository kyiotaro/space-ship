using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using SkillTree.Core.Data;

namespace SkillTree.Editor.GraphView
{
    /// <summary>
    /// Search window for creating skill nodes with different types
    /// </summary>
    public class SkillNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private SkillTreeGraphView _graphView;
        private Vector2 _nodePosition;

        public void Initialize(SkillTreeGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Skill Node"), 0),
            };

            // Add node type entries
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Node Types"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Standard Node", EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo").image))
            {
                level = 2,
                userData = SkillNodeType.Standard
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Root Node", EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo").image))
            {
                level = 2,
                userData = SkillNodeType.Root
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Milestone Node", EditorGUIUtility.IconContent("sv_icon_dot4_pix16_gizmo").image))
            {
                level = 2,
                userData = SkillNodeType.Milestone
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Capstone Node", EditorGUIUtility.IconContent("sv_icon_dot5_pix16_gizmo").image))
            {
                level = 2,
                userData = SkillNodeType.Capstone
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Special Node", EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo").image))
            {
                level = 2,
                userData = SkillNodeType.Special
            });

            // Add quick templates
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Quick Templates"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Passive Skill"))
            {
                level = 2,
                userData = new NodeTemplate
                {
                    NodeType = SkillNodeType.Standard,
                    NamePrefix = "Passive",
                    Description = "Passive skill that provides continuous benefits"
                }
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Active Ability"))
            {
                level = 2,
                userData = new NodeTemplate
                {
                    NodeType = SkillNodeType.Standard,
                    NamePrefix = "Active",
                    Description = "Active ability that can be triggered"
                }
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Stat Boost"))
            {
                level = 2,
                userData = new NodeTemplate
                {
                    NodeType = SkillNodeType.Standard,
                    NamePrefix = "Stat Boost",
                    Description = "Increases character statistics"
                }
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Ultimate Ability"))
            {
                level = 2,
                userData = new NodeTemplate
                {
                    NodeType = SkillNodeType.Capstone,
                    NamePrefix = "Ultimate",
                    Description = "Powerful capstone ability"
                }
            });

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (_graphView == null || _graphView.Graph == null)
                return false;

            // Calculate mouse position in graph space
            var windowRoot = _graphView.window.rootVisualElement;
            var windowMousePosition = context.screenMousePosition - _graphView.window.position.position;
            // Convert to graph local position (simplified)
            var graphMousePosition = _graphView.contentViewContainer.transform.matrix.inverse.MultiplyPoint3x4(windowMousePosition);

            // Create node based on selection
            SkillNode node = null;

            if (entry.userData is SkillNodeType nodeType)
            {
                node = CreateNodeByType(nodeType, graphMousePosition);
            }
            else if (entry.userData is NodeTemplate template)
            {
                node = CreateNodeFromTemplate(template, graphMousePosition);
            }

            if (node != null)
            {
                Undo.RecordObject(_graphView.Graph, "Create Skill Node");
                _graphView.Graph.AddNode(node);
                _graphView.PopulateView(_graphView.Graph);
                EditorUtility.SetDirty(_graphView.Graph);
                return true;
            }

            return false;
        }

        private SkillNode CreateNodeByType(SkillNodeType nodeType, Vector2 position)
        {
            string typeName = nodeType.ToString();
            return new SkillNode
            {
                DisplayName = $"{typeName} Node",
                Description = $"A {typeName.ToLower()} skill node",
                NodeType = nodeType,
                EditorPosition = position
            };
        }

        private SkillNode CreateNodeFromTemplate(NodeTemplate template, Vector2 position)
        {
            int nodeCount = _graphView.Graph?.NodeCount ?? 0;
            return new SkillNode
            {
                DisplayName = $"{template.NamePrefix} {nodeCount + 1}",
                Description = template.Description,
                NodeType = template.NodeType,
                EditorPosition = position
            };
        }

        private class NodeTemplate
        {
            public SkillNodeType NodeType;
            public string NamePrefix;
            public string Description;
        }
    }
}
