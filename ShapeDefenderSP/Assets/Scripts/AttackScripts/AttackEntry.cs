using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class AttackEntry : BaseDamagingObjectEntry
{
    // GENERAL DATA
    [SerializeField] private AttackName attacksName = AttackName.None;
    public AttackName AttacksName => attacksName;

    public int AttacksLevel { get; set; } = 1;

    [SerializeField] private MaterialType materialTypes;
    public MaterialType MaterialTypes => materialTypes;

    [SerializeField] private AttackTargetingBehaviour targetingBehaviour;
    public AttackTargetingBehaviour TargetingBehaviour => TargetingBehaviour;

    // COMPLEX ATTACK DATA
    [SerializeField] private bool doesAnAreaOfEffect;
    public bool DoesAnAreaOfEffect => doesAnAreaOfEffect;

    [SerializeField] private AreaOfEffectController areaOfEffectPrefabController;
    public AreaOfEffectController AreaOfEffectPrefabController => areaOfEffectPrefabController;

    public void CopyAttackEntry(AttackEntry attackEntryToCopyFrom)
    {
        if (attackEntryToCopyFrom == null) { return; }

        effectsStats.CopyDamagingObjectStatEntryContainer(attackEntryToCopyFrom.effectsStats);
        effectsStatusEffects.CopyStatusEffectEntryContainer(attackEntryToCopyFrom.effectsStatusEffects);

        if (attackEntryToCopyFrom.areaOfEffectPrefabController != null)
        {
            areaOfEffectPrefabController.CopyAreaOfEffectsControllerData(attackEntryToCopyFrom.areaOfEffectPrefabController);
        }
    }

    public void ResetToDefaults()
    {
        AttacksLevel = 1;
        effectsStats.ResetDamagingObjectStatEntryContainer();
        if (areaOfEffectPrefabController != null)
        {
            areaOfEffectPrefabController.ResetAreaOfEffectsController();
        }

        effectsStatusEffects.ResetStatusEffectEntryDict();
    }
}
