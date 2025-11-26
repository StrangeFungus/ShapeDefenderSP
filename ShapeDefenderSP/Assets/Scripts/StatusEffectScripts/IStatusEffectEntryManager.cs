using SDSPEnums;

public interface IStatusEffectEntryManager
{
    void ApplyStatusEffect(BaseEntityController targetEntitiesController, StatusEffectEntryContainer statusEffectEntryContainerToApply, StatusEffectName statusEffectsName, BaseEntityController attackingEntitiesController = null);
}
