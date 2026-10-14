# Skill Tree Save System

A generic, extensible save system for the Unity Skill Tree package. This system is designed to be independent and can be integrated with any skill tree implementation.

## Features

- **Generic interface** - Works with any serializable data type
- **Multiple backends** - JSON implementation included (Binary can be added)
- **Async operations** - Non-blocking save/load operations
- **Automatic backups** - Configurable backup system with rotation
- **AES-256 Encryption** - Secure save data with customizable encryption keys
- **Device-locked saves** - Tie saves to specific devices to prevent copying
- **Version migration** - Automatic migration between save data versions
- **Human-readable** - JSON format is easy to debug and version control
- **Error recovery** - Automatic backup restoration on corruption

## Quick Start

### Basic Usage

```csharp
using SkillTree.SaveSystem;

// Create a save system instance
var saveSystem = new JsonSaveSystem<SkillTreeSaveData>();

// Create some save data
var saveData = new SkillTreeSaveData
{
    SkillTreeId = "player_skills",
};

// Add unlocked nodes
saveData.AddUnlockedNode("skill_001");
saveData.AddUnlockedNode("skill_002");

// Set resources
saveData.SetResource("SkillPoints", 10f);
saveData.SetResource("Gold", 1000f);

// Save
saveSystem.Save("player_save", saveData);

// Load
var loadedData = saveSystem.Load("player_save");

// Check if a node is unlocked
bool isUnlocked = loadedData.IsNodeUnlocked("skill_001");
```

### Async Operations

```csharp
// Async save (non-blocking)
await saveSystem.SaveAsync("player_save", saveData);

// Async load (non-blocking)
var loadedData = await saveSystem.LoadAsync("player_save");
```

### Configuration

```csharp
var config = new SaveSystemConfig
{
    SaveDirectory = "MyGame/SkillTrees",   // Subdirectory in persistentDataPath
    CreateBackups = true,                   // Enable automatic backups
    MaxBackups = 5,                         // Keep last 5 backups
    PrettyPrint = true,                     // Human-readable JSON (no effect with encryption)
    UseEncryption = false,                  // Enable AES-256 encryption
    EncryptionKey = null,                   // Custom encryption key (null = default key)
    UseDeviceUniqueKey = false              // Lock saves to specific device
};

var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);
```

### Utility Methods

```csharp
// Check if save exists
if (saveSystem.SaveExists("player_save"))
{
    var data = saveSystem.Load("player_save");
}

// Get all save files
string[] allSaves = saveSystem.GetAllSaveKeys();
foreach (var saveKey in allSaves)
{
    Debug.Log($"Found save: {saveKey}");
}

// Delete a save
saveSystem.DeleteSave("player_save");
```

## Encryption

The save system includes AES-256 encryption to protect save data from tampering and cheating. Encryption is transparent to your code - simply enable it in the configuration.

### Basic Encryption (Default Key)

```csharp
var config = new SaveSystemConfig
{
    UseEncryption = true  // Uses default encryption key
};

var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);

// Save and load work exactly the same - encryption is automatic
saveSystem.Save("player_save", saveData);
var loaded = saveSystem.Load("player_save");
```

**WARNING**: The default encryption key should be changed for production! It's defined in the source code and provides only basic protection.

### Custom Encryption Key (Recommended for Production)

```csharp
// Generate a unique key for your game (do this once and store it securely)
string myGameKey = "MyGame_UniqueKey_2024_ChangeThisToYourOwnKey";

var config = new SaveSystemConfig
{
    UseEncryption = true,
    EncryptionKey = myGameKey  // Use your custom key
};

var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);
```

### Device-Unique Encryption (Prevent Save Sharing)

Device-unique encryption ties saves to a specific device, preventing players from easily copying saves between devices or sharing them online.

```csharp
var config = new SaveSystemConfig
{
    UseEncryption = true,
    UseDeviceUniqueKey = true  // Saves can only be loaded on this device
};

var saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);
```

This uses `SystemInfo.deviceUniqueIdentifier` combined with other device properties to create a unique key per device.

### Generating Random Keys

You can generate cryptographically secure random keys for per-user or per-installation encryption:

```csharp
// Generate a random key
string randomKey = SaveEncryption.GenerateRandomKey();

// Store it securely (e.g., PlayerPrefs, local config file, or cloud storage)
PlayerPrefs.SetString("GameEncryptionKey", randomKey);
PlayerPrefs.Save();

// Use it in your save system
var config = new SaveSystemConfig
{
    UseEncryption = true,
    EncryptionKey = PlayerPrefs.GetString("GameEncryptionKey")
};
```

### Manual Encryption/Decryption

You can also use the encryption utilities directly for individual values:

```csharp
using SkillTree.SaveSystem;

// Encrypt a string with default key
string encrypted = SaveEncryption.Encrypt("sensitive data");

// Decrypt with default key
string decrypted = SaveEncryption.Decrypt(encrypted);

// Encrypt with custom key
string customEncrypted = SaveEncryption.Encrypt("sensitive data", "myCustomKey");
string customDecrypted = SaveEncryption.Decrypt(customEncrypted, "myCustomKey");

// Validate encrypted data
bool isValid = SaveEncryption.IsValidEncryptedData(encrypted);

// Get device-unique key
string deviceKey = SaveEncryption.GetDeviceUniqueKey();
```

### Encryption Best Practices

1. **Change the default key** - Never ship with the default encryption key in production
2. **Use device-unique keys for multiplayer** - Prevents save sharing in competitive games
3. **Store keys securely** - Consider obfuscation or secure key storage solutions
4. **Test thoroughly** - Ensure saves can be loaded after encryption changes
5. **Document key changes** - Keep track of encryption key versions for support purposes
6. **Backup unencrypted** - Consider keeping unencrypted backups during development

### Production-Ready Example

```csharp
public class GameSaveManager : MonoBehaviour
{
    private ISaveSystem<SkillTreeSaveData> _saveSystem;

    private void Awake()
    {
        // For competitive/multiplayer games - use device-unique encryption
        var config = new SaveSystemConfig
        {
            UseEncryption = true,
            UseDeviceUniqueKey = true,  // Prevent save sharing
            CreateBackups = true,
            MaxBackups = 5,
            PrettyPrint = false  // No effect with encryption
        };

        // For single-player games - use custom key
        // var config = new SaveSystemConfig
        // {
        //     UseEncryption = true,
        //     EncryptionKey = "YourGameUniqueKey2024",
        //     CreateBackups = true,
        //     MaxBackups = 5
        // };

        _saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);
    }

    public void SaveGame()
    {
        var saveData = CollectSaveData();
        bool success = _saveSystem.Save("player_save", saveData);

        if (success)
        {
            Debug.Log("Game saved successfully (encrypted)");
        }
        else
        {
            Debug.LogError("Failed to save game");
        }
    }

    public SkillTreeSaveData LoadGame()
    {
        if (!_saveSystem.SaveExists("player_save"))
        {
            return new SkillTreeSaveData();  // New game
        }

        var saveData = _saveSystem.Load("player_save");
        if (saveData == null)
        {
            Debug.LogError("Failed to load save - corrupted or wrong encryption key");
            return new SkillTreeSaveData();
        }

        return saveData;
    }
}
```

### Encryption Security Notes

- **AES-256 CBC Mode** - Industry-standard encryption algorithm
- **PBKDF2 Key Derivation** - Keys are derived using 10,000 iterations with SHA-256
- **Unique IV per save** - Each save file uses a unique initialization vector
- **Cryptographically secure** - Protects against casual tampering and cheating
- **Not DRM** - This is not copy protection; determined users can still modify saves

## Working with SkillTreeSaveData

### Managing Unlocked Nodes

```csharp
var saveData = new SkillTreeSaveData();

// Add unlocked nodes
saveData.AddUnlockedNode("skill_fireball");
saveData.AddUnlockedNode("skill_ice_blast");

// Check if unlocked
bool hasFireball = saveData.IsNodeUnlocked("skill_fireball");

// Get node data (includes unlock timestamp)
var nodeData = saveData.GetUnlockedNode("skill_fireball");
Debug.Log($"Unlocked at: {new DateTime(nodeData.UnlockTimestamp)}");

// Remove unlocked node (e.g., for respec)
saveData.RemoveUnlockedNode("skill_fireball");
```

### Managing Resources

```csharp
// Set resource
saveData.SetResource("SkillPoints", 15f);

// Get resource (returns 0 if not found)
float skillPoints = saveData.GetResource("SkillPoints");

// Add to resource
saveData.AddResource("SkillPoints", 5f);  // Now 20

// Multiple resources
saveData.SetResource("Gold", 5000f);
saveData.SetResource("Gems", 100f);
saveData.SetResource("XP", 1250.5f);
```

### Custom Data

```csharp
// Store custom JSON data at tree level
var customSettings = JsonUtility.ToJson(new { difficulty = "hard", region = "north" });
saveData.SetCustomData("game_settings", customSettings);

// Retrieve custom data
string settingsJson = saveData.GetCustomData("game_settings");
var settings = JsonUtility.FromJson<GameSettings>(settingsJson);

// Store custom data per node
var nodeData = saveData.GetUnlockedNode("skill_fireball");
var nodeStats = JsonUtility.ToJson(new { level = 3, experience = 450 });
nodeData.SetNodeCustomData("stats", nodeStats);
```

## Version Migration

The save system supports automatic migration between data format versions.

### Setting Up Migrations

```csharp
using SkillTree.SaveSystem;

// Create migration manager (current version is 2)
var migrationManager = new SaveDataMigrationManager<SkillTreeSaveData>(
    currentVersion: 2,
    getVersion: data => data.Version
);

// Register migrations
migrationManager.RegisterMigration(new SkillTreeSaveDataMigration_V1_to_V2());

// Wrap the save system with migration support
var baseSaveSystem = new JsonSaveSystem<SkillTreeSaveData>();
var saveSystem = new MigratingSaveSystem<SkillTreeSaveData>(
    baseSaveSystem,
    migrationManager
);

// Now loads will automatically migrate old saves
var data = saveSystem.Load("player_save");  // Automatically migrates from V1 to V2
```

### Creating Custom Migrations

```csharp
public class SkillTreeSaveDataMigration_V2_to_V3 : ISaveDataMigration<SkillTreeSaveData>
{
    public int FromVersion => 2;
    public int ToVersion => 3;

    public SkillTreeSaveData Migrate(SkillTreeSaveData oldData)
    {
        var newData = new SkillTreeSaveData
        {
            Version = ToVersion,
            SaveTimestamp = oldData.SaveTimestamp,
            SkillTreeId = oldData.SkillTreeId,
            UnlockedNodes = new List<UnlockedNodeData>(oldData.UnlockedNodes),
            Resources = new Dictionary<string, float>(oldData.Resources),
            CustomData = new Dictionary<string, string>(oldData.CustomData)
        };

        // V3-specific changes
        // Example: Convert old resource format
        if (oldData.Resources.ContainsKey("Points"))
        {
            newData.Resources["SkillPoints"] = oldData.Resources["Points"];
            newData.Resources.Remove("Points");
        }

        return newData;
    }
}
```

## Integration with VContainer

```csharp
using VContainer;
using VContainer.Unity;
using SkillTree.SaveSystem;

public class SaveSystemInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        // Register config
        builder.RegisterInstance(new SaveSystemConfig
        {
            SaveDirectory = "MyGame/SkillTrees",
            CreateBackups = true,
            MaxBackups = 3
        });

        // Register save system as singleton
        builder.Register<ISaveSystem<SkillTreeSaveData>, JsonSaveSystem<SkillTreeSaveData>>(
            Lifetime.Singleton
        );

        // Or with migration support
        builder.Register<SaveDataMigrationManager<SkillTreeSaveData>>(
            Lifetime.Singleton
        ).WithParameter(2)  // Current version
         .WithParameter<Func<SkillTreeSaveData, int>>(data => data.Version);

        builder.Register<ISaveSystem<SkillTreeSaveData>>(resolver =>
        {
            var baseSaveSystem = new JsonSaveSystem<SkillTreeSaveData>(
                resolver.Resolve<SaveSystemConfig>()
            );
            var migrationManager = resolver.Resolve<SaveDataMigrationManager<SkillTreeSaveData>>();
            return new MigratingSaveSystem<SkillTreeSaveData>(baseSaveSystem, migrationManager);
        }, Lifetime.Singleton);
    }
}

// Usage in game code
public class GameSaveController : MonoBehaviour
{
    [Inject] private ISaveSystem<SkillTreeSaveData> _saveSystem;

    private void SaveGame()
    {
        var saveData = CreateSaveData();
        _saveSystem.Save("player_save", saveData);
    }

    private void LoadGame()
    {
        var saveData = _saveSystem.Load("player_save");
        ApplySaveData(saveData);
    }
}
```

## Using with Custom Data Types

The save system is generic and works with any serializable type:

```csharp
[Serializable]
public class CustomGameSave
{
    public int Version = 1;
    public string PlayerName;
    public int Level;
    public List<string> CompletedQuests = new List<string>();
}

// Create save system for custom type
var customSaveSystem = new JsonSaveSystem<CustomGameSave>();

var save = new CustomGameSave
{
    PlayerName = "Hero",
    Level = 10
};
save.CompletedQuests.Add("quest_001");

customSaveSystem.Save("game_save", save);
```

## File Locations

Save files are stored in:
- **Windows**: `C:\Users\[User]\AppData\LocalLow\[Company]\[Game]\[SaveDirectory]`
- **Mac**: `~/Library/Application Support/[Company]/[Game]/[SaveDirectory]`
- **Linux**: `~/.config/unity3d/[Company]/[Game]/[SaveDirectory]`

Files are named: `[key].json`
Backups are named: `[key].backup_[timestamp].json`

## Error Handling

The save system includes robust error handling:

```csharp
// Save returns false on failure
if (!saveSystem.Save("player_save", saveData))
{
    Debug.LogError("Save failed!");
}

// Load returns null on failure
var loadedData = saveSystem.Load("player_save");
if (loadedData == null)
{
    Debug.LogWarning("Could not load save, starting new game");
    loadedData = new SkillTreeSaveData();
}

// Automatic backup restoration
// If a save file is corrupted, the system automatically tries to restore from the most recent backup
```

## Best Practices

1. **Save frequently** - Use async operations to avoid blocking gameplay
2. **Validate loaded data** - Check for null and validate version
3. **Use meaningful keys** - Use descriptive save keys like "player_skills", "tech_tree"
4. **Enable backups in production** - Protects against save corruption
5. **Test migrations** - Always test save data migrations thoroughly
6. **Version your data** - Increment version when changing save data structure

## Future Integration Points

This save system is designed to integrate with the core skill tree system once Phase 1 and 2 are complete:

```csharp
// Future integration example (after core implementation)
public class SkillTreeManager : ISkillTreeManager
{
    private readonly ISaveSystem<SkillTreeSaveData> _saveSystem;
    private SkillTreeSaveData _currentSave;

    public SkillTreeManager(ISaveSystem<SkillTreeSaveData> saveSystem)
    {
        _saveSystem = saveSystem;
    }

    public void LoadFromSave(string saveKey)
    {
        _currentSave = _saveSystem.Load(saveKey);
        // Apply save data to skill tree state
    }

    public void SaveProgress(string saveKey)
    {
        // Update save data from current state
        _saveSystem.SaveAsync(saveKey, _currentSave);
    }
}
```

## Extending the Save System

### Custom Save System Implementation

```csharp
public class BinarySaveSystem<T> : ISaveSystem<T> where T : class
{
    public bool Save(string key, T data)
    {
        // Implement binary serialization
        // Use BinaryFormatter or custom binary writer
        return true;
    }

    public T Load(string key)
    {
        // Implement binary deserialization
        return null;
    }

    // Implement other interface methods...
}
```

### Cloud Save Implementation

```csharp
public class CloudSaveSystem<T> : ISaveSystem<T> where T : class
{
    private readonly ICloudProvider _cloudProvider;

    public async Task<bool> SaveAsync(string key, T data)
    {
        var json = JsonUtility.ToJson(data);
        return await _cloudProvider.UploadAsync(key, json);
    }

    public async Task<T> LoadAsync(string key)
    {
        var json = await _cloudProvider.DownloadAsync(key);
        return JsonUtility.FromJson<T>(json);
    }

    // Implement other interface methods...
}
```

## Testing

```csharp
using NUnit.Framework;
using SkillTree.SaveSystem;

public class SaveSystemTests
{
    private ISaveSystem<SkillTreeSaveData> _saveSystem;

    [SetUp]
    public void Setup()
    {
        _saveSystem = new JsonSaveSystem<SkillTreeSaveData>();
    }

    [Test]
    public void SaveAndLoad_RoundTrip_Success()
    {
        // Arrange
        var saveData = new SkillTreeSaveData();
        saveData.AddUnlockedNode("test_node");
        saveData.SetResource("Points", 100f);

        // Act
        _saveSystem.Save("test_save", saveData);
        var loaded = _saveSystem.Load("test_save");

        // Assert
        Assert.IsNotNull(loaded);
        Assert.IsTrue(loaded.IsNodeUnlocked("test_node"));
        Assert.AreEqual(100f, loaded.GetResource("Points"));

        // Cleanup
        _saveSystem.DeleteSave("test_save");
    }
}
```

## Summary

The Save System is production-ready and can be used immediately for any serializable data. It provides:

- ✅ Generic, reusable interface
- ✅ JSON implementation with backups
- ✅ AES-256 encryption with multiple key options
- ✅ Device-locked saves to prevent cheating
- ✅ Async operations
- ✅ Version migration support
- ✅ Comprehensive error handling
- ✅ Easy integration with VContainer
- ✅ Extensible for custom implementations
- ✅ Ready for Phase 3 integration

For questions or issues, please refer to the main package documentation.
