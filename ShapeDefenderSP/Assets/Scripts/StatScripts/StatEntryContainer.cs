using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class StatEntryContainer : MonoBehaviour
{
    [SerializeField] private StatTemplateSO defaultStatEntries;

    private Dictionary<StatName, StatEntry> statEntryDictionary = new();
    public IReadOnlyDictionary<StatName, StatEntry> StatEntryDictionary => statEntryDictionary;

    private void Awake()
    {
        InitializeStatEntryDict();
    }

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
            }
        }
    }

    public StatEntry GetStatEntry(StatName statName)
    {
        if (!statEntryDictionary.TryGetValue(statName, out var entry))
        {
            Debug.LogWarning($"Stat {statName} not found in {gameObject.name}'s stat container.");
        }
        return entry;
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

    public void ResetStatEntryDict()
    {
        statEntryDictionary.Clear();

        InitializeStatEntryDict();
    }
}
