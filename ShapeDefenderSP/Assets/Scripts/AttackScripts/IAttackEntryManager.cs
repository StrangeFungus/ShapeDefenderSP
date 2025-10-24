using SDSPEnums;
using UnityEngine;

public interface IAttackEntryManager
{
    BaseAttackController GetDefaultAttackController(AttackName attacksName);
    void AddNewAttack(AttackName attacksName, AttackEntryContainer entitiesAttackContainer, Transform parentsTransform);
    void RemoveAttack(AttackName attacksName, AttackEntryContainer entitiesAttackContainer);
    void LevelUpAttack(AttackName attacksName, AttackEntryContainer entitiesAttackContainer, int numberOfLevelUps);
    void ChangeAttackUsability(AttackName attacksName, AttackEntryContainer entitiesAttackContainer, bool canAttackBeUsed);
    void AddToEntitiesPendingAttackUpdates(AttackName attacksName, AttackModificationAction attacksModificationAction, BaseEntityController entityCallingRequest);
    void ProcessPendingAttackUpdates(BaseEntityController callingEntitiesController);
}
