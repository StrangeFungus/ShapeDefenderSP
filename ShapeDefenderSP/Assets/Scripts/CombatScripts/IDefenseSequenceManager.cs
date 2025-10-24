public interface IDefenseSequenceManager
{
    void ActivateCombatCooldownCoroutines(BaseEntityController baseEntityController);
    void AttemptToDamageTarget(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController, BaseEntityController attackingEntitiesController = null);
    void AttemptToDefendDamage(BaseAttackController baseAttackController, BaseEntityController targetEntitiesController,
        float attackDamage, bool isDamageCritical, BaseEntityController attackingEntitiesController = null);
    float CalculateBaseDamage(StatEntryContainer statEntryContainer);
}
