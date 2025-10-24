using System;

namespace SDSPEnums
{
    public enum StatName
    {
        Default,

        // Health Related:
        CurrentHealthPointsValue,
        MaxHealthPointsValue,
        HealthRegenAmountValue,
        HealthRegenCooldownTimer,
        HealthPointsStolenOnHitPercent,
        HealthPointsStolenOnKillPercent,
        CurrentOverhealCapacityValue,
        MaxOverhealCapacityValue,
        CurrentEnergyShieldValue,
        MaxEnergyShieldValue,
        EnergyShieldRegenAmountValue,
        EnergyShieldRegenCooldownTimer,
        EnergyShieldStolenOnHitPercent,
        EnergyShieldStolenOnKillPercent,

        // Offensive Related:
        AttackRangeValue,
        AttackCooldownTimer,
        AttackAccuracySpreadValue,
        MinimumAttackDamageValue,
        MaximumAttackDamageValue,
        CriticalHitChancePercent,
        CriticalHitDamageMultiplier,
        IgnoreArmorAmountValue,
        MultistrikeChancePercent,
        MaxMultistrikeHitsComboValue,
        ProjectileSpeedValue,
        ProjectileCountValue,
        ProjectilePierceCountValue,
        SplashDamageRadiusValue,
        StatusEffectInflictionChancePercent,
        StatusEffectStackDurationTimer,
        AreaOfEffectRadiusValue,
        DamageOverTimeHitRateTimer,

        // Defensive Related:
        ParryAttackChancePercent,
        ParryCooldownTimer,
        CounterAttackChancePercent,
        BlockChancePercent,
        BlockCooldownTimer,
        BlockAmountValue,
        ReflectAttackChancePercent,
        ReflectedAttackDamageMultiplier,
        ArmorValue,
        ThornsDamageValue,
        CriticalHitResistancePercent,
        CriticalDamageResistancePercent,
        DodgeCooldownTimer,
        DodgeChancePercent,
        StatusEffectInflictionResistanceValue,

        // Resistance Related:
        MagicResistanceValue,
        RangedResistanceValue,
        MeleeResistanceValue,
        BurnResistanceValue,
        SlowResistanceValue,
        FreezeChillResistanceValue,
        StunResistanceValue,
        BlindResistanceValue,
        BleedResistanceValue,
        FearResistanceValue,
        SilenceResistanceValue,

        // Movement Related:
        GroundMovementSpeedValue,
        DashDistanceValue,
        DashChargesValue,
        DashSpeedValue,
        SwimSpeedValue,
        FlyingSpeedValue,
        SpaceTravelSpeedValue,

        // Utility Related: (From Tower Upgrades)
        XPGainMultiplierValue,
        GoldGainMultiplierValue,
        BuffDurationMultiplier,
        DebuffDurationMultiplier,
        SummonsBonusHealthPercent,
        SummonsLimitValue,
        SummonsDamageBonusPercent,

        // Progression Stats:
        LevelValue,
        CurrentExperienceValue,
        ExperienceNeededForNextLevelValue,
        PrestigeLevelValue,
    }

    public enum StatModificationAction
    {
        AddToValue,
        SubtractFromValue,
        SetValueTo
    }

    [Flags]
    public enum MaterialType
    {
        None = 0,
        Organic = 1 << 0,
        Rubber = 1 << 1,
        Plastic = 1 << 2,
        Leather = 1 << 3,
        Wood = 1 << 4,
        Fiberglass = 1 << 5,
        CarbonFiber = 1 << 6,
        Stone = 1 << 7,
        Brick = 1 << 8,
        Concrete = 1 << 9,
        CoatedComposite = 1 << 10,
        Aluminum = 1 << 11,
        Copper = 1 << 12,
        Steel = 1 << 13,
        BioOrganic = 1 << 14,
        HardenedSteel = 1 << 15,
        CompositeSteel = 1 << 16,
        Titanium = 1 << 17,
        Cybernetic = 1 << 18,
        Plasteel = 1 << 19,
        Nanomaterial = 1 << 20,
        VoidAlloy = 1 << 21,
        ReactiveArmor = 1 << 22,
        EnergyTechCoating = 1 << 23,
    }

    public enum EntityMobilityType
    {
        None,
        WagonWheels,
        RubberTires,
        TankTreads,
        WaterPropeller,
        WaterJet,
        MechanicalLegs,
        HoverEngine,
        HelicopterRotor,
        FlyingPropeller,
        JetEngine,
        SpacePropulsionEngine,
        IonThruster,
        WarpDrive,
        TeleportationEngine,
    }

    public enum EnemyRoleType
    {
        Default, // Balanced stats, no strong strengths or weaknesses

        Healer, // + HealthRegenAmountValue, + BuffDurationMultiplier, - AttackDamage, - Defense. Supports allies by healing.
        Support, // + BuffDurationMultiplier, + GoldGainMultiplierValue, - AttackDamage, - Defense. Boosts allies and utility.
        Disruptor, // + StatusEffectInflictionChancePercent, + DebuffDurationMultiplier, - AttackDamage, - Defense. Disables enemies.
        Mage, // + StatusEffectDamageValue, + MagicResistanceValue, - Health, - Defense. Uses powerful spells and AoE.
        Sniper, // + AttackRangeValue, + CriticalHitChancePercent, - Health, - Defense. Deals precise long-range damage.
        Gunner, // + AttackSpeed (AttackCooldownTimer↓), + ProjectileCountValue, - Defense. Rapid-fire ranged attacker.
        Tank, // + MaxHealthPointsValue, + ArmorValue, + BlockChancePercent, - Speed, - AttackRange. Absorbs damage, frontline.
        Assassin, // + GroundMovementSpeedValue, + CriticalHitChancePercent, - Health, - Defense. Fast, high burst damage.
        Boss // + Health, + Damage, + Resistances, + SpecialAbilities, - Speed. Powerful unique enemy with multiple strengths.
    }

    public enum SpawningPattern
    {
        Radial,
        Arc,
        Line,
        Row,
        Grid,
        Spiral,
        Linked,
        Mirror,
        RandomScatter,
        DirectionalScatter,
        DirectionalSpread,
    }

    public enum TargetingPatterns
    {
        Direct,
        Directional,
        Homing,
        Randomized,
    }

    public enum MovementPatterns
    {
        LinearPath,
        LerpedFollow,
        CurvedPath,
        OscillatingPath,
        OrbitingCenter,
        SpiralFromCenter,
        WanderRandomly,
        SnapTo,
        PingPongOffObjects,
        PingPongOffScreenEdge,
        HomingPath,
        Stationary,
        BurstExpansion,
        WaveExpansion,
        RingExpansion,
    }

    public enum LifetimeDurationRules
    {
        TimeBased,
        HitBased,
        DistanceBased,
        ParentLinked,
    }

    [Flags]
    public enum VisualFeedbackBehavior
    {
        None = 0,
        SpawnFlash = 1 << 1,
        Trail = 1 << 2,
        Afterimage = 1 << 3,
        Expand = 1 << 4,
        Shrink = 1 << 5,
        Oscillate = 1 << 6,
        ColorLerp = 1 << 7,
        RotationSpin = 1 << 8,
        Billboard = 1 << 9,
        AttachParticleTo = 1 << 10,
        ScreenShake = 1 << 11,
        LightFlash = 1 << 12,
        SoundTrigger = 1 << 13,
        ShaderPulse = 1 << 14,
        ImpactDecal = 1 << 15,
        ComboAnimation = 1 << 16,
    }

    public enum AttackName
    {
        None,

        //MEDIEVAL MELEE PASSIVE
        //MEDIEVAL  -- >> MODERN >> SPACE >> ADVANCED SPACE
        StationaryWallBlades,
        SpikedWallCollar,
        StoneGrinder,
        ThornedVines,
        PitchCoatedWall,
        //MEDIEVAL MELEE ACTIVE
        WallBlades,
        SpikeTrap,
        LogRamThrust,
        BoulderDrop,
        PoisonCloudBurst,
        FlamingOilPour,
        HotTarSplash,
        SteamVentBlast,

        //MEDIEVAL RANGED PASSIVE
        TarBubbleVents,
        SulfurFumeVents,
        EmberVents,
        HotCoalsBed,
        MudSlick,
        SmokeChimney,
        //MEDIEVAL RANGED ACTIVE
        StoneCatapult,
        BurningLogLauncher,
        FlamingOilPotLauncher,
        BarbedNetLauncher,
        SpringWallTrap,
        ThornedVinesLauncher,
        TarSlickDisperser,
        SmokePelletDropper,
        CaltropsLauncher,

        //MEDIEVAL MAGIC PASSIVE
        ReinforcementRunes,
        ColdShroud,
        HeatShroud,
        CrystalRadiation,
        HealingMoss,
        //MEDIEVAL MAGIC ACTIVE
        WindGust,
        SparkShower,
        LightFlash,
        StonePulse,
        MistBurst,
        EchoBlast,
        DustWhirl,
        VineLash,

        //MEDIEVAL >> MODERN  -- >> SPACE >> ADVANCED SPACE
        //More to come...
    }

    public enum AttackModificationAction
    {
        AddNewAttack,
        RemoveAttack,
        LevelUp,
        SetToTrue,
        SetToFalse
    }

    public enum AttackTargetingBehaviour
    {
        NoTargetObject,
        TargetLocationVector,
        LockOnToObject
    }

    public enum AttackMovementPattern
    {
        // I can add more to act like a boomerang and things like that.

        // ENEMY TARGET PATTERNS
        TargetEnemyDirectly,

        // SELF TARGET PATTERNS
        OrbitSelf,
        StaticOnSelf,

        // RANDOM PATTERNS OR NO TARGET PATTERNS

    }

    [Flags]
    public enum DamageType
    {
        Default = 0,

        Slashing = 1 << 0,
        Piercing = 1 << 1,
        Crushing = 1 << 2,

        Wind = 1 << 3,
        Water = 1 << 4,
        Cold = 1 << 5,
        Earth = 1 << 6,
        Poison = 1 << 7,
        Envenom = 1 << 8,
        Fire = 1 << 9,
        Electric = 1 << 10,
        Radiation = 1 << 11,
        EMP = 1 << 12,

        Shadow = 1 << 13,
        Light = 1 << 14,
        Unholy = 1 << 15,
        Holy = 1 << 16,

        Magic = 1 << 17,
        Ranged = 1 << 18,
        Melee = 1 << 19,

        True = 1 << 20,
    }

    public enum HealingType
    {
        Default = 0,

        EnergyShieldRegen = 1,
        OverHealthRegen = 2,
        HealthPointRegen = 3,
    }

    public enum AreaOfEffectType
    {
        Circle, 
        Square,
        HorizontalBar,
        VerticalBar,
    }

    public enum AreaOfEffectPattern
    {
        Line,
        Wave,
        Scatter,
        Twist,
    }

    public enum StatusEffectName
    {
        Default,

        PowerSurgeStack,
        PiercingStrikeStack,
        CriticalBoostStack,
        FortifiedStack,
        RegenerationStack,
        ResistanceStack,
        HasteStack,
        FocusStack,
        EmpowerStack,
        InspirationStack,
        AdaptiveStack,
        CooldownReductionStack,

        BleedingStack,
        SlowedStack,
        PoisoningStack,
        EnvenomationStack,
        WeakenedStack,
        VulnerabilityStack,
        BurningStack,
        ChilledStack,
        ParalyzationStack,
        BlindedStack,
        CursedStack,
        HexedStack,
        StunEffect,
        KnockbackEffect,
        FreezeEffect,
        FearEffect,
        ImmobilizeEffect,
    }


}
