using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using SkillTree.Core.Data;

namespace SkillTree.Editor.GraphView
{
    /// <summary>
    /// Custom Edge view for skill tree connections with visual customization
    /// </summary>
    public class SkillConnectionView : Edge
    {
        private SkillConnection _connection;
        private Label _connectionTypeLabel;

        public SkillConnection Connection => _connection;

        public SkillConnectionView() : base()
        {
            // Setup default capabilities
            capabilities |= Capabilities.Selectable | Capabilities.Deletable;

            // Add connection type label
            _connectionTypeLabel = new Label();
            _connectionTypeLabel.AddToClassList("connection-type-label");
            _connectionTypeLabel.style.position = Position.Absolute;
            _connectionTypeLabel.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            _connectionTypeLabel.style.color = Color.white;
            _connectionTypeLabel.style.paddingLeft = 4;
            _connectionTypeLabel.style.paddingRight = 4;
            _connectionTypeLabel.style.paddingTop = 2;
            _connectionTypeLabel.style.paddingBottom = 2;
            _connectionTypeLabel.style.borderTopLeftRadius = 3;
            _connectionTypeLabel.style.borderTopRightRadius = 3;
            _connectionTypeLabel.style.borderBottomLeftRadius = 3;
            _connectionTypeLabel.style.borderBottomRightRadius = 3;
            _connectionTypeLabel.style.fontSize = 9;
            _connectionTypeLabel.style.display = DisplayStyle.None; // Hidden by default, shown on hover

            Add(_connectionTypeLabel);

            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        public void SetConnection(SkillConnection connection)
        {
            _connection = connection;
            userData = connection;

            if (_connection != null)
            {
                ApplyConnectionVisuals();
                UpdateTypeLabel();
            }
        }

        private void ApplyConnectionVisuals()
        {
            if (_connection == null)
                return;

            // Apply color based on connection type
            Color edgeColor = GetConnectionTypeColor();
            if (_connection.Color != Color.white)
            {
                edgeColor = _connection.Color;
            }

            // Apply edge color
            edgeControl.inputColor = edgeColor;
            edgeControl.outputColor = edgeColor;

            // Apply width
            edgeControl.edgeWidth = (int)_connection.Width;

            // Apply style based on connection type
            ApplyConnectionTypeStyle();
        }

        private Color GetConnectionTypeColor()
        {
            return _connection.Type switch
            {
                ConnectionType.Prerequisite => new Color(0.3f, 0.8f, 1f), // Light blue
                ConnectionType.Soft => new Color(0.8f, 0.8f, 0.3f), // Yellow
                ConnectionType.Exclusive => new Color(1f, 0.3f, 0.3f), // Red
                ConnectionType.Bidirectional => new Color(0.5f, 1f, 0.5f), // Green
                _ => Color.white
            };
        }

        private void ApplyConnectionTypeStyle()
        {
            // Remove all type classes
            RemoveFromClassList("connection-prerequisite");
            RemoveFromClassList("connection-soft");
            RemoveFromClassList("connection-exclusive");
            RemoveFromClassList("connection-bidirectional");

            // Add appropriate class based on connection type
            switch (_connection.Type)
            {
                case ConnectionType.Prerequisite:
                    AddToClassList("connection-prerequisite");
                    break;
                case ConnectionType.Soft:
                    AddToClassList("connection-soft");
                    break;
                case ConnectionType.Exclusive:
                    AddToClassList("connection-exclusive");
                    break;
                case ConnectionType.Bidirectional:
                    AddToClassList("connection-bidirectional");
                    break;
            }
        }

        private void UpdateTypeLabel()
        {
            if (_connection == null)
                return;

            _connectionTypeLabel.text = GetConnectionTypeDisplayName();
        }

        private string GetConnectionTypeDisplayName()
        {
            return _connection.Type switch
            {
                ConnectionType.Prerequisite => "Prerequisite",
                ConnectionType.Soft => "Soft",
                ConnectionType.Exclusive => "Exclusive",
                ConnectionType.Bidirectional => "Bidirectional",
                _ => "Unknown"
            };
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            // Show connection type label on hover
            if (_connectionTypeLabel != null && _connection != null)
            {
                _connectionTypeLabel.style.display = DisplayStyle.Flex;
                UpdateLabelPosition();
            }

            // Highlight the edge
            if (edgeControl != null)
            {
                edgeControl.edgeWidth = (int)((_connection?.Width ?? 2f) * 1.5f);
            }
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            // Hide connection type label
            if (_connectionTypeLabel != null)
            {
                _connectionTypeLabel.style.display = DisplayStyle.None;
            }

            // Reset edge width
            if (edgeControl != null && _connection != null)
            {
                edgeControl.edgeWidth = (int)_connection.Width;
            }
        }

        private void UpdateLabelPosition()
        {
            if (_connectionTypeLabel == null || edgeControl == null)
                return;

            // Position label at the midpoint of the edge
            var bounds = edgeControl.layout;
            _connectionTypeLabel.style.left = bounds.width / 2 - _connectionTypeLabel.layout.width / 2;
            _connectionTypeLabel.style.top = bounds.height / 2 - _connectionTypeLabel.layout.height / 2;
        }

        // Note: BuildContextualMenu is not available on Edge in this version of Unity
        // Context menu functionality can be added through the GraphView instead
        /*
        protected override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (_connection == null)
                return;

            evt.menu.AppendSeparator();

            // Connection type submenu
            evt.menu.AppendAction($"Set Type/Prerequisite",
                a => SetConnectionType(ConnectionType.Prerequisite),
                a => _connection.Type == ConnectionType.Prerequisite ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            evt.menu.AppendAction($"Set Type/Soft",
                a => SetConnectionType(ConnectionType.Soft),
                a => _connection.Type == ConnectionType.Soft ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            evt.menu.AppendAction($"Set Type/Exclusive",
                a => SetConnectionType(ConnectionType.Exclusive),
                a => _connection.Type == ConnectionType.Exclusive ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            evt.menu.AppendAction($"Set Type/Bidirectional",
                a => SetConnectionType(ConnectionType.Bidirectional),
                a => _connection.Type == ConnectionType.Bidirectional ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Edit Connection Properties", a => OpenConnectionInspector());
        }
        */

        private void SetConnectionType(ConnectionType type)
        {
            if (_connection == null)
                return;

            _connection.Type = type;
            ApplyConnectionVisuals();
            UpdateTypeLabel();
        }

        private void OpenConnectionInspector()
        {
            // Could open a detailed inspector window for connection properties
            // For now, we'll use a simple dialog
            if (_connection == null)
                return;

            // This is a placeholder - could create a full inspector window similar to SkillNodeInspectorWindow
            Debug.Log($"Connection Inspector: {_connection}");
        }
    }
}
