using System.Collections;
using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;
using UnityEngine.InputSystem.XR;

[System.Serializable]
public class AttackEntryContainer
{
    [SerializeField] private List<AttackName> defaultAttacks;

    private Dictionary<AttackName, BaseAttackController> attackControllerDictionary = new();
    public IReadOnlyDictionary<AttackName, BaseAttackController> AttackControllerDictionary => attackControllerDictionary;

    private IAttackEntryManager iAttackEntryManager;
    private IStatEntryManager iStatEntryManager;

    public void InitializeAttackEntryDict()
    {
        iAttackEntryManager ??= InterfaceContainer.Request<IAttackEntryManager>();
        iStatEntryManager ??= InterfaceContainer.Request<IStatEntryManager>();

        if (defaultAttacks != null)
        {
            if (defaultAttacks.Count > 0)
            {
                foreach (var entry in defaultAttacks)
                {
                    AddDefaultAttack(entry);
                }
            }
        }
    }

    private void AddDefaultAttack(AttackName attackName)
    {
        if (!attackControllerDictionary.ContainsKey(attackName))
        {
            BaseAttackController controller = iAttackEntryManager.GetDefaultAttackController(attackName);
            if (controller != null)
            {
                attackControllerDictionary.Add(attackName, controller);
            }
        }
    }

    public void AddAttack(BaseAttackController baseAttackController)
    {
        if (baseAttackController != null)
        {
            if (baseAttackController.AttacksEntry.AttacksName != AttackName.None)
            {
                attackControllerDictionary.Add(baseAttackController.AttacksEntry.AttacksName, baseAttackController);
            }
        }
    }

    public void RemoveAttack(AttackName attacksName)
    {
        if (attackControllerDictionary.ContainsKey(attacksName))
        {
            // I need to add this object back to the attack pool instead of destroying it.
            attackControllerDictionary.Remove(attacksName);
        }
    }

    public void ResetAttackEntryDict()
    {
        foreach (var entry in attackControllerDictionary)
        {
            RemoveAttack(entry.Key);
        }

        InitializeAttackEntryDict();
    }
}
