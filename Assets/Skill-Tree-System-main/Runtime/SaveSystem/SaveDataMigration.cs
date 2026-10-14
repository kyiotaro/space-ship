using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.SaveSystem
{
    /// <summary>
    /// Interface for save data migration between versions
    /// </summary>
    public interface ISaveDataMigration<T> where T : class
    {
        /// <summary>
        /// The version this migration targets (migrates FROM this version)
        /// </summary>
        int FromVersion { get; }

        /// <summary>
        /// The version this migration produces (migrates TO this version)
        /// </summary>
        int ToVersion { get; }

        /// <summary>
        /// Performs the migration
        /// </summary>
        /// <param name="oldData">The data to migrate</param>
        /// <returns>The migrated data</returns>
        T Migrate(T oldData);
    }

    /// <summary>
    /// Manager for handling save data migrations across versions
    /// </summary>
    /// <typeparam name="T">Type of save data</typeparam>
    public class SaveDataMigrationManager<T> where T : class
    {
        private readonly Dictionary<int, ISaveDataMigration<T>> _migrations = new Dictionary<int, ISaveDataMigration<T>>();
        private readonly Func<T, int> _getVersion;
        private readonly int _currentVersion;

        public SaveDataMigrationManager(int currentVersion, Func<T, int> getVersion)
        {
            _currentVersion = currentVersion;
            _getVersion = getVersion;
        }

        /// <summary>
        /// Registers a migration for a specific version
        /// </summary>
        public void RegisterMigration(ISaveDataMigration<T> migration)
        {
            if (_migrations.ContainsKey(migration.FromVersion))
            {
                Debug.LogWarning($"[SaveDataMigration] Migration from version {migration.FromVersion} already registered. Overwriting.");
            }

            _migrations[migration.FromVersion] = migration;
            Debug.Log($"[SaveDataMigration] Registered migration: V{migration.FromVersion} -> V{migration.ToVersion}");
        }

        /// <summary>
        /// Migrates data to the current version if needed
        /// </summary>
        /// <param name="data">The data to migrate</param>
        /// <returns>Migrated data, or original if no migration needed</returns>
        public T MigrateToLatest(T data)
        {
            if (data == null)
            {
                return null;
            }

            var version = _getVersion(data);

            if (version == _currentVersion)
            {
                // Already at current version
                return data;
            }

            if (version > _currentVersion)
            {
                Debug.LogWarning($"[SaveDataMigration] Save data version ({version}) is newer than current version ({_currentVersion}). Cannot migrate.");
                return data;
            }

            Debug.Log($"[SaveDataMigration] Migrating save data from V{version} to V{_currentVersion}");

            // Apply migrations sequentially
            var migratedData = data;
            var currentVersion = version;

            while (currentVersion < _currentVersion)
            {
                if (_migrations.TryGetValue(currentVersion, out var migration))
                {
                    Debug.Log($"[SaveDataMigration] Applying migration: V{migration.FromVersion} -> V{migration.ToVersion}");
                    migratedData = migration.Migrate(migratedData);
                    currentVersion = migration.ToVersion;
                }
                else
                {
                    Debug.LogError($"[SaveDataMigration] No migration found for version {currentVersion}. Cannot continue migration.");
                    return migratedData;
                }
            }

            Debug.Log($"[SaveDataMigration] Migration complete. Final version: V{currentVersion}");
            return migratedData;
        }

        /// <summary>
        /// Checks if migration is needed for the given data
        /// </summary>
        public bool NeedsMigration(T data)
        {
            if (data == null)
            {
                return false;
            }

            var version = _getVersion(data);
            return version < _currentVersion;
        }

        /// <summary>
        /// Gets the version of the given data
        /// </summary>
        public int GetDataVersion(T data)
        {
            return data == null ? 0 : _getVersion(data);
        }
    }

    /// <summary>
    /// Example migration for SkillTreeSaveData (V1 to V2)
    /// This serves as a template for future migrations
    /// </summary>
    public class SkillTreeSaveDataMigration_V1_to_V2 : ISaveDataMigration<SkillTreeSaveData>
    {
        public int FromVersion => 1;
        public int ToVersion => 2;

        public SkillTreeSaveData Migrate(SkillTreeSaveData oldData)
        {
            // Example migration logic:
            // 1. Create new version data
            // 2. Copy old data
            // 3. Add new fields with default values
            // 4. Transform data if needed

            // For now, this is a placeholder as we're at V1
            var newData = new SkillTreeSaveData
            {
                Version = ToVersion,
                SaveTimestamp = oldData.SaveTimestamp,
                SkillTreeId = oldData.SkillTreeId,
                UnlockedNodes = new List<UnlockedNodeData>(oldData.UnlockedNodes),
                Resources = new List<ResourceData>(oldData.Resources),
                CustomData = new List<CustomDataEntry>(oldData.CustomData)
            };

            // Add any V2-specific migrations here
            // Example: newData.NewField = DefaultValue;

            return newData;
        }
    }

    /// <summary>
    /// Save system wrapper with automatic migration support
    /// </summary>
    /// <typeparam name="T">Type of save data</typeparam>
    public class MigratingSaveSystem<T> : ISaveSystem<T> where T : class
    {
        private readonly ISaveSystem<T> _baseSaveSystem;
        private readonly SaveDataMigrationManager<T> _migrationManager;

        public MigratingSaveSystem(ISaveSystem<T> baseSaveSystem, SaveDataMigrationManager<T> migrationManager)
        {
            _baseSaveSystem = baseSaveSystem ?? throw new ArgumentNullException(nameof(baseSaveSystem));
            _migrationManager = migrationManager ?? throw new ArgumentNullException(nameof(migrationManager));
        }

        public bool Save(string key, T data)
        {
            return _baseSaveSystem.Save(key, data);
        }

        public T Load(string key)
        {
            var data = _baseSaveSystem.Load(key);
            return data != null ? _migrationManager.MigrateToLatest(data) : null;
        }

        public async System.Threading.Tasks.Task<bool> SaveAsync(string key, T data)
        {
            return await _baseSaveSystem.SaveAsync(key, data);
        }

        public async System.Threading.Tasks.Task<T> LoadAsync(string key)
        {
            var data = await _baseSaveSystem.LoadAsync(key);
            return data != null ? _migrationManager.MigrateToLatest(data) : null;
        }

        public bool SaveExists(string key)
        {
            return _baseSaveSystem.SaveExists(key);
        }

        public bool DeleteSave(string key)
        {
            return _baseSaveSystem.DeleteSave(key);
        }

        public string[] GetAllSaveKeys()
        {
            return _baseSaveSystem.GetAllSaveKeys();
        }
    }
}
