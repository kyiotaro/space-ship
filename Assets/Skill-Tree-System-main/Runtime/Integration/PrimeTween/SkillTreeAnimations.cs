using UnityEngine;
using PrimeTween;
using SkillTree.UI;

namespace SkillTree.Integration.PrimeTween
{
    /// <summary>
    /// Collection of predefined PrimeTween animations for skill tree UI.
    /// Provides consistent animation styles across the system.
    /// </summary>
    public static class SkillTreeAnimations
    {
        #region Node Animations

        /// <summary>
        /// Plays an unlock animation on a node transform.
        /// </summary>
        public static Sequence NodeUnlock(Transform nodeTransform)
        {
            return Sequence.Create()
                .Chain(Tween.Scale(nodeTransform, 1.3f, 0.2f, Ease.OutQuad))
                .Chain(Tween.Scale(nodeTransform, 1f, 0.2f, Ease.InOutQuad))
                .Group(Tween.PunchLocalRotation(nodeTransform, new Vector3(0, 0, 10f), 0.4f));
        }

        /// <summary>
        /// Plays a pulse animation (breathing effect) on a node.
        /// </summary>
        public static Tween NodePulse(Transform nodeTransform, float intensity = 0.1f, float duration = 1f)
        {
            float targetScale = 1f + intensity;
            return Tween.Scale(nodeTransform, targetScale, duration / 2f, cycles: -1, cycleMode: CycleMode.Yoyo);
        }

        /// <summary>
        /// Plays a shake animation on a locked node.
        /// </summary>
        public static Tween NodeShake(Transform nodeTransform, float intensity = 10f)
        {
            return Tween.ShakeLocalPosition(nodeTransform, new Vector3(intensity, 0f, 0f), 0.3f, frequency: 20);
        }

        /// <summary>
        /// Plays a glow animation on a node (scale + fade).
        /// </summary>
        public static Sequence NodeGlow(Transform nodeTransform, CanvasGroup canvasGroup, float duration = 0.5f)
        {
            float originalAlpha = canvasGroup.alpha;

            return Sequence.Create()
                .Chain(Tween.Scale(nodeTransform, 1.2f, duration / 2f, Ease.OutQuad))
                .Group(Tween.Alpha(canvasGroup, 0.7f, duration / 2f))
                .Chain(Tween.Scale(nodeTransform, 1f, duration / 2f, Ease.InQuad))
                .Group(Tween.Alpha(canvasGroup, originalAlpha, duration / 2f));
        }

        /// <summary>
        /// Plays a bounce-in animation for newly created nodes.
        /// </summary>
        public static Sequence NodeBounceIn(Transform nodeTransform)
        {
            nodeTransform.localScale = Vector3.zero;

            return Sequence.Create()
                .Chain(Tween.Scale(nodeTransform, 1.1f, 0.3f, Ease.OutBack))
                .Chain(Tween.Scale(nodeTransform, 1f, 0.1f, Ease.InOutQuad));
        }

        #endregion

        #region Connection Animations

        /// <summary>
        /// Plays a flow animation along a connection line.
        /// </summary>
        public static Tween ConnectionFlow(UnityEngine.UI.Image lineImage, float duration = 1f)
        {
            Color originalColor = lineImage.color;
            Color brightColor = Color.Lerp(originalColor, Color.white, 0.5f);

            return Tween.Color(lineImage, brightColor, duration / 2f, Ease.InOutSine)
                .OnComplete(() => Tween.Color(lineImage, originalColor, duration / 2f, Ease.InOutSine));
        }

        /// <summary>
        /// Plays a pulse animation on a connection.
        /// </summary>
        public static Tween ConnectionPulse(UnityEngine.UI.Image lineImage, float intensity = 0.3f, float duration = 1f)
        {
            Color originalColor = lineImage.color;
            Color targetColor = Color.Lerp(originalColor, Color.white, intensity);

            return Tween.Color(lineImage, targetColor, duration / 2f, cycles: -1, cycleMode: CycleMode.Yoyo);
        }

        #endregion

        #region UI Element Animations

        /// <summary>
        /// Fades in a UI element.
        /// </summary>
        public static Tween FadeIn(CanvasGroup canvasGroup, float duration = 0.3f)
        {
            canvasGroup.alpha = 0f;
            return Tween.Alpha(canvasGroup, 1f, duration, Ease.OutCubic);
        }

        /// <summary>
        /// Fades out a UI element.
        /// </summary>
        public static Tween FadeOut(CanvasGroup canvasGroup, float duration = 0.3f)
        {
            return Tween.Alpha(canvasGroup, 0f, duration, Ease.InCubic);
        }

        /// <summary>
        /// Slides in a UI element from the side.
        /// </summary>
        public static Sequence SlideIn(RectTransform rectTransform, Vector2 fromOffset, float duration = 0.3f)
        {
            Vector2 originalPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = originalPosition + fromOffset;

            return Sequence.Create()
                .Chain(Tween.UIAnchoredPosition(rectTransform, originalPosition, duration, Ease.OutCubic));
        }

        /// <summary>
        /// Plays a button click animation (punch scale).
        /// </summary>
        public static Tween ButtonClick(Transform buttonTransform)
        {
            return Tween.PunchScale(buttonTransform, Vector3.one * 0.1f, 0.2f);
        }

        #endregion

        #region Tooltip Animations

        /// <summary>
        /// Plays a tooltip show animation.
        /// </summary>
        public static Sequence TooltipShow(Transform tooltipTransform, CanvasGroup canvasGroup, float delay = 0.3f)
        {
            tooltipTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;

            return Sequence.Create()
                .ChainDelay(delay)
                .Chain(Tween.Scale(tooltipTransform, 1f, 0.2f, Ease.OutBack))
                .Group(Tween.Alpha(canvasGroup, 1f, 0.2f, Ease.OutCubic));
        }

        /// <summary>
        /// Plays a tooltip hide animation.
        /// </summary>
        public static Sequence TooltipHide(Transform tooltipTransform, CanvasGroup canvasGroup)
        {
            return Sequence.Create()
                .Chain(Tween.Scale(tooltipTransform, 0f, 0.15f, Ease.InBack))
                .Group(Tween.Alpha(canvasGroup, 0f, 0.15f, Ease.InCubic));
        }

        #endregion

        #region Skill Tree Animations

        /// <summary>
        /// Plays an animation when the entire tree is reset.
        /// </summary>
        public static void TreeReset(SkillTreeUI treeUI)
        {
            float delay = 0f;

            foreach (var nodeUI in treeUI.GetAllNodeUIs())
            {
                Sequence.Create()
                    .ChainDelay(delay)
                    .Chain(Tween.Scale(nodeUI.transform, 0f, 0.2f, Ease.InBack))
                    .Chain(Tween.Scale(nodeUI.transform, 1f, 0.3f, Ease.OutBack));

                delay += 0.02f; // Stagger effect
            }
        }

        /// <summary>
        /// Plays a cascade unlock animation (unlocks propagate visually).
        /// </summary>
        public static void CascadeUnlock(SkillTreeUI treeUI, string startNodeId)
        {
            // Future enhancement: animate unlock propagating through dependencies
            // For now, just pulse the unlocked node
            var nodeUI = treeUI.GetNodeUI(startNodeId);
            if (nodeUI != null)
            {
                NodeUnlock(nodeUI.transform);
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Creates a staggered sequence for animating multiple elements.
        /// </summary>
        public static void StaggeredAnimation(System.Collections.Generic.IEnumerable<Transform> transforms,
            System.Func<Transform, Tween> animationFactory, float staggerDelay = 0.05f)
        {
            float delay = 0f;

            foreach (var transform in transforms)
            {
                Sequence.Create()
                    .ChainDelay(delay)
                    .Chain(animationFactory(transform));

                delay += staggerDelay;
            }
        }

        #endregion
    }
}
