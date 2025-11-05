using System.Collections;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class BaseAttackController : MonoBehaviour
{
    // MAIN ENTITY PREFAB DATA
    public bool IsAttackAbleToBeUsed { get; set; } = true;

    // GENERAL DATA
    [SerializeField] private AttackEntry attacksEntry;
    public AttackEntry AttacksEntry => attacksEntry;

    private float timesAttackWasReflected = 0;
    public float TimesAttackWasReflected { get => timesAttackWasReflected; set => timesAttackWasReflected = value; }

    public bool HasMadeFinalHit { get; set; } = false;

    // TRACKING DATA
    public Vector3 StartingLocation { get; set; }

    private IDefenseSequenceManager iDefenseSequenceManager;

    private void Awake()
    {
        iDefenseSequenceManager ??= InterfaceContainer.Request<IDefenseSequenceManager>();
    }

    private void Start()
    {
        StartingLocation = gameObject.transform.position;

        if (AttacksEntry.AttackingEntitiesController != null)
        {
            attacksEntry.ParentsTagType = gameObject.tag;
        }
        else
        {
            Debug.Log($"AttacksEntry.AttackingEntitiesController was null for the attacks controller target data.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(attacksEntry.ParentsTagType))
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<BaseEntityController>(out var baseEntityController))
        {
            if (AttacksEntry.DoesDamageOverTime)
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

    private void CheckIfAttackShouldFinishLifeCycle()
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
