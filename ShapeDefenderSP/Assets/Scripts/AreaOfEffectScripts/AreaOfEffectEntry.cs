using System.Collections.Generic;
using SDSPEnums;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class AreaOfEffectEntry : BaseDamagingObjectEntry
{
    // AREA OF EFFECT DATA
    [SerializeField] private AreaOfEffectType areaOfEffectType = AreaOfEffectType.Circle;
    public AreaOfEffectType AreaOfEffectType => areaOfEffectType;

    [SerializeField] private AreaOfEffectPattern areaOfEffectPatternPattern = AreaOfEffectPattern.Line;
    public AreaOfEffectPattern AreaOfEffectPatternPattern => areaOfEffectPatternPattern;

    [SerializeField] private MaterialType materialTypes;
    public MaterialType MaterialTypes => materialTypes;

    // COLLIDER
    [SerializeField] private bool hasColliderBeenValidated;
    public bool HasColliderBeenValidated { get =>  hasColliderBeenValidated; set => hasColliderBeenValidated = value; }

    [SerializeField] private float defaultRadiusSize = 10.0f;
    private float radiusSize = 10.0f;
    public float RadiusSize { get => radiusSize; set => radiusSize = value; }

    // AREA OF EFFECT SETTINGS
    [SerializeField] private bool canEffectMove = true;
    public bool CanEffectMove { get => canEffectMove; set => canEffectMove = value; }

    [SerializeField] private bool spawnsWhenAttackSpawns;
    public bool SpawnsWhenAttackSpawns => spawnsWhenAttackSpawns;

    [SerializeField] private bool followsAttackProjectile;
    public bool FollowsAttackProjectile => followsAttackProjectile;

    [SerializeField] private bool spawnsWhenAttackHits;
    public bool SpawnsWhenAttackHits => spawnsWhenAttackHits;

    [SerializeField] private int maxSpawnableEffects = 0;
    public int MaxSpawnableEffects => maxSpawnableEffects;

    public void OnAwake()
    {
        radiusSize = defaultRadiusSize;
    }

    public void ResetDefaultRadius()
    {
        radiusSize = defaultRadiusSize;
    }
}
