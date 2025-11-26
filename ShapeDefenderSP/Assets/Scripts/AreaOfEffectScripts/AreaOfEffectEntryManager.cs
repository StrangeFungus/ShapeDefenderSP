using UnityEngine;

public class AreaOfEffectEntryManager : MonoBehaviour, IAreaOfEffectEntryManager
{
    public static AreaOfEffectEntryManager Instance;
    private IDefenseSequenceManager iDefenseSequenceManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InterfaceContainer.Register<IAreaOfEffectEntryManager>(this);
    }

    private void Start()
    {
        iDefenseSequenceManager ??= InterfaceContainer.Request<IDefenseSequenceManager>();
    }

    public void CalculateAndActivateAreaOfEffect(AreaOfEffectController areaOfEffectControllerToSpawn, BaseAttackController callingBaseAttackController = null)
    {
        if (areaOfEffectControllerToSpawn == null) { return; }

        if (callingBaseAttackController != null) { areaOfEffectControllerToSpawn.ParentAttacksController = callingBaseAttackController; }

        GameObject spawnedAreaOfEffectCopy = Instantiate(areaOfEffectControllerToSpawn.gameObject);
        if (spawnedAreaOfEffectCopy.TryGetComponent<AreaOfEffectController>(out var spawnedAreaOfEffectsController))
        {
            spawnedAreaOfEffectsController.CopyAreaOfEffectsControllerData(areaOfEffectControllerToSpawn);

            if (spawnedAreaOfEffectsController.AttacksEntry.DoesDamageOverTime)
            {
                iDefenseSequenceManager.ApplyDamageOverTime(spawnedAreaOfEffectsController);
            }
        }
    }
}
