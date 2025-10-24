using System.Collections.Generic;
using SDSPEnums;

public interface IStatEntryManager
{
    void ApplyEnemyStatReductions(StatEntryContainer targetStatEntryContainer, StatusEffectName statusEffectsName, int stackNumber, StatEntryModifier statEntryModifier);
    void RemoveEnemyStatReductions(StatEntryContainer targetStatEntryContainer, StatusEffectName statusEffectsName, int? stackNumber = null);
    Dictionary<StatName, StatEntry> CopyAStatDictionary(Dictionary<StatName, StatEntry> statsDictionary);
    void LevelUpStat(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, BaseEntityController baseEntityController);
    void LevelUpStat(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, BaseAttackController baseAttackController);
    void LevelUpStat(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, StatusEffectEntryContainer statusEffectEntryContainer);
}
