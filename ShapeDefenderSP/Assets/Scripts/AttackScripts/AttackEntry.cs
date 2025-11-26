using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class AttackEntry : BaseDamagingObjectEntry
{
    // GENERAL DATA
    [SerializeField] private AttackName attacksName = AttackName.None;
    public AttackName AttacksName => attacksName;

    public int AttacksLevel { get; set; } = 1;

    [SerializeField] private MaterialType materialTypes;
    public MaterialType MaterialTypes => materialTypes;

    [SerializeField] private AttackTargetingBehaviour targetingBehaviour;
    public AttackTargetingBehaviour TargetingBehaviour => TargetingBehaviour;

    // COMPLEX ATTACK DATA
    [SerializeField] private bool doesAnAreaOfEffect;
    public bool DoesAnAreaOfEffect => doesAnAreaOfEffect;

    [SerializeField] private AreaOfEffectController areaOfEffectPrefabController;
    public AreaOfEffectController AreaOfEffectPrefabController => areaOfEffectPrefabController;

    public static AttackEntry CopyAttackEntry(AttackEntry attackEntryToCopyFrom)
    {
        if (attackEntryToCopyFrom == null) return null;

        AttackEntry copy = new()
        {
            // GENERAL DATA
            attacksName = attackEntryToCopyFrom.attacksName,
            AttacksLevel = attackEntryToCopyFrom.AttacksLevel,
            materialTypes = attackEntryToCopyFrom.materialTypes,

            // HEALING SETTINGS
            doesEffectHeal = attackEntryToCopyFrom.doesEffectHeal,
            healingTypes = attackEntryToCopyFrom.healingTypes,
            doesHealingCauseDamageToEnemies = attackEntryToCopyFrom.doesHealingCauseDamageToEnemies,

            // DAMAGE SETTINGS
            damageTypes = attackEntryToCopyFrom.damageTypes,
            doesDamageIgnoresEnergyShields = attackEntryToCopyFrom.doesDamageIgnoresEnergyShields,
            doesDamageOverTime = attackEntryToCopyFrom.doesDamageOverTime,

            // ATTACKS GENERAL SETTINGS
            isProjectileBlockable = attackEntryToCopyFrom.isProjectileBlockable,
            canProjectileBeReflected = attackEntryToCopyFrom.canProjectileBeReflected,
            maxAllowedReflections = attackEntryToCopyFrom.maxAllowedReflections,
            canProjectileBeParried = attackEntryToCopyFrom.canProjectileBeParried,
            canProjectileBeDodged = attackEntryToCopyFrom.canProjectileBeDodged,
            blocksEnemiesAbilityToMove = attackEntryToCopyFrom.blocksEnemiesAbilityToMove,
            blocksEnemiesAbilityToAttack = attackEntryToCopyFrom.blocksEnemiesAbilityToAttack,
            blocksEnemiesAbilityToHeal = attackEntryToCopyFrom.blocksEnemiesAbilityToHeal,

            // PROJECTILE SETTINGS
            isEffectAPhysicalObject = attackEntryToCopyFrom.isEffectAPhysicalObject,
            maxTravelDistanceMultiplier = attackEntryToCopyFrom.maxTravelDistanceMultiplier,
            destroyDelayTimer = attackEntryToCopyFrom.destroyDelayTimer,
            stopsAfterFinalHit = attackEntryToCopyFrom.stopsAfterFinalHit,

            // COMPLEX ATTACK DATA PT1
            doesAnAreaOfEffect = attackEntryToCopyFrom.doesAnAreaOfEffect,
            areaOfEffectPrefabController = attackEntryToCopyFrom.areaOfEffectPrefabController,
        };

        // COMPLEX ATTACK DATA PT2
        copy.effectsStats.CopyDamagingObjectStatEntryContainer(attackEntryToCopyFrom.effectsStats);
        copy.effectsStatusEffects.CopyStatusEffectEntryContainer(attackEntryToCopyFrom.effectsStatusEffects);

        if (attackEntryToCopyFrom.areaOfEffectPrefabController != null)
        {
            copy.areaOfEffectPrefabController.CopyAreaOfEffectsControllerData(attackEntryToCopyFrom.areaOfEffectPrefabController);
        }

        return copy;
    }

    public void ResetToDefaults()
    {
        AttacksLevel = 1;
        effectsStats.ResetDamagingObjectStatEntryContainer();
        if (areaOfEffectPrefabController != null)
        {
            areaOfEffectPrefabController.ResetAreaOfEffectsController();
        }

        effectsStatusEffects.ResetStatusEffectEntryDict();
    }
}
