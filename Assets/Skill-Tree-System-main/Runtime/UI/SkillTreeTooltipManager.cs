using UnityEngine;
using SkillTree.Core.Logic;

namespace SkillTree.UI
{
    /// <summary>
    /// Manages tooltip display for skill tree nodes.
    /// Automatically subscribes to node hover events and shows/hides tooltips.
    /// </summary>
    public class SkillTreeTooltipManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkillTreeUI _skillTreeUI;
        [SerializeField] private SkillTooltipUI _tooltipUI;

        [Header("Settings")]
        [SerializeField] private bool _enableTooltips = true;

        private ISkillTreeManager _manager;

        #region Unity Lifecycle

        private void Start()
        {
            if (_skillTreeUI != null)
            {
                _manager = _skillTreeUI.Manager;
                SubscribeToNodes();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromNodes();
        }

        #endregion

        #region Node Subscription

        /// <summary>
        /// Subscribes to hover events for all existing nodes.
        /// </summary>
        private void SubscribeToNodes()
        {
            if (_skillTreeUI == null) return;

            foreach (var nodeUI in _skillTreeUI.GetAllNodeUIs())
            {
                SubscribeToNode(nodeUI);
            }
        }

        /// <summary>
        /// Unsubscribes from all node hover events.
        /// </summary>
        private void UnsubscribeFromNodes()
        {
            if (_skillTreeUI == null) return;

            foreach (var nodeUI in _skillTreeUI.GetAllNodeUIs())
            {
                UnsubscribeFromNode(nodeUI);
            }
        }

        /// <summary>
        /// Subscribes to a specific node's hover events.
        /// </summary>
        private void SubscribeToNode(SkillNodeUI nodeUI)
        {
            if (nodeUI == null) return;

            nodeUI.OnHoverEnter += HandleNodeHoverEnter;
            nodeUI.OnHoverExit += HandleNodeHoverExit;
        }

        /// <summary>
        /// Unsubscribes from a specific node's hover events.
        /// </summary>
        private void UnsubscribeFromNode(SkillNodeUI nodeUI)
        {
            if (nodeUI == null) return;

            nodeUI.OnHoverEnter -= HandleNodeHoverEnter;
            nodeUI.OnHoverExit -= HandleNodeHoverExit;
        }

        #endregion

        #region Event Handlers

        private void HandleNodeHoverEnter(SkillNodeUI nodeUI)
        {
            if (!_enableTooltips || _tooltipUI == null || nodeUI?.Node == null) return;

            _tooltipUI.Show(nodeUI.Node, _manager, Input.mousePosition);
        }

        private void HandleNodeHoverExit(SkillNodeUI nodeUI)
        {
            if (_tooltipUI == null) return;

            _tooltipUI.Hide();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enables or disables tooltips.
        /// </summary>
        public void SetTooltipsEnabled(bool enabled)
        {
            _enableTooltips = enabled;

            if (!enabled && _tooltipUI != null)
            {
                _tooltipUI.Hide(immediate: true);
            }
        }

        /// <summary>
        /// Manually refreshes node subscriptions (call after adding new nodes).
        /// </summary>
        public void RefreshSubscriptions()
        {
            UnsubscribeFromNodes();
            SubscribeToNodes();
        }

        /// <summary>
        /// Sets the skill tree UI reference.
        /// </summary>
        public void SetSkillTreeUI(SkillTreeUI treeUI)
        {
            if (_skillTreeUI == treeUI) return;

            UnsubscribeFromNodes();
            _skillTreeUI = treeUI;
            _manager = treeUI?.Manager;
            SubscribeToNodes();
        }

        /// <summary>
        /// Sets the tooltip UI reference.
        /// </summary>
        public void SetTooltipUI(SkillTooltipUI tooltipUI)
        {
            _tooltipUI = tooltipUI;
        }

        #endregion
    }
}
