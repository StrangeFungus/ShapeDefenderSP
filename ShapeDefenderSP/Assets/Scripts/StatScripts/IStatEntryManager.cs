using System.Collections.Generic;
using SDSPEnums;

public interface IStatEntryManager
{
    void ApplyEnemyStatReductions(EntityStatEntryContainer targetStatEntryContainer, StatusEffectEntry statusEffectEntry);
    void RemoveEnemyStatReductions(EntityStatEntryContainer targetStatEntryContainer, StatusEffectEntry statusEffectEntry, int stackNumber);
    Dictionary<StatName, StatEntry> CopyAStatDictionary(Dictionary<StatName, StatEntry> statsDictionary);
}
