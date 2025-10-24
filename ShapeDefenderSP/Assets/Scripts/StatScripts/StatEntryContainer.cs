using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class StatEntryContainer
{
    [SerializeField] private StatDefaultTemplateSO defaultStatEntries;

    private Dictionary<StatName, StatEntry> statEntryDictionary = new();
    public IReadOnlyDictionary<StatName, StatEntry> StatEntryDictionary => statEntryDictionary;

    public void InitializeStatEntryDict()
    {
        if (defaultStatEntries != null)
        {
            if (defaultStatEntries.StatEntries.Count > 0)
            {
                foreach (var entry in defaultStatEntries.StatEntries)
                {
                    AddStatEntry(entry);
                }

                if (statEntryDictionary.TryGetValue(StatName.MaxHealthPointsValue, out var maxHpEntry))
                {
                    if (!statEntryDictionary.ContainsKey(StatName.CurrentHealthPointsValue))
                    {
                        StatEntry copiedEntry = StatEntry.CopyStatEntry(maxHpEntry, StatName.CurrentHealthPointsValue);

                        AddStatEntry(copiedEntry);
                    }
                }
            }
        }
    }

    public StatEntry GetStatEntry(StatName statName)
    {
        if (!statEntryDictionary.TryGetValue(statName, out var entry))
        {
            entry = new StatEntry(statName, 0.0f);
        }

        return entry;
    }

    public float GetStatEntriesTotalValue(StatName statName)
    {
        StatEntry statEntry = GetStatEntry(statName);

        if (statEntry == null)
        {
            return 0.0f;
        }
        else
        {
            return statEntry.StatsTotalValue;
        }        
    }

    public void AddStatEntry(StatEntry newStatEntry)
    {
        if (!statEntryDictionary.ContainsKey(newStatEntry.StatsName))
        {
            statEntryDictionary.Add(newStatEntry.StatsName, StatEntry.CopyStatEntry(newStatEntry));
        }
    }

    public void RemoveStatEntry(StatName statName)
    {
        if (statEntryDictionary.ContainsKey(statName))
        {
            statEntryDictionary.Remove(statName);
        }
    }

    public StatEntryContainer CopyStatEntryDict(StatEntryContainer statEntryContainer)
    {
        StatEntryContainer copiedStatEntryContainer = new();

        if (statEntryContainer != null)
        {
            foreach (var stat in statEntryContainer.statEntryDictionary)
            {
                copiedStatEntryContainer.AddStatEntry(stat.Value);
            }
        }

        return copiedStatEntryContainer;
    }

    public void ResetStatEntryDict()
    {
        statEntryDictionary.Clear();

        InitializeStatEntryDict();
    }
}
