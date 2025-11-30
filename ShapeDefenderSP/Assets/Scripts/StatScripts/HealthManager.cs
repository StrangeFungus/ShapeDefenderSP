using System.Collections;
using SDSPEnums;
using UnityEngine;

public class HealthManager : MonoBehaviour, IHealthManager
{
    private static HealthManager Instance { get; set; }

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

        InterfaceContainer.Register<IHealthManager>(this);
    }

    public void ApplyDamageToTarget(bool doesDamageIgnoreEnergyShields, DamageType damageTypes, bool isAttackAPhysicalObject, 
        float incomingDamageAmount, bool isDamageCritical,
        BaseEntityController targetEntitiesController, float targetEntitiesModifiedArmorValue,
        BaseEntityController attackingEntitiesController = null)
    {
        if (targetEntitiesController == null) { Debug.Log($"The targets controller was null, returning..."); return; }

        if (incomingDamageAmount <= 0 || targetEntitiesController.IsEntityDead) { Debug.Log($"Cannot damage target, returning..."); return; }

        float damageAppliedToEnergyShields = 0.0f;
        float damageAppliedToOverhealHealthPoints = 0.0f;
        float damageAppliedToHealthPoints = 0.0f;

        if (!doesDamageIgnoreEnergyShields || !damageTypes.HasFlag(DamageType.True))
        {
            CalculateAndRemoveLife(StatName.CurrentEnergyShieldValue, targetEntitiesController, ref incomingDamageAmount, isDamageCritical, damageTypes, targetEntitiesModifiedArmorValue, doesDamageIgnoreEnergyShields, ref damageAppliedToEnergyShields);
        }

        if (isAttackAPhysicalObject && !damageTypes.HasFlag(DamageType.True))
        {
            incomingDamageAmount -= targetEntitiesModifiedArmorValue;
            incomingDamageAmount = Mathf.Max(0, incomingDamageAmount);
        }

        CalculateAndRemoveLife(StatName.CurrentOverhealCapacityValue, targetEntitiesController, ref incomingDamageAmount, isDamageCritical, damageTypes, targetEntitiesModifiedArmorValue, doesDamageIgnoreEnergyShields, ref damageAppliedToOverhealHealthPoints);

        CalculateAndRemoveLife(StatName.CurrentHealthPointsValue, targetEntitiesController, ref incomingDamageAmount, isDamageCritical, damageTypes, targetEntitiesModifiedArmorValue, doesDamageIgnoreEnergyShields, ref damageAppliedToHealthPoints);

        CheckEntitiesHealthPoints(targetEntitiesController);

        if (attackingEntitiesController == null) { Debug.Log($"The attacking controller was null, returning..."); return; }
        if (!attackingEntitiesController.IsEntityDead)
        {
            float energyShieldPercentStolenOnHit = attackingEntitiesController.EntitiesStats.GetStatEntriesTotalValue(StatName.EnergyShieldStolenOnHitPercent) / 100.0f;
            if (energyShieldPercentStolenOnHit > 0.0f)
            {
                float energyShieldStolenAmount = damageAppliedToEnergyShields * energyShieldPercentStolenOnHit;
                if (energyShieldStolenAmount > 0.0f)
                {
                    AddToEntitiesLife(attackingEntitiesController, energyShieldStolenAmount, isDamageCritical, HealingType.EnergyShieldRegen);
                }
            }
            
            float healthPercentStolenOnHit = attackingEntitiesController.EntitiesStats.GetStatEntriesTotalValue(StatName.HealthPointsStolenOnHitPercent) / 100.0f;
            if (healthPercentStolenOnHit > 0.0f)
            {
                float healthPointsStolenAmount = damageAppliedToHealthPoints * healthPercentStolenOnHit;

                if (healthPointsStolenAmount > 0.0f)
                {
                    AddToEntitiesLife(attackingEntitiesController, healthPointsStolenAmount, isDamageCritical, HealingType.HealthPointRegen);
                }

                float overhealHealthPointsStolenAmount = damageAppliedToOverhealHealthPoints * healthPercentStolenOnHit;

                if (overhealHealthPointsStolenAmount > 0.0f)
                {
                    AddToEntitiesLife(attackingEntitiesController, overhealHealthPointsStolenAmount, isDamageCritical, HealingType.OverHealthRegen);
                }
            }
        }
    }

    private void CalculateAndRemoveLife(StatName statsName, BaseEntityController targetEntityController,
        ref float incomingDamageAmount, bool isDamageCritical, DamageType damageTypes, float targetsModifiedArmorValue, bool doesDamageIgnoreEnergyShields, ref float damageAppliedTracker)
    {
        if (targetEntityController.EntitiesStats.StatEntryDictionary.ContainsKey(statsName))
        {
            float statsTotalValue = targetEntityController.EntitiesStats.GetStatEntriesTotalValue(statsName);

            if (incomingDamageAmount > 0.0f && statsTotalValue > 0)
            {
                float statTotalValueAfterDamage = statsTotalValue - incomingDamageAmount;

                if (statTotalValueAfterDamage <= 0.0f)
                {
                    targetEntityController.EntitiesStats.StatEntryDictionary[statsName].ModifyBaseValue(StatModificationAction.SubtractFromValue, targetEntityController.EntitiesStats.StatEntryDictionary[statsName].StatsTotalValue);
                    damageAppliedTracker = statsTotalValue;
                }
                else
                {
                    targetEntityController.EntitiesStats.StatEntryDictionary[statsName].ModifyBaseValue(StatModificationAction.SubtractFromValue, incomingDamageAmount);
                    damageAppliedTracker = incomingDamageAmount;
                }

                incomingDamageAmount -= damageAppliedTracker;
            }
        }
    }

    public void AddToEntitiesLife(BaseEntityController targetEntityController, float healingAmount, bool isHealingCritical, HealingType healingType)
    {
        if (targetEntityController == null) { Debug.Log($"The targets controller was null... returning."); return; }

        switch (healingType)
        {
            case HealingType.EnergyShieldRegen:
                ProcessHealingEffect(targetEntityController, healingAmount, isHealingCritical, healingType, StatName.CurrentEnergyShieldValue, StatName.MaxEnergyShieldValue);
                break;
            case HealingType.OverHealthRegen:
                ProcessHealingEffect(targetEntityController, healingAmount, isHealingCritical, healingType, StatName.CurrentOverhealCapacityValue, StatName.MaxOverhealCapacityValue);
                break;
            case HealingType.HealthPointRegen:
                ProcessHealingEffect(targetEntityController, healingAmount, isHealingCritical, healingType, StatName.CurrentHealthPointsValue, StatName.MaxHealthPointsValue);
                break;
            default:
                return;
        }
    }

    private void ProcessHealingEffect(BaseEntityController targetEntityController, float healingAmount, bool isHealingCritical, HealingType healingTypes, StatName currentStatName, StatName maximumStatName)
    {
        healingAmount = Mathf.Abs(healingAmount);

        if (targetEntityController.EntitiesStats.StatEntryDictionary.ContainsKey(maximumStatName))
        {
            float maxStatPointsValue = targetEntityController.EntitiesStats.GetStatEntriesTotalValue(maximumStatName);

            float currentStatPointsValue = targetEntityController.EntitiesStats.GetStatEntriesTotalValue(currentStatName);

            if (currentStatPointsValue < maxStatPointsValue)
            {
                float totalAfterHealing = currentStatPointsValue + healingAmount;

                if (totalAfterHealing >= maxStatPointsValue)
                {
                    float amountRegen = maxStatPointsValue - currentStatPointsValue;
                    targetEntityController.EntitiesStats.StatEntryDictionary[currentStatName].ModifyBaseValue(StatModificationAction.AddToValue, amountRegen);
                }
                else
                {
                    targetEntityController.EntitiesStats.StatEntryDictionary[currentStatName].ModifyBaseValue(StatModificationAction.AddToValue, healingAmount);
                }
            }
        }
    }

    private void CheckEntitiesHealthPoints(BaseEntityController baseEntityController)
    {
        if (baseEntityController != null)
        {
            if (baseEntityController.EntitiesStats.StatEntryDictionary.ContainsKey(StatName.CurrentHealthPointsValue))
            {
                if (baseEntityController.EntitiesStats.StatEntryDictionary[StatName.CurrentHealthPointsValue].StatsTotalValue <= 0.0f)
                {
                    baseEntityController.IsEntityDead = true;
                }
            }
        }
    }

    public IEnumerator HealthRegenCoroutine(BaseEntityController baseEntityController)
    {
        while (true)
        {
            if (baseEntityController == null || !baseEntityController.gameObject.activeSelf) { yield break; }

            if (GameStateManager.IsGamePaused)
            {
                yield return null;
                continue;
            }

            if (!baseEntityController.IsEntityDead)
            {
                float currentHealthPointsValue = baseEntityController.EntitiesStats.GetStatEntriesTotalValue(StatName.CurrentHealthPointsValue);
                float maxHealthPointsValue = baseEntityController.EntitiesStats.GetStatEntriesTotalValue(StatName.MaxHealthPointsValue);

                if (currentHealthPointsValue < maxHealthPointsValue)
                {
                    float cooldownSeconds = baseEntityController.EntitiesStats.GetStatEntriesTotalValue(StatName.HealthRegenCooldownTimer);

                    if (cooldownSeconds < GlobalCONSTValuesContainer.MINIMUMCOOLDOWNTIMER)
                        cooldownSeconds = GlobalCONSTValuesContainer.MINIMUMCOOLDOWNTIMER;

                    float timer = cooldownSeconds;
                    while (timer > 0f)
                    {
                        if (baseEntityController == null)
                            yield break;

                        if (GameStateManager.IsGamePaused)
                        {
                            yield return null;
                            continue;
                        }

                        timer -= Time.deltaTime;
                        yield return new WaitForSeconds(GlobalCONSTValuesContainer.DEFAULTCOROUTINEDELAYTIMER);
                    }

                    float amountToHeal = baseEntityController.EntitiesStats.GetStatEntriesTotalValue(StatName.HealthRegenAmountValue);

                    if (amountToHeal > 0f)
                    {
                        float healClamped = Mathf.Min(amountToHeal, maxHealthPointsValue - currentHealthPointsValue);
                        AddToEntitiesLife(baseEntityController, healClamped, false, HealingType.HealthPointRegen);
                    }
                }
            }

            yield return null;
        }
    }
}
