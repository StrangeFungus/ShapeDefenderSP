using System.Collections;
using SDSPEnums;

public interface IAttackSequenceManager
{
    bool AttemptToUseAnAttack(BaseEntityController callingEntitiesController, AttackName attacksName, bool activateThisAttacksCooldown);
    IEnumerator AttemptToUseAllAttacksCoroutine(BaseEntityController callingEntitiesController);
    IEnumerator DealColliderDamageOverTimeToTargets(BaseAttackController baseAttackController);
    void AttemptToCounterAttack(BaseEntityController callingEntitiesController);
}
