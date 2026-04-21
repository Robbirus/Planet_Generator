using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceRule", menuName = "Game/Celestials/Planet Resource Rule")]
public class PlanetResourceRuleSO : ScriptableObject
{
    [Header("Condition")]
    [Tooltip("What characteristic of the planet triggers this rule.")]
    public PlanetResourceCondition condition;

    [Tooltip("Numeric threshold for MinMoonCount, MinDensity or MaxDensity conditions. " +
             "Ignored for NoMoons and HasRing.")]
    public float threshold = 1f;

    [Header("Boosted Resources")]
    [Tooltip("Resources that receive a weight bonus when this rule is active. " +
             "Only resources that are also in ResourceSO.availableResources can appear.")]
    public List<ResourceType> boostedResources = new();

    [Header("Weight Multiplier")]
    [Tooltip("How much to multiply the base weight of boosted resources. " +
             "1 = no change, 3 = three times more likely than a normal resource.")]
    [Min(1f)]
    public float weightMultiplier = 3f;

    /// <summary>
    /// Returns true if this rule applies to the given planet context.
    /// </summary>
    public bool Evaluate(PlanetContext ctx)
    {
        return condition switch
        {
            PlanetResourceCondition.NO_MOONS        => ctx.GetMoonCount() == 0,
            PlanetResourceCondition.MIN_MOON_COUNT  => ctx.GetMoonCount() >= (int)threshold,
            PlanetResourceCondition.MIN_DENSITY     => ctx.GetDensity() >= threshold,
            PlanetResourceCondition.MAX_DENSITY     => ctx.GetDensity() <= threshold,
            PlanetResourceCondition.HAS_RING        => ctx.HasRing(),
            _ => false
        };
    }
}