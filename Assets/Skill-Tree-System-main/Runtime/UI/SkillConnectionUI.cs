using System;
using UnityEngine;
using UnityEngine.UI;
using SkillTree.Core.Data;
using PrimeTween;

namespace SkillTree.UI
{
    /// <summary>
    /// Renders a visual connection line between two skill nodes.
    /// Uses Unity UI Image with filled mode or a custom line renderer.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SkillConnectionUI : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField] private Image _lineImage;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private bool _useLineRenderer = false;

        [Header("Visual Settings")]
        [SerializeField] private float _defaultWidth = 3f;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private int _lineSegments = 20;
        [SerializeField] private float _curvature = 0.3f;

        [Header("State Colors")]
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _partialColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        [SerializeField] private Color _activeColor = new Color(0.2f, 0.8f, 0.2f, 1f);

        // Component references
        private RectTransform _rectTransform;
        private Canvas _canvas;

        // Data
        private SkillConnection _connection;
        private SkillNodeUI _fromNodeUI;
        private SkillNodeUI _toNodeUI;

        // Animation
        private Tween _colorTween;

        // Properties
        public SkillConnection Connection => _connection;
        public SkillNodeUI FromNodeUI => _fromNodeUI;
        public SkillNodeUI ToNodeUI => _toNodeUI;

        #region Unity Lifecycle

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();

            if (_useLineRenderer && _lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
                ConfigureLineRenderer();
            }
            else if (!_useLineRenderer && _lineImage == null)
            {
                _lineImage = gameObject.AddComponent<Image>();
            }
        }

        private void LateUpdate()
        {
            // Update connection visual position each frame
            // (nodes may move due to animations or layout changes)
            UpdateVisual();
        }

        private void OnDestroy()
        {
            _colorTween.Stop();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the connection UI with connection data and node UIs.
        /// </summary>
        public void Initialize(SkillConnection connection, SkillNodeUI fromNodeUI, SkillNodeUI toNodeUI)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _fromNodeUI = fromNodeUI ?? throw new ArgumentNullException(nameof(fromNodeUI));
            _toNodeUI = toNodeUI ?? throw new ArgumentNullException(nameof(toNodeUI));

            UpdateVisual();
        }

        private void ConfigureLineRenderer()
        {
            if (_lineRenderer == null) return;

            _lineRenderer.useWorldSpace = false;
            _lineRenderer.positionCount = _lineSegments;
            _lineRenderer.startWidth = _defaultWidth;
            _lineRenderer.endWidth = _defaultWidth;
            _lineRenderer.material = new Material(Shader.Find("UI/Default"));
            _lineRenderer.startColor = _defaultColor;
            _lineRenderer.endColor = _defaultColor;
        }

        #endregion

        #region Visual Update

        /// <summary>
        /// Updates the visual appearance of the connection.
        /// Called whenever node positions or states change.
        /// </summary>
        public void UpdateVisual()
        {
            if (_fromNodeUI == null || _toNodeUI == null) return;

            if (_useLineRenderer)
            {
                UpdateLineRenderer();
            }
            else
            {
                UpdateImageRenderer();
            }

            UpdateColor();
        }

        private void UpdateLineRenderer()
        {
            if (_lineRenderer == null) return;

            Vector3 startPos = _fromNodeUI.GetWorldPosition();
            Vector3 endPos = _toNodeUI.GetWorldPosition();

            // Create a curved line using Bezier curve
            for (int i = 0; i < _lineSegments; i++)
            {
                float t = i / (float)(_lineSegments - 1);
                Vector3 position = CalculateBezierPoint(t, startPos, endPos);
                _lineRenderer.SetPosition(i, position);
            }

            // Apply connection settings
            if (_connection != null)
            {
                float width = _connection.Width > 0 ? _connection.Width : _defaultWidth;
                _lineRenderer.startWidth = width;
                _lineRenderer.endWidth = width;
            }
        }

        private void UpdateImageRenderer()
        {
            if (_lineImage == null || _rectTransform == null) return;

            Vector2 startPos = _fromNodeUI.Position;
            Vector2 endPos = _toNodeUI.Position;

            // Calculate the center point between nodes
            Vector2 center = (startPos + endPos) / 2f;
            _rectTransform.anchoredPosition = center;

            // Calculate the distance and angle
            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Set the size (width = distance, height = line width)
            float width = _connection?.Width ?? _defaultWidth;
            _rectTransform.sizeDelta = new Vector2(distance, width);

            // Set the rotation
            _rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            // Apply color
            if (_connection != null && _connection.Color != default)
            {
                _lineImage.color = _connection.Color;
            }
        }

        private Vector3 CalculateBezierPoint(float t, Vector3 start, Vector3 end)
        {
            // Simple quadratic Bezier curve with a control point
            Vector3 controlPoint = (start + end) / 2f + Vector3.up * _curvature * Vector3.Distance(start, end);

            // Quadratic Bezier formula: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 point = uu * start;
            point += 2 * u * t * controlPoint;
            point += tt * end;

            return point;
        }

        #endregion

        #region State-Based Coloring

        private void UpdateColor()
        {
            if (_fromNodeUI == null || _toNodeUI == null) return;

            Color targetColor = DetermineConnectionColor();

            if (_useLineRenderer && _lineRenderer != null)
            {
                _colorTween.Stop();
                // Animate line renderer color
                var startColor = _lineRenderer.startColor;
                _colorTween = Tween.Custom(startColor, targetColor, 0.3f, onValueChange: color =>
                {
                    _lineRenderer.startColor = color;
                    _lineRenderer.endColor = color;
                });
            }
            else if (_lineImage != null)
            {
                _colorTween.Stop();
                _colorTween = Tween.Color(_lineImage, targetColor, 0.3f);
            }
        }

        private Color DetermineConnectionColor()
        {
            // Connection color depends on the state of both nodes
            bool fromUnlocked = _fromNodeUI.CurrentState == SkillNodeState.Unlocked;
            bool toUnlocked = _toNodeUI.CurrentState == SkillNodeState.Unlocked;
            bool toAvailable = _toNodeUI.CurrentState == SkillNodeState.Available;

            if (fromUnlocked && toUnlocked)
            {
                // Both nodes unlocked - active color
                return _activeColor;
            }
            else if (fromUnlocked && toAvailable)
            {
                // From unlocked, to available - partial color
                return _partialColor;
            }
            else
            {
                // Otherwise - locked color
                return _lockedColor;
            }
        }

        #endregion

        #region Connection Types

        /// <summary>
        /// Updates visual style based on connection type.
        /// </summary>
        public void SetConnectionType(ConnectionType type)
        {
            if (_connection != null)
            {
                _connection.Type = type;
            }

            ApplyConnectionTypeVisuals(type);
        }

        private void ApplyConnectionTypeVisuals(ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.Prerequisite:
                    // Solid line (default)
                    break;
                case ConnectionType.Soft:
                    // Dashed line (can be implemented with custom shader)
                    break;
                case ConnectionType.Exclusive:
                    // Different color or pattern
                    if (_lineImage != null)
                    {
                        _lineImage.color = Color.red;
                    }
                    break;
                case ConnectionType.Bidirectional:
                    // Arrow on both ends (can add arrow sprites)
                    break;
            }
        }

        #endregion

        #region Animation

        /// <summary>
        /// Plays a flow animation along the connection (e.g., when a node is unlocked).
        /// </summary>
        public void PlayFlowAnimation()
        {
            // Future enhancement: animate particles or flow along the line
            // For now, just pulse the color briefly
            if (_lineImage != null)
            {
                var originalColor = _lineImage.color;
                _lineImage.color = Color.white;
                Tween.Color(_lineImage, originalColor, 0.5f);
            }
        }

        #endregion
    }
}
