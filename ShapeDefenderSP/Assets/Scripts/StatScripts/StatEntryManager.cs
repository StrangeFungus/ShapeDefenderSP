using System;
using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

public class StatEntryManager : MonoBehaviour, IStatEntryManager
{
    private static StatEntryManager Instance { get; set; }
    private static Dictionary<StatName, StatModificationAction> howToApplyStatReductions = new();
    private static Dictionary<StatName, StatModificationAction> howToRemoveStatReductionsOrLevelUp = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InterfaceContainer.Register<IStatEntryManager>(this);

        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        foreach (StatName name in Enum.GetValues(typeof(StatName)))
        {
            bool endsWithTimer = name.ToString().EndsWith("Timer");

            if (!howToApplyStatReductions.ContainsKey(name))
            {
                if (endsWithTimer)
                {
                    howToApplyStatReductions.Add(name, StatModificationAction.AddToValue);
                }
                else
                {
                    howToApplyStatReductions.Add(name, StatModificationAction.SubtractFromValue);
                }
            }

            if (!howToRemoveStatReductionsOrLevelUp.ContainsKey(name))
            {
                if (endsWithTimer)
                {
                    howToRemoveStatReductionsOrLevelUp.Add(name, StatModificationAction.SubtractFromValue);
                }
                else
                {
                    howToRemoveStatReductionsOrLevelUp.Add(name, StatModificationAction.AddToValue);
                }
            }
        }
    }

    public StatModificationAction GetActionForLevelingUp(StatName statsName)
    {
        if (howToRemoveStatReductionsOrLevelUp.TryGetValue(statsName, out StatModificationAction action))
        {
            return action;
        }

        return StatModificationAction.AddToValue;
    }

    public Dictionary<StatName, StatEntry> CopyAStatDictionary(Dictionary<StatName, StatEntry> statsDictionary)
    {
        Dictionary<StatName, StatEntry> copiedDictionary = new Dictionary<StatName, StatEntry>();

        if (statsDictionary != null)
        {
            foreach (var stat in statsDictionary)
            {
                copiedDictionary.Add(stat.Key, new StatEntry(stat.Key, stat.Value.BaseValue));
            }
        }
        else
        {
            Debug.Log("CopyAStatDictionary couldnt find the statsDictionary.");
        }

        return copiedDictionary;
    }

    // Level Up Stat Distribution Overview:
    // Entity Stats -> Attack Stats;
    //                 Attack Stats -> Status Effects Stats;
    //                                 Status Effects Stats -> Area Of Effects Stats;
    //                 Attack Stats -> Area Of Effects Stats;
    // We can do 10% of the starting value per level for now until I design a ScriptableObject for stat level ups for entity role types.

    public void LevelUpEntitiesStats(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, EntityStatEntryContainer entityStatEntryContainer, AttackEntryContainer attackEntryContainer)
    {
        if (statNameAndNumOfLevelUpsDict == null) { return; }

        if (statNameAndNumOfLevelUpsDict.Count <= 0) { return; }

        foreach (var stat in statNameAndNumOfLevelUpsDict)
        {
            if (stat.Key != StatName.Default)
            {
                if (entityStatEntryContainer.StatEntryDictionary.TryGetValue(stat.Key, out StatEntry targetStatToLevel))
                {
                    StatModificationAction statModAction = howToRemoveStatReductionsOrLevelUp[stat.Key];
                    float startingValue = targetStatToLevel.StartingBaseValue;
                    targetStatToLevel.ModifyBaseValue(statModAction, (startingValue * statNameAndNumOfLevelUpsDict[stat.Key]) * GlobalCONSTValuesContainer.LEVELUPSTATMULTIPLIER);

                    LevelUpAttacksStats(statNameAndNumOfLevelUpsDict, attackEntryContainer);
                }
            }
        }
    }

    public void LevelUpAttacksStats(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, AttackEntryContainer attackEntryContainer)
    {
        if (statNameAndNumOfLevelUpsDict == null) { return; }

        if (statNameAndNumOfLevelUpsDict.Count <= 0) { return; }

        foreach (var stat in statNameAndNumOfLevelUpsDict)
        {
            if (stat.Key != StatName.Default)
            {
                foreach (var attackEntry in attackEntryContainer.AttackControllerDictionary)
                {
                    if (attackEntry.Value.AttacksEntry.EffectsStats.StatEntryDictionary.TryGetValue(stat.Key, out StatEntry targetStatToLevel))
                    {
                        StatModificationAction statModAction = howToRemoveStatReductionsOrLevelUp[stat.Key];
                        float startingValue = targetStatToLevel.StartingBaseValue;
                        targetStatToLevel.ModifyBaseValue(statModAction, (startingValue * statNameAndNumOfLevelUpsDict[stat.Key]) * GlobalCONSTValuesContainer.LEVELUPSTATMULTIPLIER);

                        if (attackEntry.Value.AttacksEntry.EffectsStatusEffects != null)
                        {
                            if (attackEntry.Value.AttacksEntry.EffectsStatusEffects.StatusEffectsDictionary.Count > 0)
                            {
                                LevelUpStatusEffectsStats(statNameAndNumOfLevelUpsDict, attackEntry.Value.AttacksEntry.EffectsStatusEffects);
                            }
                        }

                        if (attackEntry.Value.AttacksEntry.DoesAnAreaOfEffect && attackEntry.Value.AttacksEntry.AreaOfEffectPrefabController != null)
                        {
                            LevelUpAreaOfEffectsStats(statNameAndNumOfLevelUpsDict, attackEntry.Value.AttacksEntry.AreaOfEffectPrefabController);
                        }
                    }
                }
            }
        }
    }

    public void LevelUpStatusEffectsStats(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, StatusEffectEntryContainer statusEffectEntryContainer)
    {
        if (statNameAndNumOfLevelUpsDict == null) { return; }

        if (statNameAndNumOfLevelUpsDict.Count <= 0) { return; }

        foreach (var stat in statNameAndNumOfLevelUpsDict)
        {
            if (stat.Key != StatName.Default)
            {
                foreach (var statusEffect in statusEffectEntryContainer.StatusEffectsDictionary)
                {
                    if (statusEffect.Value.EffectsStats.StatEntryDictionary.TryGetValue(stat.Key, out StatEntry targetStatToLevel))
                    {
                        StatModificationAction statModAction = howToRemoveStatReductionsOrLevelUp[stat.Key];
                        float startingValue = targetStatToLevel.StartingBaseValue;
                        targetStatToLevel.ModifyBaseValue(statModAction, (startingValue * statNameAndNumOfLevelUpsDict[stat.Key]) * GlobalCONSTValuesContainer.LEVELUPSTATMULTIPLIER);

                        if (statusEffect.Value.DoesAnAreaOfEffect && statusEffect.Value.AreaOfEffectPrefabController != null)
                        {
                            LevelUpAreaOfEffectsStats(statNameAndNumOfLevelUpsDict, statusEffect.Value.AreaOfEffectPrefabController);
                        }
                    }
                }
            }
        }
    }

    public void LevelUpAreaOfEffectsStats(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, AreaOfEffectController areaOfEffectController)
    {
        if (statNameAndNumOfLevelUpsDict == null) { return; }

        if (statNameAndNumOfLevelUpsDict.Count <= 0) { return; }

        foreach (var stat in statNameAndNumOfLevelUpsDict)
        {
            if (stat.Key != StatName.Default)
            {
                if (areaOfEffectController.AttacksEntry.EffectsStats.StatEntryDictionary.TryGetValue(stat.Key, out StatEntry targetStatToLevel))
                {
                    StatModificationAction statModAction = howToRemoveStatReductionsOrLevelUp[stat.Key];
                    float startingValue = targetStatToLevel.StartingBaseValue;
                    targetStatToLevel.ModifyBaseValue(statModAction, (startingValue * statNameAndNumOfLevelUpsDict[stat.Key]) * GlobalCONSTValuesContainer.LEVELUPSTATMULTIPLIER);

                    if (!areaOfEffectController.AreaOfEffectsEntry.FollowsAttackProjectile)
                    {
                        if (areaOfEffectController.AttacksEntry.EffectsStatusEffects != null)
                        {
                            if (areaOfEffectController.AttacksEntry.EffectsStatusEffects.StatusEffectsDictionary.Count > 0)
                            {
                                LevelUpStatusEffectsStats(statNameAndNumOfLevelUpsDict, areaOfEffectController.AttacksEntry.EffectsStatusEffects);
                            }
                        }
                    }
                }
            }
        }
    }
}
