using System.Collections;
using SDSPEnums;
using UnityEngine;

public interface IHealthManager
{
    void ApplyDamageToTarget(bool doesDamageIgnoreEnergyShields, DamageType damageTypes, bool isAttackAPhysicalObject,
        float incomingDamageAmount, bool isDamageCritical,
        BaseEntityController targetEntitiesController, float targetEntitiesModifiedArmorValue,
        BaseEntityController attackingEntitiesController = null);

    void AddToEntitiesLife(BaseEntityController targetEntityController, float healingAmount, bool isHealingCritical, HealingType healingType);

    IEnumerator HealthRegenCoroutine(BaseEntityController baseEntityController);
}
