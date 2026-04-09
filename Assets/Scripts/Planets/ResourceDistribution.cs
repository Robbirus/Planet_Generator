using System;
using UnityEngine;

/// <summary>
/// Represents a resource and its percentage share on a celestial body.
/// All entries on a planet should sum to 100.
/// </summary>
[Serializable]
public struct ResourceDistribution
{
    [Range(0f, 100f)]
    public float percentage;
    public ResourceType resourceType;

    public ResourceDistribution(ResourceType type, float percentage)
    {
        this.resourceType = type;
        this.percentage = percentage;
    }
}