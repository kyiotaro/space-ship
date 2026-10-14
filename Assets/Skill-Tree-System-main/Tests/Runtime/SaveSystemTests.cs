using NUnit.Framework;
using SkillTree.SaveSystem;
using System;
using System.IO;
using UnityEngine;

namespace SkillTree.Tests.Runtime
{
    /// <summary>
    /// Unit tests for the save/load system.
    /// Tests serialization, deserialization, and data integrity.
    /// </summary>
    public class SaveSystemTests
    {
        private JsonSaveSystem<SkillTreeSaveData> _saveSystem;
        private string _testSaveDirectory;

        [SetUp]
        public void SetUp()
        {
            _testSaveDirectory = Path.Combine(Application.temporaryCachePath, "SkillTreeTests");
            Directory.CreateDirectory(_testSaveDirectory);

            var config = new SaveSystemConfig
            {
                SaveDirectory = _testSaveDirectory,
                CreateBackups = false,
                PrettyPrint = true
            };

            _saveSystem = new JsonSaveSystem<SkillTreeSaveData>(config);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testSaveDirectory))
            {
                Directory.Delete(_testSaveDirectory, true);
            }
        }

        #region Basic Save/Load Tests

        [Test]
        public void Save_NewSave_CreatesFile()
        {
            // Arrange
            var saveData = new SkillTreeSaveData
            {
                SkillTreeId = "test_tree"
            };

            // Act
            _saveSystem.Save("test_save", saveData);

            // Assert
            // Note: Exists method not implemented in JsonSaveSystem
            // Check by attempting to load instead
            var loaded = _saveSystem.Load("test_save");
            Assert.IsNotNull(loaded);
        }

        [Test]
        public void Load_ExistingSave_ReturnsCorrectData()
        {
            // Arrange
            var originalData = new SkillTreeSaveData
            {
                SkillTreeId = "test_tree"
            };
            originalData.AddUnlockedNode("skill1");
            originalData.AddUnlockedNode("skill2");
            originalData.SetResource("SkillPoints", 10);

            _saveSystem.Save("test_save", originalData);

            // Act
            var loadedData = _saveSystem.Load("test_save");

            // Assert
            Assert.IsNotNull(loadedData);
            Assert.AreEqual("test_tree", loadedData.SkillTreeId);
            Assert.IsTrue(loadedData.IsNodeUnlocked("skill1"));
            Assert.IsTrue(loadedData.IsNodeUnlocked("skill2"));
            Assert.AreEqual(10, loadedData.GetResource("SkillPoints"));
        }

        [Test]
        public void Load_NonexistentSave_ReturnsNull()
        {
            // Act
            var result = _saveSystem.Load("nonexistent_save");

            // Assert
            Assert.IsNull(result);
        }

        // Note: Exists and Delete methods are not part of ISaveSystem interface
        // These tests are commented out until those methods are added to the API

        /*
        [Test]
        public void Exists_ExistingSave_ReturnsTrue()
        {
            // Arrange
            var saveData = new SkillTreeSaveData { SkillTreeId = "test_tree" };
            _saveSystem.Save("test_save", saveData);

            // Act
            var result = _saveSystem.Exists("test_save");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Exists_NonexistentSave_ReturnsFalse()
        {
            // Act
            var result = _saveSystem.Exists("nonexistent_save");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Delete_ExistingSave_RemovesFile()
        {
            // Arrange
            var saveData = new SkillTreeSaveData { SkillTreeId = "test_tree" };
            _saveSystem.Save("test_save", saveData);

            // Act
            _saveSystem.Delete("test_save");

            // Assert
            Assert.IsFalse(_saveSystem.Exists("test_save"));
        }
        */

        #endregion

        #region SaveData Tests

        [Test]
        public void SaveData_AddUnlockedNode_StoresCorrectly()
        {
            // Arrange
            var saveData = new SkillTreeSaveData { SkillTreeId = "test_tree" };

            // Act
            saveData.AddUnlockedNode("skill1");
            saveData.AddUnlockedNode("skill2");

            // Assert
            Assert.IsTrue(saveData.IsNodeUnlocked("skill1"));
            Assert.IsTrue(saveData.IsNodeUnlocked("skill2"));
            Assert.AreEqual(2, saveData.UnlockedNodes.Count);
        }

        [Test]
        public void SaveData_AddDuplicateNode_IgnoresDuplicate()
        {
            // Arrange
            var saveData = new SkillTreeSaveData { SkillTreeId = "test_tree" };

            // Act
            saveData.AddUnlockedNode("skill1");
            saveData.AddUnlockedNode("skill1"); // Duplicate

            // Assert
            Assert.AreEqual(1, saveData.UnlockedNodes.Count);
        }

        [Test]
        public void SaveData_RemoveUnlockedNode_RemovesCorrectly()
        {
            // Arrange
            var saveData = new SkillTreeSaveData { SkillTreeId = "test_tree" };
            saveData.AddUnlockedNode("skill1");
            saveData.AddUnlockedNode("skill2");

            // Act
            saveData.RemoveUnlockedNode("skill1");

            // Assert
            Assert.IsFalse(saveData.IsNodeUnlocked("skill1"));
            Assert.IsTrue(saveData.IsNodeUnlocked("skill2"));
            Assert.AreEqual(1, saveData.UnlockedNodes.Count);
        }

        [Test]
        public void SaveData_SetResource_StoresCorrectly()
        {
            // Arrange
            var saveData = new SkillTreeSaveData { SkillTreeId = "test_tree" };

            // Act
            saveData.SetResource("Gold", 100);
            saveData.SetResource("Gems", 50);

            // Assert
            Assert.AreEqual(100, saveData.GetResource("Gold"));
            Assert.AreEqual(50, saveData.GetResource("Gems"));
        }

        [Test]
        public void SaveData_UpdateResource_UpdatesCorrectly()
        {
            // Arrange
            var saveData = new SkillTreeSaveData { SkillTreeId = "test_tree" };
            saveData.SetResource("Gold", 100);

            // Act
            saveData.SetResource("Gold", 150);

            // Assert
            Assert.AreEqual(150, saveData.GetResource("Gold"));
        }

        [Test]
        public void SaveData_GetNonexistentResource_ReturnsZero()
        {
            // Arrange
            var saveData = new SkillTreeSaveData { SkillTreeId = "test_tree" };

            // Act
            var result = saveData.GetResource("NonexistentResource");

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion

        #region Round-Trip Tests

        [Test]
        public void RoundTrip_ComplexData_PreservesAllData()
        {
            // Arrange
            var originalData = new SkillTreeSaveData
            {
                SkillTreeId = "complex_tree"
            };

            // Add multiple nodes
            originalData.AddUnlockedNode("skill1");
            originalData.AddUnlockedNode("skill2");
            originalData.AddUnlockedNode("skill3");

            // Add resources
            originalData.SetResource("SkillPoints", 25);
            originalData.SetResource("Gold", 1000);
            originalData.SetResource("Gems", 50);

            // Save and load
            _saveSystem.Save("complex_save", originalData);
            var loadedData = _saveSystem.Load("complex_save");

            // Assert
            Assert.AreEqual(originalData.SkillTreeId, loadedData.SkillTreeId);
            Assert.AreEqual(originalData.UnlockedNodes.Count, loadedData.UnlockedNodes.Count);

            foreach (var nodeData in originalData.UnlockedNodes)
            {
                Assert.IsTrue(loadedData.IsNodeUnlocked(nodeData.NodeId));
            }

            Assert.AreEqual(originalData.GetResource("SkillPoints"), loadedData.GetResource("SkillPoints"));
            Assert.AreEqual(originalData.GetResource("Gold"), loadedData.GetResource("Gold"));
            Assert.AreEqual(originalData.GetResource("Gems"), loadedData.GetResource("Gems"));
        }

        #endregion
    }
}
