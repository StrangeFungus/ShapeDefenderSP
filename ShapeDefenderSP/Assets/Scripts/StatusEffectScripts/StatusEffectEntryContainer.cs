using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

public class StatusEffectEntryContainer
{
    [SerializeField] private Dictionary<StatusEffectName, StatusEffectEntry> statusEffectsDictionary = new();
    public Dictionary<StatusEffectName, StatusEffectEntry> StatusEffectsDictionary => statusEffectsDictionary;

    public void AddNewStatusEffect(StatusEffectEntry statusEffectEntryToAdd)
    {
        if (statusEffectEntryToAdd != null)
        {
            if (!statusEffectsDictionary.ContainsKey(statusEffectEntryToAdd.StatusEffectsName))
            {
                statusEffectsDictionary.Add(statusEffectEntryToAdd.StatusEffectsName, statusEffectEntryToAdd);
            }
        }
    }

    public void CopyStatusEffectEntryContainer(StatusEffectEntryContainer statusEffectEntryContainerToCopyFrom)
    {
        if (statusEffectEntryContainerToCopyFrom != null)
        {
            foreach (var statusEffect in statusEffectEntryContainerToCopyFrom.StatusEffectsDictionary)
            {
                AddNewStatusEffect(statusEffect.Value);
            }
        }
    }

    public void ResetStatusEffectEntryDict()
    {
        if (statusEffectsDictionary != null)
        {
            statusEffectsDictionary.Clear();
        }
        else
        {
            statusEffectsDictionary = new();
        }
    }
}
