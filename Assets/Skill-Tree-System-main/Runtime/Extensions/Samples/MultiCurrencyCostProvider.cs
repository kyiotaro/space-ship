using SkillTree.Core.Data;
using SkillTree.Core.Logic;
using UnityEngine;

namespace SkillTree.Extensions.Samples
{
    /// <summary>
    /// Sample cost provider that handles multiple currencies.
    /// This is a reference implementation showing how to create custom cost systems.
    /// </summary>
    /// <remarks>
    /// To use this provider:
    /// 1. Register it: SkillTreeExtensionRegistry.Instance.RegisterCostProvider(new MultiCurrencyCostProvider());
    /// 2. Create a SkillCost
    /// 3. Set cost.CustomData["CostProvider"] = "MultiCurrency"
    /// 4. Set cost.CustomData["Gold"] = "50"
    /// 5. Set cost.CustomData["Gems"] = "10"
    /// 6. (Optional) Add more currencies as needed
    /// </remarks>
    public class MultiCurrencyCostProvider : ISkillCostProvider
    {
        public string CostType => "MultiCurrency";

        public bool CanAfford(SkillCost cost, ISkillTreeState state, object context = null)
        {
            // Check each currency defined in the cost
            foreach (var currency in GetCurrencies(cost))
            {
                if (!state.HasResource(currency.Key, currency.Value))
                {
                    Debug.Log($"[MultiCurrency] Insufficient {currency.Key}: has {state.GetResource(currency.Key)}, needs {currency.Value}");
                    return false;
                }
            }

            return true;
        }

        public void PayCost(SkillCost cost, object state, object context = null)
        {
            // Cast to mutable state (SkillTreeState)
            if (state is SkillTreeState skillTreeState)
            {
                foreach (var currency in GetCurrencies(cost))
                {
                    float currentAmount = skillTreeState.GetResource(currency.Key);
                    skillTreeState.SetResource(currency.Key, currentAmount - currency.Value);
                    Debug.Log($"[MultiCurrency] Paid {currency.Value} {currency.Key}");
                }
            }
            else
            {
                Debug.LogError("[MultiCurrency] State must be of type SkillTreeState for PayCost");
            }
        }

        public string GetCostDescription(SkillCost cost)
        {
            var currencies = GetCurrencies(cost);
            if (currencies.Count == 0)
                return "Free";

            var parts = new System.Collections.Generic.List<string>();
            foreach (var currency in currencies)
            {
                parts.Add($"{currency.Value} {currency.Key}");
            }

            return string.Join(", ", parts);
        }

        public void RefundCost(SkillCost cost, object state, object context = null)
        {
            // Refund when node is locked (respec)
            if (state is SkillTreeState skillTreeState)
            {
                foreach (var currency in GetCurrencies(cost))
                {
                    float currentAmount = skillTreeState.GetResource(currency.Key);
                    skillTreeState.SetResource(currency.Key, currentAmount + currency.Value);
                    Debug.Log($"[MultiCurrency] Refunded {currency.Value} {currency.Key}");
                }
            }
        }

        private System.Collections.Generic.Dictionary<string, float> GetCurrencies(SkillCost cost)
        {
            var currencies = new System.Collections.Generic.Dictionary<string, float>();

            // Known currency types
            string[] knownCurrencies = { "Gold", "Gems", "Tokens", "Honor", "Essence", "Dust" };

            foreach (var currencyType in knownCurrencies)
            {
                if (cost.CustomData.TryGetValue(currencyType, out string amountStr))
                {
                    if (float.TryParse(amountStr, out float amount) && amount > 0)
                    {
                        currencies[currencyType] = amount;
                    }
                }
            }

            return currencies;
        }
    }

    /// <summary>
    /// Simple cost provider for experience points.
    /// Shows how to create a single-currency custom provider.
    /// </summary>
    public class ExperienceCostProvider : ISkillCostProvider
    {
        public string CostType => "Experience";

        public bool CanAfford(SkillCost cost, ISkillTreeState state, object context = null)
        {
            if (cost.CustomData.TryGetValue("Experience", out string expStr))
            {
                if (float.TryParse(expStr, out float requiredExp))
                {
                    return state.HasResource("Experience", requiredExp);
                }
            }

            Debug.LogWarning("[Experience] Missing or invalid 'Experience' value in cost data");
            return false;
        }

        public void PayCost(SkillCost cost, object state, object context = null)
        {
            if (state is SkillTreeState skillTreeState)
            {
                if (cost.CustomData.TryGetValue("Experience", out string expStr))
                {
                    if (float.TryParse(expStr, out float requiredExp))
                    {
                        float currentExp = skillTreeState.GetResource("Experience");
                        skillTreeState.SetResource("Experience", currentExp - requiredExp);
                        Debug.Log($"[Experience] Spent {requiredExp} experience");
                    }
                }
            }
        }

        public string GetCostDescription(SkillCost cost)
        {
            if (cost.CustomData.TryGetValue("Experience", out string expStr))
            {
                return $"{expStr} Experience";
            }
            return "Unknown cost";
        }

        public void RefundCost(SkillCost cost, object state, object context = null)
        {
            if (state is SkillTreeState skillTreeState)
            {
                if (cost.CustomData.TryGetValue("Experience", out string expStr))
                {
                    if (float.TryParse(expStr, out float requiredExp))
                    {
                        float currentExp = skillTreeState.GetResource("Experience");
                        skillTreeState.SetResource("Experience", currentExp + requiredExp);
                        Debug.Log($"[Experience] Refunded {requiredExp} experience");
                    }
                }
            }
        }
    }
}
