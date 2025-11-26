public interface IAreaOfEffectEntryManager
{
    void CalculateAndActivateAreaOfEffect(AreaOfEffectController areaOfEffectControllerToSpawn, BaseAttackController callingBaseAttackController = null);
}
