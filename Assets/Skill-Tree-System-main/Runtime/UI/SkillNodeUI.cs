using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PrimeTween;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;

namespace SkillTree.UI
{
    /// <summary>
    /// Individual skill node UI component.
    /// Displays node information, handles visual state changes,
    /// and processes user interactions.
    /// </summary>
    public class SkillNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Visual Elements")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _border;
        [SerializeField] private GameObject _lockIndicator;

        [Header("State Colors")]
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _availableColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color _unlockedColor = new Color(0.2f, 0.8f, 0.2f, 1f);

        [Header("Hover Effect")]
        [SerializeField] private float _hoverScale = 1.1f;
        [SerializeField] private float _hoverDuration = 0.2f;

        [Header("Animation Settings")]
        [SerializeField] private float _stateTransitionDuration = 0.3f;

        // Component references
        private RectTransform _rectTransform;
        private Canvas _canvas;

        // Data
        private SkillNode _node;
        private SkillTreeUI _treeUI;
        private SkillNodeState _currentState = SkillNodeState.Locked;

        // Animation
        private Tween _scaleTween;
        private Tween _colorTween;

        // Properties
        public SkillNode Node => _node;
        public SkillNodeState CurrentState => _currentState;
        public RectTransform RectTransform => _rectTransform;
        public Vector2 Position => _rectTransform.anchoredPosition;

        // Events
        public event Action<SkillNodeUI> OnClicked;
        public event Action<SkillNodeUI> OnHoverEnter;
        public event Action<SkillNodeUI> OnHoverExit;

        #region Unity Lifecycle

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();

            if (_rectTransform == null)
            {
                _rectTransform = gameObject.AddComponent<RectTransform>();
            }
        }

        private void OnDestroy()
        {
            // Stop any running animations
            _scaleTween.Stop();
            _colorTween.Stop();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the node UI with a skill node and parent tree UI.
        /// </summary>
        public void Initialize(SkillNode node, SkillTreeUI treeUI)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
            _treeUI = treeUI ?? throw new ArgumentNullException(nameof(treeUI));

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_node == null) return;

            // Update icon
            if (_icon != null && _node.Icon != null)
            {
                _icon.sprite = _node.Icon;
                _icon.enabled = true;
            }
            else if (_icon != null)
            {
                _icon.enabled = false;
            }

            // Update name
            if (_nameText != null)
            {
                _nameText.text = _node.DisplayName;
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Sets the visual state of the node (locked, available, unlocked).
        /// Animates the transition using PrimeTween.
        /// </summary>
        public void SetState(SkillNodeState newState, bool animate = true)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            ApplyStateVisuals(animate);
        }

        private void ApplyStateVisuals(bool animate)
        {
            Color targetColor = GetColorForState(_currentState);
            bool shouldShowLock = _currentState == SkillNodeState.Locked;

            if (animate)
            {
                // Animate color change
                if (_background != null)
                {
                    _colorTween.Stop();
                    _colorTween = Tween.Color(_background, targetColor, _stateTransitionDuration, Ease.OutCubic);
                }

                // Animate lock indicator
                if (_lockIndicator != null)
                {
                    if (shouldShowLock && !_lockIndicator.activeSelf)
                    {
                        _lockIndicator.SetActive(true);
                        var lockTransform = _lockIndicator.transform;
                        lockTransform.localScale = Vector3.zero;
                        Tween.Scale(lockTransform, Vector3.one, _stateTransitionDuration, Ease.OutBack);
                    }
                    else if (!shouldShowLock && _lockIndicator.activeSelf)
                    {
                        var lockTransform = _lockIndicator.transform;
                        Tween.Scale(lockTransform, Vector3.zero, _stateTransitionDuration, Ease.InBack)
                            .OnComplete(() => _lockIndicator.SetActive(false));
                    }
                }

                // Add a small bounce effect for unlocked state
                if (_currentState == SkillNodeState.Unlocked)
                {
                    Tween.Scale(transform, 1.2f, 0.2f, Ease.OutQuad)
                        .OnComplete(() => Tween.Scale(transform, 1f, 0.2f, Ease.InOutQuad));
                }
            }
            else
            {
                // Apply immediately without animation
                if (_background != null)
                {
                    _background.color = targetColor;
                }

                if (_lockIndicator != null)
                {
                    _lockIndicator.SetActive(shouldShowLock);
                }

                transform.localScale = Vector3.one;
            }
        }

        private Color GetColorForState(SkillNodeState state)
        {
            return state switch
            {
                SkillNodeState.Locked => _lockedColor,
                SkillNodeState.Available => _availableColor,
                SkillNodeState.Unlocked => _unlockedColor,
                _ => _lockedColor
            };
        }

        #endregion

        #region Position

        /// <summary>
        /// Sets the position of the node in the tree.
        /// </summary>
        public void SetPosition(Vector2 position, bool animate = false)
        {
            if (_rectTransform == null) return;

            if (animate)
            {
                Tween.UIAnchoredPosition(_rectTransform, position, 0.3f, Ease.OutCubic);
            }
            else
            {
                _rectTransform.anchoredPosition = position;
            }
        }

        /// <summary>
        /// Gets the world position of the node center (useful for connections).
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            if (_rectTransform == null) return Vector3.zero;
            return _rectTransform.position;
        }

        #endregion

        #region User Interaction

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_node == null || _treeUI == null) return;

            // Notify listeners
            OnClicked?.Invoke(this);

            // Try to unlock if available
            if (_currentState == SkillNodeState.Available && _treeUI.Manager != null)
            {
                bool success = _treeUI.Manager.UnlockNode(_node.Id);

                if (success)
                {
                    // Play success feedback
                    PlayUnlockFeedback();
                }
                else
                {
                    // Play failure feedback
                    PlayFailureFeedback();
                }
            }
            else if (_currentState == SkillNodeState.Locked)
            {
                // Play locked feedback (shake)
                PlayLockedFeedback();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Scale up on hover
            _scaleTween.Stop();
            _scaleTween = Tween.Scale(transform, _hoverScale, _hoverDuration, Ease.OutCubic);

            // Notify listeners (for tooltip)
            OnHoverEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Scale back to normal
            _scaleTween.Stop();
            _scaleTween = Tween.Scale(transform, 1f, _hoverDuration, Ease.OutCubic);

            // Notify listeners (for tooltip)
            OnHoverExit?.Invoke(this);
        }

        #endregion

        #region Visual Feedback

        private void PlayUnlockFeedback()
        {
            // Already handled by state change animation
        }

        private void PlayFailureFeedback()
        {
            // Flash red briefly
            if (_border != null)
            {
                var originalColor = _border.color;
                _border.color = Color.red;
                Tween.Color(_border, originalColor, 0.5f, Ease.OutQuad);
            }
        }

        private void PlayLockedFeedback()
        {
            // Shake the node
            if (_lockIndicator != null && _lockIndicator.activeSelf)
            {
                var lockTransform = _lockIndicator.transform;
                Tween.ShakeLocalPosition(lockTransform, new Vector3(5f, 0f, 0f), 0.3f, 20);
            }

            // Also shake the main node slightly
            Tween.PunchScale(transform, Vector3.one * 0.1f, 0.3f);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Gets detailed validation information for this node.
        /// </summary>
        public UnlockValidationResult GetValidationResult()
        {
            if (_treeUI?.Manager == null) return null;
            return _treeUI.Manager.ValidateUnlock(_node.Id);
        }

        #endregion
    }
}
