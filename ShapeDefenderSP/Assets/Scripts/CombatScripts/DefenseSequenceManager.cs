using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SDSPEnums;
using UnityEngine;

public class DefenseSequenceManager : MonoBehaviour, IDefenseSequenceManager
{
    public static DefenseSequenceManager Instance;

    [SerializeField] private float defaultMinimumAllowedDefenseActionDelay = 0.05f;
    private static float parriedOrReflectedIncomingDamageReduction = 0.65f;
    private static float parriedAttackReflectionAngle = 45.0f;
    private static float reflectedAttackReflectionAngle = 45.0f;

    private Dictionary<BaseAttackController, Dictionary<BaseEntityController, float>> attacksDoingDamageOverTime = new();
    private Dictionary<AreaOfEffectController, Dictionary<BaseEntityController, float>> areaOfEffectsDoingDamageOverTime = new();
    private Dictionary<BaseEntityController, Dictionary<StatusEffectEntry, float>> statusEffectsDoingDamageOverTime = new();

    private float damageOverTimeMSBudget = 4.0f;

    private IAttackSequenceManager iAttackSequenceManager;
    private IAreaOfEffectEntryManager iAreaOfEffectEntryManager;
    private IStatusEffectEntryManager iStatusEffectEntryManager;
    private IHealthManager iHealthManager;

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

        InterfaceContainer.Register<IDefenseSequenceManager>(this);
    }

    private void Start()
    {
        iAttackSequenceManager ??= InterfaceContainer.Request<IAttackSequenceManager>();
        iAreaOfEffectEntryManager ??= InterfaceContainer.Request<IAreaOfEffectEntryManager>();
        iStatusEffectEntryManager ??= InterfaceContainer.Request<IStatusEffectEntryManager>();
        iHealthManager ??= InterfaceContainer.Request<IHealthManager>();
    }

    public void ActivateCombatCooldownCoroutines(BaseEntityController baseEntityController)
    {
        if (baseEntityController.gameObject.activeSelf && !baseEntityController.IsEntityDead)
        {
            if (!baseEntityController.EntitiesDefenseCooldownData.isParryOnCooldown && !baseEntityController.EntitiesDefenseCooldownData.isParryCooldownCoroutineRunning)
            {
                float cooldownSeconds = baseEntityController.EntitiesStats.GetStatEntriesTotalValue(StatName.ParryCooldownTimer);
                StartCoroutine(DefenseCooldownCoroutine(baseEntityController, cooldownSeconds, StatName.ParryCooldownTimer));
            }

            if (!baseEntityController.EntitiesDefenseCooldownData.isBlockOnCooldown && !baseEntityController.EntitiesDefenseCooldownData.isBlockCooldownCoroutineRunning)
            {
                float cooldownSeconds = baseEntityController.EntitiesStats.GetStatEntriesTotalValue(StatName.BlockCooldownTimer);
                StartCoroutine(DefenseCooldownCoroutine(baseEntityController, cooldownSeconds, StatName.BlockCooldownTimer));
            }

            if (!baseEntityController.EntitiesDefenseCooldownData.isDodgeOnCooldown && !baseEntityController.EntitiesDefenseCooldownData.isDodgeCooldownCoroutineRunning)
            {
                float cooldownSeconds = baseEntityController.EntitiesStats.GetStatEntriesTotalValue(StatName.DodgeCooldownTimer);
                StartCoroutine(DefenseCooldownCoroutine(baseEntityController, cooldownSeconds, StatName.DodgeCooldownTimer));
            }
        }
    }

    private IEnumerator DefenseCooldownCoroutine(BaseEntityController baseEntityController, float cooldownTimer, StatName statsName)
    {
        if (baseEntityController == null) { yield break; }

        if (cooldownTimer < defaultMinimumAllowedDefenseActionDelay)
        { cooldownTimer = defaultMinimumAllowedDefenseActionDelay; }

        if (statsName == StatName.ParryCooldownTimer)
        {
            baseEntityController.EntitiesDefenseCooldownData.isParryCooldownCoroutineRunning = true;
        }
        else if (statsName == StatName.BlockCooldownTimer)
        {
            baseEntityController.EntitiesDefenseCooldownData.isBlockCooldownCoroutineRunning = true;
        }
        else if (statsName == StatName.DodgeCooldownTimer)
        {
            baseEntityController.EntitiesDefenseCooldownData.isDodgeCooldownCoroutineRunning = true;
        }

        float timer = cooldownTimer;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            yield return new WaitForSeconds(defaultMinimumAllowedDefenseActionDelay);
        }

        if (baseEntityController == null) { yield break; }

        if (statsName == StatName.ParryCooldownTimer)
        {
            baseEntityController.EntitiesDefenseCooldownData.isParryCooldownCoroutineRunning = false;
            baseEntityController.EntitiesDefenseCooldownData.isParryOnCooldown = false;
        }
        else if (statsName == StatName.BlockCooldownTimer)
        {
            baseEntityController.EntitiesDefenseCooldownData.isBlockCooldownCoroutineRunning = false;
            baseEntityController.EntitiesDefenseCooldownData.isBlockOnCooldown = false;
        }
        else if (statsName == StatName.DodgeCooldownTimer)
        {
            baseEntityController.EntitiesDefenseCooldownData.isDodgeCooldownCoroutineRunning = false;
            baseEntityController.EntitiesDefenseCooldownData.isDodgeOnCooldown = false;
        }
    }

    public void ApplyDamageOverTime(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController)
    {
        if (baseAttackController == null) { return; }
        if (targetEntitiesController == null) { return; }

        if (!attacksDoingDamageOverTime.ContainsKey(baseAttackController))
        {
            attacksDoingDamageOverTime.Add(baseAttackController, new());
        }

        if (!attacksDoingDamageOverTime[baseAttackController].ContainsKey(targetEntitiesController))
        {
            attacksDoingDamageOverTime[baseAttackController].Add(targetEntitiesController, new());
        }

        attacksDoingDamageOverTime[baseAttackController][targetEntitiesController] = Time.time;
    }

    public void ApplyDamageOverTime(AreaOfEffectController areaOfEffectController, BaseEntityController targetEntitiesController)
    {
        if (areaOfEffectController == null) { return; }
        if (targetEntitiesController == null) { return; }

        if (!areaOfEffectsDoingDamageOverTime.ContainsKey(areaOfEffectController))
        {
            areaOfEffectsDoingDamageOverTime.Add(areaOfEffectController, new());
        }

        if (!areaOfEffectsDoingDamageOverTime[areaOfEffectController].ContainsKey(targetEntitiesController))
        {
            areaOfEffectsDoingDamageOverTime[areaOfEffectController].Add(targetEntitiesController, new());
        }

        areaOfEffectsDoingDamageOverTime[areaOfEffectController][targetEntitiesController] = Time.time;
    }

    public void ApplyDamageOverTime(BaseEntityController targetEntitiesController,
       StatusEffectEntryContainer statusEffectEntryContainerToApply, StatusEffectName statusEffectsName)
    {
        if (targetEntitiesController == null) { return; }
        if (statusEffectEntryContainerToApply == null) { return; }
        if (statusEffectsName == StatusEffectName.Default) { return; }
        if (!statusEffectEntryContainerToApply.StatusEffectsDictionary.ContainsKey(statusEffectsName)) { return; }

        if (!statusEffectsDoingDamageOverTime.ContainsKey(targetEntitiesController))
        {
            statusEffectsDoingDamageOverTime.Add(targetEntitiesController, new());
        }

        if (!statusEffectsDoingDamageOverTime[targetEntitiesController].ContainsKey(statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName]))
        {
            statusEffectsDoingDamageOverTime[targetEntitiesController].Add(statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName], new());
        }

        statusEffectsDoingDamageOverTime[targetEntitiesController][statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName]] = Time.time;
    }

    public void RemoveDamageOverTime(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController)
    {
        if (baseAttackController == null) { return; }
        if (targetEntitiesController == null) { return; }

        if (attacksDoingDamageOverTime.ContainsKey(baseAttackController))
        {
            if (attacksDoingDamageOverTime[baseAttackController].ContainsKey(targetEntitiesController))
            {
                attacksDoingDamageOverTime[baseAttackController].Remove(targetEntitiesController);
            }
        }
    }

    public void RemoveDamageOverTime(AreaOfEffectController areaOfEffectController, BaseEntityController targetEntitiesController)
    {
        if (areaOfEffectController == null) { return; }
        if (targetEntitiesController == null) { return; }

        if (areaOfEffectsDoingDamageOverTime.ContainsKey(areaOfEffectController))
        {
            if (areaOfEffectsDoingDamageOverTime[areaOfEffectController].ContainsKey(targetEntitiesController))
            {
                areaOfEffectsDoingDamageOverTime[areaOfEffectController].Remove(targetEntitiesController);
            }
        }
    }

    public void RemoveDamageOverTime(BaseEntityController targetEntitiesController,
       StatusEffectEntryContainer statusEffectEntryContainerToApply, StatusEffectName statusEffectsName)
    {
        if (targetEntitiesController == null) { return; }
        if (statusEffectEntryContainerToApply == null) { return; }
        if (statusEffectsName == StatusEffectName.Default) { return; }
        if (!statusEffectEntryContainerToApply.StatusEffectsDictionary.ContainsKey(statusEffectsName)) { return; }

        if (statusEffectsDoingDamageOverTime.ContainsKey(targetEntitiesController))
        {
            if (statusEffectsDoingDamageOverTime[targetEntitiesController].ContainsKey(statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName]))
            {
                statusEffectsDoingDamageOverTime[targetEntitiesController].Remove(statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName]);
            }
        }
    }

    public void RemoveDamageOverTime(BaseEntityController targetEntitiesController)
    {
        if (targetEntitiesController == null) { return; }

        if (statusEffectsDoingDamageOverTime.ContainsKey(targetEntitiesController))
        {
            statusEffectsDoingDamageOverTime.Remove(targetEntitiesController);
        }
    }

    private IEnumerator ProjectileDamageOverTimeCoroutine()
    {
        var currentFrameStopwatch = new Stopwatch();
        int objectsProcessed = 0;

        while (true)
        {
            if (GameStateManager.IsGamePaused)
            {
                yield return null;
                continue;
            }

            currentFrameStopwatch.Restart();

            foreach (var attackDictEntry in attacksDoingDamageOverTime.ToList())
            {
                if (attackDictEntry.Key == null || attackDictEntry.Key.HasMadeFinalHit)
                {
                    continue;
                }

                foreach (var target in attackDictEntry.Value)
                {
                    if (target.Key == null)
                    {
                        continue;
                    }

                    float waitTime = attackDictEntry.Key.AttacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.DamageOverTimeHitRateTimer);

                    if (Time.time - target.Value >= waitTime)
                    {
                        AttemptToDamageTarget(attackDictEntry.Key, target.Key);
                        attacksDoingDamageOverTime[attackDictEntry.Key][target.Key] = Time.time;
                    }

                    objectsProcessed++;
                }

                objectsProcessed++;
            }

            if (currentFrameStopwatch.Elapsed.TotalMilliseconds >= damageOverTimeMSBudget)
            {
                yield return null;
                currentFrameStopwatch.Restart();
            }
        }
    }


    private IEnumerator AreaOfEffectDamageOverTimeCoroutine()
    {
        var currentFrameStopwatch = new Stopwatch();
        int objectsProcessed = 0;

        while (true)
        {
            if (GameStateManager.IsGamePaused)
            {
                yield return null;
                continue;
            }

            currentFrameStopwatch.Restart();

            foreach (var areaOfEffect in areaOfEffectsDoingDamageOverTime.ToList())
            {
                if (areaOfEffect.Key == null || areaOfEffect.Key.FinishLifecycle)
                {
                    continue;
                }

                foreach (var target in areaOfEffect.Value)
                {
                    if (target.Key == null)
                    {
                        continue;
                    }

                    float waitTime = areaOfEffect.Key.AreaOfEffectsEntry.AreaOfEffectsStats.GetStatEntriesTotalValue(StatName.DamageOverTimeHitRateTimer);

                    if (Time.time - target.Value >= waitTime)
                    {
                        AttemptToDamageTarget(areaOfEffect.Key, target.Key);
                        areaOfEffectsDoingDamageOverTime[areaOfEffect.Key][target.Key] = Time.time;
                    }

                    objectsProcessed++;
                }

                objectsProcessed++;
            }

            if (currentFrameStopwatch.Elapsed.TotalMilliseconds >= damageOverTimeMSBudget)
            {
                yield return null;
                currentFrameStopwatch.Restart();
            }
        }
    }

    private IEnumerator StatusEffectsDamageOverTimeCoroutine()
    {
        var currentFrameStopwatch = new Stopwatch();
        int objectsProcessed = 0;

        while (true)
        {
            if (GameStateManager.IsGamePaused)
            {
                yield return null;
                continue;
            }

            currentFrameStopwatch.Restart();

            foreach (var target in statusEffectsDoingDamageOverTime.ToList())
            {
                if (target.Key == null)
                {
                    continue;
                }

                foreach (var statusEffect in target.Value)
                {
                    if (statusEffect.Key == null)
                    {
                        continue;
                    }

                    float waitTime = statusEffect.Key.StatusEffectsStats.GetStatEntriesTotalValue(StatName.DamageOverTimeHitRateTimer);

                    if (Time.time - statusEffect.Value >= waitTime)
                    {
                        AttemptToDamageTarget(statusEffect.Key, target.Key);
                        statusEffectsDoingDamageOverTime[target.Key][statusEffect.Key] = Time.time;
                    }

                    objectsProcessed++;
                }

                objectsProcessed++;
            }

            if (currentFrameStopwatch.Elapsed.TotalMilliseconds >= damageOverTimeMSBudget)
            {
                yield return null;
                currentFrameStopwatch.Restart();
            }
        }
    }

    public void AttemptToDamageTarget(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController)
    {
        CalculateAttackSequencePrechecks(targetEntitiesController, out float targetsCritHitResist, out float targetsCritDamResist);
        CalculateAttackSequence(baseAttackController.AttacksEntry.AttacksStats, targetsCritHitResist, targetsCritDamResist, out float attacksDamageAmount, out bool wasTheAttackACrit);
        AttemptToDefendDamage(baseAttackController, targetEntitiesController, attacksDamageAmount, wasTheAttackACrit);
    }

    public void AttemptToDamageTarget(AreaOfEffectController areaOfEffectController, BaseEntityController targetEntitiesController)
    {
        CalculateAttackSequencePrechecks(targetEntitiesController, out float targetsCritHitResist, out float targetsCritDamResist);
        CalculateAttackSequence(areaOfEffectController.AreaOfEffectsEntry.AreaOfEffectsStats, targetsCritHitResist, targetsCritDamResist, out float attacksDamageAmount, out bool wasTheAttackACrit);
        AttemptToDefendDamage(areaOfEffectController, targetEntitiesController, attacksDamageAmount, wasTheAttackACrit);
    }

    public void AttemptToDamageTarget(StatusEffectEntry statusEffectEntry, BaseEntityController targetEntitiesController)
    {
        CalculateAttackSequencePrechecks(targetEntitiesController, out float targetsCritHitResist, out float targetsCritDamResist);
        CalculateAttackSequence(statusEffectEntry.StatusEffectsStats, targetsCritHitResist, targetsCritDamResist, out float attacksDamageAmount, out bool wasTheAttackACrit);
        AttemptToDefendDamage(statusEffectEntry, targetEntitiesController, attacksDamageAmount, wasTheAttackACrit);
    }

    private void CalculateAttackSequencePrechecks(BaseEntityController targetEntitiesController,
        out float critHitChanceResist, out float critHitDamResist)
    {
        if (targetEntitiesController != null)
        {
            critHitChanceResist = CalculateCriticalHitChanceResistance(targetEntitiesController.EntitiesStats);
            critHitDamResist = CalculateCriticalHitDamageResistance(targetEntitiesController.EntitiesStats);
        }
        else
        {
            critHitChanceResist = 0;
            critHitDamResist = 0;
        }
    }

    private void CalculateAttackSequence(StatEntryContainer statEntryContainerCausingDamage,
    float targetsCritHitChanceResist, float targetsCritHitDamResist, 
    out float attackDamage, out bool wasAttackACriticalHit)
    {
        attackDamage = 0.0f;
        wasAttackACriticalHit = false;
        if (statEntryContainerCausingDamage != null)
        {
            attackDamage = CalculateBaseDamage(statEntryContainerCausingDamage);
            float criticalHitChance = CalculateCriticalHitChance(statEntryContainerCausingDamage);

            if (Random.value < (criticalHitChance - targetsCritHitChanceResist))
            {
                attackDamage = CalculateCriticalHitDamage(statEntryContainerCausingDamage, attackDamage);
                float resistedDamage = attackDamage * (1f + targetsCritHitDamResist);
                attackDamage -= resistedDamage;
                wasAttackACriticalHit = true;
            }
        }

        if (attackDamage <= 0.0f)
        {
            UnityEngine.Debug.Log($"Attack damage too low to cause damage! ({attackDamage})");
            attackDamage = 0.0f;
        }
    }

    private float CalculateBaseDamage(StatEntryContainer statEntryContainer)
    {
        float minAttackDam = statEntryContainer.GetStatEntriesTotalValue(StatName.MinimumAttackDamageValue);
        minAttackDam = Mathf.Max(0, minAttackDam);

        float maxAttackDam = statEntryContainer.GetStatEntriesTotalValue(StatName.MaximumAttackDamageValue);
        maxAttackDam = Mathf.Max(0, maxAttackDam);

        if (minAttackDam > maxAttackDam) { minAttackDam = maxAttackDam; }
        float damageToApply = Random.Range(minAttackDam, maxAttackDam);

        return damageToApply;
    }

    private float CalculateCriticalHitChance(StatEntryContainer statEntryContainer)
    {
        float chanceToCrit = statEntryContainer.GetStatEntriesTotalValue(StatName.CriticalHitChancePercent) / 100.0f;
        chanceToCrit = Mathf.Max(0, chanceToCrit);

        return chanceToCrit;
    }

    private float CalculateCriticalHitDamage(StatEntryContainer statEntryContainer, float damage)
    {
        float critHitDam = statEntryContainer.GetStatEntriesTotalValue(StatName.CriticalHitDamageMultiplier) / 100.0f;
        critHitDam = Mathf.Max(0, critHitDam);

        damage *= (1f + critHitDam);

        return damage;
    }

    public void AttemptToDefendDamage(MonoBehaviour attackingMonobehaviour, BaseEntityController targetEntitiesController, float attackDamage, bool isDamageCritical)
    {
        if (attackingMonobehaviour == null) { return; }
        DamageType damageTypes = DamageType.Default;
        bool isAttackBlockable = false;
        bool isAttackAPhysicalObject = false;
        StatusEffectEntryContainer attacksStatusEffectContainer = null;
        StatEntryContainer attacksStatEntryContainer = null;

        if (attackingMonobehaviour is BaseAttackController baseAttackController)
        {
            damageTypes = baseAttackController.AttacksEntry.DamageTypes;
            isAttackBlockable = baseAttackController.AttacksEntry.IsProjectileBlockable;
            isAttackAPhysicalObject = baseAttackController.AttacksEntry.IsProjectilePhysicalObject;
            attacksStatusEffectContainer = baseAttackController.AttacksEntry.AttacksStatusEffects;
            attacksStatEntryContainer = baseAttackController.AttacksEntry.AttacksStats;
        }
        else if (attackingMonobehaviour is AreaOfEffectController areaOfEffectController)
        {
            damageTypes = areaOfEffectController.AreaOfEffectsEntry.DamageTypes;
            isAttackBlockable = areaOfEffectController.AreaOfEffectsEntry.IsAreaOfEffectBlockable;
            isAttackAPhysicalObject = areaOfEffectController.AreaOfEffectsEntry.IsAreaOfEffectPhysicalObject;
            attacksStatusEffectContainer = areaOfEffectController.AreaOfEffectsEntry.AreaOfEffectsStatusEffects;
            attacksStatEntryContainer = areaOfEffectController.AreaOfEffectsEntry.AreaOfEffectsStats;
        }
        else if (attackingMonobehaviour is StatusEffectEntry statusEffectEntry)
        {
            damageTypes = statusEffectEntry.DamageTypes;
            isAttackBlockable = statusEffectEntry.IsDamageBlockable;
            isAttackAPhysicalObject = statusEffectEntry.IsPhysicalObject;
            attacksStatEntryContainer = statusEffectEntry.StatusEffectsStats;
        }
        else
        {
            return;
        }

        if (damageTypes.HasFlag(DamageType.Default) || attacksStatEntryContainer == null) { return; }

        if (targetEntitiesController != null)
        {
            bool wasAbleToParry = false;
            bool wasAbleToBlock = false;
            bool wasAbleToDodge = false;

            if (!damageTypes.HasFlag(DamageType.True))
            {
                if (isAttackBlockable)
                {
                    if (isAttackAPhysicalObject)
                    {
                        wasAbleToParry = AttemptToParry(attackingMonobehaviour.gameObject, targetEntitiesController, ref attackDamage);
                    }

                    if (!wasAbleToParry && attacksStatEntryContainer != null)
                    {
                        wasAbleToBlock = AttemptToBlock(attacksStatEntryContainer, targetEntitiesController, ref attackDamage, attackingMonobehaviour.gameObject);
                    }
                }

                if (!wasAbleToBlock)
                {
                    wasAbleToDodge = AttemptToDodge(targetEntitiesController);
                }
            }

            if (!wasAbleToDodge)
            {
                if (attackDamage <= 0)
                {
                    UnityEngine.Debug.Log($"Damage is too low... {attackDamage}");

                    //iFloatingTextManager.GenerateFloatingText(defendingEntityController.transform.position, attackDamage.ToString(), DefenseAction.ArmorNullifiedDamage.ToString());
                }
                else
                {
                    float targetEntitiesModifiedArmorValue = targetEntitiesController.EntitiesStats.GetStatEntriesTotalValue(StatName.ArmorValue);

                    if (attacksStatEntryContainer != null)
                    {
                        targetEntitiesModifiedArmorValue = CalculateTargetsArmorValue(attacksStatEntryContainer, targetEntitiesController);
                    }

                    iHealthManager.ApplyDamageToTarget(attackingMonobehaviour, attackDamage, isDamageCritical, targetEntitiesController, targetEntitiesModifiedArmorValue);

                    CheckIfAttackSpawnsAreaOfEffectsOnHit(attackingMonobehaviour, attackDamage, isDamageCritical);

                    if (attacksStatusEffectContainer != null)
                    {
                        CheckIfAttackAppliesStatusEffects(attacksStatusEffectContainer, attacksStatEntryContainer, targetEntitiesController);
                    }
                }
            }
        }
    }

    private bool AttemptToParry(GameObject attackingObject, BaseEntityController targetEntitiesController,
    ref float attackDamage)
    {
        if (targetEntitiesController != null)
        {
            float parryChance = CalculateParryChance(targetEntitiesController.EntitiesStats);

            if (!targetEntitiesController.EntitiesDefenseCooldownData.isParryOnCooldown && Random.value < parryChance)
            {
                targetEntitiesController.EntitiesDefenseCooldownData.isParryOnCooldown = true;

                float counterAttackChance = CalculateCounterAttackChance(targetEntitiesController.EntitiesStats);
                if (Random.value < counterAttackChance)
                {
                    iAttackSequenceManager.AttemptToCounterAttack(targetEntitiesController);

                    //iFloatingTextManager.GenerateFloatingText(defendingEntityController.transform.position, DefenseAction.CounteredAttack.ToString().Replace("", " "), DefenseAction.CounteredAttack.ToString());
                }
                else
                {
                    //iFloatingTextManager.GenerateFloatingText(defendingEntityController.transform.position, DefenseAction.ParriedAttack.ToString().Replace("", " "), DefenseAction.ParriedAttack.ToString());
                }

                if (attackingObject != null)
                {
                    attackingObject.transform.Rotate(0, 0, Random.Range(-parriedAttackReflectionAngle, parriedAttackReflectionAngle));
                }

                attackDamage *= parriedOrReflectedIncomingDamageReduction;

                return true;
            }
        }

        return false;
    }

    private bool AttemptToBlock(StatEntryContainer attacksStatContainer, BaseEntityController targetEntitiesController, ref float attackDamage, GameObject attackingObject = null)
    {
        if (targetEntitiesController != null)
        {
            float blockChance = CalculateBlockChance(targetEntitiesController.EntitiesStats);

            if (!targetEntitiesController.EntitiesDefenseCooldownData.isBlockOnCooldown && Random.value <= blockChance)
            {
                targetEntitiesController.EntitiesDefenseCooldownData.isBlockOnCooldown = true;

                float reflectAttackChance = CalculateReflectAttackChance(targetEntitiesController.EntitiesStats);
                if (Random.value <= reflectAttackChance && attackingObject != null)
                {
                    attackDamage *= parriedOrReflectedIncomingDamageReduction;

                    if (attacksStatContainer != null)
                    {
                        float reflectedAttackDamageMultiplier = attacksStatContainer.GetStatEntriesTotalValue(StatName.ReflectedAttackDamageMultiplier) / 100.0f;
                        
                        StatEntry minAttackDamageEntry = attacksStatContainer.GetStatEntry(StatName.MinimumAttackDamageValue);
                        minAttackDamageEntry?.ModifyBaseValue(StatModificationAction.SetValueTo, minAttackDamageEntry.StatsTotalValue * reflectedAttackDamageMultiplier);
                        
                        StatEntry maxAttackDamageEntry = attacksStatContainer.GetStatEntry(StatName.MaximumAttackDamageValue);
                        maxAttackDamageEntry?.ModifyBaseValue(StatModificationAction.SetValueTo, maxAttackDamageEntry.StatsTotalValue * reflectedAttackDamageMultiplier);

                        attackingObject.transform.Rotate(0, 0, Random.Range(-45.0f, 45.0f));

                        if (attackingObject.TryGetComponent<BaseAttackController>(out var baseAttackController))
                        {
                            baseAttackController.TimesAttackWasReflected++;
                            baseAttackController.StartingLocation = baseAttackController.transform.position;
                        }
                    }

                    //iFloatingTextManager.GenerateFloatingText(defendingEntityController.transform.position, DefenseAction.ReflectedAttackDamage.ToString().Replace("", " "), DefenseAction.ReflectedAttackDamage.ToString());
                }
                else
                {
                    float blockedAmount = CalculateBlockedAmount(targetEntitiesController.EntitiesStats);
                    attackDamage -= blockedAmount;

                    //iFloatingTextManager.GenerateFloatingText(defendingEntityController.transform.position, DefenseAction.BlockedAttack.ToString().Replace("", " "), DefenseAction.BlockedAttack.ToString());
                }

                return true;
            }
        }

        return false;
    }

    private bool AttemptToDodge(BaseEntityController targetEntitiesController)
    {
        if (targetEntitiesController != null)
        {
            float dodgeChance = CalculateDodgeChance(targetEntitiesController.EntitiesStats);

            if (!targetEntitiesController.EntitiesDefenseCooldownData.isDodgeOnCooldown && Random.value <= dodgeChance)
            {
                targetEntitiesController.EntitiesDefenseCooldownData.isDodgeOnCooldown = true;

                // I COULD ADD A DODGE ANIMATIONS FADE AND MOVEMENT DELAY HERE
                //iFloatingTextManager.GenerateFloatingText(defendingEntityController.transform.position, DefenseAction.DodgedAttack.ToString().Replace("", " "), DefenseAction.DodgedAttack.ToString());
                return true;
            }
        }

        return false;
    }

    private void CheckIfAttackSpawnsAreaOfEffectsOnHit(MonoBehaviour attacksMonobehaviour, float attackDamage, bool isDamageCritical)
    {
        if (attacksMonobehaviour != null)
        {
            bool doesAttackGenerateAOEOnHit = false;
            AreaOfEffectController areaOfEffectControllerToSpawn = null;

            if (attacksMonobehaviour is BaseAttackController baseAttackController)
            {
                doesAttackGenerateAOEOnHit = baseAttackController.AttacksEntry.DoesAnAreaOfEffect;
                areaOfEffectControllerToSpawn = baseAttackController.AttacksEntry.AreaOfEffectPrefabController;
            }
            else if (attacksMonobehaviour is AreaOfEffectController areaOfEffectController)
            {
                doesAttackGenerateAOEOnHit = areaOfEffectController.AreaOfEffectsEntry.SpawnsWhenAttackHits;
                areaOfEffectControllerToSpawn = areaOfEffectController;
            }
            else if (attacksMonobehaviour is StatusEffectEntry statusEffectEntry)
            {
                doesAttackGenerateAOEOnHit = statusEffectEntry.DoesAnAreaOfEffect;
                areaOfEffectControllerToSpawn = statusEffectEntry.AreaOfEffectPrefabController;
            }

            if (doesAttackGenerateAOEOnHit)
            {
                if (areaOfEffectControllerToSpawn != null)
                {
                    if (areaOfEffectControllerToSpawn.AreaOfEffectsEntry.SpawnsWhenAttackHits)
                    {
                        iAreaOfEffectEntryManager.CalculateAndActivateAreaOfEffect(areaOfEffectControllerToSpawn);
                    }
                }
            }
        }
    }

    private void CheckIfAttackAppliesStatusEffects(StatusEffectEntryContainer attacksStatusEffectContainer, StatEntryContainer attacksStatEntryContainer, BaseEntityController targetEntitiesController)
    {
        if (attacksStatusEffectContainer != null && attacksStatEntryContainer  != null && targetEntitiesController != null)
        {
            if (attacksStatusEffectContainer.StatusEffectsDictionary != null)
            {
                foreach (var debuff in attacksStatusEffectContainer.StatusEffectsDictionary)
                {
                    float debuffChance = attacksStatEntryContainer.GetStatEntriesTotalValue(StatName.StatusEffectInflictionChancePercent) / 100.0f;
                    float statusEffectInflictionResistance = attacksStatEntryContainer.GetStatEntriesTotalValue(StatName.StatusEffectInflictionResistanceValue) / 100.0f;
                    debuffChance = Mathf.Max(0, debuffChance - statusEffectInflictionResistance);

                    if (Random.value < debuffChance)
                    {
                        iStatusEffectEntryManager.ApplyStatusEffect(targetEntitiesController, debuff.Value);
                    }
                }
            }
        }
    }

    private float CalculateTargetsArmorValue(StatEntryContainer attacksStatContainer, BaseEntityController targetEntitiesController)
    {
        float targetEntitiesModifiedArmorValue = targetEntitiesController.EntitiesStats.GetStatEntriesTotalValue(StatName.ArmorValue);
        targetEntitiesModifiedArmorValue -= attacksStatContainer.GetStatEntriesTotalValue(StatName.IgnoreArmorAmountValue);
        
        return targetEntitiesModifiedArmorValue;
    }

    private float CalculateCriticalHitChanceResistance(StatEntryContainer statEntryContainer)
    {
        float critHitResist = statEntryContainer.GetStatEntriesTotalValue(StatName.CriticalHitResistancePercent) / 100.0f;
        critHitResist = Mathf.Max(0, critHitResist);

        return critHitResist;
    }

    private float CalculateCriticalHitDamageResistance(StatEntryContainer statEntryContainer)
    {
        float critHitDamResist = statEntryContainer.GetStatEntriesTotalValue(StatName.CriticalDamageResistancePercent) / 100.0f;
        critHitDamResist = Mathf.Max(0, critHitDamResist);

        return critHitDamResist;
    }

    private float CalculateParryChance(StatEntryContainer statEntryContainer)
    {
        float parryChance = statEntryContainer.GetStatEntriesTotalValue(StatName.ParryAttackChancePercent) / 100.0f;
        parryChance = Mathf.Max(0, parryChance);

        return parryChance;
    }

    private float CalculateCounterAttackChance(StatEntryContainer statEntryContainer)
    {
        float counterAttackChance = statEntryContainer.GetStatEntriesTotalValue(StatName.CounterAttackChancePercent) / 100.0f;
        counterAttackChance = Mathf.Max(0, counterAttackChance);

        return counterAttackChance;
    }

    private float CalculateBlockChance(StatEntryContainer statEntryContainer)
    {
        float blockChance = statEntryContainer.GetStatEntriesTotalValue(StatName.BlockChancePercent) / 100.0f;
        blockChance = Mathf.Max(0, blockChance);

        return blockChance;
    }

    private float CalculateBlockedAmount(StatEntryContainer statEntryContainer)
    {
        float blockedAmount = statEntryContainer.GetStatEntriesTotalValue(StatName.BlockAmountValue);
        blockedAmount = Mathf.Max(0, blockedAmount);

        return blockedAmount;
    }

    private float CalculateReflectAttackChance(StatEntryContainer statEntryContainer)
    {
        float reflectAttackChance = statEntryContainer.GetStatEntriesTotalValue(StatName.ReflectAttackChancePercent) / 100.0f;
        reflectAttackChance = Mathf.Max(0, reflectAttackChance);

        return reflectAttackChance;
    }

    private float CalculateDodgeChance(StatEntryContainer statEntryContainer)
    {
        float dodgeChance = statEntryContainer.GetStatEntriesTotalValue(StatName.DodgeChancePercent) / 100.0f;
        dodgeChance = Mathf.Max(0, dodgeChance);

        return dodgeChance;
    }
}
