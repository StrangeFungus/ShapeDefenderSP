using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class StatEntry
{
    [SerializeField] private StatName statsName = StatName.Default;
    public StatName StatsName => statsName;

    [SerializeField] private float baseValue = 0f;
    public float BaseValue => baseValue;

    private float startingBaseValue = 0f;
    public float StartingBaseValue => startingBaseValue;

    private Dictionary<StatusEffectName, Dictionary<int, StatEntryModifier>> statEntryModifiers = new();
    public IReadOnlyDictionary<StatusEffectName, Dictionary<int, StatEntryModifier>> StatEntryModifiers => statEntryModifiers;

    private bool isStatsTotalValueDirty = true;
    private float cachedStatTotal;

    public float StatsTotalValue
    {
        get
        {
            if (isStatsTotalValueDirty)
            {
                cachedStatTotal = RecalculateStatsTotalValue();
                isStatsTotalValueDirty = false;
            }

            return cachedStatTotal;
        }
    }

    public StatEntry(StatName StatsName, float BaseValue)
    {
        statsName = StatsName;
        baseValue = BaseValue;
        startingBaseValue = BaseValue;

        if (StatsName == StatName.Default)
        {
            Debug.Log($"Added a stat with the stats name of Default.");
        }
    }

    private float RecalculateStatsTotalValue()
    {
        float totalModifyingValue = 0f;
        float totalModifyingPercent = 0f;

        foreach (var statusEffect in statEntryModifiers)
        {
            foreach (var stack in statusEffect.Value)
            {
                totalModifyingValue += stack.Value.ModifyingValue;
                totalModifyingPercent += stack.Value.ModifyingPercent;
            }
        }

        return (baseValue + totalModifyingValue) * (1f + (totalModifyingPercent / 100f));
    }

    public void ModifyBaseValue(StatModificationAction statModificationAction, float? incomingBaseValue = null)
    {
        if (incomingBaseValue.HasValue)
        {
            switch (statModificationAction)
            {
                case StatModificationAction.AddToValue:
                    baseValue += incomingBaseValue.Value;
                    break;
                case StatModificationAction.SubtractFromValue:
                    baseValue -= incomingBaseValue.Value;
                    break;
                case StatModificationAction.SetValueTo:
                    baseValue = incomingBaseValue.Value;
                    break;
                default:
                    break;
            }

            isStatsTotalValueDirty = true;
        }
    }

    public void AddNewModifier(StatusEffectName statusEffectsName, int stacksNumber, StatEntryModifier newStatEntryModifier)
    {
        if (!statEntryModifiers.ContainsKey(statusEffectsName))
        {
            statEntryModifiers.Add(statusEffectsName, new());
        }

        if (!statEntryModifiers[statusEffectsName].ContainsKey(stacksNumber))
        {
            statEntryModifiers[statusEffectsName].Add(stacksNumber, newStatEntryModifier);
        }

        isStatsTotalValueDirty = true;
    }
    
    public void RemoveModifierStatusEffect(StatusEffectName statusEffectsName)
    {
        if (statEntryModifiers.ContainsKey(statusEffectsName))
        {
            statEntryModifiers.Remove(statusEffectsName);
        }

        isStatsTotalValueDirty = true;
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

        if (statEntryModifiers[statusEffectsName].Count == 0)
        {
            RemoveModifierStatusEffect(statusEffectsName);
        }
        else
        {
            isStatsTotalValueDirty = true;
        }
    }

    public void ResetStatToStartingValues()
    {
        baseValue = startingBaseValue;
        statEntryModifiers.Clear();

        isStatsTotalValueDirty = true;
    }

    public static StatEntry CopyStatEntry(StatEntry statEntryToCopy, StatName? alterStatsName = null)
    {
        if (statEntryToCopy != null)
        {
            StatName statsNameToCopy = statEntryToCopy.StatsName;
            if (alterStatsName != null) { statsNameToCopy = (StatName)alterStatsName; }

            StatEntry newStatEntry = new StatEntry(statsNameToCopy, statEntryToCopy.baseValue);
            foreach (var entry in statEntryToCopy.statEntryModifiers)
            {
                foreach (var entryStack in entry.Value)
                {
                    newStatEntry.AddNewModifier(entry.Key, entryStack.Key, entryStack.Value);
                }
            }

            return newStatEntry;
        }

        Debug.LogWarning($"CopyStatEntry's stat entry to copy was null.");
        return new StatEntry(StatName.Default, 0.0f);
    }
}
