using System;
using System.Threading.Tasks;

namespace SkillTree.SaveSystem
{
    /// <summary>
    /// Generic interface for save/load operations.
    /// Implementations can use JSON, Binary, PlayerPrefs, Cloud saves, etc.
    /// </summary>
    /// <typeparam name="T">The type of data to save/load. Must be serializable.</typeparam>
    public interface ISaveSystem<T> where T : class
    {
        /// <summary>
        /// Saves data synchronously to the specified key/path.
        /// </summary>
        /// <param name="key">Unique identifier for this save (e.g., "player_skills", "skill_tree_1")</param>
        /// <param name="data">The data to save</param>
        /// <returns>True if save succeeded, false otherwise</returns>
        bool Save(string key, T data);

        /// <summary>
        /// Loads data synchronously from the specified key/path.
        /// </summary>
        /// <param name="key">Unique identifier for the save to load</param>
        /// <returns>The loaded data, or null if not found or failed</returns>
        T Load(string key);

        /// <summary>
        /// Saves data asynchronously to the specified key/path.
        /// </summary>
        /// <param name="key">Unique identifier for this save</param>
        /// <param name="data">The data to save</param>
        /// <returns>Task that completes when save is finished</returns>
        Task<bool> SaveAsync(string key, T data);

        /// <summary>
        /// Loads data asynchronously from the specified key/path.
        /// </summary>
        /// <param name="key">Unique identifier for the save to load</param>
        /// <returns>Task containing the loaded data, or null if not found</returns>
        Task<T> LoadAsync(string key);

        /// <summary>
        /// Checks if a save exists for the given key.
        /// </summary>
        /// <param name="key">Unique identifier to check</param>
        /// <returns>True if save exists, false otherwise</returns>
        bool SaveExists(string key);

        /// <summary>
        /// Deletes a save with the specified key.
        /// </summary>
        /// <param name="key">Unique identifier of the save to delete</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        bool DeleteSave(string key);

        /// <summary>
        /// Gets all available save keys.
        /// </summary>
        /// <returns>Array of save keys, or empty array if none exist</returns>
        string[] GetAllSaveKeys();
    }

    /// <summary>
    /// Configuration options for save systems.
    /// </summary>
    [Serializable]
    public class SaveSystemConfig
    {
        /// <summary>
        /// Base directory for save files (relative to Application.persistentDataPath)
        /// </summary>
        public string SaveDirectory = "SkillTreeSaves";

        /// <summary>
        /// Whether to use encryption for save files
        /// </summary>
        public bool UseEncryption = false;

        /// <summary>
        /// Custom encryption key. If null or empty, uses default key.
        /// For production, set a unique key or use device-unique key.
        /// </summary>
        public string EncryptionKey = null;

        /// <summary>
        /// If true, uses a device-unique encryption key (ties saves to specific device).
        /// This prevents save data from being easily copied between devices.
        /// Takes precedence over EncryptionKey if set.
        /// </summary>
        public bool UseDeviceUniqueKey = false;

        /// <summary>
        /// Whether to create backups before overwriting saves
        /// </summary>
        public bool CreateBackups = true;

        /// <summary>
        /// Maximum number of backup files to keep
        /// </summary>
        public int MaxBackups = 3;

        /// <summary>
        /// Whether to pretty-print JSON (human-readable but larger file size)
        /// Note: Has no effect when encryption is enabled.
        /// </summary>
        public bool PrettyPrint = true;
    }
}
