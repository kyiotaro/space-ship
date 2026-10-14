using UnityEngine;
using SkillTree.SaveSystem;

namespace SkillTree.Examples
{
    /// <summary>
    /// Example demonstrating basic save system usage.
    /// This can be used standalone before the core skill tree system is implemented.
    /// </summary>
    public class SaveSystemExample : MonoBehaviour
    {
        private ISaveSystem<SkillTreeSaveData> _saveSystem;
        private SkillTreeSaveData _currentSave;

        private void Awake()
        {
            // Initialize save system with configuration
            var config = new SaveSystemConfig
            {
                SaveDirectory = "SkillTreeDemo",
                CreateBackups = true,
                MaxBackups = 3,
                PrettyPrint = true
            };

            _saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);
        }

        private void Start()
        {
            // Try to load existing save, or create new one
            LoadOrCreateNewSave();

            // Display current save state
            DisplaySaveState();
        }

        private void LoadOrCreateNewSave()
        {
            const string saveKey = "demo_save";

            if (_saveSystem.SaveExists(saveKey))
            {
                Debug.Log("Loading existing save...");
                _currentSave = _saveSystem.Load(saveKey);
            }
            else
            {
                Debug.Log("Creating new save...");
                _currentSave = new SkillTreeSaveData
                {
                    SkillTreeId = "demo_tree",
                };

                // Initialize with some starting resources
                _currentSave.SetResource("SkillPoints", 5f);
                _currentSave.SetResource("Gold", 1000f);

                // Save the initial state
                _saveSystem.Save(saveKey, _currentSave);
            }
        }

        private void DisplaySaveState()
        {
            Debug.Log("=== Current Save State ===");
            Debug.Log($"Skill Tree ID: {_currentSave.SkillTreeId}");
            Debug.Log($"Save Timestamp: {new System.DateTime(_currentSave.SaveTimestamp)}");
            Debug.Log($"Unlocked Nodes: {_currentSave.UnlockedNodes.Count}");

            foreach (var node in _currentSave.UnlockedNodes)
            {
                Debug.Log($"  - {node.NodeId} (unlocked at {new System.DateTime(node.UnlockTimestamp)})");
            }

            Debug.Log("Resources:");
            foreach (var resource in _currentSave.Resources)
            {
                Debug.Log($"  - {resource.Key}: {resource.Value}");
            }
        }

        // Example: Unlock a skill
        public void UnlockSkill(string skillId)
        {
            if (_currentSave.IsNodeUnlocked(skillId))
            {
                Debug.LogWarning($"Skill {skillId} is already unlocked!");
                return;
            }

            // Check if player has enough skill points
            float skillPoints = _currentSave.GetResource("SkillPoints");
            if (skillPoints < 1)
            {
                Debug.LogWarning("Not enough skill points!");
                return;
            }

            // Unlock the skill
            _currentSave.AddUnlockedNode(skillId);

            // Deduct skill point
            _currentSave.AddResource("SkillPoints", -1);

            Debug.Log($"Unlocked skill: {skillId}");

            // Save progress
            SaveProgress();
        }

        // Example: Add resources (e.g., from quest rewards)
        public void AddReward(string resourceName, float amount)
        {
            _currentSave.AddResource(resourceName, amount);
            Debug.Log($"Added {amount} {resourceName}");

            SaveProgress();
        }

        private void SaveProgress()
        {
            const string saveKey = "demo_save";

            if (_saveSystem.Save(saveKey, _currentSave))
            {
                Debug.Log("Progress saved successfully!");
            }
            else
            {
                Debug.LogError("Failed to save progress!");
            }
        }

        // Example: Async save (non-blocking)
        private async void SaveProgressAsync()
        {
            const string saveKey = "demo_save";

            bool success = await _saveSystem.SaveAsync(saveKey, _currentSave);
            if (success)
            {
                Debug.Log("Progress saved asynchronously!");
            }
        }

        // Example: Reset progress
        public void ResetProgress()
        {
            const string saveKey = "demo_save";

            _currentSave = new SkillTreeSaveData
            {
                SkillTreeId = "demo_tree",
            };
            _currentSave.SetResource("SkillPoints", 5f);
            _currentSave.SetResource("Gold", 1000f);

            _saveSystem.Save(saveKey, _currentSave);
            Debug.Log("Progress reset!");

            DisplaySaveState();
        }

        // Example UI button handlers
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));

            GUILayout.Label("Save System Example", GUI.skin.box);
            GUILayout.Space(10);

            if (GUILayout.Button("Unlock Skill: Fireball"))
            {
                UnlockSkill("skill_fireball");
            }

            if (GUILayout.Button("Unlock Skill: Ice Blast"))
            {
                UnlockSkill("skill_ice_blast");
            }

            if (GUILayout.Button("Unlock Skill: Lightning"))
            {
                UnlockSkill("skill_lightning");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Add 3 Skill Points"))
            {
                AddReward("SkillPoints", 3f);
            }

            if (GUILayout.Button("Add 500 Gold"))
            {
                AddReward("Gold", 500f);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Display Save State"))
            {
                DisplaySaveState();
            }

            if (GUILayout.Button("Reset Progress"))
            {
                ResetProgress();
            }

            GUILayout.Space(10);

            // Display current state
            GUILayout.Label($"Skill Points: {_currentSave.GetResource("SkillPoints")}", GUI.skin.box);
            GUILayout.Label($"Gold: {_currentSave.GetResource("Gold")}", GUI.skin.box);
            GUILayout.Label($"Unlocked Skills: {_currentSave.UnlockedNodes.Count}", GUI.skin.box);

            GUILayout.EndArea();
        }
    }
}
