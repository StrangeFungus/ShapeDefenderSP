using System.Collections.Generic;
using System.Xml.Schema;
using SDSPEnums;
using UnityEngine;

public class StatusEffectEntry : BaseDamagingObjectEntry
{
    // GENERAL DATA
    [SerializeField] private StatusEffectName statusEffectsName = StatusEffectName.Default;
    public StatusEffectName StatusEffectsName => statusEffectsName;

    public int StatusEffectsLevel { get; set; } = 1;

    [SerializeField] private int maxStackAmount = 1;
    public int MaxStackAmount { get; }

    // COMPLEX STATUS EFFECT DATA
    [SerializeField] private bool doesAnAreaOfEffect;
    public bool DoesAnAreaOfEffect => doesAnAreaOfEffect;

    [SerializeField] private AreaOfEffectController areaOfEffectPrefabController;
    public AreaOfEffectController AreaOfEffectPrefabController => areaOfEffectPrefabController;

    public void CopyStatusEffectEntry(StatusEffectEntry statusEffectEntryToCopy)
    {
        // GENERAL DATA
        statusEffectsName = statusEffectEntryToCopy.statusEffectsName;
        StatusEffectsLevel = statusEffectEntryToCopy.StatusEffectsLevel;
        maxStackAmount = statusEffectEntryToCopy.maxStackAmount;
        // HEALING SETTINGS
        doesEffectHeal = statusEffectEntryToCopy.doesEffectHeal;
        healingTypes = statusEffectEntryToCopy.healingTypes;
        doesHealingCauseDamageToEnemies = statusEffectEntryToCopy.doesHealingCauseDamageToEnemies;
        // DAMAGE SETTINGS
        damageTypes = statusEffectEntryToCopy.damageTypes;
        doesDamageIgnoresEnergyShields = statusEffectEntryToCopy.doesDamageIgnoresEnergyShields;
        doesDamageOverTime = statusEffectEntryToCopy.doesDamageOverTime;
        // ATTACKS GENERAL SETTINGS
        isProjectileBlockable = statusEffectEntryToCopy.isProjectileBlockable;
        canProjectileBeDodged = statusEffectEntryToCopy.canProjectileBeDodged;
        blocksEnemiesAbilityToMove = statusEffectEntryToCopy.blocksEnemiesAbilityToMove;
        blocksEnemiesAbilityToAttack = statusEffectEntryToCopy.blocksEnemiesAbilityToAttack;
        blocksEnemiesAbilityToHeal = statusEffectEntryToCopy.blocksEnemiesAbilityToHeal;
        // COMPLEX ATTACK DATA
        doesAnAreaOfEffect = statusEffectEntryToCopy.doesAnAreaOfEffect;
        areaOfEffectPrefabController = statusEffectEntryToCopy.areaOfEffectPrefabController;
        effectsStats = statusEffectEntryToCopy.effectsStats;
        // TRACKING DATA
        attackingEntitiesController = statusEffectEntryToCopy.attackingEntitiesController;

        if (statusEffectEntryToCopy.effectsStats != null)
        {
            if (statusEffectEntryToCopy.effectsStats.StatEntryDictionary != null)
            {
                foreach (var statEntry in statusEffectEntryToCopy.effectsStats.StatEntryDictionary)
                {
                    effectsStats.AddStatEntry(statEntry.Value);
                }
            }
        }
    }
}
