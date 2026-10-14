using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SkillTree.Core.Data
{
    [Serializable]
    public class SkillNode
    {
        // Identity
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(3, 6)] private string _description;
        [SerializeField] private Sprite _icon;

        // Localization support
        [SerializeField] private string _displayNameLocalizationKey;
        [SerializeField] private string _descriptionLocalizationKey;

        // Graph structure
        [SerializeField] private Vector2 _editorPosition;
        [SerializeField] private List<string> _prerequisiteNodeIds = new List<string>();

        // Unlock logic
        [SerializeField] private SkillNodeType _nodeType = SkillNodeType.Standard;
        [SerializeField] private List<SkillRequirement> _requirements = new List<SkillRequirement>();
        [SerializeField] private List<SkillCost> _costs = new List<SkillCost>();

        // Extensibility
        [SerializeField] private SerializableDictionary<string, string> _customData = new SerializableDictionary<string, string>();

        // Events
        public UnityEvent OnUnlocked = new UnityEvent();
        public UnityEvent OnLocked = new UnityEvent();

        // Properties
        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public Sprite Icon
        {
            get => _icon;
            set => _icon = value;
        }

        public string DisplayNameLocalizationKey
        {
            get => _displayNameLocalizationKey;
            set => _displayNameLocalizationKey = value;
        }

        public string DescriptionLocalizationKey
        {
            get => _descriptionLocalizationKey;
            set => _descriptionLocalizationKey = value;
        }

        public Vector2 EditorPosition
        {
            get => _editorPosition;
            set => _editorPosition = value;
        }

        public List<string> PrerequisiteNodeIds
        {
            get => _prerequisiteNodeIds;
            set => _prerequisiteNodeIds = value;
        }

        public SkillNodeType NodeType
        {
            get => _nodeType;
            set => _nodeType = value;
        }

        public List<SkillRequirement> Requirements
        {
            get => _requirements;
            set => _requirements = value;
        }

        public List<SkillCost> Costs
        {
            get => _costs;
            set => _costs = value;
        }

        public SerializableDictionary<string, string> CustomData
        {
            get => _customData;
            set => _customData = value;
        }

        // Constructor
        public SkillNode()
        {
            _id = Guid.NewGuid().ToString();
        }

        public SkillNode(string id)
        {
            _id = id;
        }

        // Helper methods
        public bool HasPrerequisites()
        {
            return _prerequisiteNodeIds != null && _prerequisiteNodeIds.Count > 0;
        }

        public bool HasRequirements()
        {
            return _requirements != null && _requirements.Count > 0;
        }

        public bool HasCosts()
        {
            return _costs != null && _costs.Count > 0;
        }

        public void AddPrerequisite(string nodeId)
        {
            if (!_prerequisiteNodeIds.Contains(nodeId))
            {
                _prerequisiteNodeIds.Add(nodeId);
            }
        }

        public void RemovePrerequisite(string nodeId)
        {
            _prerequisiteNodeIds.Remove(nodeId);
        }

        public override string ToString()
        {
            return $"SkillNode[{_id}]: {_displayName}";
        }
    }

    [Serializable]
    public enum SkillNodeType
    {
        Standard,
        Root,
        Milestone,
        Capstone,
        Special
    }

    [Serializable]
    public class SkillRequirement
    {
        [SerializeField] private RequirementType _type;
        [SerializeField] private RequirementLogic _logic = RequirementLogic.And;
        [SerializeField] private List<string> _requiredNodeIds = new List<string>();
        [SerializeField] private int _requiredCount;
        [SerializeField] private string _customCondition;
        [SerializeField] private SerializableDictionary<string, string> _parameters = new SerializableDictionary<string, string>();

        public RequirementType Type
        {
            get => _type;
            set => _type = value;
        }

        public RequirementLogic Logic
        {
            get => _logic;
            set => _logic = value;
        }

        public List<string> RequiredNodeIds
        {
            get => _requiredNodeIds;
            set => _requiredNodeIds = value;
        }

        public int RequiredCount
        {
            get => _requiredCount;
            set => _requiredCount = value;
        }

        public string CustomCondition
        {
            get => _customCondition;
            set => _customCondition = value;
        }

        public SerializableDictionary<string, string> Parameters
        {
            get => _parameters;
            set => _parameters = value;
        }
    }

    [Serializable]
    public enum RequirementType
    {
        NodeUnlocked,
        StatCheck,
        LevelCheck,
        Custom
    }

    [Serializable]
    public enum RequirementLogic
    {
        And,
        Or,
        Count
    }

    [Serializable]
    public class SkillCost
    {
        [SerializeField] private string _resourceType;
        [SerializeField] private int _amount;
        [SerializeField] private SerializableDictionary<string, string> _customData = new SerializableDictionary<string, string>();

        public string ResourceType
        {
            get => _resourceType;
            set => _resourceType = value;
        }

        public int Amount
        {
            get => _amount;
            set => _amount = value;
        }

        public SerializableDictionary<string, string> CustomData
        {
            get => _customData;
            set => _customData = value;
        }
    }

    // Simple serializable dictionary implementation
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [SerializeField] private List<TKey> _keys = new List<TKey>();
        [SerializeField] private List<TValue> _values = new List<TValue>();

        public TValue this[TKey key]
        {
            get
            {
                int index = _keys.IndexOf(key);
                if (index >= 0)
                    return _values[index];
                throw new KeyNotFoundException($"Key '{key}' not found in dictionary");
            }
            set
            {
                int index = _keys.IndexOf(key);
                if (index >= 0)
                {
                    _values[index] = value;
                }
                else
                {
                    _keys.Add(key);
                    _values.Add(value);
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _keys.Contains(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = _keys.IndexOf(key);
            if (index >= 0)
            {
                value = _values[index];
                return true;
            }
            value = default;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            if (_keys.Contains(key))
                throw new ArgumentException($"Key '{key}' already exists in dictionary");
            _keys.Add(key);
            _values.Add(value);
        }

        public bool Remove(TKey key)
        {
            int index = _keys.IndexOf(key);
            if (index >= 0)
            {
                _keys.RemoveAt(index);
                _values.RemoveAt(index);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
        }

        public int Count => _keys.Count;

        public IEnumerable<TKey> Keys => _keys;
        public IEnumerable<TValue> Values => _values;
    }
}
