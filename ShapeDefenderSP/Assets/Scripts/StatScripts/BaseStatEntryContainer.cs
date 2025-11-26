using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

public class BaseStatEntryContainer
{
    [SerializeField] protected StatDefaultTemplateSO defaultStatEntries;
    protected Dictionary<StatName, StatEntry> statEntryDictionary = new();
    public IReadOnlyDictionary<StatName, StatEntry> StatEntryDictionary => statEntryDictionary;

    protected IStatEntryManager iStatEntryManager;

    protected void InitializeStatEntryDict()
    {
        iStatEntryManager ??= InterfaceContainer.Request<IStatEntryManager>();

        if (defaultStatEntries != null)
        {
            if (defaultStatEntries.StatEntries.Count > 0)
            {
                foreach (var entry in defaultStatEntries.StatEntries)
                {
                    AddStatEntry(entry);
                }
            }
        }
    }

    public void AddStatEntry(StatEntry newStatEntry, Dictionary<StatName, StatEntry> targetStatEntryDictionary = null)
    {
        Dictionary<StatName, StatEntry> targetDict = statEntryDictionary;

        if (targetStatEntryDictionary != null)
        {
            targetDict = targetStatEntryDictionary;
        }

        if (!targetDict.ContainsKey(newStatEntry.StatsName))
        {
            targetDict.Add(newStatEntry.StatsName, new StatEntry(newStatEntry.StatsName, newStatEntry.BaseValue));
        }
        else
        {
            StatModificationAction actionToTake = iStatEntryManager.GetHowToRemoveStatReductionsOrLevelUp(newStatEntry.StatsName);
            targetDict[newStatEntry.StatsName].ModifyBaseValue(actionToTake, newStatEntry.BaseValue);
        }
    }

    public void RemoveStatEntry(StatName statName, Dictionary<StatName, StatEntry> targetStatEntryDictionary = null)
    {
        Dictionary<StatName, StatEntry> targetDict = statEntryDictionary;

        if (targetStatEntryDictionary != null)
        {
            targetDict = targetStatEntryDictionary;
        }

        if (targetDict.ContainsKey(statName))
        {
            targetDict.Remove(statName);
        }
    }

    public float GetStatEntriesTotalValue(StatName statName)
    {
        if (statEntryDictionary.TryGetValue(statName, out var targetStatEntry))
        {
            return targetStatEntry.CurrentValueTotal;
        }
        else
        {
            return 0.0f;
        }
    }
}
