using UnityEngine;
using UnityEditor;
using SkillTree.Core.Data;

namespace SkillTree.Editor.Utilities
{
    public static class SkillTreeValidator
    {
        [MenuItem("Tools/Skill Tree/Validate All Skill Trees")]
        public static void ValidateAllSkillTrees()
        {
            string[] guids = AssetDatabase.FindAssets("t:SkillTreeGraph");

            if (guids.Length == 0)
            {
                Debug.Log("No skill tree assets found in project.");
                return;
            }

            int validCount = 0;
            int invalidCount = 0;
            int warningCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SkillTreeGraph graph = AssetDatabase.LoadAssetAtPath<SkillTreeGraph>(path);

                if (graph != null)
                {
                    ValidationResult result = graph.Validate();

                    if (result.IsValid && !result.HasWarnings)
                    {
                        validCount++;
                        Debug.Log($"✓ {graph.name}: Valid", graph);
                    }
                    else if (result.IsValid && result.HasWarnings)
                    {
                        warningCount++;
                        Debug.LogWarning($"⚠ {graph.name}: Valid with warnings\n{result}", graph);
                    }
                    else
                    {
                        invalidCount++;
                        Debug.LogError($"✗ {graph.name}: Invalid\n{result}", graph);
                    }
                }
            }

            Debug.Log($"Validation Complete: {validCount} valid, {warningCount} with warnings, {invalidCount} invalid");
        }

        [MenuItem("Tools/Skill Tree/Create Example Skill Tree")]
        public static void CreateExampleSkillTree()
        {
            // Create a new skill tree
            SkillTreeGraph graph = ScriptableObject.CreateInstance<SkillTreeGraph>();
            graph.DisplayName = "Example Skill Tree";
            graph.Description = "A simple example skill tree demonstrating basic functionality.";

            // Create root nodes
            SkillNode warrior = new SkillNode
            {
                DisplayName = "Warrior",
                Description = "Basic combat training",
                NodeType = SkillNodeType.Root,
                EditorPosition = new Vector2(100, 100)
            };

            SkillNode mage = new SkillNode
            {
                DisplayName = "Mage",
                Description = "Basic magic training",
                NodeType = SkillNodeType.Root,
                EditorPosition = new Vector2(300, 100)
            };

            // Create tier 2 nodes
            SkillNode swordMastery = new SkillNode
            {
                DisplayName = "Sword Mastery",
                Description = "Improved sword combat",
                NodeType = SkillNodeType.Standard,
                EditorPosition = new Vector2(100, 250)
            };
            swordMastery.AddPrerequisite(warrior.Id);

            SkillNode fireball = new SkillNode
            {
                DisplayName = "Fireball",
                Description = "Launch a ball of fire",
                NodeType = SkillNodeType.Standard,
                EditorPosition = new Vector2(300, 250)
            };
            fireball.AddPrerequisite(mage.Id);

            // Create capstone node
            SkillNode battleMage = new SkillNode
            {
                DisplayName = "Battle Mage",
                Description = "Master of sword and sorcery",
                NodeType = SkillNodeType.Capstone,
                EditorPosition = new Vector2(200, 400)
            };
            battleMage.AddPrerequisite(swordMastery.Id);
            battleMage.AddPrerequisite(fireball.Id);

            // Add nodes to graph
            graph.AddNode(warrior);
            graph.AddNode(mage);
            graph.AddNode(swordMastery);
            graph.AddNode(fireball);
            graph.AddNode(battleMage);

            // Create connections
            graph.AddConnection(new SkillConnection(warrior.Id, swordMastery.Id, ConnectionType.Prerequisite));
            graph.AddConnection(new SkillConnection(mage.Id, fireball.Id, ConnectionType.Prerequisite));
            graph.AddConnection(new SkillConnection(swordMastery.Id, battleMage.Id, ConnectionType.Prerequisite));
            graph.AddConnection(new SkillConnection(fireball.Id, battleMage.Id, ConnectionType.Prerequisite));

            // Validate
            ValidationResult result = graph.Validate();
            if (result.IsValid)
            {
                // Save asset
                string path = "Assets/ExampleSkillTree.asset";
                AssetDatabase.CreateAsset(graph, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Created example skill tree at {path}");
                Selection.activeObject = graph;
                EditorGUIUtility.PingObject(graph);
            }
            else
            {
                Debug.LogError($"Failed to create example skill tree:\n{result}");
            }
        }
    }
}
