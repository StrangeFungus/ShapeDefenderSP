using System.Collections.Generic;
using System.Xml.Linq;
using SDSPEnums;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[System.Serializable]
public class EntityStatEntryContainer
{
    [SerializeField] private StatDefaultTemplateSO defaultStatEntries;
    private Dictionary<StatName, StatEntry> statEntryDictionary = new();
    public Dictionary<StatName, StatEntry> StatEntryDictionary => statEntryDictionary;

    private class StatEntryModifier
    {
        public float modifyingValue;
        public float modifyingPercent;
    }
    private Dictionary<StatusEffectName, Dictionary<int, Dictionary<StatName, StatEntryModifier>>> statEntryModifiers = new();

    private IStatEntryManager IStatEntryManager => InterfaceContainer.Request<IStatEntryManager>();

    public void InitializeStatEntryDict()
    {
        if (defaultStatEntries != null)
        {
            if (defaultStatEntries.StatEntries.Count > 0 && statEntryDictionary.Count == 0)
            {
                foreach (var entry in defaultStatEntries.StatEntries)
                {
                    AddStatEntry(entry);
                }
            }
        }

        if (statEntryDictionary.TryGetValue(StatName.MaxHealthPointsValue, out var maxHpEntry))
        {
            if (!statEntryDictionary.ContainsKey(StatName.CurrentHealthPointsValue))
            {
                AddStatEntry(new StatEntry(StatName.CurrentHealthPointsValue, maxHpEntry.BaseValue));
            }
        }
    }

    public void AddStatEntry(StatEntry newStatEntry)
    {
        if (!statEntryDictionary.ContainsKey(newStatEntry.StatsName))
        {
            statEntryDictionary.Add(newStatEntry.StatsName, new StatEntry(newStatEntry.StatsName, newStatEntry.BaseValue));
        }
        else
        {
            StatModificationAction actionToTake = IStatEntryManager.GetActionForLevelingUp(newStatEntry.StatsName);
            statEntryDictionary[newStatEntry.StatsName].ModifyBaseValue(actionToTake, newStatEntry.BaseValue);
        }
    }

    public void RemoveStatEntry(StatName statName)
    {
        if (statEntryDictionary.ContainsKey(statName))
        {
            statEntryDictionary.Remove(statName);
        }
    }

    public void AddNewModifier(StatusEffectName statusEffectsName, Dictionary<StatName, StatEntry> statReductionEntryDictionary)
    {
        int stackNumber = 1;
        if (!statEntryModifiers.ContainsKey(statusEffectsName))
        {
            statEntryModifiers.Add(statusEffectsName, new());
            statEntryModifiers[statusEffectsName].Add(stackNumber, new());
        }
        else
        {
            stackNumber = statEntryModifiers[statusEffectsName].Count + 1;
            statEntryModifiers[statusEffectsName].Add(stackNumber, new());
        }

        if (statReductionEntryDictionary != null)
        {
            foreach (var statReduction in statReductionEntryDictionary)
            {
                float reductionValue = 0;
                float reductionPercent = 0;

                if (statReduction.Key.ToString().EndsWith("Value"))
                {
                    reductionValue = statReduction.Value.CurrentValueTotal;
                }
                else if (statReduction.Key.ToString().EndsWith("Percent"))
                {
                    reductionPercent = statReduction.Value.CurrentValueTotal;
                }

                if (!statEntryModifiers[statusEffectsName][stackNumber].ContainsKey(statReduction.Key))
                {
                    statEntryModifiers[statusEffectsName][stackNumber].Add(statReduction.Key, new());
                }
                statEntryModifiers[statusEffectsName][stackNumber][statReduction.Key].modifyingValue = reductionValue;
                statEntryModifiers[statusEffectsName][stackNumber][statReduction.Key].modifyingPercent = reductionPercent;
            }
        }
    }

    public void RemoveModifierStatusEffectStack(StatusEffectName statusEffectsName, int stacksNumber)
    {
        if (statEntryModifiers.ContainsKey(statusEffectsName))
        {
            foreach (var statEntry in statEntryModifiers[statusEffectsName][stacksNumber])
            {
                if (statEntryDictionary.TryGetValue(statEntry.Key, out var currentStatEntry))
                {
                    currentStatEntry.ModifyingValueTotal -= statEntry.Value.modifyingValue;
                    currentStatEntry.ModifyingPercentTotal -= statEntry.Value.modifyingPercent;
                }
            }

            if (statEntryModifiers[statusEffectsName].ContainsKey(stacksNumber))
            {
                statEntryModifiers[statusEffectsName].Remove(stacksNumber);
            }
        }
        else
        {
            Debug.LogWarning($"The Stat Entry Modifier could not be removed! Issue: Does not contain the status effect!");
        }

        if (statEntryModifiers[statusEffectsName].Count == 0)
        {
            RemoveModifierStatusEffect(statusEffectsName);
        }
    }

    public void RemoveModifierStatusEffect(StatusEffectName statusEffectsName)
    {
        if (statEntryModifiers.ContainsKey(statusEffectsName))
        {
            statEntryModifiers.Remove(statusEffectsName);
        }
        else
        {
            Debug.LogWarning($"The Stat Entry Modifier could not be removed! Issue: Does not contain the status effect!");
        }
    }

    public void ApplyStatReductions(StatusEffectEntry statusEffectEntry)
    {
        if (statusEffectEntry != null)
        {
            AddNewModifier(statusEffectEntry.StatusEffectsName, statusEffectEntry.EffectsStats.StatReductionEntryDictionary);
        }
        else
        {
            Debug.Log($"The targets stat entry container count was null... returning...");
        }
    }

    public void RemoveStatReductions(StatusEffectEntry statusEffectEntry, int stackNumber)
    {
        if (statusEffectEntry != null)
        {
            RemoveModifierStatusEffectStack(statusEffectEntry.StatusEffectsName, stackNumber);
        }
        else
        {
            Debug.Log($"The targets stat entry container count was null... returning...");
        }
    }

    public void CopyEntityStatEntryContainer(EntityStatEntryContainer statEntryContainer)
    {
        if (statEntryContainer != null)
        {
            foreach (var stat in statEntryContainer.statEntryDictionary)
            {
                AddStatEntry(stat.Value);
            }

            foreach (var statusEffectEntry in statEntryContainer.statEntryModifiers)
            {
                if (!statEntryModifiers.ContainsKey(statusEffectEntry.Key))
                {
                    statEntryModifiers.Add(statusEffectEntry.Key, new());
                }

                foreach (var stack in statusEffectEntry.Value)
                {
                    statEntryModifiers[statusEffectEntry.Key].Add(stack.Key, new());

                    foreach (var statReductionModifier in stack.Value)
                    {
                        statEntryModifiers[statusEffectEntry.Key][stack.Key].Add(statReductionModifier.Key, new());
                        statEntryModifiers[statusEffectEntry.Key][stack.Key][statReductionModifier.Key].modifyingValue = statReductionModifier.Value.modifyingValue;
                        statEntryModifiers[statusEffectEntry.Key][stack.Key][statReductionModifier.Key].modifyingPercent = statReductionModifier.Value.modifyingPercent;
                    }
                }
            }
        }
    }

    public void ResetEntityStatEntryContainer()
    {
        statEntryDictionary.Clear();
        statEntryModifiers.Clear();

        InitializeStatEntryDict();
    }
}
