using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SkillTree.SaveSystem
{
    /// <summary>
    /// JSON-based save system implementation using Unity's JsonUtility.
    /// Supports backups, async operations, and human-readable format.
    /// </summary>
    /// <typeparam name="T">Type of data to save/load. Must be serializable with [Serializable] attribute.</typeparam>
    public class JsonSaveSystem<T> : ISaveSystem<T> where T : class
    {
        private readonly SaveSystemConfig _config;
        private readonly string _saveDirectory;
        private readonly string _encryptionKey;

        public JsonSaveSystem() : this(new SaveSystemConfig())
        {
        }

        public JsonSaveSystem(SaveSystemConfig config)
        {
            _config = config ?? new SaveSystemConfig();
            _saveDirectory = Path.Combine(Application.persistentDataPath, _config.SaveDirectory);

            // Determine encryption key to use
            if (_config.UseEncryption)
            {
                if (_config.UseDeviceUniqueKey)
                {
                    _encryptionKey = SaveEncryption.GetDeviceUniqueKey();
                    Debug.Log("[JsonSaveSystem] Using device-unique encryption key");
                }
                else if (!string.IsNullOrEmpty(_config.EncryptionKey))
                {
                    _encryptionKey = _config.EncryptionKey;
                    Debug.Log("[JsonSaveSystem] Using custom encryption key");
                }
                else
                {
                    _encryptionKey = null; // Will use default key in SaveEncryption
                    Debug.LogWarning("[JsonSaveSystem] Using default encryption key. For production, set a custom key!");
                }
            }

            // Ensure save directory exists
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        public bool Save(string key, T data)
        {
            try
            {
                var filePath = GetFilePath(key);

                // Create backup if file exists
                if (_config.CreateBackups && File.Exists(filePath))
                {
                    CreateBackup(key);
                }

                // Serialize to JSON
                var json = JsonUtility.ToJson(data, _config.PrettyPrint);

                // Encrypt if enabled
                if (_config.UseEncryption)
                {
                    json = SaveEncryption.Encrypt(json, _encryptionKey);
                }

                // Write to file
                File.WriteAllText(filePath, json);

                Debug.Log($"[JsonSaveSystem] Saved data to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveSystem] Failed to save data for key '{key}': {ex.Message}");
                return false;
            }
        }

        public T Load(string key)
        {
            try
            {
                var filePath = GetFilePath(key);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[JsonSaveSystem] Save file not found for key '{key}'");
                    return null;
                }

                // Read from file
                var json = File.ReadAllText(filePath);

                // Decrypt if enabled
                if (_config.UseEncryption)
                {
                    json = SaveEncryption.Decrypt(json, _encryptionKey);
                }

                // Deserialize from JSON
                var data = JsonUtility.FromJson<T>(json);

                Debug.Log($"[JsonSaveSystem] Loaded data from: {filePath}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveSystem] Failed to load data for key '{key}': {ex.Message}");

                // Try to restore from backup if available
                if (_config.CreateBackups)
                {
                    Debug.Log($"[JsonSaveSystem] Attempting to restore from backup...");
                    return TryRestoreFromBackup(key);
                }

                return null;
            }
        }

        public async Task<bool> SaveAsync(string key, T data)
        {
            try
            {
                var filePath = GetFilePath(key);

                // Create backup if file exists
                if (_config.CreateBackups && File.Exists(filePath))
                {
                    await Task.Run(() => CreateBackup(key));
                }

                // Serialize to JSON
                var json = await Task.Run(() => JsonUtility.ToJson(data, _config.PrettyPrint));

                // Encrypt if enabled
                if (_config.UseEncryption)
                {
                    json = await Task.Run(() => SaveEncryption.Encrypt(json, _encryptionKey));
                }

                // Write to file asynchronously
                await Task.Run(() => File.WriteAllText(filePath, json));

                Debug.Log($"[JsonSaveSystem] Async saved data to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveSystem] Failed to async save data for key '{key}': {ex.Message}");
                return false;
            }
        }

        public async Task<T> LoadAsync(string key)
        {
            try
            {
                var filePath = GetFilePath(key);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[JsonSaveSystem] Save file not found for key '{key}'");
                    return null;
                }

                // Read from file asynchronously
                var json = await Task.Run(() => File.ReadAllText(filePath));

                // Decrypt if enabled
                if (_config.UseEncryption)
                {
                    json = await Task.Run(() => SaveEncryption.Decrypt(json, _encryptionKey));
                }

                // Deserialize from JSON
                var data = await Task.Run(() => JsonUtility.FromJson<T>(json));

                Debug.Log($"[JsonSaveSystem] Async loaded data from: {filePath}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveSystem] Failed to async load data for key '{key}': {ex.Message}");

                // Try to restore from backup if available
                if (_config.CreateBackups)
                {
                    Debug.Log($"[JsonSaveSystem] Attempting to restore from backup...");
                    return await Task.Run(() => TryRestoreFromBackup(key));
                }

                return null;
            }
        }

        public bool SaveExists(string key)
        {
            var filePath = GetFilePath(key);
            return File.Exists(filePath);
        }

        public bool DeleteSave(string key)
        {
            try
            {
                var filePath = GetFilePath(key);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[JsonSaveSystem] Save file not found for key '{key}'");
                    return false;
                }

                File.Delete(filePath);

                // Also delete backups
                DeleteBackups(key);

                Debug.Log($"[JsonSaveSystem] Deleted save: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveSystem] Failed to delete save for key '{key}': {ex.Message}");
                return false;
            }
        }

        public string[] GetAllSaveKeys()
        {
            try
            {
                if (!Directory.Exists(_saveDirectory))
                {
                    return Array.Empty<string>();
                }

                var files = Directory.GetFiles(_saveDirectory, "*.json")
                    .Where(f => !f.Contains(".backup"))
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToArray();

                return files;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveSystem] Failed to get save keys: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        #region Backup Management

        private void CreateBackup(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                if (!File.Exists(filePath))
                {
                    return;
                }

                // Get existing backups
                var backups = GetBackupFiles(key);

                // Create new backup with timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = GetBackupPath(key, timestamp);
                File.Copy(filePath, backupPath, true);

                // Clean up old backups if exceeding limit
                if (backups.Length >= _config.MaxBackups)
                {
                    var oldestBackups = backups
                        .OrderBy(f => File.GetCreationTime(f))
                        .Take(backups.Length - _config.MaxBackups + 1);

                    foreach (var oldBackup in oldestBackups)
                    {
                        File.Delete(oldBackup);
                    }
                }

                Debug.Log($"[JsonSaveSystem] Created backup: {backupPath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JsonSaveSystem] Failed to create backup for key '{key}': {ex.Message}");
            }
        }

        private T TryRestoreFromBackup(string key)
        {
            try
            {
                var backups = GetBackupFiles(key);
                if (backups.Length == 0)
                {
                    return null;
                }

                // Try to load from most recent backup
                var mostRecentBackup = backups.OrderByDescending(f => File.GetCreationTime(f)).First();
                var json = File.ReadAllText(mostRecentBackup);

                // Decrypt if enabled
                if (_config.UseEncryption)
                {
                    json = SaveEncryption.Decrypt(json, _encryptionKey);
                }

                var data = JsonUtility.FromJson<T>(json);

                Debug.Log($"[JsonSaveSystem] Restored data from backup: {mostRecentBackup}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveSystem] Failed to restore from backup for key '{key}': {ex.Message}");
                return null;
            }
        }

        private string[] GetBackupFiles(string key)
        {
            var pattern = $"{key}.backup_*.json";
            return Directory.Exists(_saveDirectory)
                ? Directory.GetFiles(_saveDirectory, pattern)
                : Array.Empty<string>();
        }

        private void DeleteBackups(string key)
        {
            try
            {
                var backups = GetBackupFiles(key);
                foreach (var backup in backups)
                {
                    File.Delete(backup);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JsonSaveSystem] Failed to delete backups for key '{key}': {ex.Message}");
            }
        }

        #endregion

        #region Path Helpers

        private string GetFilePath(string key)
        {
            return Path.Combine(_saveDirectory, $"{key}.json");
        }

        private string GetBackupPath(string key, string timestamp)
        {
            return Path.Combine(_saveDirectory, $"{key}.backup_{timestamp}.json");
        }

        #endregion
    }
}
