using System.Collections;
using SDSPEnums;
using UnityEngine;

public interface IHealthManager
{
    void ApplyDamageToTarget(MonoBehaviour callingAttacksMonoBehaviour, float incomingDamageAmount, bool isDamageCritical,
        BaseEntityController targetEntitiesController, float targetEntitiesModifiedArmorValue);

    void AddToEntitiesLife(BaseEntityController targetEntityController, float healingAmount, bool isHealingCritical, HealingType healingType);

    IEnumerator HealthRegenCoroutine(BaseEntityController baseEntityController);
}
