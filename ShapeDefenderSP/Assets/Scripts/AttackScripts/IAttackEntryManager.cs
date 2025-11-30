using SDSPEnums;
using UnityEngine;

public interface IAttackEntryManager
{
    BaseAttackController GetDefaultAttackController(AttackName attacksName);
    void AddNewAttack(AttackName attacksName, AttackEntryContainer entitiesAttackContainer, Transform parentsTransform);
    void RemoveAttack(AttackName attacksName, AttackEntryContainer entitiesAttackContainer);
    void ChangeAttackUsability(AttackName attacksName, AttackEntryContainer entitiesAttackContainer, bool canAttackBeUsed);
}
