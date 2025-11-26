using SDSPEnums;
using UnityEngine;

public class BaseDamagingObjectEntry
{
    // HEALING SETTINGS
    [SerializeField] protected bool doesEffectHeal;
    public bool DoesEffectHeal => doesEffectHeal;

    [SerializeField] protected HealingType healingTypes;
    public HealingType HealingTypes => healingTypes;

    [SerializeField] protected bool doesHealingCauseDamageToEnemies;
    public bool DoesHealingCauseDamageToEnemies => doesHealingCauseDamageToEnemies;

    // DAMAGE SETTINGS
    [SerializeField] protected DamageType damageTypes;
    public DamageType DamageTypes => damageTypes;

    [SerializeField] protected bool doesDamageIgnoresEnergyShields;
    public bool DoesDamageIgnoresEnergyShields => doesDamageIgnoresEnergyShields;

    [SerializeField] protected bool doesDamageOverTime;
    public bool DoesDamageOverTime => doesDamageOverTime;

    // GENERAL SETTINGS
    [SerializeField] protected bool isProjectileBlockable;
    public bool IsProjectileBlockable => isProjectileBlockable;

    [SerializeField] protected bool canProjectileBeReflected;
    public bool CanProjectileBeReflected => canProjectileBeReflected;

    [SerializeField] protected int maxAllowedReflections;
    public int MaxAllowedReflections => maxAllowedReflections;

    [SerializeField] protected bool canProjectileBeParried;
    public bool CanProjectileBeParried => canProjectileBeParried;

    [SerializeField] protected bool canProjectileBeDodged;
    public bool CanProjectileBeDodged => canProjectileBeDodged;

    [SerializeField] protected bool blocksEnemiesAbilityToMove;
    public bool BlocksEnemiesAbilityToMove => blocksEnemiesAbilityToMove;

    [SerializeField] protected bool blocksEnemiesAbilityToAttack;
    public bool BlocksEnemiesAbilityToAttack => blocksEnemiesAbilityToAttack;

    [SerializeField] protected bool blocksEnemiesAbilityToHeal;
    public bool BlocksEnemiesAbilityToHeal => blocksEnemiesAbilityToHeal;

    // PROJECTILE SETTINGS
    [SerializeField] protected bool isEffectAPhysicalObject = true;
    public bool IsEffectAPhysicalObject => isEffectAPhysicalObject;

    [SerializeField] protected float maxTravelDistanceMultiplier = 1.0f;
    public float MaxTravelDistanceMultiplier => maxTravelDistanceMultiplier;

    [SerializeField] protected float destroyDelayTimer = 0.5f;
    public float DestroyDelayTimer => destroyDelayTimer;

    [SerializeField] protected bool stopsAfterFinalHit;
    public bool StopsAfterFinalHit => stopsAfterFinalHit;

    // COMPLEX DATA
    [SerializeField] protected DamagingObjectStatEntryContainer effectsStats;
    public DamagingObjectStatEntryContainer EffectsStats { get => effectsStats; set => effectsStats = value; }

    [SerializeField] protected StatusEffectEntryContainer effectsStatusEffects;
    public StatusEffectEntryContainer EffectsStatusEffects { get => effectsStatusEffects; set => effectsStatusEffects = value; }

    // TRACKING DATA
    protected BaseEntityController attackingEntitiesController;
    public BaseEntityController AttackingEntitiesController { get => attackingEntitiesController; set => attackingEntitiesController = value; }

    protected string attackingEntitiesTagType = "";
    public string AttackingEntitiesTagType { get => attackingEntitiesTagType; set => attackingEntitiesTagType = value; }
}
