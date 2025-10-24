using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AreaOfEffectController : MonoBehaviour
{
    [SerializeField] private AreaOfEffectEntry areaOfEffectsEntry;
    public AreaOfEffectEntry AreaOfEffectsEntry => areaOfEffectsEntry;
    
    public bool FinishLifecycle { get; set; } = false;

    // TARGETING DATA
    [SerializeField] private BaseAttackController attacksController;
    public BaseAttackController AttacksController => attacksController;

    private TargetTrackingContainer targetTrackingData = new();
    public TargetTrackingContainer TargetTrackingData => targetTrackingData;

    // TRACKING DATA
    public Vector3 StartingLocation { get; private set; }
}
