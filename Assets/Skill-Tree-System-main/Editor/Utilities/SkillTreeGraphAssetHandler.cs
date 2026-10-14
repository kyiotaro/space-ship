using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using SkillTree.Core.Data;

namespace SkillTree.Editor.Utilities
{
    /// <summary>
    /// Handles opening SkillTreeGraph assets by double-clicking them in the project browser.
    /// Opens the Skill Tree Editor window automatically.
    /// </summary>
    public class SkillTreeGraphAssetHandler
    {
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            // Get the asset that was double-clicked
            var asset = EditorUtility.InstanceIDToObject(instanceID);

            // Check if it's a SkillTreeGraph
            if (asset is SkillTreeGraph graph)
            {
                // Open the Skill Tree Editor window with this graph
                GraphView.SkillTreeGraphWindow.OpenWindow(graph);
                return true; // Return true to indicate we handled the open
            }

            return false; // Let Unity handle other asset types normally
        }
    }
}
