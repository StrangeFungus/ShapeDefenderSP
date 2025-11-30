using System.Collections;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class BaseAttackController : MonoBehaviour
{
    // MAIN ATTACK PREFAB DATA
    protected bool isAttackAbleToBeUsed = true;
    public bool IsAttackAbleToBeUsed { get => isAttackAbleToBeUsed; set => isAttackAbleToBeUsed = value; }

    // GENERAL DATA
    [SerializeField] protected AttackEntry attacksEntry;
    public AttackEntry AttacksEntry => attacksEntry;

    protected float timesAttackWasReflected = 0;
    public float TimesAttackWasReflected { get => timesAttackWasReflected; set => timesAttackWasReflected = value; }

    protected bool hasMadeFinalHit = true;
    public bool HasMadeFinalHit { get => HasMadeFinalHit; set => HasMadeFinalHit = value; }

    // TRACKING DATA
    public Vector3 StartingLocation { get; set; }

    protected IDefenseSequenceManager iDefenseSequenceManager;

    protected void Start()
    {
        iDefenseSequenceManager ??= InterfaceContainer.Request<IDefenseSequenceManager>();

        StartingLocation = gameObject.transform.position;

        if (AttacksEntry.AttackingEntitiesController != null)
        {
            attacksEntry.AttackingEntitiesTagType = gameObject.tag;
        }
        else
        {
            Debug.Log($"AttacksEntry.AttackingEntitiesController was null for the attacks controller target data.");
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(attacksEntry.AttackingEntitiesTagType))
        {
            if (collision.gameObject.TryGetComponent<BaseEntityController>(out var baseEntityController))
            {
                if (AttacksEntry.DoesDamageOverTime)
                {
                    iDefenseSequenceManager.ApplyDamageOverTime(this, baseEntityController);
                }
                else
                {
                    iDefenseSequenceManager.AttemptToDamageTarget(this, baseEntityController);
                }
            }
        }

        CheckIfAttackShouldFinishLifeCycle();
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<BaseEntityController>(out var baseEntityController))
        {
            if (AttacksEntry.DoesDamageOverTime)
            {
                iDefenseSequenceManager.RemoveDamageOverTime(this, baseEntityController);
            }
        }
    }

    public void CopyAttacksControllerData(BaseAttackController controllerToCopy)
    {
        IsAttackAbleToBeUsed = controllerToCopy.IsAttackAbleToBeUsed;
        attacksEntry.CopyAttackEntry(controllerToCopy.AttacksEntry);
        timesAttackWasReflected = controllerToCopy.timesAttackWasReflected;
        HasMadeFinalHit = controllerToCopy.HasMadeFinalHit;

        if (AttacksEntry.AttackingEntitiesController == null && controllerToCopy.AttacksEntry.AttackingEntitiesController != null)
        {
            AttacksEntry.AttackingEntitiesController = controllerToCopy.AttacksEntry.AttackingEntitiesController;
        }
    }

    public void ResetAttacksController()
    {
        attacksEntry.ResetToDefaults();
        timesAttackWasReflected = 0;
        HasMadeFinalHit = false;
        AttacksEntry.AttackingEntitiesController = null;
    }

    protected void CheckIfAttackShouldFinishLifeCycle()
    {
        float distanceFromStart = Vector2.Distance(transform.position, StartingLocation);
        if (HasMadeFinalHit ||
            attacksEntry.CanProjectileBeReflected && TimesAttackWasReflected >= attacksEntry.MaxAllowedReflections ||
            distanceFromStart >= attacksEntry.EffectsStats.GetStatsCurrentTotal(StatName.AttackRangeValue, false) * attacksEntry.MaxTravelDistanceMultiplier)
        {
            StartCoroutine(FinishAttacksLifetime(attacksEntry.DestroyDelayTimer));
        }
    }

    protected IEnumerator FinishAttacksLifetime(float destroyDelayTimer)
    {
        yield return new WaitForSeconds(destroyDelayTimer);

        Destroy(gameObject);
    }
}
