using System.Collections.Generic;
using SDSPEnums;

public interface IStatEntryManager
{
    StatModificationAction GetActionForLevelingUp(StatName statsName);
    Dictionary<StatName, StatEntry> CopyAStatDictionary(Dictionary<StatName, StatEntry> statsDictionary);
}
