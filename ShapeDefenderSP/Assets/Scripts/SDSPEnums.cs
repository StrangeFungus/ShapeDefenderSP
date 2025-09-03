using System;

namespace SDSPEnums
{
    [Flags]
    public enum EntityMaterialTypes
    {
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
        StatusEffectDurationTimer,
        StatusEffectDamageValue,
        StatusEffectDamageHitRateTimer,
        StatusEffectDistanceOrRadiusValue,
        AreaOfEffectRadiusValue,

        // Defensive Related:
        ParryAttackChancePercent,
        ParryCooldownTimer,
        CounterAttackChancePercent,
        BlockChancePercent,
        BlockCooldownTimer,
        BlockAmountValue,
        ReflectDamageChancePercent,
        ReflectDamageAmountValue,
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
    public enum DamageTypes
    {
        Default = 1 << 0,

        Slashing = 1 << 1,
        Piercing = 1 << 2,
        Crushing = 1 << 3,

        Wind = 1 << 4,
        Water = 1 << 5,
        Cold = 1 << 6,
        Earth = 1 << 7,
        Poison = 1 << 8,
        Envenom = 1 << 9,
        Fire = 1 << 10,
        Electric = 1 << 11,
        Radiation = 1 << 12,
        EMP = 1 << 13,

        Shadow = 1 << 14,
        Light = 1 << 15,
        Unholy = 1 << 16,
        Holy = 1 << 17,

        Magic = 1 << 18,
        Ranged = 1 << 19,
        Melee = 1 << 20,

        True = 1 << 21,
    }

    public enum StatusEffectName
    {
        Default,
    }
}
