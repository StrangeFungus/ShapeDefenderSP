using System.Collections.Generic;
using System.Xml.Schema;
using SDSPEnums;
using UnityEngine;

public class StatusEffectEntry : MonoBehaviour
{
    // GENERAL DATA
    [SerializeField] private StatusEffectName statusEffectsName = StatusEffectName.Default;
    public StatusEffectName StatusEffectsName => statusEffectsName;

    public int StatusEffectsLevel { get; set; } = 1;

    [SerializeField] private int maxStackAmount = 1;
    public int MaxStackAmount { get; }

    // HEALING SETTINGS
    [SerializeField] private bool doesStatusEffectHeal;
    public bool DoesStatusEffectHeal => doesStatusEffectHeal;

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

    // STATUS EFFECTS GENERAL SETTINGS
    [SerializeField] private bool isPhysicalObject = true;
    public bool IsPhysicalObject => isPhysicalObject;

    [SerializeField] private bool isDamageBlockable;
    public bool IsDamageBlockable => isDamageBlockable;

    [SerializeField] private bool canDamageBeDodged;
    public bool CanDamageBeDodged => canDamageBeDodged;

    [SerializeField] private bool blocksEnemiesAbilityToMove;
    public bool BlocksEnemiesAbilityToMove => blocksEnemiesAbilityToMove;

    [SerializeField] private bool blocksEnemiesAbilityToAttack;
    public bool BlocksEnemiesAbilityToAttack => blocksEnemiesAbilityToAttack;

    [SerializeField] private bool blocksEnemiesAbilityToHeal;
    public bool BlocksEnemiesAbilityToHeal => blocksEnemiesAbilityToHeal;

    // COMPLEX STATUS EFFECT DATA
    [SerializeField] private bool doesAnAreaOfEffect;
    public bool DoesAnAreaOfEffect => doesAnAreaOfEffect;

    [SerializeField] private AreaOfEffectController areaOfEffectPrefabController;
    public AreaOfEffectController AreaOfEffectPrefabController => areaOfEffectPrefabController;

    [SerializeField] private StatEntryContainer statusEffectsStats;
    public StatEntryContainer StatusEffectsStats => statusEffectsStats;

    // TRACKING DATA
    private BaseEntityController attackingEntitiesController;
    public BaseEntityController AttackingEntitiesController { get => attackingEntitiesController; set => attackingEntitiesController = value; }

    public void CopyStatusEffectEntry(StatusEffectEntry statusEffectEntryToCopy)
    {
        // GENERAL DATA
        statusEffectsName = statusEffectEntryToCopy.statusEffectsName;
        StatusEffectsLevel = statusEffectEntryToCopy.StatusEffectsLevel;
        maxStackAmount = statusEffectEntryToCopy.maxStackAmount;
        // HEALING SETTINGS
        doesStatusEffectHeal = statusEffectEntryToCopy.doesStatusEffectHeal;
        healingTypes = statusEffectEntryToCopy.healingTypes;
        doesHealingCauseDamageToEnemies = statusEffectEntryToCopy.doesHealingCauseDamageToEnemies;
        // DAMAGE SETTINGS
        damageTypes = statusEffectEntryToCopy.damageTypes;
        doesDamageIgnoresEnergyShields = statusEffectEntryToCopy.doesDamageIgnoresEnergyShields;
        doesDamageOverTime = statusEffectEntryToCopy.doesDamageOverTime;
        // ATTACKS GENERAL SETTINGS
        isDamageBlockable = statusEffectEntryToCopy.isDamageBlockable;
        canDamageBeDodged = statusEffectEntryToCopy.canDamageBeDodged;
        blocksEnemiesAbilityToMove = statusEffectEntryToCopy.blocksEnemiesAbilityToMove;
        blocksEnemiesAbilityToAttack = statusEffectEntryToCopy.blocksEnemiesAbilityToAttack;
        blocksEnemiesAbilityToHeal = statusEffectEntryToCopy.blocksEnemiesAbilityToHeal;
        // COMPLEX ATTACK DATA
        doesAnAreaOfEffect = statusEffectEntryToCopy.doesAnAreaOfEffect;
        areaOfEffectPrefabController = statusEffectEntryToCopy.areaOfEffectPrefabController;
        statusEffectsStats = statusEffectEntryToCopy.statusEffectsStats;
        // TRACKING DATA
        attackingEntitiesController = statusEffectEntryToCopy.attackingEntitiesController;

        if (statusEffectEntryToCopy.StatusEffectsStats != null)
        {
            if (statusEffectEntryToCopy.StatusEffectsStats.StatEntryDictionary != null)
            {
                foreach (var statEntry in statusEffectEntryToCopy.StatusEffectsStats.StatEntryDictionary)
                {
                    statusEffectsStats.AddStatEntry(statEntry.Value);
                }
            }
        }
    }
}
