using System.Collections;
using System.Collections.Generic;
using SDSPEnums;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AreaOfEffectController : MonoBehaviour
{
    [SerializeField] private AreaOfEffectEntry areaOfEffectsEntry;
    public AreaOfEffectEntry AreaOfEffectsEntry => areaOfEffectsEntry;

    private float timesEffectWasReflected = 0;
    public float TimesEffectWasReflected { get { return timesEffectWasReflected; } set { timesEffectWasReflected++; } }
    public bool FinishLifecycle { get; set; } = false;

    // TARGETING DATA
    [SerializeField] private BaseAttackController attacksController;
    public BaseAttackController AttacksController => attacksController;

    // TRACKING DATA
    public Vector3 StartingLocation { get; set; }

    private IDefenseSequenceManager iDefenseSequenceManager;

    private void Awake()
    {
        iDefenseSequenceManager ??= InterfaceContainer.Request<IDefenseSequenceManager>();
        areaOfEffectsEntry.OnAwake();
    }

    private void Start()
    {
        StartingLocation = gameObject.transform.position;

        if (areaOfEffectsEntry.AttackingEntitiesController != null)
        {
            areaOfEffectsEntry.ParentsTagType = gameObject.tag;
        }
        else
        {
            Debug.Log($"AttacksEntry.AttackingEntitiesController was null for the attacks controller target data.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(areaOfEffectsEntry.ParentsTagType))
        {
            if (collision.gameObject.TryGetComponent<BaseEntityController>(out var baseEntityController))
            {
                if (areaOfEffectsEntry.DoesDamageOverTime)
                {
                    iDefenseSequenceManager.ApplyDamageOverTime(this, baseEntityController);
                }
                else
                {
                    iDefenseSequenceManager.AttemptToDamageTarget(this, baseEntityController);
                }
            }
        }

        CheckIfAreaOfEffectShouldFinishLifeCycle();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<BaseEntityController>(out var baseEntityController))
        {
            if (areaOfEffectsEntry.DoesDamageOverTime)
            {
                iDefenseSequenceManager.RemoveDamageOverTime(this, baseEntityController);
            }
        }
    }

    public void CopyControllerData(BaseAttackController controllerToCopy)
    {
        IsAttackAbleToBeUsed = controllerToCopy.IsAttackAbleToBeUsed;
        iDefenseSequenceManager = controllerToCopy.iDefenseSequenceManager;
        attacksEntry = AttackEntry.CopyAttackEntry(controllerToCopy.AttacksEntry);
        timesAttackWasReflected = controllerToCopy.timesAttackWasReflected;
        HasMadeFinalHit = controllerToCopy.HasMadeFinalHit;
        AttacksEntry.AttackingEntitiesController = controllerToCopy.AttacksEntry.AttackingEntitiesController;
    }

    public void ResetAttacksController()
    {
        attacksEntry.ResetToDefaults();
        timesAttackWasReflected = 0;
        HasMadeFinalHit = false;
        AttacksEntry.AttackingEntitiesController = null;
    }

    private void CheckIfAreaOfEffectShouldFinishLifeCycle()
    {
        float distanceFromStart = Vector2.Distance(transform.position, StartingLocation);

        if (HasMadeFinalHit ||
            attacksEntry.CanProjectileBeReflected && TimesAttackWasReflected >= attacksEntry.MaxAllowedReflections ||
            distanceFromStart >= attacksEntry.AttacksStats.GetStatEntriesTotalValue(StatName.AttackRangeValue) * attacksEntry.MaxTravelDistanceMultiplier)
        {
            StartCoroutine(FinishAttacksLifetime(attacksEntry.DestroyDelayTimer));
        }
    }

    private IEnumerator FinishAttacksLifetime(float destroyDelayTimer)
    {
        yield return new WaitForSeconds(destroyDelayTimer);

        Destroy(gameObject);
    }
}
