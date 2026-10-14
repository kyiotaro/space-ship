using SkillTree.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SkillTree.Extensions.Samples
{
    /// <summary>
    /// Sample visual provider that colors nodes based on rarity.
    /// This is a reference implementation showing how to create custom visual effects.
    /// </summary>
    /// <remarks>
    /// To use this provider:
    /// 1. Register it: SkillTreeExtensionRegistry.Instance.RegisterVisualProvider(new RarityVisualProvider());
    /// 2. Set node's CustomData["Rarity"] = "Common" (or "Rare", "Epic", "Legendary")
    /// 3. The node UI will automatically be colored based on rarity
    /// </remarks>
    public class RarityVisualProvider : ISkillVisualProvider
    {
        public string VisualProviderId => "Rarity";
        public int Priority => 0;

        private readonly Color _commonColor = new Color(0.8f, 0.8f, 0.8f); // Light gray
        private readonly Color _rareColor = new Color(0.3f, 0.6f, 1.0f);   // Blue
        private readonly Color _epicColor = new Color(0.8f, 0.3f, 1.0f);   // Purple
        private readonly Color _legendaryColor = new Color(1.0f, 0.8f, 0.2f); // Gold

        public void CustomizeNodeVisual(GameObject nodeUI, SkillNode node, SkillNodeVisualState state)
        {
            if (!node.CustomData.TryGetValue("Rarity", out string rarity))
                return;

            Color rarityColor = GetRarityColor(rarity);

            // Apply color to the node's Image component
            var image = nodeUI.GetComponent<Image>();
            if (image != null)
            {
                // Blend rarity color with state-based tint
                Color stateTint = GetStateTint(state);
                image.color = rarityColor * stateTint;
            }

            // Apply glow effect for higher rarities
            ApplyGlowEffect(nodeUI, rarity, state);

            // Update border thickness for higher rarities
            ApplyBorderEffect(nodeUI, rarity);
        }

        public void CustomizeConnectionVisual(GameObject connectionUI, SkillConnection connection)
        {
            // Optional: Color connections based on type
            var lineRenderer = connectionUI.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                Color connectionColor = connection.Type switch
                {
                    ConnectionType.Prerequisite => Color.white,
                    ConnectionType.Soft => new Color(0.7f, 0.7f, 0.7f, 0.5f),
                    ConnectionType.Exclusive => new Color(1.0f, 0.3f, 0.3f),
                    ConnectionType.Bidirectional => new Color(0.3f, 1.0f, 0.3f),
                    _ => Color.white
                };

                lineRenderer.startColor = connectionColor;
                lineRenderer.endColor = connectionColor;
            }
        }

        private Color GetRarityColor(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => _commonColor,
                "rare" => _rareColor,
                "epic" => _epicColor,
                "legendary" => _legendaryColor,
                _ => Color.white
            };
        }

        private Color GetStateTint(SkillNodeVisualState state)
        {
            return state switch
            {
                SkillNodeVisualState.Locked => new Color(0.5f, 0.5f, 0.5f, 0.6f),
                SkillNodeVisualState.Available => new Color(1.0f, 1.0f, 1.0f, 0.9f),
                SkillNodeVisualState.Unlocked => Color.white,
                SkillNodeVisualState.Highlighted => new Color(1.2f, 1.2f, 1.2f),
                SkillNodeVisualState.Invalid => new Color(1.0f, 0.3f, 0.3f),
                _ => Color.white
            };
        }

        private void ApplyGlowEffect(GameObject nodeUI, string rarity, SkillNodeVisualState state)
        {
            // Only apply glow for Epic and Legendary when available or unlocked
            if (state != SkillNodeVisualState.Available && state != SkillNodeVisualState.Unlocked)
                return;

            bool shouldGlow = rarity?.ToLower() switch
            {
                "epic" => true,
                "legendary" => true,
                _ => false
            };

            // Try to find a glow effect component (custom implementation)
            var glow = nodeUI.transform.Find("GlowEffect");
            if (glow != null)
            {
                glow.gameObject.SetActive(shouldGlow);
            }
        }

        private void ApplyBorderEffect(GameObject nodeUI, string rarity)
        {
            var border = nodeUI.transform.Find("Border");
            if (border != null)
            {
                var borderImage = border.GetComponent<Image>();
                if (borderImage != null)
                {
                    // Thicker border for higher rarities
                    float thickness = rarity?.ToLower() switch
                    {
                        "rare" => 2.0f,
                        "epic" => 3.0f,
                        "legendary" => 4.0f,
                        _ => 1.0f
                    };

                    // This assumes you have a way to set border thickness
                    // Implementation depends on your UI setup
                }
            }
        }
    }
}
