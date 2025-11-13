using SDSPEnums;
using UnityEngine;

public interface IDefenseSequenceManager
{
    void ActivateCombatCooldownCoroutines(BaseEntityController baseEntityController);
    void ApplyDamageOverTime(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController);
    void ApplyDamageOverTime(AreaOfEffectController areaOfEffectController, BaseEntityController targetEntitiesController);
    void ApplyDamageOverTime(BaseEntityController targetEntitiesController,
       StatusEffectEntryContainer statusEffectEntryContainerToApply, StatusEffectName statusEffectsName);
    void RemoveDamageOverTime(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController);
    void RemoveDamageOverTime(AreaOfEffectController areaOfEffectController, BaseEntityController targetEntitiesController);
    void RemoveDamageOverTime(BaseEntityController targetEntitiesController,
       StatusEffectEntryContainer statusEffectEntryContainerToApply, StatusEffectName statusEffectsName);
    void RemoveDamageOverTime(BaseEntityController targetEntitiesController);
    void AttemptToDamageTarget(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController);
    void AttemptToDamageTarget(AreaOfEffectController areaOfEffectController, BaseEntityController targetEntitiesController);
    void AttemptToDamageTarget(StatusEffectEntry statusEffectEntry, BaseEntityController targetEntitiesController);
    void AttemptToDefendDamage(MonoBehaviour attackingMonobehaviour, BaseEntityController targetEntitiesController, float attackDamage, bool isDamageCritical);
}
