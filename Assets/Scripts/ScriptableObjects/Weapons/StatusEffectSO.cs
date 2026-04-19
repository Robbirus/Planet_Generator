using UnityEngine;

/// <summary>
/// ScriptableObject that holds the tunable parameters for one status effect type.
///
/// USAGE:
///   1. Create a StatusEffectSO asset per effect (FlameData, AcidDataÅc)
///   2. Assign it in ShellSO.effectData
///   3. StatusEffectFactory reads the SO to build the runtime StatusEffect
/// </summary>
[CreateAssetMenu(fileName = "newEffect", menuName = "Game/Weapons/Effect")]
public class StatusEffectSO : ScriptableObject
{
    [Header("Type")]
    [Tooltip("Which effect this data asset configures.")]
    public TypeEffect effectType = TypeEffect.NONE;

    [Header("Timing")]
    [Tooltip("Total duration of the effect in seconds.")]
    [Min(0.1f)]
    public float duration = 4f;

    [Tooltip("Seconds between each damage tick.")]
    [Min(0.05f)]
    public float tickInterval = 0.5f;

    [Header("Damage")]
    [Tooltip("Damage applied per tick.")]
    [Min(0f)]
    public float damagePerTick = 5f;

    [Header("Stack Behaviour")]
    [Tooltip("If true, re-applying the effect adds duration instead of resetting it.")]
    public bool stackDuration = false;

    [Tooltip("Maximum duration cap when stacking (ignored if stackDuration is false).")]
    public float maxStackedDuration = 12f;

    [Header("Color Font")]
    [Tooltip("The color the font will appear.")]
    public Color color;
}
