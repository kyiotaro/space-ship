using System;
using UnityEngine;

namespace SkillTree.Core.Data
{
    [Serializable]
    public class SkillConnection
    {
        // Connection identity
        [SerializeField] private string _id;
        [SerializeField] private string _fromNodeId;
        [SerializeField] private string _toNodeId;

        // Connection type
        [SerializeField] private ConnectionType _connectionType = ConnectionType.Prerequisite;

        // Visual customization
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private float _width = 2f;
        [SerializeField] private AnimationCurve _curve;

        // Properties
        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string FromNodeId
        {
            get => _fromNodeId;
            set => _fromNodeId = value;
        }

        public string ToNodeId
        {
            get => _toNodeId;
            set => _toNodeId = value;
        }

        public ConnectionType Type
        {
            get => _connectionType;
            set => _connectionType = value;
        }

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        public float Width
        {
            get => _width;
            set => _width = value;
        }

        public AnimationCurve Curve
        {
            get => _curve;
            set => _curve = value;
        }

        // Constructor
        public SkillConnection()
        {
            _id = Guid.NewGuid().ToString();
            InitializeDefaultCurve();
        }

        public SkillConnection(string fromNodeId, string toNodeId)
        {
            _id = Guid.NewGuid().ToString();
            _fromNodeId = fromNodeId;
            _toNodeId = toNodeId;
            InitializeDefaultCurve();
        }

        public SkillConnection(string fromNodeId, string toNodeId, ConnectionType connectionType)
        {
            _id = Guid.NewGuid().ToString();
            _fromNodeId = fromNodeId;
            _toNodeId = toNodeId;
            _connectionType = connectionType;
            InitializeDefaultCurve();
        }

        private void InitializeDefaultCurve()
        {
            if (_curve == null)
            {
                _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(_fromNodeId) &&
                   !string.IsNullOrEmpty(_toNodeId) &&
                   _fromNodeId != _toNodeId;
        }

        public override string ToString()
        {
            return $"SkillConnection[{_id}]: {_fromNodeId} -> {_toNodeId} ({_connectionType})";
        }

        public override bool Equals(object obj)
        {
            if (obj is SkillConnection other)
            {
                return _fromNodeId == other._fromNodeId &&
                       _toNodeId == other._toNodeId &&
                       _connectionType == other._connectionType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_fromNodeId, _toNodeId, _connectionType);
        }
    }

    [Serializable]
    public enum ConnectionType
    {
        Prerequisite,
        Soft,
        Exclusive,
        Bidirectional
    }
}
