using System.Collections.Generic;
using System.Xml.Linq;
using SDSPEnums;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[System.Serializable]
public class EntityStatEntryContainer : BaseStatEntryContainer
{
    private class StatEntryModifier
    {
        public float modifyingValue;
        public float modifyingPercent;
    }
    private Dictionary<StatusEffectName, Dictionary<int, StatEntryModifier>> statEntryModifiers = new();

    public void InitializeEntityStatEntryContainer()
    {
        InitializeStatEntryDict();

        if (statEntryDictionary.TryGetValue(StatName.MaxHealthPointsValue, out var maxHpEntry))
        {
            if (!statEntryDictionary.ContainsKey(StatName.CurrentHealthPointsValue))
            {
                AddStatEntry(new StatEntry(StatName.CurrentHealthPointsValue, maxHpEntry.BaseValue));
            }
        }
    }

    public void AddNewModifier(StatusEffectName statusEffectsName, float modifyingValue, float modifyingPercent)
    {
        if (!statEntryModifiers.ContainsKey(statusEffectsName))
        {
            statEntryModifiers.Add(statusEffectsName, new());
            statEntryModifiers[statusEffectsName].Add(1, new());
            statEntryModifiers[statusEffectsName][1].modifyingValue = modifyingValue;
            statEntryModifiers[statusEffectsName][1].modifyingPercent = modifyingPercent;
        }
        else
        {
            int stackNumber = statEntryModifiers[statusEffectsName].Count + 1;
            statEntryModifiers[statusEffectsName].Add(stackNumber, new());
            statEntryModifiers[statusEffectsName][stackNumber].modifyingValue = modifyingValue;
            statEntryModifiers[statusEffectsName][stackNumber].modifyingPercent = modifyingPercent;
        }
    }

    public void RemoveModifierStatusEffectStack(StatusEffectName statusEffectsName, int stacksNumber)
    {
        if (statEntryModifiers.ContainsKey(statusEffectsName))
        {
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
                foreach (var stack in statusEffectEntry.Value)
                {
                    AddNewModifier(statusEffectEntry.Key, stack.Value.modifyingValue, stack.Value.modifyingPercent);
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
