using UnityEngine;

/// <summary>
/// All stats that a skill node can modify.
/// Add new entries here whenever a new mechanic needs to be upgradeable.
/// </summary>
public enum SkillEffectType
{
    // Ship movement
    ForwardSpeed,
    StrafeSpeed,
    HoverSpeed,
    RotationSpeed,
    RollSpeed,
    LookRateSpeed,

    // Boost
    BoostMultiplier,
    BoostDuration,
    BoostRegenRate,
    BoostRegenDelay,

    // Harvesting
    HarvestRate,
    InventoryCapacity,
}
