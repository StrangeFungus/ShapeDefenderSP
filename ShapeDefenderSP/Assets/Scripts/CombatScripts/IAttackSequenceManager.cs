using System.Collections;
using SDSPEnums;

public interface IAttackSequenceManager
{
    bool AttemptToUseAnAttack(BaseEntityController callingEntitiesController, AttackName attacksName, bool activateThisAttacksCooldown);
    IEnumerator AttemptToUseAllAttacksCoroutine(BaseEntityController callingEntitiesController);
    void AttemptToCounterAttack(BaseEntityController callingEntitiesController);
}
