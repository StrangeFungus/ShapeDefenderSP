using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

public class DamagingObjectStatEntryContainer : BaseStatEntryContainer
{
    [SerializeField] private StatDefaultTemplateSO defaultStatReductionEntries;
    private Dictionary<StatName, StatEntry> statReductionEntryDictionary = new();
    public IReadOnlyDictionary<StatName, StatEntry> StatReductionEntryDictionary => statReductionEntryDictionary;

    public void InitializeDamagingObjectStatEntryContainer()
    {
        InitializeStatEntryDict();

        if (defaultStatReductionEntries != null)
        {
            if (defaultStatReductionEntries.StatEntries.Count > 0)
            {
                foreach (var entry in defaultStatReductionEntries.StatEntries)
                {
                    AddStatEntry(entry, statReductionEntryDictionary);
                }
            }
        }
    }

    public void CopyDamagingObjectStatEntryContainer(DamagingObjectStatEntryContainer statEntryContainer)
    {
        if (statEntryContainer != null)
        {
            foreach (var stat in statEntryContainer.statEntryDictionary)
            {
                AddStatEntry(stat.Value, statReductionEntryDictionary);
            }

            foreach (var stat in statEntryContainer.statReductionEntryDictionary)
            {
                AddStatEntry(stat.Value, statReductionEntryDictionary);
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
