using SkillTree.Core.Data;
using UnityEngine;

namespace SkillTree.Extensions
{
    /// <summary>
    /// Interface for customizing visual appearance of nodes and connections.
    /// Implement this to add specialized visual effects based on node properties.
    /// </summary>
    /// <example>
    /// public class RarityVisualProvider : ISkillVisualProvider
    /// {
    ///     public string VisualProviderId => "Rarity";
    ///
    ///     public void CustomizeNodeVisual(GameObject nodeUI, SkillNode node, SkillNodeVisualState state)
    ///     {
    ///         if (node.CustomData.TryGetValue("Rarity", out string rarity))
    ///         {
    ///             var image = nodeUI.GetComponent<Image>();
    ///             image.color = rarity switch
    ///             {
    ///                 "Common" => Color.white,
    ///                 "Rare" => Color.blue,
    ///                 "Epic" => Color.magenta,
    ///                 "Legendary" => Color.yellow,
    ///                 _ => Color.gray
    ///             };
    ///         }
    ///     }
    ///
    ///     public void CustomizeConnectionVisual(GameObject connectionUI, SkillConnection connection)
    ///     {
    ///         // Customize connection appearance
    ///     }
    /// }
    /// </example>
    public interface ISkillVisualProvider
    {
        /// <summary>
        /// Unique identifier for this visual provider.
        /// </summary>
        string VisualProviderId { get; }

        /// <summary>
        /// Customizes the visual appearance of a skill node.
        /// Called when the node is created or its state changes.
        /// </summary>
        /// <param name="nodeUI">The GameObject representing the node in the UI</param>
        /// <param name="node">The skill node data</param>
        /// <param name="state">The current visual state of the node</param>
        void CustomizeNodeVisual(GameObject nodeUI, SkillNode node, SkillNodeVisualState state);

        /// <summary>
        /// Optional: Customizes the visual appearance of a connection between nodes.
        /// </summary>
        /// <param name="connectionUI">The GameObject representing the connection</param>
        /// <param name="connection">The connection data</param>
        void CustomizeConnectionVisual(GameObject connectionUI, SkillConnection connection) { }

        /// <summary>
        /// Optional: Returns a priority for this provider (higher priority runs first).
        /// Default is 0. Use higher values to override other providers.
        /// </summary>
        int Priority => 0;
    }

    /// <summary>
    /// Represents the current visual state of a skill node.
    /// </summary>
    public enum SkillNodeVisualState
    {
        /// <summary>Node is locked and prerequisites are not met</summary>
        Locked,

        /// <summary>Node is locked but prerequisites are met (can be unlocked)</summary>
        Available,

        /// <summary>Node is unlocked</summary>
        Unlocked,

        /// <summary>Node is highlighted (e.g., hovered, selected)</summary>
        Highlighted,

        /// <summary>Node is in an invalid state (e.g., validation error)</summary>
        Invalid
    }
}
