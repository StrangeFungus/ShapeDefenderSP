using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SDSPEnums;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AttackSequenceManager : MonoBehaviour, IAttackSequenceManager
{
    public static AttackSequenceManager Instance;

    [SerializeField] private float defaultShortestAttackRange = 3.0f;
    [SerializeField] private float defaultMinimumAllowedAttackSpeed = 0.05f;
    [SerializeField] private float defaultMinimumMultistrikeAttackSpeed = 0.05f;
    
    private static readonly float intStatRoundingCheckAmount = 0.98f;

    private Dictionary<BaseAttackController, Dictionary<BaseEntityController, float>> attacksDoingDamageOverTime = new();
    private Dictionary<AreaOfEffectController, Dictionary<BaseEntityController, float>> areaOfEffectsDoingDamageOverTime = new();
    private Dictionary<BaseEntityController, Dictionary<StatusEffectEntry, float>> statusEffectsDoingDamageOverTime = new();

    private IStatEntryManager iStatEntryManager;
    private IAttackEntryManager iAttackEntryManager;
    private IDefenseSequenceManager iDefenseSequenceManager;
    private IStatusEffectEntryManager iStatusEffectEntryManager;

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

        InterfaceContainer.Register<IAttackSequenceManager>(this);
    }

    private void Start()
    {
        iStatEntryManager ??= InterfaceContainer.Request<IStatEntryManager>();
        iAttackEntryManager ??= InterfaceContainer.Request<IAttackEntryManager>();
        iDefenseSequenceManager ??= InterfaceContainer.Request<IDefenseSequenceManager>();
        iStatusEffectEntryManager ??= InterfaceContainer.Request<IStatusEffectEntryManager>();
    }

    public bool AttemptToUseAnAttack(BaseEntityController callingEntitiesController, AttackName attacksName, bool activateThisAttacksCooldown)
    {
        if (callingEntitiesController == null) { Debug.Log($"attackEntryContainer was null, returning..."); return false; }
        if (attacksName == AttackName.None) { Debug.Log($"Attacks Name was {AttackName.None.ToString()}, returning..."); return false; }

        if (callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary.TryGetValue(attacksName, out var baseAttackControllerToCopy))
        {
            if (baseAttackControllerToCopy.IsAttackAbleToBeUsed)
            {
                if (baseAttackControllerToCopy.AttacksEntry.TargetingBehaviour == AttackTargetingBehaviour.NoTargetObject)
                {
                    InstantiateNewAttack(callingEntitiesController, attacksName, activateThisAttacksCooldown);
                }
                else if (callingEntitiesController.CurrentTarget != null)
                {
                    float attackRange = baseAttackControllerToCopy.AttacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.AttackRangeValue);
                    if (attackRange <= defaultShortestAttackRange) { attackRange = defaultShortestAttackRange; }

                    float targetsDistance = Vector2.Distance(callingEntitiesController.transform.localPosition, callingEntitiesController.CurrentTarget.transform.localPosition);
                    if (targetsDistance <= attackRange)
                    {
                        InstantiateNewAttack(callingEntitiesController, attacksName, activateThisAttacksCooldown);
                    }
                }

                return true;
            }
        }

        return false;
    }

    public IEnumerator AttemptToUseAllAttacksCoroutine(BaseEntityController callingEntitiesController)
    {
        while (true)
        {
            if (callingEntitiesController == null) { yield break; }

            if (callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary != null)
            {
                iAttackEntryManager.ProcessPendingAttackUpdates(callingEntitiesController);

                foreach (var attack in callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary)
                {
                    if (callingEntitiesController.gameObject.activeSelf && !callingEntitiesController.IsEntityDead)
                    {
                        if (!callingEntitiesController.CompareTag("ObjectAttackSource") && !callingEntitiesController.CompareTag("WeatherAttackSource"))
                        {
                            float paralyzationStunTimer = iStatusEffectEntryManager.EntityParalyzationCheck(callingEntitiesController);

                            yield return new WaitForSeconds(paralyzationStunTimer);
                            if (!callingEntitiesController.CanEntityAttack) { break; }
                        }

                        AttemptToUseAnAttack(callingEntitiesController, attack.Key, true);

                        yield return new WaitForSeconds(defaultMinimumAllowedAttackSpeed);
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }
    }

    private void InstantiateNewAttack(BaseEntityController callingEntitiesController, AttackName attacksName, bool activateThisAttacksCooldown)
    {
        if (callingEntitiesController == null) { Debug.Log($"callingEntitiesController was null, returning..."); return; }
        if (callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary.Count == 0) { Debug.Log($"AttackControllerDictionary was empty, returning..."); return; }
        
        if (callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary.TryGetValue(attacksName, out var baseAttackControllerToCopy))
        {
            // CALCULATE THE NUMBER OR ATTACKS IN THE MULTI ATTACK THAT WILL BE USED.
            int maxMultistrikeCount = CalculateIntStat(baseAttackControllerToCopy.AttacksEntry.AttacksStats, StatName.MaxMultistrikeHitsComboValue);

            int currentMultistrikeCount = 1;
            float currentMultistrikeChance = 100.0f;
            while (Random.value <= currentMultistrikeChance && currentMultistrikeCount <= maxMultistrikeCount)
            {
                // CALCULATE THE AMOUNT OF PROJECTILES PER ATTACK TO BE SPAWNED IN.
                int projectileAmountToSpawn = CalculateIntStat(baseAttackControllerToCopy.AttacksEntry.AttacksStats, StatName.ProjectileCountValue);

                //CALCULATE THE OFFSET AND POSITION FOR THE UPCOMING SPAWNING PROJECTILES.
                float initialOffset = 1.5f;
                if (projectileAmountToSpawn % 2 == 1)
                {
                    initialOffset = 2.0f;
                }

                for (int i = 0; i < projectileAmountToSpawn; i++)
                {
                    // ATTACKER PARALYZATION CHECK IF ITS NOT AN OBJECT TYPE
                    if (!callingEntitiesController.CompareTag("ObjectAttackSource") && !callingEntitiesController.CompareTag("WeatherAttackSource"))
                    {
                        iStatusEffectEntryManager.EntityParalyzationCheck(callingEntitiesController);

                        if (!callingEntitiesController.CanEntityAttack) { break; }
                    }

                    // !!! SPAWN IN DIRECTIONAL OFFSET SO THAT PROCECTILES SPAWN IN THE CENTER FIRST,
                    // !!! THEN OFFSET TO THE LEFT THE RIGHT UNTIL ALL PROJECTILES HAVE BEEN SPAWNED IN.
                    // Note: We can change the spawning patters later and use an enum to select them from the attack entry settings
                    float direction = -0.25f;
                    if (i % 2 == 0)
                    {
                        direction = 0.25f;
                    }
                    float offsetValue = (i / 2) * initialOffset;
                    float positionOffset = direction * offsetValue;

                    // SPAWN IN THE PROJECTILES HERE BASED OFF THE INITIAL ATTACK PROJECTILE AND ITS ADJUSTED / DATA VALUES.
                    GameObject spawnedAttack = Instantiate(baseAttackControllerToCopy.gameObject, callingEntitiesController.transform.localPosition, callingEntitiesController.transform.rotation);
                    spawnedAttack.SetActive(true);

                    // SPAWNING OFFSET FOR THE FORWARD SPAWNING POSITION IN FRONT OF THE ENTITY.
                    Vector3 spawningOffset = baseAttackControllerToCopy.AttacksEntry.TargetingBehaviour switch
                    {
                        AttackTargetingBehaviour.NoTargetObject => spawnedAttack.transform.localPosition.normalized,
                        _ => callingEntitiesController.CurrentTarget != null ? (callingEntitiesController.CurrentTarget.transform.localPosition - spawnedAttack.transform.localPosition).normalized : spawnedAttack.transform.localPosition.normalized
                    };
                    spawnedAttack.transform.localPosition = new Vector3(spawnedAttack.transform.localPosition.x + positionOffset + spawningOffset.x, spawnedAttack.transform.localPosition.y + positionOffset + spawningOffset.y, spawnedAttack.transform.localPosition.z);
                    float angle = Mathf.Atan2(spawningOffset.y, spawningOffset.x) * Mathf.Rad2Deg - 90f; // Note: Subtract 90 to align with sprites "up"
                    spawnedAttack.transform.rotation = Quaternion.Euler(0, 0, angle);

                    // SETTING NEW ATTACKS DATA AND ENSURING DEFAULT STATE
                    if (spawnedAttack.TryGetComponent<BaseAttackController>(out var spawnedAttacksController))
                    {
                        spawnedAttacksController.enabled = true;
                        spawnedAttacksController.CopyControllerData(baseAttackControllerToCopy);

                        if (spawnedAttack.TryGetComponent<Collider2D>(out var spawnedAttacksCollider2D))
                        {
                            spawnedAttacksCollider2D.enabled = true;
                        }

                        // SO WHEN SPAWNING IN MULTIPLE PROJECTILES, ONLY THE FIRST ATTACK SHOULD ACTIVATE THE MULTISTRIKE CHANCES AND COMBOS.
                        if (i > 0)
                        {
                            // I could add in a stat later that when the chance is over 300% you can form a new stat with projectile
                            // counts that go over 25 to have a small chance to double it based on the total MultistrikeChancePercent
                            // 300% (-100%) = 2% chance (1% per 100% starting at this value)

                            // SETS TO VERY LOW NUMBER TO ENSURE NO CHANCE FOR ERRORS. NEEDED??
                            StatEntry multistrikeChangeEntry = spawnedAttacksController.AttacksEntry.AttacksStats.GetStatEntry(StatName.MultistrikeChancePercent);
                            multistrikeChangeEntry.ModifyBaseValue(StatModificationAction.SetValueTo, -(multistrikeChangeEntry.StatsTotalValue * 20.0f));
                        }
                    }
                }

                currentMultistrikeCount++;
                currentMultistrikeChance = CalculateMultistrikeChance(baseAttackControllerToCopy.AttacksEntry.AttacksStats, currentMultistrikeCount, maxMultistrikeCount);
            }

            if (activateThisAttacksCooldown)
            {
                float attackCooldownTime = baseAttackControllerToCopy.AttacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.AttackCooldownTimer);
                attackCooldownTime = Mathf.Max(defaultMinimumAllowedAttackSpeed, attackCooldownTime);

                StartCoroutine(StartAttackCooldown(callingEntitiesController, attacksName, attackCooldownTime));
            }
        }
    }

    private IEnumerator StartAttackCooldown(BaseEntityController entityCallingRequest, AttackName attackName, float attackCooldownTime)
    {
        iAttackEntryManager.ChangeAttackUsability(attackName, entityCallingRequest.EntitiesAttackContainer, false);

        while (attackCooldownTime > 0.0f)
        {
            attackCooldownTime -= Time.deltaTime;
            yield return null;
        }

        iAttackEntryManager.ChangeAttackUsability(attackName, entityCallingRequest.EntitiesAttackContainer, true);
    }

    private int CalculateIntStat(StatEntryContainer statEntryContainer, StatName statsName, bool truncateInt = true)
    {
        float statsTotalValue = statEntryContainer.GetStatEntriesTotalValue(statsName);
        if (statsTotalValue > 0.0f)
        {
            int wholeValue = (int)statsTotalValue;
            float decimalValue = statsTotalValue - wholeValue;

            if (!truncateInt)
            {
                if (decimalValue >= intStatRoundingCheckAmount)
                {
                    wholeValue += 1;
                }
            }
            
            return wholeValue;
        }

        return 0;
    }

    private float CalculateMultistrikeChance(StatEntryContainer statEntryContainer, int currentMultistrikeCount, int maxMultistrikeCount)
    {
        float currentComboReduction = 1.0f;
        currentComboReduction -= (float)currentMultistrikeCount / (float)maxMultistrikeCount;

        float chanceToAttackAgain = statEntryContainer.GetStatEntriesTotalValue(StatName.MultistrikeChancePercent) / 100.0f;
        chanceToAttackAgain *= currentComboReduction;
        chanceToAttackAgain = Mathf.Max(0, chanceToAttackAgain);

        return chanceToAttackAgain;
    }

    // We need an attack sequence for dealing damage to entities over time from projectiles/area of effects
    // as well as dealing damage over time to entities via status effects.

    // what do I need in order to track projectiles/area of effects as well as status effects damaging entities. 
    // for projectiles and area of effects we can draw from the colliders current populated list of entities
    // so, damage them as we add them to the list and track the time they were damaged. if the projectile or aoe is null or has finished dealing damage we can clear 
    // the entry from the list. This will be easier since we can track the controller/

    // status effects will need more data to track them correctly. Entity to target, status effects on entity, attacker and so on since one entity can apply multiple kinds of dots 
    
    
    // Attack to track, entities to damage, timestap they were last damaged
    // Attacks track entities inside collider

    public void ApplyDamageOverTime(BaseAttackController baseAttackController)
    {
        if (baseAttackController == null) { return; }
        if (!attacksDoingDamageOverTime.Contains(baseAttackController))
        {
            attacksDoingDamageOverTime.Add(baseAttackController);
        }
    }

    public void ApplyDamageOverTime(BaseEntityController targetEntitiesController,
       StatusEffectEntryContainer statusEffectEntryContainerToApply, StatusEffectName statusEffectsName)
    {
        if (targetEntitiesController == null) { return; }
        if (statusEffectEntryContainerToApply == null) { return; }
        if (statusEffectsName == StatusEffectName.Default) { return; }

        if (!statusEffectsDoingDamageOverTime.ContainsKey(targetEntitiesController))
        {
            statusEffectsDoingDamageOverTime.Add(targetEntitiesController, new());
        }

        if (!statusEffectsDoingDamageOverTime[targetEntitiesController].Contains(statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName]))
        {
            statusEffectsDoingDamageOverTime[targetEntitiesController].Add(statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName]);
        }
    }

    private IEnumerator DotCoroutineForAttackControllers()
    {
        while (true)
        {
            foreach (var attackController in attacksDoingDamageOverTime)
            {
                if (attackController == null) { break; }
                if (attackController.HasMadeFinalHit) { break; }

                HashSet<BaseEntityController> targetsToDamage = attackController.TargetTrackingData.TargetsToDoDamageTo;
                foreach (var target in targetsToDamage)
                {
                    float waitTime = attackController.AttacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.AttackCooldownTimer);
                    yield return new WaitForSeconds(waitTime);

                    iDefenseSequenceManager.AttemptToDamageTarget(attackController, target);
                }
            }
        }
    }

    private IEnumerator DotCoroutineForStatusEffects()
    {
        while (true)
        {
            foreach (var targetEntity in statusEffectsDoingDamageOverTime)
            {
                if (targetEntity.Key == null) { break; }
                if (targetEntity.Value == null) { break; }

                foreach (var statusEffect in targetEntity.Value)
                {
                    if (statusEffect != null)
                    {
                        float waitTime = statusEffect.StatusEffectsStats.GetStatEntriesTotalValue(StatName.AttackCooldownTimer);
                        yield return new WaitForSeconds(waitTime);

                        iDefenseSequenceManager.AttemptToDamageTarget(statusEffect, targetEntity.Key);
                    }
                }
            }
        }
    }

    public void AttemptToCounterAttack(BaseEntityController callingEntitiesController)
    {
        if (callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary != null)
        {
            iAttackEntryManager.ProcessPendingAttackUpdates(callingEntitiesController);

            foreach (var attack in callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary)
            {
                if (callingEntitiesController.gameObject.activeSelf && !callingEntitiesController.IsEntityDead)
                {
                    if (!callingEntitiesController.CompareTag("ObjectAttackSource") && !callingEntitiesController.CompareTag("WeatherAttackSource"))
                    {
                        float paralyzationStunTimer = iStatusEffectEntryManager.EntityParalyzationCheck(callingEntitiesController);

                        while (paralyzationStunTimer > 0.0f)
                        {
                            paralyzationStunTimer -= Time.deltaTime;
                        }

                        if (!callingEntitiesController.CanEntityAttack) { break; }
                    }

                    bool attemptedTheAttack = AttemptToUseAnAttack(callingEntitiesController, attack.Key, true);

                    if (attemptedTheAttack) { break; }
                }
            }
        }
    }
}
