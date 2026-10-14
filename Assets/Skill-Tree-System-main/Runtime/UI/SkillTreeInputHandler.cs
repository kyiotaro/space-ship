using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;

namespace SkillTree.UI
{
    /// <summary>
    /// Handles user input for the skill tree UI.
    /// Supports zoom (mouse wheel), pan (drag), and click interactions.
    /// </summary>
    public class SkillTreeInputHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        private SkillTreeUI _treeUI;
        private RectTransform _nodesContainer;

        [Header("Zoom Settings")]
        private float _minZoom = 0.5f;
        private float _maxZoom = 2f;
        private float _zoomSpeed = 0.1f;
        private float _currentZoom = 1f;

        [Header("Pan Settings")]
        [SerializeField] private float _panSensitivity = 1f;
        [SerializeField] private bool _invertPan = false;

        [Header("Bounds")]
        [SerializeField] private bool _constrainToBounds = false;
        [SerializeField] private Rect _panBounds = new Rect(-2000, -2000, 4000, 4000);

        // Drag state
        private bool _isDragging = false;
        private Vector2 _lastDragPosition;

        // Animation
        private Tween _zoomTween;
        private Tween _panTween;

        #region Initialization

        /// <summary>
        /// Initializes the input handler with a reference to the tree UI.
        /// </summary>
        public void Initialize(SkillTreeUI treeUI, float minZoom, float maxZoom, float zoomSpeed)
        {
            _treeUI = treeUI;
            _nodesContainer = treeUI.NodesContainer;
            _minZoom = minZoom;
            _maxZoom = maxZoom;
            _zoomSpeed = zoomSpeed;
            _currentZoom = 1f;
        }

        #endregion

        #region Zoom

        public void OnScroll(PointerEventData eventData)
        {
            if (_nodesContainer == null) return;

            // Calculate new zoom level
            float zoomDelta = eventData.scrollDelta.y * _zoomSpeed;
            float newZoom = Mathf.Clamp(_currentZoom + zoomDelta, _minZoom, _maxZoom);

            if (Mathf.Approximately(newZoom, _currentZoom)) return;

            // Get mouse position in container space (for zoom-to-cursor)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _nodesContainer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localMousePosition
            );

            // Calculate pivot for zooming towards cursor
            Vector2 pivotBefore = localMousePosition / _currentZoom;

            // Apply zoom
            _currentZoom = newZoom;
            _zoomTween.Stop();
            _zoomTween = Tween.Scale(_nodesContainer, Vector3.one * _currentZoom, 0.1f, Ease.OutQuad);

            // Adjust position to zoom towards cursor
            Vector2 pivotAfter = localMousePosition / _currentZoom;
            Vector2 pivotDelta = (pivotBefore - pivotAfter) * _currentZoom;
            _nodesContainer.anchoredPosition += pivotDelta;

            // Constrain to bounds if enabled
            if (_constrainToBounds)
            {
                ConstrainToBounds();
            }
        }

        /// <summary>
        /// Sets the zoom level programmatically.
        /// </summary>
        public void SetZoom(float zoom, bool animate = true)
        {
            if (_nodesContainer == null) return;

            zoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);
            _currentZoom = zoom;

            _zoomTween.Stop();

            if (animate)
            {
                _zoomTween = Tween.Scale(_nodesContainer, Vector3.one * zoom, 0.3f, Ease.OutCubic);
            }
            else
            {
                _nodesContainer.localScale = Vector3.one * zoom;
            }
        }

        /// <summary>
        /// Resets zoom to default (1.0).
        /// </summary>
        public void ResetZoom(bool animate = true)
        {
            SetZoom(1f, animate);
        }

        public float CurrentZoom => _currentZoom;

        #endregion

        #region Pan

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Check if we're clicking on a node (if so, don't pan)
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                var nodeUI = eventData.pointerCurrentRaycast.gameObject.GetComponent<SkillNodeUI>();
                if (nodeUI != null)
                {
                    // Clicking on a node - don't pan
                    return;
                }
            }

            _isDragging = true;
            _lastDragPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _nodesContainer == null) return;

            Vector2 currentDragPosition = eventData.position;
            Vector2 delta = (currentDragPosition - _lastDragPosition) * _panSensitivity;

            if (_invertPan)
            {
                delta = -delta;
            }

            // Apply pan
            _nodesContainer.anchoredPosition += delta;

            // Constrain to bounds if enabled
            if (_constrainToBounds)
            {
                ConstrainToBounds();
            }

            _lastDragPosition = currentDragPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

        /// <summary>
        /// Pans the view to a specific position.
        /// </summary>
        public void PanTo(Vector2 position, bool animate = true)
        {
            if (_nodesContainer == null) return;

            _panTween.Stop();

            if (animate)
            {
                _panTween = Tween.UIAnchoredPosition(_nodesContainer, position, 0.5f, Ease.OutCubic);
            }
            else
            {
                _nodesContainer.anchoredPosition = position;
            }

            if (_constrainToBounds)
            {
                ConstrainToBounds();
            }
        }

        /// <summary>
        /// Centers the view on the entire skill tree.
        /// </summary>
        public void CenterView(bool animate = true)
        {
            if (_treeUI?.Graph == null) return;

            // Calculate the center of all nodes
            Vector2 center = Vector2.zero;
            int count = 0;

            foreach (var node in _treeUI.Graph.Nodes)
            {
                center += node.EditorPosition;
                count++;
            }

            if (count > 0)
            {
                center /= count;
                PanTo(-center, animate);
            }
        }

        /// <summary>
        /// Focuses the view on a specific node.
        /// </summary>
        public void FocusOnNode(string nodeId, bool animate = true)
        {
            if (_treeUI?.Graph == null) return;

            var node = _treeUI.Graph.GetNodeById(nodeId);
            if (node != null)
            {
                PanTo(-node.EditorPosition, animate);
            }
        }

        #endregion

        #region Bounds

        private void ConstrainToBounds()
        {
            if (_nodesContainer == null) return;

            Vector2 position = _nodesContainer.anchoredPosition;

            position.x = Mathf.Clamp(position.x, _panBounds.xMin, _panBounds.xMax);
            position.y = Mathf.Clamp(position.y, _panBounds.yMin, _panBounds.yMax);

            _nodesContainer.anchoredPosition = position;
        }

        /// <summary>
        /// Sets the pan bounds.
        /// </summary>
        public void SetPanBounds(Rect bounds)
        {
            _panBounds = bounds;
        }

        /// <summary>
        /// Enables or disables pan bounds constraint.
        /// </summary>
        public void SetConstrainToBounds(bool constrain)
        {
            _constrainToBounds = constrain;
        }

        #endregion

        #region Keyboard Shortcuts

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            // F key - Frame all (center and fit entire tree)
            if (Input.GetKeyDown(KeyCode.F))
            {
                FrameAll();
            }

            // Home key - Reset view
            if (Input.GetKeyDown(KeyCode.Home))
            {
                ResetView();
            }

            // WASD or Arrow keys - Pan
            Vector2 keyboardPan = Vector2.zero;
            float panSpeed = 10f;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                keyboardPan.y += panSpeed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                keyboardPan.y -= panSpeed;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                keyboardPan.x += panSpeed;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                keyboardPan.x -= panSpeed;
            }

            if (keyboardPan != Vector2.zero && _nodesContainer != null)
            {
                _nodesContainer.anchoredPosition += keyboardPan;

                if (_constrainToBounds)
                {
                    ConstrainToBounds();
                }
            }

            // Plus/Minus keys - Zoom
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                SetZoom(_currentZoom + 0.1f, true);
            }
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                SetZoom(_currentZoom - 0.1f, true);
            }
        }

        #endregion

        #region View Operations

        /// <summary>
        /// Frames the entire skill tree (zooms and pans to show all nodes).
        /// </summary>
        public void FrameAll(bool animate = true)
        {
            if (_treeUI?.Graph == null || _nodesContainer == null) return;

            // Get bounding box of all nodes
            var layoutEngine = new SkillTreeLayoutEngine(_treeUI);
            Rect bounds = layoutEngine.GetBoundingBox();

            // Calculate required zoom to fit
            RectTransform canvasRect = _nodesContainer.parent as RectTransform;
            if (canvasRect == null) return;

            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            float zoomX = canvasWidth / (bounds.width + 200f); // Add padding
            float zoomY = canvasHeight / (bounds.height + 200f);
            float zoom = Mathf.Min(zoomX, zoomY);
            zoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);

            // Center view
            Vector2 center = bounds.center;

            // Apply zoom and pan
            SetZoom(zoom, animate);
            PanTo(-center, animate);
        }

        /// <summary>
        /// Resets the view to default (zoom 1.0, centered).
        /// </summary>
        public void ResetView(bool animate = true)
        {
            SetZoom(1f, animate);
            CenterView(animate);
        }

        #endregion
    }
}
