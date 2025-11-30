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

    private IStatEntryManager iStatEntryManager;
    private IAttackEntryManager iAttackEntryManager;
    private IDefenseSequenceManager iDefenseSequenceManager;
    private IStatusEffectEntryManager iStatusEffectEntryManager;
    private IAreaOfEffectEntryManager iAreaOfEffectEntryManager;

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
        iAreaOfEffectEntryManager ??= InterfaceContainer.Request<IAreaOfEffectEntryManager>();
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
                    float attackRange = defaultShortestAttackRange;
                    if (baseAttackControllerToCopy.AttacksEntry.EffectsStats.StatEntryDictionary.TryGetValue(StatName.AttackRangeValue, out var attackRangeValueEntry))
                    {
                        attackRange = attackRangeValueEntry.CurrentValueTotal;
                    }
                    attackRange = Mathf.Max(defaultShortestAttackRange, attackRange);

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
                foreach (var attack in callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary.ToList())
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
            int maxMultistrikeCount = CalculateIntStat(baseAttackControllerToCopy.AttacksEntry.EffectsStats.StatEntryDictionary, StatName.MaxMultistrikeHitsComboValue);

            int currentMultistrikeCount = 1;
            float currentMultistrikeChance = 100.0f;
            while (Random.value <= currentMultistrikeChance && currentMultistrikeCount <= maxMultistrikeCount)
            {
                // CALCULATE THE AMOUNT OF PROJECTILES PER ATTACK TO BE SPAWNED IN.
                int projectileAmountToSpawn = CalculateIntStat(baseAttackControllerToCopy.AttacksEntry.EffectsStats.StatEntryDictionary, StatName.ProjectileCountValue);

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
                        spawnedAttacksController.CopyAttacksControllerData(baseAttackControllerToCopy);

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
                            if (spawnedAttacksController.AttacksEntry.EffectsStats.StatEntryDictionary.TryGetValue(StatName.MultistrikeChancePercent, out var multistrikeChanceEntry))
                            {
                                multistrikeChanceEntry.ModifyBaseValue(StatModificationAction.SetValueTo, -(multistrikeChanceEntry.CurrentValueTotal * 20.0f));
                            }
                        }

                        CheckIfAttackSpawnsAreaOfEffectsOnSpawn(spawnedAttacksController);
                    }
                }

                currentMultistrikeCount++;
                currentMultistrikeChance = CalculateMultistrikeChance(baseAttackControllerToCopy.AttacksEntry.EffectsStats.StatEntryDictionary, currentMultistrikeCount, maxMultistrikeCount);
            }

            if (activateThisAttacksCooldown)
            {
                float attackCooldownTime = defaultMinimumAllowedAttackSpeed;
                if (baseAttackControllerToCopy.AttacksEntry.EffectsStats.StatEntryDictionary.TryGetValue(StatName.AttackCooldownTimer, out var attackCooldownTimerEntry))
                {
                    attackCooldownTime = attackCooldownTimerEntry.CurrentValueTotal;
                }
                attackCooldownTime = Mathf.Max(defaultMinimumAllowedAttackSpeed, attackCooldownTime);

                StartCoroutine(StartAttackCooldown(callingEntitiesController, attacksName, attackCooldownTime));
            }
        }
    }

    private void CheckIfAttackSpawnsAreaOfEffectsOnSpawn(BaseAttackController baseAttackController)
    {
        if (baseAttackController != null)
        {
            if (baseAttackController.AttacksEntry.DoesAnAreaOfEffect)
            {
                if (baseAttackController.AttacksEntry.AreaOfEffectPrefabController != null)
                {
                    if (baseAttackController.AttacksEntry.AreaOfEffectPrefabController.AreaOfEffectsEntry.SpawnsWhenAttackSpawns)
                    {
                        iAreaOfEffectEntryManager.CalculateAndActivateAreaOfEffect(baseAttackController.AttacksEntry.AreaOfEffectPrefabController);
                    }
                }
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

    private int CalculateIntStat(Dictionary<StatName, StatEntry> statEntryDictionary, StatName statsName, bool truncateInt = true)
    {
        if (statEntryDictionary.TryGetValue(statsName, out var statEntryToCalculate))
        {
            float statsTotalValue = statEntryToCalculate.CurrentValueTotal;
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
        }

        return 0;
    }

    private float CalculateMultistrikeChance(Dictionary<StatName, StatEntry> statEntryDictionary, int currentMultistrikeCount, int maxMultistrikeCount)
    {
        float currentComboReduction = 1.0f;
        currentComboReduction -= (float)currentMultistrikeCount / (float)maxMultistrikeCount;

        if (statEntryDictionary.TryGetValue(StatName.MultistrikeChancePercent, out var multistrikeChancePercent))
        {
            float chanceToAttackAgain = multistrikeChancePercent.CurrentValueTotal / 100.0f;
            chanceToAttackAgain *= currentComboReduction;
            chanceToAttackAgain = Mathf.Max(0, chanceToAttackAgain);

            return chanceToAttackAgain;
        }

        return 0.0f;
    }

    public void AttemptToCounterAttack(BaseEntityController callingEntitiesController)
    {
        if (callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary != null)
        {
            foreach (var attack in callingEntitiesController.EntitiesAttackContainer.AttackControllerDictionary.ToList())
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
