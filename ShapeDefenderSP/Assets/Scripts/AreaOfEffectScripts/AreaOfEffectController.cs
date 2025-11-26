using System.Collections;
using SDSPEnums;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class AreaOfEffectController : BaseAttackController
{
    // MAIN AREA OF EFFECT PREFAB DATA
    [SerializeField] private AreaOfEffectEntry areaOfEffectsEntry;
    public AreaOfEffectEntry AreaOfEffectsEntry => areaOfEffectsEntry;

    [SerializeField] private BaseAttackController parentAttacksController;
    public BaseAttackController ParentAttacksController { get => parentAttacksController; set => parentAttacksController = value; }

    // I need to go through the attack entry and the area of effect entry settings and design functions to reflect the types of actions-
    // it takes that would be different from the behaviour of an attack.
    // Examples being things like following an object.
    private new void Awake()
    {
        base.Awake();
        areaOfEffectsEntry.OnAwake();
    }

    private new void Start()
    {
        base.Start();
        ValidateAndSetUpAOECollider();
    }

    private void ValidateAndSetUpAOECollider()
    {
        float areaOfEffectRadius = areaOfEffectsEntry.RadiusSize + attacksEntry.EffectsStats.GetStatEntriesTotalValue(StatName.AreaOfEffectRadiusValue);
        switch (areaOfEffectsEntry.AreaOfEffectType)
        {
            case AreaOfEffectType.Square:
                if (TryGetComponent<BoxCollider2D>(out var boxCollider2D))
                {
                    if (boxCollider2D == null)
                    {
                        boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
                    }

                    boxCollider2D.isTrigger = true;
                    boxCollider2D.size = new Vector2(areaOfEffectRadius * 2f, areaOfEffectRadius * 2f);
                    areaOfEffectsEntry.HasColliderBeenValidated = true;
                }
                break;

            case AreaOfEffectType.HorizontalBar:
                if (TryGetComponent<CapsuleCollider2D>(out var horzCapsuleCollider2D))
                {
                    if (horzCapsuleCollider2D == null)
                    {
                        horzCapsuleCollider2D = gameObject.AddComponent<CapsuleCollider2D>();
                    }

                    horzCapsuleCollider2D.isTrigger = true;
                    horzCapsuleCollider2D.direction = CapsuleDirection2D.Horizontal;
                    horzCapsuleCollider2D.size = new Vector2(areaOfEffectRadius * 2f, areaOfEffectRadius * 0.5f);
                    areaOfEffectsEntry.HasColliderBeenValidated = true;
                }
                break;

            case AreaOfEffectType.VerticalBar:
                if (TryGetComponent<CapsuleCollider2D>(out var vertCapsuleCollider2D))
                {
                    if (vertCapsuleCollider2D == null)
                    {
                        vertCapsuleCollider2D = gameObject.AddComponent<CapsuleCollider2D>();
                    }

                    vertCapsuleCollider2D.isTrigger = true;
                    vertCapsuleCollider2D.direction = CapsuleDirection2D.Vertical;
                    vertCapsuleCollider2D.size = new Vector2(areaOfEffectRadius * 0.5f, areaOfEffectRadius * 2f);
                    areaOfEffectsEntry.HasColliderBeenValidated = true;
                }
                break;

            case AreaOfEffectType.Circle:
                if (TryGetComponent<CircleCollider2D>(out var circleCollider2D))
                {
                    if (circleCollider2D == null)
                    {
                        circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
                    }

                    circleCollider2D.isTrigger = true;
                    circleCollider2D.radius = areaOfEffectRadius;
                    areaOfEffectsEntry.HasColliderBeenValidated = true;
                }
                break;

            default:
                Debug.LogError($"Failed to set up {areaOfEffectsEntry.AreaOfEffectType} collider for {gameObject.name}");
                StartCoroutine(FinishAttacksLifetime(0.0f));
                break;
        }
    }

    public void CopyAreaOfEffectsControllerData(AreaOfEffectController controllerToCopy)
    {
        IsAttackAbleToBeUsed = controllerToCopy.IsAttackAbleToBeUsed;
        iDefenseSequenceManager = controllerToCopy.iDefenseSequenceManager;
        attacksEntry = AttackEntry.CopyAttackEntry(controllerToCopy.AttacksEntry);
        timesAttackWasReflected = controllerToCopy.timesAttackWasReflected;
        HasMadeFinalHit = controllerToCopy.HasMadeFinalHit;
    }

    public void ResetAreaOfEffectsController()
    {
        attacksEntry.ResetToDefaults();
        timesAttackWasReflected = 0;
        HasMadeFinalHit = false;
        AttacksEntry.AttackingEntitiesController = null;

        areaOfEffectsEntry.ResetDefaultRadius();
        AttacksEntry.ResetToDefaults();
    }
}
