using System;
using System.Collections.Generic;
using System.Linq;
using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using UnityEngine;

namespace SkillTree.Extensions
{
    /// <summary>
    /// Central registry for managing skill tree extensions.
    /// Register custom implementations of extension interfaces here.
    /// </summary>
    public class SkillTreeExtensionRegistry
    {
        private static SkillTreeExtensionRegistry _instance;
        public static SkillTreeExtensionRegistry Instance => _instance ??= new SkillTreeExtensionRegistry();

        private readonly Dictionary<string, ISkillNodeExtension> _nodeExtensions = new Dictionary<string, ISkillNodeExtension>();
        private readonly Dictionary<string, IConditionEvaluator> _conditionEvaluators = new Dictionary<string, IConditionEvaluator>();
        private readonly Dictionary<string, ISkillCostProvider> _costProviders = new Dictionary<string, ISkillCostProvider>();
        private readonly Dictionary<string, ISkillVisualProvider> _visualProviders = new Dictionary<string, ISkillVisualProvider>();

        private SkillTreeExtensionRegistry() { }

        #region Registration Methods

        /// <summary>
        /// Registers a node extension.
        /// </summary>
        public void RegisterNodeExtension(ISkillNodeExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            if (_nodeExtensions.ContainsKey(extension.NodeTypeId))
            {
                Debug.LogWarning($"Node extension '{extension.NodeTypeId}' is already registered. Overwriting.");
            }

            _nodeExtensions[extension.NodeTypeId] = extension;
        }

        /// <summary>
        /// Registers a condition evaluator.
        /// </summary>
        public void RegisterConditionEvaluator(IConditionEvaluator evaluator)
        {
            if (evaluator == null)
                throw new ArgumentNullException(nameof(evaluator));

            if (_conditionEvaluators.ContainsKey(evaluator.ConditionType))
            {
                Debug.LogWarning($"Condition evaluator '{evaluator.ConditionType}' is already registered. Overwriting.");
            }

            _conditionEvaluators[evaluator.ConditionType] = evaluator;
        }

        /// <summary>
        /// Registers a cost provider.
        /// </summary>
        public void RegisterCostProvider(ISkillCostProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (_costProviders.ContainsKey(provider.CostType))
            {
                Debug.LogWarning($"Cost provider '{provider.CostType}' is already registered. Overwriting.");
            }

            _costProviders[provider.CostType] = provider;
        }

        /// <summary>
        /// Registers a visual provider.
        /// </summary>
        public void RegisterVisualProvider(ISkillVisualProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (_visualProviders.ContainsKey(provider.VisualProviderId))
            {
                Debug.LogWarning($"Visual provider '{provider.VisualProviderId}' is already registered. Overwriting.");
            }

            _visualProviders[provider.VisualProviderId] = provider;
        }

        #endregion

        #region Retrieval Methods

        /// <summary>
        /// Gets a node extension by type ID.
        /// </summary>
        public ISkillNodeExtension GetNodeExtension(string nodeTypeId)
        {
            _nodeExtensions.TryGetValue(nodeTypeId, out var extension);
            return extension;
        }

        /// <summary>
        /// Gets a condition evaluator by type.
        /// </summary>
        public IConditionEvaluator GetConditionEvaluator(string conditionType)
        {
            _conditionEvaluators.TryGetValue(conditionType, out var evaluator);
            return evaluator;
        }

        /// <summary>
        /// Gets a cost provider by type.
        /// </summary>
        public ISkillCostProvider GetCostProvider(string costType)
        {
            _costProviders.TryGetValue(costType, out var provider);
            return provider;
        }

        /// <summary>
        /// Gets a visual provider by ID.
        /// </summary>
        public ISkillVisualProvider GetVisualProvider(string providerId)
        {
            _visualProviders.TryGetValue(providerId, out var provider);
            return provider;
        }

        /// <summary>
        /// Gets all registered visual providers sorted by priority (highest first).
        /// </summary>
        public IEnumerable<ISkillVisualProvider> GetAllVisualProviders()
        {
            return _visualProviders.Values.OrderByDescending(p => p.Priority);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Tries to invoke a node extension's unlock method.
        /// Returns true if the extension was found and executed.
        /// </summary>
        public bool TryInvokeNodeUnlocked(SkillNode node, ISkillTreeState state)
        {
            if (node.CustomData.TryGetValue("NodeExtensionType", out string extensionType))
            {
                var extension = GetNodeExtension(extensionType);
                if (extension != null)
                {
                    try
                    {
                        extension.OnNodeUnlocked(node, state);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error invoking node extension '{extensionType}' for node '{node.Id}': {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Node extension '{extensionType}' not found for node '{node.Id}'");
                }
            }
            return false;
        }

        /// <summary>
        /// Tries to invoke a node extension's lock method.
        /// Returns true if the extension was found and executed.
        /// </summary>
        public bool TryInvokeNodeLocked(SkillNode node, ISkillTreeState state)
        {
            if (node.CustomData.TryGetValue("NodeExtensionType", out string extensionType))
            {
                var extension = GetNodeExtension(extensionType);
                if (extension != null)
                {
                    try
                    {
                        extension.OnNodeLocked(node, state);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error invoking node extension '{extensionType}' for node '{node.Id}': {ex.Message}");
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Validates a custom requirement using registered evaluators.
        /// </summary>
        public bool EvaluateCustomCondition(SkillRequirement requirement, ISkillTreeState state, object context = null)
        {
            if (string.IsNullOrEmpty(requirement.CustomCondition))
                return true;

            var evaluator = GetConditionEvaluator(requirement.CustomCondition);
            if (evaluator != null)
            {
                try
                {
                    return evaluator.Evaluate(requirement, state, context);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error evaluating condition '{requirement.CustomCondition}': {ex.Message}");
                    return false;
                }
            }

            Debug.LogWarning($"Condition evaluator '{requirement.CustomCondition}' not found");
            return false;
        }

        /// <summary>
        /// Checks if a cost can be afforded using registered providers.
        /// </summary>
        public bool CanAffordCustomCost(SkillCost cost, ISkillTreeState state, object context = null)
        {
            if (cost.CustomData.TryGetValue("CostProvider", out string providerType))
            {
                var provider = GetCostProvider(providerType);
                if (provider != null)
                {
                    try
                    {
                        return provider.CanAfford(cost, state, context);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error checking cost with provider '{providerType}': {ex.Message}");
                        return false;
                    }
                }

                Debug.LogWarning($"Cost provider '{providerType}' not found");
            }

            return false;
        }

        /// <summary>
        /// Clears all registered extensions. Useful for testing.
        /// </summary>
        public void Clear()
        {
            _nodeExtensions.Clear();
            _conditionEvaluators.Clear();
            _costProviders.Clear();
            _visualProviders.Clear();
        }

        #endregion
    }
}
