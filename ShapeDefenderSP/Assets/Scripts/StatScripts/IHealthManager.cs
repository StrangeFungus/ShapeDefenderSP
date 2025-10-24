using System.Collections;
using SDSPEnums;

public interface IHealthManager
{
    void ApplyDamageToTarget(BaseAttackController callingAttackController, float incomingDamageAmount, bool isDamageCritical,
        BaseEntityController targetEntitiesController, float targetEntitiesModifiedArmorValue);

    void ApplyDamageToTarget(AreaOfEffectController callingAreaOfEffectController, float incomingDamageAmount, bool isDamageCritical,
    BaseEntityController targetEntitiesController, float targetEntitiesModifiedArmorValue);

    void ApplyDamageToTarget(StatusEffectEntry callingStatusEffect, float incomingDamageAmount, bool isDamageCritical,
    BaseEntityController targetEntitiesController, float targetEntitiesModifiedArmorValue);

    void AddToEntitiesLife(BaseEntityController targetEntityController, float healingAmount, bool isHealingCritical, HealingType healingType);

    IEnumerator HealthRegenCoroutine(BaseEntityController baseEntityController);
}
