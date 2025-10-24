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

    public void ApplyEnemyStatReductions(StatEntryContainer targetStatEntryContainer, StatusEffectName statusEffectsName, int stackNumber, StatEntryModifier statEntryModifier)
    {
        if (targetStatEntryContainer != null)
        {
            foreach (var statEntry in targetStatEntryContainer.StatEntryDictionary)
            {
                statEntry.Value.AddNewModifier(statusEffectsName, stackNumber, statEntryModifier);
            }
        }
        else
        {
            Debug.Log($"The targets stat entry container count was null... returning...");
        }
    }

    public void RemoveEnemyStatReductions(StatEntryContainer targetStatEntryContainer, StatusEffectName statusEffectsName, int? stackNumber = null)
    {
        if (targetStatEntryContainer != null)
        {
            foreach (var statEntry in targetStatEntryContainer.StatEntryDictionary)
            {
                if (stackNumber != null)
                {
                    statEntry.Value.RemoveModifierStatusEffectStack(statusEffectsName, (int)stackNumber);
                }
                else
                {
                    statEntry.Value.RemoveModifierStatusEffect(statusEffectsName);
                }
            }
        }
        else
        {
            Debug.Log($"The targets stat entry container count was null... returning...");
        }
    }

    public Dictionary<StatName, StatEntry> CopyAStatDictionary(Dictionary<StatName, StatEntry> statsDictionary)
    {
        Dictionary<StatName, StatEntry> copiedDictionary = new Dictionary<StatName, StatEntry>();

        if (statsDictionary != null)
        {
            foreach (var stat in statsDictionary)
            {
                copiedDictionary.Add(stat.Key, StatEntry.CopyStatEntry(stat.Value));
            }
        }
        else
        {
            Debug.Log("CopyAStatDictionary couldnt find the statsDictionary.");
        }

        return copiedDictionary;
    }

    // Entity Stats -> Attack Stats;
    //                 Attack Stats -> Status Effect Stats;
    //                 Attack Stats -> Area Of Effects Radius and Status Effects;

    public void LevelUpStat(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, BaseEntityController baseEntityController)
    {
        if (baseEntityController != null)
        {
            foreach (var stat in statNameAndNumOfLevelUpsDict)
            {
                if (stat.Key != StatName.Default)
                {
                    ProcessStatLevelUp(stat.Key, stat.Value, baseEntityController.EntitiesStats);

                    if (baseEntityController.EntitiesAttackContainer.AttackControllerDictionary != null)
                    {
                        foreach (var attackController in baseEntityController.EntitiesAttackContainer.AttackControllerDictionary)
                        {
                            LevelUpStat(statNameAndNumOfLevelUpsDict, attackController.Value);
                        }
                    }
                }
            }
        }
    }

    public void LevelUpStat(Dictionary<StatName, int> statNameAndNumOfLevelUpsDictionary, BaseAttackController baseAttackController)
    {
        if (baseAttackController != null)
        {
            foreach (var stat in statNameAndNumOfLevelUpsDictionary)
            {
                if (stat.Key != StatName.Default)
                {
                    ProcessStatLevelUp(stat.Key, stat.Value, baseAttackController.AttacksEntry.AttacksStats);

                    if (baseAttackController.AttacksEntry.AttacksStatusEffects != null)
                    {
                        foreach (var effect in baseAttackController.AttacksEntry.AttacksStatusEffects.StatusEffectsDictionary)
                        {
                            ProcessStatLevelUp(stat.Key, stat.Value, effect.Value.StatusEffectsStats);
                        }
                    }
                    
                    if (baseAttackController.AttacksEntry.DoesAnAreaOfEffect && baseAttackController.AttacksEntry.AreaOfEffectPrefabController != null)
                    {
                        if (baseAttackController.AttacksEntry.AreaOfEffectPrefabController.AreaOfEffectsEntry.AreaOfEffectsStatusEffects != null)
                        {
                            if (stat.Key == StatName.AreaOfEffectRadiusValue)
                            {
                                baseAttackController.AttacksEntry.AreaOfEffectPrefabController.AreaOfEffectsEntry.RadiusSize = stat.Value;
                            }
                            else
                            {
                                foreach (var effect in baseAttackController.AttacksEntry.AreaOfEffectPrefabController.AreaOfEffectsEntry.AreaOfEffectsStatusEffects.StatusEffectsDictionary)
                                {
                                    ProcessStatLevelUp(stat.Key, stat.Value, effect.Value.StatusEffectsStats);
                                }
                            }
                        }

                        if (stat.Key == StatName.AreaOfEffectRadiusValue)
                        {
                            float areaOfEffectRadius = baseAttackController.AttacksEntry.AttacksStats.StatEntryDictionary[StatName.AreaOfEffectRadiusValue].StatsTotalValue;
                            baseAttackController.AttacksEntry.AreaOfEffectPrefabController.AreaOfEffectsEntry.RadiusSize = areaOfEffectRadius;
                        }
                    }
                }
            }
        }
    }

    public void LevelUpStat(Dictionary<StatName, int> statNameAndNumOfLevelUpsDict, StatusEffectEntryContainer statusEffectEntryContainer)
    {
        if (statusEffectEntryContainer != null)
        {
            foreach (var stat in statNameAndNumOfLevelUpsDict)
            {
                if (stat.Key != StatName.Default)
                {
                    foreach (var statusEffect in statusEffectEntryContainer.StatusEffectsDictionary)
                    {
                        ProcessStatLevelUp(stat.Key, stat.Value, statusEffect.Value.StatusEffectsStats);
                    }
                }
            }
        }
    }

    private void ProcessStatLevelUp(StatName statsName, int numberOfLevelUps, StatEntryContainer statEntryContainer)
    {
        if (statEntryContainer.StatEntryDictionary != null)
        {
            if (statEntryContainer.StatEntryDictionary.Count > 0)
            {
                // We can do 10% of the starting value per level for now until I design a ScriptableObject for stat level ups for entity role types.

                StatModificationAction statModAction = howToRemoveStatReductionsOrLevelUp[statsName];
                float startingValue = statEntryContainer.StatEntryDictionary[statsName].StartingBaseValue;
                float percentToLevelUpBy = 0.1f;

                statEntryContainer.GetStatEntry(statsName).ModifyBaseValue(statModAction, (startingValue * numberOfLevelUps) * percentToLevelUpBy);
            }
        }
    }
}
