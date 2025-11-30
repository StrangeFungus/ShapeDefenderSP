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

    public void ApplyDamageOverTime(AreaOfEffectController areaOfEffectController, BaseEntityController targetEntitiesController = null)
    {
        if (areaOfEffectController == null) { return; }

        if (!areaOfEffectsDoingDamageOverTime.ContainsKey(areaOfEffectController))
        {
            areaOfEffectsDoingDamageOverTime.Add(areaOfEffectController, new());
        }

        if (targetEntitiesController != null)
        {
            if (!areaOfEffectsDoingDamageOverTime[areaOfEffectController].ContainsKey(targetEntitiesController))
            {
                areaOfEffectsDoingDamageOverTime[areaOfEffectController].Add(targetEntitiesController, new());
            }

            areaOfEffectsDoingDamageOverTime[areaOfEffectController][targetEntitiesController] = Time.time;
        }
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

                    float waitTime = attackDictEntry.Key.AttacksEntry.EffectsStats.GetStatsCurrentTotal(StatName.DamageOverTimeHitRateTimer, false);

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
                if (areaOfEffect.Key == null || areaOfEffect.Key.HasMadeFinalHit)
                {
                    continue;
                }

                foreach (var target in areaOfEffect.Value)
                {
                    if (target.Key == null)
                    {
                        continue;
                    }

                    float waitTime = areaOfEffect.Key.AttacksEntry.EffectsStats.GetStatsCurrentTotal(StatName.DamageOverTimeHitRateTimer, false);

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

                    float waitTime = statusEffect.Key.EffectsStats.GetStatsCurrentTotal(StatName.DamageOverTimeHitRateTimer, false);

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
        CalculateAttackSequence(baseAttackController.AttacksEntry.EffectsStats.StatEntryDictionary, targetsCritHitResist, targetsCritDamResist, out float attacksDamageAmount, out bool wasTheAttackACrit);
        AttemptToDefendDamage(baseAttackController.AttacksEntry.DamageTypes, baseAttackController.AttacksEntry.IsProjectileBlockable, baseAttackController.AttacksEntry.IsEffectAPhysicalObject,
            baseAttackController.AttacksEntry.EffectsStatusEffects, baseAttackController.AttacksEntry.EffectsStats, attacksDamageAmount, wasTheAttackACrit, baseAttackController.AttacksEntry.DoesDamageIgnoresEnergyShields,
            baseAttackController.AttacksEntry.DoesAnAreaOfEffect, baseAttackController.AttacksEntry.AreaOfEffectPrefabController, targetEntitiesController, baseAttackController.gameObject, baseAttackController.AttacksEntry.AttackingEntitiesController);
    }

    public void AttemptToDamageTarget(AreaOfEffectController areaOfEffectController, BaseEntityController targetEntitiesController)
    {
        CalculateAttackSequencePrechecks(targetEntitiesController, out float targetsCritHitResist, out float targetsCritDamResist);
        CalculateAttackSequence(areaOfEffectController.AttacksEntry.EffectsStats.StatEntryDictionary, targetsCritHitResist, targetsCritDamResist, out float attacksDamageAmount, out bool wasTheAttackACrit);
        AttemptToDefendDamage(areaOfEffectController.AttacksEntry.DamageTypes, areaOfEffectController.AttacksEntry.IsProjectileBlockable, areaOfEffectController.AttacksEntry.IsEffectAPhysicalObject,
            areaOfEffectController.AttacksEntry.EffectsStatusEffects, areaOfEffectController.AttacksEntry.EffectsStats, attacksDamageAmount, wasTheAttackACrit, areaOfEffectController.AttacksEntry.DoesDamageIgnoresEnergyShields,
            areaOfEffectController.AttacksEntry.DoesAnAreaOfEffect, areaOfEffectController.AttacksEntry.AreaOfEffectPrefabController, targetEntitiesController, areaOfEffectController.gameObject, areaOfEffectController.AttacksEntry.AttackingEntitiesController);
    }

    public void AttemptToDamageTarget(StatusEffectEntry statusEffectEntry, BaseEntityController targetEntitiesController)
    {
        CalculateAttackSequencePrechecks(targetEntitiesController, out float targetsCritHitResist, out float targetsCritDamResist);
        CalculateAttackSequence(statusEffectEntry.EffectsStats.StatEntryDictionary, targetsCritHitResist, targetsCritDamResist, out float attacksDamageAmount, out bool wasTheAttackACrit);
        AttemptToDefendDamage(statusEffectEntry.DamageTypes, statusEffectEntry.IsProjectileBlockable, statusEffectEntry.IsEffectAPhysicalObject,
            statusEffectEntry.EffectsStatusEffects, statusEffectEntry.EffectsStats, attacksDamageAmount, wasTheAttackACrit, statusEffectEntry.DoesDamageIgnoresEnergyShields,
            statusEffectEntry.DoesAnAreaOfEffect, statusEffectEntry.AreaOfEffectPrefabController, targetEntitiesController, null, statusEffectEntry.AttackingEntitiesController);
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

    private void CalculateAttackSequence(Dictionary<StatName, StatEntry> statEntryDictionaryCausingDamage,
    float targetsCritHitChanceResist, float targetsCritHitDamResist, 
    out float attackDamage, out bool wasAttackACriticalHit)
    {
        attackDamage = 0.0f;
        wasAttackACriticalHit = false;
        if (statEntryDictionaryCausingDamage != null)
        {
            attackDamage = CalculateBaseDamage(statEntryDictionaryCausingDamage);
            float criticalHitChance = CalculateCriticalHitChance(statEntryDictionaryCausingDamage);

            if (Random.value < (criticalHitChance - targetsCritHitChanceResist))
            {
                attackDamage = CalculateCriticalHitDamage(statEntryDictionaryCausingDamage, attackDamage);
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

    private float CalculateBaseDamage(DamagingObjectStatEntryContainer statEntryDictionary)
    {
        float minAttackDam = statEntryDictionary.GetStatsCurrentTotal(StatName.MinimumAttackDamageValue, false);
        minAttackDam = Mathf.Max(0, minAttackDam);

        float maxAttackDam = statEntryDictionary.GetStatsCurrentTotal(StatName.MaximumAttackDamageValue, false);
        maxAttackDam = Mathf.Max(0, maxAttackDam);

        if (minAttackDam > maxAttackDam) { minAttackDam = maxAttackDam; }
        float damageToApply = Random.Range(minAttackDam, maxAttackDam);

        return damageToApply;
    }

    private float CalculateCriticalHitChance(DamagingObjectStatEntryContainer statEntryDictionary)
    {
        float chanceToCrit = statEntryDictionary.GetStatsCurrentTotal(StatName.CriticalHitChancePercent, false) / 100.0f;
        chanceToCrit = Mathf.Max(0, chanceToCrit);

        return chanceToCrit;
    }

    private float CalculateCriticalHitDamage(DamagingObjectStatEntryContainer statEntryDictionary, float damage)
    {
        float critHitDam = statEntryDictionary.GetStatsCurrentTotal(StatName.CriticalHitDamageMultiplier, false) / 100.0f;
        critHitDam = Mathf.Max(0, critHitDam);

        damage *= (1f + critHitDam);

        return damage;
    }

    public void AttemptToDefendDamage(DamageType damageTypes, bool isAttackBlockable, bool isAttackAPhysicalObject, StatusEffectEntryContainer attacksStatusEffectContainer,
        DamagingObjectStatEntryContainer damagingObjectStatEntryContainer, float attackDamage, bool isDamageCritical, bool doesDamageIgnoreEnergyShields, bool doesAttackGenerateAnAOE, AreaOfEffectController areaOfEffectController,
        BaseEntityController targetEntitiesController, GameObject attackingObject = null, BaseEntityController attackingEntitiesController = null)
    {
        if (targetEntitiesController == null) { return; }

        if (damageTypes.HasFlag(DamageType.Default) || damagingObjectStatEntryContainer == null) { return; }

        if (targetEntitiesController != null)
        {
            bool wasAbleToParry = false;
            bool wasAbleToBlock = false;
            bool wasAbleToDodge = false;

            if (!damageTypes.HasFlag(DamageType.True))
            {
                if (isAttackBlockable)
                {
                    if (isAttackAPhysicalObject && attackingObject != null)
                    {
                        wasAbleToParry = AttemptToParry(attackingObject, targetEntitiesController, ref attackDamage);
                    }

                    if (!wasAbleToParry)
                    {
                        wasAbleToBlock = AttemptToBlock(damagingObjectStatEntryContainer, targetEntitiesController, ref attackDamage, attackingObject);
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
                    float targetEntitiesModifiedArmorValue = CalculateTargetsArmorValue(damagingObjectStatEntryContainer, targetEntitiesController);

                    iHealthManager.ApplyDamageToTarget(doesDamageIgnoreEnergyShields, damageTypes, isAttackAPhysicalObject, attackDamage, isDamageCritical, targetEntitiesController, targetEntitiesModifiedArmorValue, attackingEntitiesController);

                    if (doesAttackGenerateAnAOE)
                    {
                        CheckIfAttackSpawnsAreaOfEffectsOnHit(areaOfEffectController, attackDamage, isDamageCritical);
                    }

                    if (attacksStatusEffectContainer != null)
                    {
                        CheckIfAttackAppliesStatusEffects(attacksStatusEffectContainer, damagingObjectStatEntryContainer, targetEntitiesController);
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

    private bool AttemptToBlock(DamagingObjectStatEntryContainer attacksStatContainer, BaseEntityController targetEntitiesController, ref float attackDamage, GameObject attackingObject = null)
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
                        float reflectedAttackDamageMultiplier = attacksStatContainer.GetStatsCurrentTotal(StatName.ReflectedAttackDamageMultiplier, false) / 100.0f;
                        
                        if (attacksStatContainer.StatEntryDictionary.TryGetValue(StatName.MinimumAttackDamageValue, out var minAttackDamEntry))
                        {
                            minAttackDamEntry.ModifyBaseValue(StatModificationAction.SetValueTo, minAttackDamEntry.CurrentValueTotal * reflectedAttackDamageMultiplier);
                        }

                        if (attacksStatContainer.StatEntryDictionary.TryGetValue(StatName.MaximumAttackDamageValue, out var maxAttackDamEntry))
                        {
                            maxAttackDamEntry.ModifyBaseValue(StatModificationAction.SetValueTo, maxAttackDamEntry.CurrentValueTotal * reflectedAttackDamageMultiplier);
                        }

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

    private void CheckIfAttackSpawnsAreaOfEffectsOnHit(AreaOfEffectController areaOfEffectController, float attackDamage, bool isDamageCritical)
    {
        if (areaOfEffectController != null)
        {
            if (areaOfEffectController.AreaOfEffectsEntry.SpawnsWhenAttackHits)
            {
                iAreaOfEffectEntryManager.CalculateAndActivateAreaOfEffect(areaOfEffectController, attackDamage, isDamageCritical);
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
