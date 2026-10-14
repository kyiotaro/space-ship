using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;

namespace SkillTree.UI
{
    /// <summary>
    /// Displays detailed information about a skill node when hovering.
    /// Shows name, description, prerequisites, requirements, and costs.
    /// </summary>
    public class SkillTooltipUI : MonoBehaviour
    {
        [Header("Visual Elements")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _container;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;

        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _prerequisitesText;
        [SerializeField] private TextMeshProUGUI _requirementsText;
        [SerializeField] private TextMeshProUGUI _costsText;

        [Header("Layout")]
        [SerializeField] private Vector2 _offset = new Vector2(10f, 10f);
        [SerializeField] private float _maxWidth = 400f;

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.2f;
        [SerializeField] private float _fadeOutDuration = 0.15f;
        [SerializeField] private float _showDelay = 0.3f;

        // State
        private SkillNode _currentNode;
        private ISkillTreeManager _manager;
        private bool _isVisible = false;
        private Tween _fadeTween;
        private Tween _delayTween;

        // Cached canvas for screen space calculations
        private Canvas _canvas;
        private RectTransform _canvasRectTransform;

        #region Unity Lifecycle

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _canvasRectTransform = _canvas?.GetComponent<RectTransform>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (_container == null)
            {
                _container = transform as RectTransform;
            }

            // Start hidden
            Hide(immediate: true);
        }

        private void Update()
        {
            // Follow mouse position if visible
            if (_isVisible)
            {
                UpdatePosition();
            }
        }

        #endregion

        #region Show/Hide

        /// <summary>
        /// Shows the tooltip for a specific skill node.
        /// </summary>
        public void Show(SkillNode node, ISkillTreeManager manager, Vector2 screenPosition)
        {
            if (node == null) return;

            _currentNode = node;
            _manager = manager;

            // Cancel any pending animations
            _delayTween.Stop();
            _fadeTween.Stop();

            // Populate content
            PopulateContent();

            // Delay before showing
            _delayTween = Tween.Delay(_showDelay, () =>
            {
                _isVisible = true;
                gameObject.SetActive(true);
                _fadeTween = Tween.Alpha(_canvasGroup, 1f, _fadeInDuration, Ease.OutCubic);
                UpdatePosition();
            });
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public void Hide(bool immediate = false)
        {
            _delayTween.Stop();
            _fadeTween.Stop();

            if (immediate)
            {
                _canvasGroup.alpha = 0f;
                _isVisible = false;
                gameObject.SetActive(false);
            }
            else
            {
                _fadeTween = Tween.Alpha(_canvasGroup, 0f, _fadeOutDuration, Ease.InCubic)
                    .OnComplete(() =>
                    {
                        _isVisible = false;
                        gameObject.SetActive(false);
                    });
            }

            _currentNode = null;
        }

        #endregion

        #region Content Population

        private void PopulateContent()
        {
            if (_currentNode == null) return;

            // Name
            if (_nameText != null)
            {
                _nameText.text = _currentNode.DisplayName;
            }

            // Icon
            if (_icon != null)
            {
                if (_currentNode.Icon != null)
                {
                    _icon.sprite = _currentNode.Icon;
                    _icon.enabled = true;
                }
                else
                {
                    _icon.enabled = false;
                }
            }

            // Description
            if (_descriptionText != null)
            {
                _descriptionText.text = _currentNode.Description;
            }

            // Status
            if (_statusText != null && _manager != null)
            {
                _statusText.text = GetStatusText();
            }

            // Prerequisites
            if (_prerequisitesText != null)
            {
                _prerequisitesText.text = GetPrerequisitesText();
            }

            // Requirements
            if (_requirementsText != null)
            {
                _requirementsText.text = GetRequirementsText();
            }

            // Costs
            if (_costsText != null)
            {
                _costsText.text = GetCostsText();
            }

            // Force rebuild layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
        }

        private string GetStatusText()
        {
            if (_manager == null) return "";

            bool isUnlocked = _manager.IsNodeUnlocked(_currentNode.Id);
            bool canUnlock = _manager.CanUnlockNode(_currentNode.Id);

            if (isUnlocked)
            {
                return "<color=#00FF00>UNLOCKED</color>";
            }
            else if (canUnlock)
            {
                return "<color=#00AAFF>AVAILABLE</color>";
            }
            else
            {
                return "<color=#888888>LOCKED</color>";
            }
        }

        private string GetPrerequisitesText()
        {
            if (!_currentNode.HasPrerequisites())
            {
                return "";
            }

            var sb = new StringBuilder();
            sb.AppendLine("<b>Prerequisites:</b>");

            foreach (var prerequisiteId in _currentNode.PrerequisiteNodeIds)
            {
                if (_manager != null)
                {
                    var prerequisiteNode = _manager.GetNode(prerequisiteId);
                    bool isUnlocked = _manager.IsNodeUnlocked(prerequisiteId);

                    string status = isUnlocked ? "<color=#00FF00>✓</color>" : "<color=#FF0000>✗</color>";
                    string nodeName = prerequisiteNode?.DisplayName ?? prerequisiteId;

                    sb.AppendLine($"  {status} {nodeName}");
                }
            }

            return sb.ToString();
        }

        private string GetRequirementsText()
        {
            if (!_currentNode.HasRequirements())
            {
                return "";
            }

            var sb = new StringBuilder();
            sb.AppendLine("<b>Requirements:</b>");

            foreach (var requirement in _currentNode.Requirements)
            {
                string reqText = FormatRequirement(requirement);
                if (!string.IsNullOrEmpty(reqText))
                {
                    sb.AppendLine($"  • {reqText}");
                }
            }

            return sb.ToString();
        }

        private string FormatRequirement(SkillRequirement requirement)
        {
            switch (requirement.Type)
            {
                case RequirementType.NodeUnlocked:
                    return $"Unlock {requirement.RequiredNodeIds.Count} node(s)";
                case RequirementType.LevelCheck:
                    if (requirement.Parameters.TryGetValue("Level", out string level))
                    {
                        return $"Level {level}+";
                    }
                    return "Level requirement";
                case RequirementType.StatCheck:
                    return "Stat requirement";
                case RequirementType.Custom:
                    return requirement.CustomCondition ?? "Custom requirement";
                default:
                    return "Unknown requirement";
            }
        }

        private string GetCostsText()
        {
            if (!_currentNode.HasCosts())
            {
                return "";
            }

            var sb = new StringBuilder();
            sb.AppendLine("<b>Cost:</b>");

            foreach (var cost in _currentNode.Costs)
            {
                bool canAfford = false;
                if (_manager != null)
                {
                    float currentAmount = _manager.GetResource(cost.ResourceType);
                    canAfford = currentAmount >= cost.Amount;
                }

                string affordableColor = canAfford ? "#00FF00" : "#FF0000";
                sb.AppendLine($"  • {cost.Amount} <color={affordableColor}>{cost.ResourceType}</color>");
            }

            return sb.ToString();
        }

        #endregion

        #region Positioning

        private void UpdatePosition()
        {
            if (_canvas == null || _canvasRectTransform == null || _container == null)
            {
                return;
            }

            // Get mouse position
            Vector2 mousePosition = Input.mousePosition;

            // Convert to canvas space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform,
                mousePosition,
                _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
                out Vector2 localPoint
            );

            // Apply offset
            Vector2 tooltipPosition = localPoint + _offset;

            // Clamp to canvas bounds
            tooltipPosition = ClampToCanvas(tooltipPosition);

            // Apply position
            _container.anchoredPosition = tooltipPosition;
        }

        private Vector2 ClampToCanvas(Vector2 position)
        {
            if (_canvasRectTransform == null || _container == null)
            {
                return position;
            }

            Vector2 canvasSize = _canvasRectTransform.rect.size;
            Vector2 tooltipSize = _container.rect.size;

            // Calculate bounds
            float minX = -canvasSize.x / 2f;
            float maxX = canvasSize.x / 2f - tooltipSize.x;
            float minY = -canvasSize.y / 2f;
            float maxY = canvasSize.y / 2f - tooltipSize.y;

            // Clamp position
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);

            return position;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the maximum width of the tooltip.
        /// </summary>
        public void SetMaxWidth(float width)
        {
            _maxWidth = width;
            if (_container != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
            }
        }

        /// <summary>
        /// Checks if the tooltip is currently visible.
        /// </summary>
        public bool IsVisible => _isVisible;

        #endregion
    }
}
