using System.Collections.Generic;
using SDSPEnums;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class AreaOfEffectEntry
{
    // GENERAL DATA
    [SerializeField] private AreaOfEffectType areaOfEffectType = AreaOfEffectType.Circle;
    public AreaOfEffectType AreaOfEffectType => areaOfEffectType;

    [SerializeField] private AreaOfEffectPattern areaOfEffectPatternPattern = AreaOfEffectPattern.Line;
    public AreaOfEffectPattern AreaOfEffectPatternPattern => areaOfEffectPatternPattern;

    // COLLIDER
    [SerializeField] private Collider objectsCollider2D;
    public Collider ObjectsCollider2D => objectsCollider2D;

    [SerializeField] private bool hasColliderBeenValidated;
    public bool HasColliderBeenValidated => hasColliderBeenValidated;

    [SerializeField] private float defaultRadiusSize = 10.0f;
    private float radiusSize = 10.0f;
    public float RadiusSize { get => radiusSize; set => radiusSize = value; }

    // AREA OF EFFECT SETTINGS
    [SerializeField] private bool canEffectMove = true;
    public bool CanEffectMove { get => canEffectMove; set => canEffectMove = value; }

    [SerializeField] private bool spawnsWhenAttackSpawns;
    public bool SpawnsWhenAttackSpawns => spawnsWhenAttackSpawns;

    [SerializeField] private bool followsAttackProjectile;
    public bool FollowsAttackProjectile => followsAttackProjectile;

    [SerializeField] private bool spawnsWhenAttackHits;
    public bool SpawnsWhenAttackHits => spawnsWhenAttackHits;

    [SerializeField] private int maxSpawnableEffects = 0;
    public int MaxSpawnableEffects => maxSpawnableEffects;

    [SerializeField] private float maxTravelDistanceMultiplier = 1.0f;
    public float MaxTravelDistanceMultiplier => maxTravelDistanceMultiplier;

    [SerializeField] private float destroyDelayTimer = 0.5f;
    public float DestroyDelayTimer => destroyDelayTimer;

    [SerializeField] private bool stopsAfterFinalHit;
    public bool StopsAfterFinalHit => stopsAfterFinalHit;

    // HEALING SETTINGS
    [SerializeField] private bool doesAreaOfEffectHeal;
    public bool DoesAreaOfEffectHeal => doesAreaOfEffectHeal;

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
    [SerializeField] private bool isAreaOfEffectPhysicalObject = true;
    public bool IsAreaOfEffectPhysicalObject => isAreaOfEffectPhysicalObject;

    [SerializeField] private bool isAreaOfEffectBlockable;
    public bool IsAreaOfEffectBlockable => isAreaOfEffectBlockable;

    [SerializeField] private bool canAreaOfEffectBeDodged;
    public bool CanAreaOfEffectBeDodged => canAreaOfEffectBeDodged;

    [SerializeField] private bool canEffectBeReflected;
    public bool CanEffectBeReflected => canEffectBeReflected;

    [SerializeField] private int maxAllowedReflections;
    public int MaxAllowedReflections => maxAllowedReflections;

    [SerializeField] private bool blocksEnemiesAbilityToMove;
    public bool BlocksEnemiesAbilityToMove => blocksEnemiesAbilityToMove;

    [SerializeField] private bool blocksEnemiesAbilityToAttack;
    public bool BlocksEnemiesAbilityToAttack => blocksEnemiesAbilityToAttack;

    [SerializeField] private bool blocksEnemiesAbilityToHeal;
    public bool BlocksEnemiesAbilityToHeal => blocksEnemiesAbilityToHeal;

    // COMPLEX AREA OF EFFECT DATA
    [SerializeField] private StatEntryContainer areaOfEffectsStats;
    public StatEntryContainer AreaOfEffectsStats => areaOfEffectsStats;

    [SerializeField] private StatusEffectEntryContainer areaOfEffectsStatusEffects;
    public StatusEffectEntryContainer AreaOfEffectsStatusEffects => areaOfEffectsStatusEffects;

    // TRACKING DATA
    private BaseEntityController attackingEntitiesController;
    public BaseEntityController AttackingEntitiesController { get => attackingEntitiesController; set => attackingEntitiesController = value; }
    private string parentsTagType = "";
    public string ParentsTagType { get => parentsTagType; set => parentsTagType = value; }

    public void OnAwake()
    {
        radiusSize = defaultRadiusSize;
    }

    public void CopyAreaOfEffectEntry(AreaOfEffectEntry areaOfEffectEntryToCopy)
    {
        areaOfEffectsStats = StatEntryContainer.CopyStatEntryDict(areaOfEffectEntryToCopy.areaOfEffectsStats);
        areaOfEffectsStatusEffects = StatusEffectEntryContainer.CopyStatusEffectEntryContainer(areaOfEffectEntryToCopy.areaOfEffectsStatusEffects);
        attackingEntitiesController = areaOfEffectEntryToCopy.attackingEntitiesController;
    }

    public void ResetAreaOfEffectEntry()
    {
        radiusSize = defaultRadiusSize;
        areaOfEffectsStats.ResetStatEntryDict();
        areaOfEffectsStatusEffects.ResetStatusEffectEntryDict();
    }
}
