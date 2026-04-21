
public enum PlanetResourceCondition
{
    /// <summary>Planet has zero moons.</summary>
    NO_MOONS,

    /// <summary>Planet has at least 'threshold' moons.</summary>
    MIN_MOON_COUNT,

    /// <summary>Planet density is at or above 'threshold'.</summary>
    MIN_DENSITY,

    /// <summary>Planet density is at or below 'threshold'.</summary>
    MAX_DENSITY,

    /// <summary>Planet has a ring.</summary>
    HAS_RING,
}
