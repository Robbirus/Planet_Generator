using System;
using UnityEngine;

/// <summary>
/// Describes how a skill modifies one stat.
/// 
/// Two modes :
/// Flat = value is added directly
/// Ratio = value is a percentage modifier
/// </summary>
[Serializable]
public struct SkillEffect
{
    [Tooltip("Which stat this effect modifies")]
    public SkillEffectType type;

    [Tooltip("Addition / Multiplier Mode (Flat/Percentage)")]
    public SkillModifierMode mode;

    [Tooltip("The value to add or multiply by.")]
    public float value;
}

public enum SkillModifierMode
{
    Flat,
    Ratio
}
