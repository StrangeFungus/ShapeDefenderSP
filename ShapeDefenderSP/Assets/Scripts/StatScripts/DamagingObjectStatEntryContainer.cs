using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

public class DamagingObjectStatEntryContainer
{
    [SerializeField] private StatDefaultTemplateSO defaultStatEntries;
    private Dictionary<StatName, StatEntry> statEntryDictionary = new();
    public Dictionary<StatName, StatEntry> StatEntryDictionary => statEntryDictionary;

    [SerializeField] private StatDefaultTemplateSO defaultStatReductionEntries;
    private Dictionary<StatName, StatEntry> statReductionEntryDictionary = new();
    public Dictionary<StatName, StatEntry> StatReductionEntryDictionary { get => statEntryDictionary; }

    private IStatEntryManager IStatEntryManager => InterfaceContainer.Request<IStatEntryManager>();

    private void InitializeStatEntryDict()
    {
        if (defaultStatEntries != null)
        {
            if (defaultStatEntries.StatEntries.Count > 0)
            {
                foreach (var entry in defaultStatEntries.StatEntries)
                {
                    AddStatEntry(entry, false);
                }
            }
        }

        if (defaultStatReductionEntries != null)
        {
            if (defaultStatReductionEntries.StatEntries.Count > 0)
            {
                foreach (var entry in defaultStatReductionEntries.StatEntries)
                {
                    AddStatEntry(entry, true);
                }
            }
        }
    }

    public void AddStatEntry(StatEntry newStatEntry, bool addToReductionEntries)
    {
        var dictionaryToAddTo = statEntryDictionary;
        if (addToReductionEntries)
        {
            dictionaryToAddTo = statReductionEntryDictionary;
        }

        if (!dictionaryToAddTo.ContainsKey(newStatEntry.StatsName))
        {
            dictionaryToAddTo.Add(newStatEntry.StatsName, new StatEntry(newStatEntry.StatsName, newStatEntry.BaseValue));
        }
        else
        {
            StatModificationAction actionToTake = IStatEntryManager.GetActionForLevelingUp(newStatEntry.StatsName);
            dictionaryToAddTo[newStatEntry.StatsName].ModifyBaseValue(actionToTake, newStatEntry.BaseValue);
        }
    }

    public void RemoveStatEntry(StatName statName, bool removeFromReductionEntries)
    {
        var dictionaryToRemoveFrom = statEntryDictionary;
        if (removeFromReductionEntries)
        {
            dictionaryToRemoveFrom = statReductionEntryDictionary;
        }

        if (dictionaryToRemoveFrom.ContainsKey(statName))
        {
            dictionaryToRemoveFrom.Remove(statName);
        }
    }

    public float GetStatsCurrentTotal(StatName statName, bool getFromReductionEntries)
    {
        var statEntryDictToPullFrom = statEntryDictionary;
        if (getFromReductionEntries)
        {
            statEntryDictToPullFrom = statReductionEntryDictionary;
        }

        if (statEntryDictToPullFrom.TryGetValue(statName, out var statEntry))
        {
            return statEntry.CurrentValueTotal;
        }
        else
        {
            return 0;
        }
    }

    public void CopyDamagingObjectStatEntryContainer(DamagingObjectStatEntryContainer statEntryContainer)
    {
        if (statEntryContainer != null)
        {
            foreach (var stat in statEntryContainer.statEntryDictionary)
            {
                AddStatEntry(stat.Value, false);
            }

            foreach (var stat in statEntryContainer.statReductionEntryDictionary)
            {
                AddStatEntry(stat.Value, true);
            }
        }
    }

    public void ResetDamagingObjectStatEntryContainer()
    {
        statEntryDictionary.Clear();
        statReductionEntryDictionary.Clear();

        InitializeStatEntryDict();
    }
}
