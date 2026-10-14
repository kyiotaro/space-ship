using UnityEngine;
using UnityEditor;
using SkillTree.Core.Data;
using System.Collections.Generic;

namespace SkillTree.Editor.Utilities
{
    /// <summary>
    /// Generates a Civilization-style technology tree with different ages and research paths.
    /// Features: Ancient Age → Classical Age → Medieval Age → Renaissance Age
    /// </summary>
    public static class TechTreeGenerator
    {
        [MenuItem("Tools/Skill Tree/Create Example/Tech Tree (Civilization-style)")]
        public static void CreateTechTree()
        {
            var skillTree = ScriptableObject.CreateInstance<SkillTreeGraph>();
            // TreeId is auto-generated and read-only
            skillTree.DisplayName = "Technology Tree";
            skillTree.Description = "Civilization-style technology progression tree";

            var nodes = new List<SkillNode>();

            // === ANCIENT AGE (Starting techs) ===
            var agriculture = CreateTech("tech_agriculture", "Agriculture",
                "Allows farming and food production", SkillNodeType.Root, -400, 0, "Ancient Age");
            nodes.Add(agriculture);

            var mining = CreateTech("tech_mining", "Mining",
                "Allows extraction of stone and metals", SkillNodeType.Root, -200, 0, "Ancient Age");
            nodes.Add(mining);

            var animalHusbandry = CreateTech("tech_animal_husbandry", "Animal Husbandry",
                "Enables domestication of animals", SkillNodeType.Root, 0, 0, "Ancient Age");
            nodes.Add(animalHusbandry);

            var pottery = CreateTech("tech_pottery", "Pottery",
                "Enables storage and trade", SkillNodeType.Root, 200, 0, "Ancient Age");
            nodes.Add(pottery);

            var writing = CreateTech("tech_writing", "Writing",
                "Enables record keeping and culture", SkillNodeType.Root, 400, 0, "Ancient Age");
            nodes.Add(writing);

            // === CLASSICAL AGE ===
            var irrigation = CreateTech("tech_irrigation", "Irrigation",
                "Improves farm yields by 50%", SkillNodeType.Standard, -400, 200, "Classical Age");
            irrigation.PrerequisiteNodeIds.Add(agriculture.Id);
            irrigation.Costs.Add(CreateCost("Science", 50));
            nodes.Add(irrigation);

            var bronzeWorking = CreateTech("tech_bronze_working", "Bronze Working",
                "Unlock bronze weapons and tools", SkillNodeType.Standard, -200, 200, "Classical Age");
            bronzeWorking.PrerequisiteNodeIds.Add(mining.Id);
            bronzeWorking.Costs.Add(CreateCost("Science", 50));
            nodes.Add(bronzeWorking);

            var wheel = CreateTech("tech_wheel", "The Wheel",
                "Enables carts and faster movement", SkillNodeType.Milestone, 0, 200, "Classical Age");
            wheel.PrerequisiteNodeIds.Add(animalHusbandry.Id);
            wheel.PrerequisiteNodeIds.Add(pottery.Id);
            wheel.Costs.Add(CreateCost("Science", 75));
            nodes.Add(wheel);

            var mathematics = CreateTech("tech_mathematics", "Mathematics",
                "Unlocks advanced construction", SkillNodeType.Standard, 200, 200, "Classical Age");
            mathematics.PrerequisiteNodeIds.Add(writing.Id);
            mathematics.Costs.Add(CreateCost("Science", 60));
            nodes.Add(mathematics);

            var philosophy = CreateTech("tech_philosophy", "Philosophy",
                "Increases culture and happiness", SkillNodeType.Standard, 400, 200, "Classical Age");
            philosophy.PrerequisiteNodeIds.Add(writing.Id);
            philosophy.Costs.Add(CreateCost("Science", 60));
            nodes.Add(philosophy);

            // === MEDIEVAL AGE ===
            var feudalism = CreateTech("tech_feudalism", "Feudalism",
                "Unlock castles and knight units", SkillNodeType.Standard, -400, 400, "Medieval Age");
            feudalism.PrerequisiteNodeIds.Add(irrigation.Id);
            feudalism.Costs.Add(CreateCost("Science", 100));
            nodes.Add(feudalism);

            var ironWorking = CreateTech("tech_iron_working", "Iron Working",
                "Unlock iron weapons, +50% military strength", SkillNodeType.Milestone, -200, 400, "Medieval Age");
            ironWorking.PrerequisiteNodeIds.Add(bronzeWorking.Id);
            ironWorking.Costs.Add(CreateCost("Science", 120));
            nodes.Add(ironWorking);

            var engineering = CreateTech("tech_engineering", "Engineering",
                "Build bridges, aqueducts, and siege weapons", SkillNodeType.Standard, 0, 400, "Medieval Age");
            engineering.PrerequisiteNodeIds.Add(wheel.Id);
            engineering.PrerequisiteNodeIds.Add(mathematics.Id);
            engineering.Costs.Add(CreateCost("Science", 150));
            nodes.Add(engineering);

            var education = CreateTech("tech_education", "Education",
                "+50% Science from universities", SkillNodeType.Standard, 200, 400, "Medieval Age");
            education.PrerequisiteNodeIds.Add(mathematics.Id);
            education.PrerequisiteNodeIds.Add(philosophy.Id);
            education.Costs.Add(CreateCost("Science", 110));
            nodes.Add(education);

            var theology = CreateTech("tech_theology", "Theology",
                "Build temples, +25% happiness", SkillNodeType.Standard, 400, 400, "Medieval Age");
            theology.PrerequisiteNodeIds.Add(philosophy.Id);
            theology.Costs.Add(CreateCost("Science", 100));
            nodes.Add(theology);

            // === RENAISSANCE AGE ===
            var gunpowder = CreateTech("tech_gunpowder", "Gunpowder",
                "Revolutionary military technology", SkillNodeType.Milestone, -300, 600, "Renaissance Age");
            gunpowder.PrerequisiteNodeIds.Add(ironWorking.Id);
            gunpowder.Costs.Add(CreateCost("Science", 200));
            nodes.Add(gunpowder);

            var printing = CreateTech("tech_printing_press", "Printing Press",
                "+100% Science, enables mass communication", SkillNodeType.Milestone, 0, 600, "Renaissance Age");
            printing.PrerequisiteNodeIds.Add(education.Id);
            printing.PrerequisiteNodeIds.Add(engineering.Id);
            printing.Costs.Add(CreateCost("Science", 250));
            nodes.Add(printing);

            var banking = CreateTech("tech_banking", "Banking",
                "+50% Gold from trade routes", SkillNodeType.Standard, 300, 600, "Renaissance Age");
            banking.PrerequisiteNodeIds.Add(education.Id);
            banking.Costs.Add(CreateCost("Science", 180));
            nodes.Add(banking);

            // === CAPSTONE TECHS ===
            var industrialization = CreateTech("tech_industrialization", "Industrialization",
                "Enter the modern age", SkillNodeType.Capstone, -150, 800, "Industrial Age");
            industrialization.PrerequisiteNodeIds.Add(gunpowder.Id);
            industrialization.PrerequisiteNodeIds.Add(printing.Id);
            industrialization.Costs.Add(CreateCost("Science", 400));
            nodes.Add(industrialization);

            var enlightenment = CreateTech("tech_enlightenment", "Enlightenment",
                "Ultimate cultural and scientific achievement", SkillNodeType.Capstone, 150, 800, "Industrial Age");
            enlightenment.PrerequisiteNodeIds.Add(printing.Id);
            enlightenment.PrerequisiteNodeIds.Add(banking.Id);
            enlightenment.Costs.Add(CreateCost("Science", 400));
            nodes.Add(enlightenment);

            // Add nodes to the graph
            foreach (var node in nodes)
            {
                skillTree.AddNode(node);
            }

            // Create connections
            CreateAllConnections(skillTree);

            // Save asset
            string path = "Assets/SkillTrees/TechnologyTree.asset";
            string directory = System.IO.Path.GetDirectoryName(path);

            // Create directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder(directory))
            {
                string[] folders = directory.Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }

            AssetDatabase.CreateAsset(skillTree, path);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = skillTree;

            Debug.Log($"<color=green>✓ Created Technology Tree at {path}</color>");
        }

        private static SkillNode CreateTech(string id, string name, string description,
            SkillNodeType type, float x, float y, string age)
        {
            var node = new SkillNode(id)
            {
                DisplayName = name,
                Description = description,
                NodeType = type,
                EditorPosition = new Vector2(x, y)
            };

            // Store age information
            node.CustomData["Age"] = age;

            // Set rarity based on age
            node.CustomData["Rarity"] = age switch
            {
                "Ancient Age" => "Common",
                "Classical Age" => "Common",
                "Medieval Age" => "Rare",
                "Renaissance Age" => "Epic",
                "Industrial Age" => "Legendary",
                _ => "Common"
            };

            return node;
        }

        private static SkillCost CreateCost(string resourceType, int amount)
        {
            return new SkillCost
            {
                ResourceType = resourceType,
                Amount = amount
            };
        }

        private static void CreateAllConnections(SkillTreeGraph graph)
        {
            var connections = new List<SkillConnection>();

            foreach (var node in graph.Nodes)
            {
                foreach (var prereqId in node.PrerequisiteNodeIds)
                {
                    var connection = new SkillConnection
                    {
                        FromNodeId = prereqId,
                        ToNodeId = node.Id,
                        Type = ConnectionType.Prerequisite,
                        Color = new Color(0.7f, 0.9f, 1.0f), // Light blue for tech connections
                        Width = 2f
                    };
                    connections.Add(connection);
                }
            }

            // Add connections to the graph
            foreach (var connection in connections)
            {
                graph.AddConnection(connection);
            }
        }
    }
}
