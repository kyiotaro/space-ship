using UnityEngine;
using UnityEditor;
using SkillTree.Core.Data;
using System.Collections.Generic;

namespace SkillTree.Editor.Utilities
{
    /// <summary>
    /// Generates an example RPG-style skill tree with three branching paths:
    /// - Warrior (melee combat, defense)
    /// - Mage (magic spells, elemental damage)
    /// - Rogue (stealth, critical hits, ranged)
    /// </summary>
    public static class RPGSkillTreeGenerator
    {
        [MenuItem("Tools/Skill Tree/Create Example/RPG Skill Tree")]
        public static void CreateRPGSkillTree()
        {
            var skillTree = ScriptableObject.CreateInstance<SkillTreeGraph>();
            // TreeId is auto-generated and read-only
            skillTree.DisplayName = "RPG Skill Tree";
            skillTree.Description = "Classic RPG skill tree with Warrior, Mage, and Rogue paths";

            var nodes = new List<SkillNode>();

            // === ROOT NODE ===
            var root = CreateNode("root_combat_basics", "Combat Basics",
                "Foundation of all combat skills", SkillNodeType.Root, 0, 0);
            root.CustomData["NodeExtensionType"] = "StatBoost";
            root.CustomData["StatType"] = "Health";
            root.CustomData["Amount"] = "50";
            nodes.Add(root);

            // === WARRIOR PATH ===
            var warrior1 = CreateNode("warrior_melee_training", "Melee Training",
                "+10 Melee Damage", SkillNodeType.Standard, -300, 150);
            warrior1.PrerequisiteNodeIds.Add(root.Id);
            warrior1.Costs.Add(CreateCost("SkillPoints", 1));
            warrior1.CustomData["NodeExtensionType"] = "StatBoost";
            warrior1.CustomData["StatType"] = "Damage";
            warrior1.CustomData["Amount"] = "10";
            warrior1.CustomData["Rarity"] = "Common";
            nodes.Add(warrior1);

            var warrior2 = CreateNode("warrior_shield_mastery", "Shield Mastery",
                "+20 Defense, unlock Shield Block ability", SkillNodeType.Standard, -400, 300);
            warrior2.PrerequisiteNodeIds.Add(warrior1.Id);
            warrior2.Costs.Add(CreateCost("SkillPoints", 1));
            warrior2.CustomData["NodeExtensionType"] = "StatBoost";
            warrior2.CustomData["StatType"] = "Defense";
            warrior2.CustomData["Amount"] = "20";
            warrior2.CustomData["Rarity"] = "Rare";
            nodes.Add(warrior2);

            var warrior3 = CreateNode("warrior_berserker_rage", "Berserker Rage",
                "+30 Damage, -10 Defense, gain Rage ability", SkillNodeType.Milestone, -500, 450);
            warrior3.PrerequisiteNodeIds.Add(warrior2.Id);
            warrior3.Costs.Add(CreateCost("SkillPoints", 2));
            warrior3.CustomData["NodeExtensionType"] = "StatBoost";
            warrior3.CustomData["StatType"] = "Damage";
            warrior3.CustomData["Amount"] = "30";
            warrior3.CustomData["Rarity"] = "Epic";
            nodes.Add(warrior3);

            var warriorCapstone = CreateNode("warrior_titan_strength", "Titan's Strength",
                "+50 Damage, +30 Defense, unlock Titan Form", SkillNodeType.Capstone, -450, 600);
            warriorCapstone.PrerequisiteNodeIds.Add(warrior3.Id);
            warriorCapstone.Costs.Add(CreateCost("SkillPoints", 3));
            warriorCapstone.CustomData["NodeExtensionType"] = "StatBoost";
            warriorCapstone.CustomData["StatType"] = "Damage";
            warriorCapstone.CustomData["Amount"] = "50";
            warriorCapstone.CustomData["Rarity"] = "Legendary";
            nodes.Add(warriorCapstone);

            // === MAGE PATH ===
            var mage1 = CreateNode("mage_arcane_study", "Arcane Studies",
                "+20 Mana, unlock basic spells", SkillNodeType.Standard, 0, 150);
            mage1.PrerequisiteNodeIds.Add(root.Id);
            mage1.Costs.Add(CreateCost("SkillPoints", 1));
            mage1.CustomData["NodeExtensionType"] = "StatBoost";
            mage1.CustomData["StatType"] = "Mana";
            mage1.CustomData["Amount"] = "20";
            mage1.CustomData["Rarity"] = "Common";
            nodes.Add(mage1);

            var mage2a = CreateNode("mage_fire_mastery", "Fire Mastery",
                "+15 Fire Damage, unlock Fireball", SkillNodeType.Standard, -100, 300);
            mage2a.PrerequisiteNodeIds.Add(mage1.Id);
            mage2a.Costs.Add(CreateCost("SkillPoints", 1));
            mage2a.CustomData["NodeExtensionType"] = "StatBoost";
            mage2a.CustomData["StatType"] = "FireDamage";
            mage2a.CustomData["Amount"] = "15";
            mage2a.CustomData["Rarity"] = "Rare";
            nodes.Add(mage2a);

            var mage2b = CreateNode("mage_frost_mastery", "Frost Mastery",
                "+15 Frost Damage, unlock Ice Lance", SkillNodeType.Standard, 100, 300);
            mage2b.PrerequisiteNodeIds.Add(mage1.Id);
            mage2b.Costs.Add(CreateCost("SkillPoints", 1));
            mage2b.CustomData["NodeExtensionType"] = "StatBoost";
            mage2b.CustomData["StatType"] = "FrostDamage";
            mage2b.CustomData["Amount"] = "15";
            mage2b.CustomData["Rarity"] = "Rare";
            nodes.Add(mage2b);

            var mage3 = CreateNode("mage_elemental_fusion", "Elemental Fusion",
                "Combine fire and frost magic", SkillNodeType.Milestone, 0, 450);
            mage3.PrerequisiteNodeIds.Add(mage2a.Id);
            mage3.PrerequisiteNodeIds.Add(mage2b.Id);
            mage3.Costs.Add(CreateCost("SkillPoints", 2));
            mage3.CustomData["Rarity"] = "Epic";
            nodes.Add(mage3);

            var mageCapstone = CreateNode("mage_archmage", "Archmage",
                "+100 Mana, +50 Spell Power, unlock Meteor", SkillNodeType.Capstone, 0, 600);
            mageCapstone.PrerequisiteNodeIds.Add(mage3.Id);
            mageCapstone.Costs.Add(CreateCost("SkillPoints", 3));
            mageCapstone.CustomData["NodeExtensionType"] = "StatBoost";
            mageCapstone.CustomData["StatType"] = "SpellPower";
            mageCapstone.CustomData["Amount"] = "50";
            mageCapstone.CustomData["Rarity"] = "Legendary";
            nodes.Add(mageCapstone);

            // === ROGUE PATH ===
            var rogue1 = CreateNode("rogue_stealth_training", "Stealth Training",
                "+10 Stealth, unlock Sneak ability", SkillNodeType.Standard, 300, 150);
            rogue1.PrerequisiteNodeIds.Add(root.Id);
            rogue1.Costs.Add(CreateCost("SkillPoints", 1));
            rogue1.CustomData["NodeExtensionType"] = "StatBoost";
            rogue1.CustomData["StatType"] = "Stealth";
            rogue1.CustomData["Amount"] = "10";
            rogue1.CustomData["Rarity"] = "Common";
            nodes.Add(rogue1);

            var rogue2 = CreateNode("rogue_critical_strikes", "Critical Strikes",
                "+15% Critical Hit Chance", SkillNodeType.Standard, 400, 300);
            rogue2.PrerequisiteNodeIds.Add(rogue1.Id);
            rogue2.Costs.Add(CreateCost("SkillPoints", 1));
            rogue2.CustomData["NodeExtensionType"] = "StatBoost";
            rogue2.CustomData["StatType"] = "CritChance";
            rogue2.CustomData["Amount"] = "15";
            rogue2.CustomData["Rarity"] = "Rare";
            nodes.Add(rogue2);

            var rogue3 = CreateNode("rogue_poison_mastery", "Poison Mastery",
                "+20 Poison Damage, unlock Envenom", SkillNodeType.Milestone, 500, 450);
            rogue3.PrerequisiteNodeIds.Add(rogue2.Id);
            rogue3.Costs.Add(CreateCost("SkillPoints", 2));
            rogue3.CustomData["NodeExtensionType"] = "StatBoost";
            rogue3.CustomData["StatType"] = "PoisonDamage";
            rogue3.CustomData["Amount"] = "20";
            rogue3.CustomData["Rarity"] = "Epic";
            nodes.Add(rogue3);

            var rogueCapstone = CreateNode("rogue_shadow_master", "Shadow Master",
                "+30 Stealth, +25% Crit Chance, unlock Shadow Clone", SkillNodeType.Capstone, 450, 600);
            rogueCapstone.PrerequisiteNodeIds.Add(rogue3.Id);
            rogueCapstone.Costs.Add(CreateCost("SkillPoints", 3));
            rogueCapstone.CustomData["NodeExtensionType"] = "StatBoost";
            rogueCapstone.CustomData["StatType"] = "Stealth";
            rogueCapstone.CustomData["Amount"] = "30";
            rogueCapstone.CustomData["Rarity"] = "Legendary";
            nodes.Add(rogueCapstone);

            // === HYBRID PATH (requires multiple paths) ===
            var hybrid = CreateNode("hybrid_master_combatant", "Master Combatant",
                "Ultimate mastery combining all paths", SkillNodeType.Special, 0, 750);
            hybrid.PrerequisiteNodeIds.Add(warriorCapstone.Id);
            hybrid.PrerequisiteNodeIds.Add(mageCapstone.Id);
            hybrid.PrerequisiteNodeIds.Add(rogueCapstone.Id);
            hybrid.Costs.Add(CreateCost("SkillPoints", 5));
            hybrid.CustomData["Rarity"] = "Legendary";
            nodes.Add(hybrid);

            // Add nodes to the graph
            foreach (var node in nodes)
            {
                skillTree.AddNode(node);
            }

            // Create connections
            CreateAllConnections(skillTree);

            // Save asset
            string path = "Assets/SkillTrees/RPGSkillTree.asset";
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

            Debug.Log($"<color=green>✓ Created RPG Skill Tree at {path}</color>");
        }

        private static SkillNode CreateNode(string id, string name, string description,
            SkillNodeType type, float x, float y)
        {
            var node = new SkillNode(id)
            {
                DisplayName = name,
                Description = description,
                NodeType = type,
                EditorPosition = new Vector2(x, y)
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
                        Color = Color.white,
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
