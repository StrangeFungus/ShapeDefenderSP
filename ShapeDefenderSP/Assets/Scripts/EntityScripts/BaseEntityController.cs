using System.Collections.Generic;
using UnityEngine;
using SDSPEnums;

[System.Serializable]
public class BaseEntityController : MonoBehaviour
{
    // ENTITY DATA
    [SerializeField] protected string entitiesName;
    public string EntitiesName => entitiesName;
    [SerializeField] protected MaterialType materialTypes;
    [SerializeField] protected EntityMobilityType mobilityTypes;
    [SerializeField] protected EnemyRoleType enemyRoleTypes;
    protected int entitiesLevel = 1;
    [SerializeField] protected float entitiesExperiencePoints = 0;
    public float EntitiesExperiencePoints { get => entitiesExperiencePoints; set => entitiesExperiencePoints = value; }

    [SerializeField] protected float entitiesGoldPieces = 0;
    public float EntitiesGoldPieces { get => entitiesGoldPieces; set => entitiesGoldPieces = value; }

    protected bool isEntityDead = false;
    public bool IsEntityDead { get => isEntityDead; set => isEntityDead = value; }

    // STATS
    [SerializeField] protected StatEntryContainer entitiesStats;
    public StatEntryContainer EntitiesStats { get => entitiesStats; }

    // ATTACKS
    [SerializeField] private AttackEntryContainer entitiesAttackContainer;
    public AttackEntryContainer EntitiesAttackContainer { get => entitiesAttackContainer; set => entitiesAttackContainer = value; }

    protected GameObject currentTarget = null;
    public GameObject CurrentTarget { get { return currentTarget; } }

    // STATUS EFFECTS (ACTIVE / PASSIVE ON ENTITY)
    private Dictionary<StatusEffectName, List<StatusEffectEntry>> entitiesActiveStatusEffects = new();
    public Dictionary<StatusEffectName, List<StatusEffectEntry>> EntitiesActiveStatusEffects { get => entitiesActiveStatusEffects; set => entitiesActiveStatusEffects = value; }

    // ACTION RESTRICTIONS
    [SerializeField] protected bool canEntityMove = true;
    public bool CanEntityMove { get { return canEntityMove; } set { canEntityMove = value; } }
    protected bool isEntityMoving = false;
    public bool IsEntityMoving { get { return isEntityMoving; } }
    [SerializeField] protected bool canEntityAttack = true;
    public bool CanEntityAttack { get { return canEntityAttack; } set { canEntityAttack = value; } }

    public class DefenseCooldownData
    {
        public bool isParryOnCooldown = false;
        public bool isBlockOnCooldown = false;
        public bool isDodgeOnCooldown = false;
        public bool isParryCooldownCoroutineRunning = false;
        public bool isBlockCooldownCoroutineRunning = false;
        public bool isDodgeCooldownCoroutineRunning = false;
    }

    private DefenseCooldownData entitiesDefenseCooldownData = new();
    public DefenseCooldownData EntitiesDefenseCooldownData => entitiesDefenseCooldownData;

    private void Start()
    {

        StartCoroutine(HealthManager.HealthRegenCoroutine(this));

        ActivateCombatCooldownCoroutines(baseEntityController);


        StartCoroutine(AttemptToUseAttacks(baseEntityController));
    }








}
