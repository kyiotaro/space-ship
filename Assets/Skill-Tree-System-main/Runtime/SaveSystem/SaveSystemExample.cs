using UnityEngine;
using SkillTree.SaveSystem;

namespace SkillTree.Examples
{
    /// <summary>
    /// Example demonstrating how to use the save system with encryption.
    /// This script shows different configuration options and usage patterns.
    /// </summary>
    public class SaveSystemExample : MonoBehaviour
    {
        private ISaveSystem<SkillTreeSaveData> _saveSystem;

        private void Start()
        {
            // Example 1: Create save system with default settings (no encryption)
            CreateDefaultSaveSystem();

            // Example 2: Create save system with encryption enabled
            CreateEncryptedSaveSystem();

            // Example 3: Create save system with device-unique encryption
            CreateDeviceUniqueSaveSystem();

            // Example 4: Create save system with custom encryption key
            CreateCustomKeySaveSystem();

            // Example 5: Generate a random encryption key
            GenerateRandomKey();
        }

        /// <summary>
        /// Example 1: Default save system (no encryption)
        /// </summary>
        private void CreateDefaultSaveSystem()
        {
            var config = new SaveSystemConfig
            {
                UseEncryption = false,
                CreateBackups = true,
                MaxBackups = 3,
                PrettyPrint = true
            };

            var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);

            // Save and load data
            var saveData = new SkillTreeSaveData
            {
                SkillTreeId = "MainSkillTree"
            };
            saveData.AddUnlockedNode("node_001");

            saveSystem.Save("player_save", saveData);
            var loadedData = saveSystem.Load("player_save");

            Debug.Log($"Loaded {loadedData.UnlockedNodes.Count} unlocked nodes");
        }

        /// <summary>
        /// Example 2: Encrypted save system with default key
        /// WARNING: Default key should be changed in production!
        /// </summary>
        private void CreateEncryptedSaveSystem()
        {
            var config = new SaveSystemConfig
            {
                UseEncryption = true, // Enable encryption
                CreateBackups = true,
                MaxBackups = 3,
                PrettyPrint = false // No effect with encryption
            };

            var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);

            // Save and load encrypted data
            var saveData = new SkillTreeSaveData
            {
                SkillTreeId = "EncryptedSkillTree"
            };
            saveData.AddUnlockedNode("node_002");
            saveData.SetResource("SkillPoints", 100);

            saveSystem.Save("encrypted_save", saveData);
            var loadedData = saveSystem.Load("encrypted_save");

            Debug.Log($"Loaded encrypted save with {loadedData.GetResource("SkillPoints")} skill points");
        }

        /// <summary>
        /// Example 3: Device-unique encryption
        /// Saves are tied to specific device and cannot be easily copied
        /// </summary>
        private void CreateDeviceUniqueSaveSystem()
        {
            var config = new SaveSystemConfig
            {
                UseEncryption = true,
                UseDeviceUniqueKey = true, // Ties encryption to this device
                CreateBackups = true,
                MaxBackups = 3
            };

            var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);

            // This save can only be decrypted on this specific device
            var saveData = new SkillTreeSaveData
            {
                SkillTreeId = "DeviceLockedTree"
            };

            saveSystem.Save("device_locked_save", saveData);

            Debug.Log("Save is now locked to this device");
        }

        /// <summary>
        /// Example 4: Custom encryption key
        /// Recommended for production - use a unique key for your game
        /// </summary>
        private void CreateCustomKeySaveSystem()
        {
            // Generate a random key once and store it securely
            // For example, in PlayerPrefs (though not the most secure)
            // or bake it into your build as an obfuscated constant
            string myGameEncryptionKey = "MyGame_UniqueKey_2024_ChangeThisToYourOwnKey";

            var config = new SaveSystemConfig
            {
                UseEncryption = true,
                EncryptionKey = myGameEncryptionKey, // Use custom key
                CreateBackups = true,
                MaxBackups = 3
            };

            var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);

            var saveData = new SkillTreeSaveData
            {
                SkillTreeId = "CustomKeyTree"
            };
            saveData.AddUnlockedNode("node_003");

            saveSystem.Save("custom_key_save", saveData);

            Debug.Log("Save encrypted with custom key");
        }

        /// <summary>
        /// Example 5: Generate a random encryption key
        /// Useful for creating unique keys per user or installation
        /// </summary>
        private void GenerateRandomKey()
        {
            // Generate a cryptographically secure random key
            string randomKey = SaveEncryption.GenerateRandomKey();
            Debug.Log($"Generated random key: {randomKey}");

            // You could store this key securely (e.g., in PlayerPrefs, a config file, or cloud storage)
            // PlayerPrefs.SetString("EncryptionKey", randomKey);

            // Then use it in your save system config
            var config = new SaveSystemConfig
            {
                UseEncryption = true,
                EncryptionKey = randomKey
            };

            var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);
        }

        /// <summary>
        /// Example: Using async save/load operations
        /// </summary>
        private async void AsyncSaveLoadExample()
        {
            var config = new SaveSystemConfig
            {
                UseEncryption = true,
                UseDeviceUniqueKey = true
            };

            var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);

            var saveData = new SkillTreeSaveData
            {
                SkillTreeId = "AsyncTree"
            };
            saveData.AddUnlockedNode("node_async_001");

            // Async save
            bool saveSuccess = await saveSystem.SaveAsync("async_save", saveData);
            if (saveSuccess)
            {
                Debug.Log("Async save completed");
            }

            // Async load
            var loadedData = await saveSystem.LoadAsync("async_save");
            if (loadedData != null)
            {
                Debug.Log($"Async load completed with {loadedData.UnlockedNodes.Count} nodes");
            }
        }

        /// <summary>
        /// Example: Manual encryption/decryption of strings
        /// Useful for encrypting individual values or custom data
        /// </summary>
        private void ManualEncryptionExample()
        {
            string sensitiveData = "Player secret data: 1234567";

            // Encrypt with default key
            string encrypted = SaveEncryption.Encrypt(sensitiveData);
            Debug.Log($"Encrypted: {encrypted}");

            // Decrypt with default key
            string decrypted = SaveEncryption.Decrypt(encrypted);
            Debug.Log($"Decrypted: {decrypted}");

            // Encrypt with custom key
            string customKey = "MyCustomKey123";
            string customEncrypted = SaveEncryption.Encrypt(sensitiveData, customKey);
            string customDecrypted = SaveEncryption.Decrypt(customEncrypted, customKey);

            // Validate encrypted data
            bool isValid = SaveEncryption.IsValidEncryptedData(customEncrypted, customKey);
            Debug.Log($"Is valid encrypted data: {isValid}");
        }

        /// <summary>
        /// Production-ready example showing recommended setup
        /// </summary>
        private ISaveSystem<SkillTreeSaveData> CreateProductionSaveSystem()
        {
            // Option A: Use device-unique key (best for preventing save sharing)
            var config = new SaveSystemConfig
            {
                UseEncryption = true,
                UseDeviceUniqueKey = true,
                CreateBackups = true,
                MaxBackups = 5,
                PrettyPrint = false
            };

            // Option B: Use custom key unique to your game
            // var config = new SaveSystemConfig
            // {
            //     UseEncryption = true,
            //     EncryptionKey = "YourGame_UniqueKey_2024_ChangeThis",
            //     CreateBackups = true,
            //     MaxBackups = 5,
            //     PrettyPrint = false
            // };

            return new JsonSaveSystem<SkillTreeSaveData>(config);
        }
    }
}
