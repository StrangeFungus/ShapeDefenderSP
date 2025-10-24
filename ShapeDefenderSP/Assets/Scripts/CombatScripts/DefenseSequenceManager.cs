using System.Collections;
using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

public class DefenseSequenceManager : MonoBehaviour, IDefenseSequenceManager
{
    public static DefenseSequenceManager Instance;

    [SerializeField] private float defaultMinimumAllowedDefenseActionDelay = 0.05f;
    private static float parriedOrReflectedIncomingDamageReduction = 0.65f;
    private static float parriedAttackReflectionAngle = 45.0f;
    private static float reflectedAttackReflectionAngle = 45.0f;

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

    public void AttemptToDamageTarget(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController, BaseEntityController attackingEntitiesController = null)
    {
        CalculateAttackSequencePrechecks(baseAttackController, targetEntitiesController, out float targetsCritHitResist, out float targetsCritDamResist);
        CalculateAttackSequence(baseAttackController, targetsCritHitResist, targetsCritDamResist, out float attacksDamageAmount, out bool wasTheAttackACrit);
        AttemptToDefendDamage(baseAttackController, targetEntitiesController, attacksDamageAmount, wasTheAttackACrit, attackingEntitiesController);
    }

    private void CalculateAttackSequencePrechecks(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController,
        out float critHitChanceResist, out float critHitDamResist)
    {
        if (baseAttackController != null && targetEntitiesController != null)
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

    private void CalculateAttackSequence(BaseAttackController baseAttackController,
    float targetsCritHitChanceResist, float targetsCritHitDamResist, 
    out float attackDamage, out bool wasAttackACriticalHit)
    {
        attackDamage = 0.0f;
        wasAttackACriticalHit = false;
        if (baseAttackController != null)
        {
            attackDamage = CalculateBaseDamage(baseAttackController.AttacksEntry.AttacksStats);
            float criticalHitChance = CalculateCriticalHitChance(baseAttackController.AttacksEntry.AttacksStats);

            if (Random.value < (criticalHitChance - targetsCritHitChanceResist))
            {
                attackDamage = CalculateCriticalHitDamage(baseAttackController.AttacksEntry.AttacksStats, attackDamage);
                float resistedDamage = attackDamage * (1f + targetsCritHitDamResist);
                attackDamage -= resistedDamage;
                wasAttackACriticalHit = true;
            }
        }

        if (attackDamage <= 0.0f)
        {
            Debug.Log($"Attack damage too low to cause damage! ({attackDamage})");
            attackDamage = 0.0f;
        }
    }

    public float CalculateBaseDamage(StatEntryContainer statEntryContainer)
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

    public void AttemptToDefendDamage(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController,
        float attackDamage, bool isDamageCritical, BaseEntityController attackingEntitiesController = null)
    {
        if (baseAttackController != null && targetEntitiesController != null)
        {
            bool wasAbleToParry = false;
            bool wasAbleToBlock = false;
            bool wasAbleToDodge = false;

            if (!baseAttackController.AttacksEntry.DamageTypes.HasFlag(DamageType.True))
            {
                if (baseAttackController.AttacksEntry.IsProjectilePhysicalObject)
                {
                    if (baseAttackController.AttacksEntry.IsProjectileBlockable)
                    {
                        wasAbleToParry = AttemptToParry(baseAttackController, targetEntitiesController, ref attackDamage);
                    }

                    if (!wasAbleToParry)
                    {
                        wasAbleToBlock = AttemptToBlock(baseAttackController, targetEntitiesController, ref attackDamage);
                    }
                }

                if (!wasAbleToBlock)
                {
                    wasAbleToDodge = AttemptToDodge(baseAttackController, targetEntitiesController);
                }
            }

            if (!wasAbleToDodge)
            {
                if (attackDamage <= 0)
                {
                    Debug.Log($"Attacks damage is too low to cause damage! {attackDamage}");
                    attackDamage = 0;

                    //iFloatingTextManager.GenerateFloatingText(defendingEntityController.transform.position, attackDamage.ToString(), DefenseAction.ArmorNullifiedDamage.ToString());
                }
                else
                {
                    float targetEntitiesModifiedArmorValue = CalculateTargetsArmorValue(baseAttackController, targetEntitiesController);
                    ApplyDamageToTargetEntity(baseAttackController, attackDamage, isDamageCritical, targetEntitiesController, targetEntitiesModifiedArmorValue, attackingEntitiesController);

                    CheckIfAttackSpawnsAreaOfEffectsOnHit(baseAttackController, attackDamage, isDamageCritical);

                    CheckIfAttackAppliesStatusEffects(baseAttackController, targetEntitiesController);
                }
            }
        }
    }

    private bool AttemptToParry(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController,
    ref float attackDamage)
    {
        if (baseAttackController != null && targetEntitiesController != null)
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

                if (baseAttackController != null)
                {
                    baseAttackController.transform.Rotate(0, 0, Random.Range(-parriedAttackReflectionAngle, parriedAttackReflectionAngle));
                }

                attackDamage *= parriedOrReflectedIncomingDamageReduction;

                return true;
            }
        }

        return false;
    }

    private bool AttemptToBlock(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController, ref float attackDamage)
    {
        if (baseAttackController != null && targetEntitiesController != null)
        {
            float blockChance = CalculateBlockChance(targetEntitiesController.EntitiesStats);

            if (!targetEntitiesController.EntitiesDefenseCooldownData.isBlockOnCooldown && Random.value <= blockChance)
            {
                targetEntitiesController.EntitiesDefenseCooldownData.isBlockOnCooldown = true;

                float reflectAttackChance = CalculateReflectAttackChance(targetEntitiesController.EntitiesStats);
                if (Random.value <= reflectAttackChance)
                {
                    attackDamage *= parriedOrReflectedIncomingDamageReduction;

                    if (baseAttackController != null)
                    {
                        float reflectedAttackDamageMultiplier = baseAttackController.AttacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.ReflectedAttackDamageMultiplier) / 100.0f;
                        StatEntry minAttackDamageEntry = baseAttackController.AttacksEntry.AttacksStats.GetStatEntry(StatName.MinimumAttackDamageValue);
                        minAttackDamageEntry?.ModifyBaseValue(StatModificationAction.SetValueTo, minAttackDamageEntry.StatsTotalValue * reflectedAttackDamageMultiplier);
                        StatEntry maxAttackDamageEntry = baseAttackController.AttacksEntry.AttacksStats.GetStatEntry(StatName.MaximumAttackDamageValue);
                        maxAttackDamageEntry?.ModifyBaseValue(StatModificationAction.SetValueTo, maxAttackDamageEntry.StatsTotalValue * reflectedAttackDamageMultiplier);

                        baseAttackController.TimesAttackWasReflected++;
                        baseAttackController.StartingLocation = baseAttackController.transform.position;
                        baseAttackController.transform.Rotate(0, 0, Random.Range(-45.0f, 45.0f));
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

    private bool AttemptToDodge(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController)
    {
        if (baseAttackController != null && targetEntitiesController != null)
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

    private void CheckIfAttackSpawnsAreaOfEffectsOnHit(BaseAttackController baseAttackController, float attackDamage, bool isDamageCritical)
    {
        if (baseAttackController != null)
        {
            if (baseAttackController.AttacksEntry.DoesAnAreaOfEffect)
            {
                if (baseAttackController.AttacksEntry.AreaOfEffectPrefabController != null)
                {
                    if (baseAttackController.AttacksEntry.AreaOfEffectPrefabController.AreaOfEffectsEntry.SpawnsWhenAttackHits)
                    {
                        iAreaOfEffectEntryManager.CalculateAndActivateAreaOfEffect(baseAttackController);
                    }
                }
            }
        }
    }

    private void CheckIfAttackAppliesStatusEffects(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController)
    {
        if (baseAttackController != null && targetEntitiesController != null)
        {
            if (baseAttackController.AttacksEntry.AttacksStatusEffects.StatusEffectsDictionary != null)
            {
                foreach (var debuff in baseAttackController.AttacksEntry.AttacksStatusEffects.StatusEffectsDictionary)
                {
                    float debuffChance = baseAttackController.AttacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.StatusEffectInflictionChancePercent) / 100.0f;
                    float statusEffectInflictionResistance = baseAttackController.AttacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.StatusEffectInflictionResistanceValue) / 100.0f;
                    debuffChance = Mathf.Max(0, debuffChance - statusEffectInflictionResistance);

                    if (Random.value < debuffChance)
                    {
                        iStatusEffectEntryManager.ApplyStatusEffect(targetEntitiesController, debuff.Value);
                    }
                }
            }
        }
    }

    private void ApplyDamageToTargetEntity(BaseAttackController baseAttackController, float attacksDamageTotal, bool isDamageCritical,
        BaseEntityController targetEntitiesController, float targetEntitiesModifiedArmorValue, 
        BaseEntityController attackingEntitiesController = null)
    {
        if (baseAttackController != null && targetEntitiesController != null)
        {
            iHealthManager.ApplyDamageToEntity(baseAttackController, attacksDamageTotal, isDamageCritical, targetEntitiesController, targetEntitiesModifiedArmorValue, attackingEntitiesController);
        }
    }

    private float CalculateTargetsArmorValue(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController)
    {
        float targetEntitiesModifiedArmorValue = targetEntitiesController.EntitiesStats.GetStatEntriesTotalValue(StatName.ArmorValue);
        targetEntitiesModifiedArmorValue -= baseAttackController.AttacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.IgnoreArmorAmountValue);
        
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
