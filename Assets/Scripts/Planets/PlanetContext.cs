
public class PlanetContext
{
    /// <summary>Number of moons generated around this planet.</summary>
    private int moonCount;

    /// <summary>Physical density of the planet (from CelestialObjectDataSO range).</summary>
    private float density;

    /// <summary>True if the planet was given a ring during generation.</summary>
    private bool hasRing;

    public PlanetContext(int moonCount, float density, bool hasRing)
    {
        this.moonCount = moonCount;
        this.density = density;
        this.hasRing = hasRing;
    }

    public int GetMoonCount() { return moonCount; }

    public float GetDensity() { return density; }

    public bool HasRing() { return hasRing; }
}