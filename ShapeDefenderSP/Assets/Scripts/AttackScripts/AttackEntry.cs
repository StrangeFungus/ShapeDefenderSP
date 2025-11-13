using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class AttackEntry
{
    // GENERAL DATA
    [SerializeField] private AttackName attacksName = AttackName.None;
    public AttackName AttacksName => attacksName;

    public int AttacksLevel { get; set; } = 1;

    [SerializeField] private MaterialType materialTypes;
    public MaterialType MaterialTypes => materialTypes;

    [SerializeField] private AttackTargetingBehaviour targetingBehaviour;
    public AttackTargetingBehaviour TargetingBehaviour => TargetingBehaviour;

    // HEALING SETTINGS
    [SerializeField] private bool doesAttackHeal;
    public bool DoesAttackHeal => doesAttackHeal;

    [SerializeField] private HealingType healingTypes;
    public HealingType HealingTypes => healingTypes;

    [SerializeField] private bool doesHealingCauseDamageToEnemies;
    public bool DoesHealingCauseDamageToEnemies => doesHealingCauseDamageToEnemies;

    // DAMAGE SETTINGS
    [SerializeField] private DamageType damageTypes;
    public DamageType DamageTypes => damageTypes;

    [SerializeField] private bool doesDamageIgnoresEnergyShields;
    public bool DoesDamageIgnoresEnergyShields => doesDamageIgnoresEnergyShields;

    [SerializeField] private bool doesDamageOverTime;
    public bool DoesDamageOverTime => doesDamageOverTime;

    // ATTACKS GENERAL SETTINGS
    [SerializeField] private bool isProjectileBlockable;
    public bool IsProjectileBlockable => isProjectileBlockable;

    [SerializeField] private bool canProjectileBeReflected;
    public bool CanProjectileBeReflected => canProjectileBeReflected;

    [SerializeField] private int maxAllowedReflections;
    public int MaxAllowedReflections => maxAllowedReflections;

    [SerializeField] private bool canProjectileBeParried;
    public bool CanProjectileBeParried => canProjectileBeParried;

    [SerializeField] private bool canProjectileBeDodged;
    public bool CanProjectileBeDodged => canProjectileBeDodged;

    [SerializeField] private bool blocksEnemiesAbilityToMove;
    public bool BlocksEnemiesAbilityToMove => blocksEnemiesAbilityToMove;

    [SerializeField] private bool blocksEnemiesAbilityToAttack;
    public bool BlocksEnemiesAbilityToAttack => blocksEnemiesAbilityToAttack;

    [SerializeField] private bool blocksEnemiesAbilityToHeal;
    public bool BlocksEnemiesAbilityToHeal => blocksEnemiesAbilityToHeal;

    // PROJECTILE SETTINGS
    [SerializeField] private bool isProjectilePhysicalObject = true;
    public bool IsProjectilePhysicalObject => isProjectilePhysicalObject;

    [SerializeField] private float maxTravelDistanceMultiplier = 1.0f;
    public float MaxTravelDistanceMultiplier => maxTravelDistanceMultiplier;

    [SerializeField] private float destroyDelayTimer = 0.5f;
    public float DestroyDelayTimer => destroyDelayTimer;

    [SerializeField] private bool stopsAfterFinalHit;
    public bool StopsAfterFinalHit => stopsAfterFinalHit;

    // COMPLEX ATTACK DATA
    [SerializeField] private bool doesAnAreaOfEffect;
    public bool DoesAnAreaOfEffect => doesAnAreaOfEffect;

    [SerializeField] private AreaOfEffectController areaOfEffectPrefabController;
    public AreaOfEffectController AreaOfEffectPrefabController => areaOfEffectPrefabController;

    [SerializeField] private StatEntryContainer attacksStats;
    public StatEntryContainer AttacksStats => attacksStats;

    [SerializeField] private StatusEffectEntryContainer attacksStatusEffects;
    public StatusEffectEntryContainer AttacksStatusEffects => attacksStatusEffects;

    // TRACKING DATA
    private BaseEntityController attackingEntitiesController;
    public BaseEntityController AttackingEntitiesController { get => attackingEntitiesController; set => attackingEntitiesController = value; }

    private string parentsTagType = "";
    public string ParentsTagType { get => parentsTagType;  set => parentsTagType = value; }

    public static AttackEntry CopyAttackEntry(AttackEntry attackEntryToCopyFrom)
    {
        if (attackEntryToCopyFrom == null) return null;

        AttackEntry copy = new AttackEntry
        {
            // GENERAL DATA
            attacksName = attackEntryToCopyFrom.attacksName,
            AttacksLevel = attackEntryToCopyFrom.AttacksLevel,
            materialTypes = attackEntryToCopyFrom.materialTypes,

            // HEALING SETTINGS
            doesAttackHeal = attackEntryToCopyFrom.doesAttackHeal,
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
            isProjectilePhysicalObject = attackEntryToCopyFrom.isProjectilePhysicalObject,
            maxTravelDistanceMultiplier = attackEntryToCopyFrom.maxTravelDistanceMultiplier,
            destroyDelayTimer = attackEntryToCopyFrom.destroyDelayTimer,
            stopsAfterFinalHit = attackEntryToCopyFrom.stopsAfterFinalHit,

            // COMPLEX ATTACK DATA PT1
            doesAnAreaOfEffect = attackEntryToCopyFrom.doesAnAreaOfEffect,
            areaOfEffectPrefabController = attackEntryToCopyFrom.areaOfEffectPrefabController,
        };

        // COMPLEX ATTACK DATA PT2
        copy.attacksStats = StatEntryContainer.CopyStatEntryDict(attackEntryToCopyFrom.AttacksStats);
        copy.attacksStatusEffects = StatusEffectEntryContainer.CopyStatusEffectEntryContainer(attackEntryToCopyFrom.attacksStatusEffects);

        if (attackEntryToCopyFrom.areaOfEffectPrefabController != null)
        {
            copy.areaOfEffectPrefabController.AreaOfEffectsEntry.CopyAreaOfEffectEntry(attackEntryToCopyFrom.areaOfEffectPrefabController.AreaOfEffectsEntry);
        }

        return copy;
    }

    public void ResetToDefaults()
    {
        AttacksLevel = 1;
        AttacksStats.ResetStatEntryDict();
        if (areaOfEffectPrefabController != null)
        {
            areaOfEffectPrefabController.AreaOfEffectsEntry.ResetAreaOfEffectEntry();
        }

        AttacksStatusEffects.ResetStatusEffectEntryDict();
    }
}
