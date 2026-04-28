using System.Collections.Generic;
using UnityEngine;

public class SolarSystemGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private Transform sun;
    [Space(10)]

    [Header("Seed")]
    [SerializeField] private DistantStars distantStars;
    private System.Random stellarRNG;
    private System.Random planetaryRNG;
    private System.Random lunarRNG;
    [Space(10)]

    [Header("Planets")]
    private int minPlanets = 3;
    private int maxPlanets = 8;

    private float minDistance = 20f;
    private float maxDistance = 120f;

    private float minOrbitalSpeed = 10f; 
    private float maxOrbitalSpeed = 100f;

    private float minRotationSpeed = 10f;
    private float maxRotationSpeed = 100f;    

    [Header("Planet Properties")]
    private float minPlanetMass = 5f;
    private float maxPlanetMass = 50f;
    private float minPlanetDensity = 0.5f;
    private float maxPlanetDensity = 2f;
    private float planetScale = 1;
    [SerializeField] private Color planetOrbitColor = Color.blue;
    [Space(5)]

    [Header("Planet Spacing")]
    [Tooltip("Additional safety margin between two planetary paths")]
    [SerializeField] private float planetSafetyMargin = 100f;
    [Space(10)]

    [Header("Ring property")]
    [Tooltip("Chance (0-1) that a planet with zero moon generate a ring.")]
    [Range(0f, 1f)]
    [SerializeField] private float ringChance = 0.4f;
    [Space(10)]

    [Header("Moons Properties")]
    private int minMoons = 3;
    private int maxMoons = 8;

    private float minMoonDistance = 3f;
    private float maxMoonDistance = 12f;

    private float minMoonOrbitalSpeed = 10f;
    private float maxMoonOrbitalSpeed = 100f;

    private float minMoonRotationSpeed = 10f;
    private float maxMoonRotationSpeed = 100f;

    private float minMoonMass = 0.05f;
    private float maxMoonMass = 1f;
    private float minMoonDensity = 0.5f;
    private float maxMoonDensity = 2f;

    private float moonScale = 1;

    [SerializeField] private Color moonOrbitColor = Color.cyan;
    [Space(10)]

    [Header("Moon Spacing")]
    [Tooltip("Minimum margin between two moon orbits")]
    [SerializeField] private float moonOrbitGap = 1.5f;
    [Space(10)]

    [Header("Data")]
    [SerializeField] private CelestialObjectDataSO planetData;
    [SerializeField] private CelestialObjectDataSO moonData;
    [Space(10)]

    [Header("References")]
    [SerializeField] private StellarMapManager stellarMapManager;
    [Space(10)]

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    // For each planet: orbital distance + total influence (body radius + moonDistanceMax)
    private readonly List<(float distance, float footprint)> usedPlanetOrbits = new();

    // Names already used - each name is unique
    private readonly HashSet<string> usedNames = new();

    private void Start()
    {
        if(sun == null)
        {
            Debug.LogError("[SolarSystemGenerator] 'sun' reference is not set.", this);
            enabled = false;
            return;
        }

        if(stellarMapManager == null)
        {
            stellarMapManager = GetComponent<StellarMapManager>();
        }

        GenerateSeeds();
        SetPlanetData(planetData);
        SetMoonData(moonData);
        GenerateStars();
        GeneratePlanets();
    }

    private void GenerateSeeds()
    {
        stellarRNG = SeedManager.GetRNG("stellar");
        planetaryRNG = SeedManager.GetRNG("planetary");
        lunarRNG = SeedManager.GetRNG("lunar");
    }

    private void SetPlanetData(CelestialObjectDataSO planetData)
    {
        if(planetData != null)
        {
            minPlanets = (int)planetData.numberRange.x;
            maxPlanets = (int)planetData.numberRange.y;

            minDistance = planetData.distanceRange.x;
            maxDistance = planetData.distanceRange.y;

            minOrbitalSpeed = planetData.orbitalSpeedRange.x;
            maxOrbitalSpeed = planetData.orbitalSpeedRange.y;

            minRotationSpeed = planetData.rotationSpeedRange.x;
            maxRotationSpeed = planetData.rotationSpeedRange.y;

            minPlanetMass = planetData.massRange.x;
            maxPlanetMass = planetData.massRange.y;

            minPlanetDensity = planetData.densityRange.x;
            maxPlanetDensity = planetData.densityRange.y;

            planetScale = planetData.visualScale;
        }
        else
        {
            Debug.LogWarning("Planet data is not set. Using default values.");
        }
    }

    private void SetMoonData(CelestialObjectDataSO moonData)
    {
        if(moonData != null)
        {
            minMoons = (int)moonData.numberRange.x;
            maxMoons = (int)moonData.numberRange.y;

            minMoonDistance = moonData.distanceRange.x;
            maxMoonDistance = moonData.distanceRange.y;

            minMoonOrbitalSpeed = moonData.orbitalSpeedRange.x;
            maxMoonOrbitalSpeed = moonData.orbitalSpeedRange.y;

            minMoonRotationSpeed = moonData.rotationSpeedRange.x;
            maxMoonRotationSpeed = moonData.rotationSpeedRange.y;

            minMoonMass = moonData.massRange.x;
            maxMoonMass = moonData.massRange.y;

            minMoonDensity = moonData.densityRange.x;
            maxMoonDensity = moonData.densityRange.y;

            moonScale = moonData.visualScale;
        }
        else
        {
            Debug.LogWarning("Moon data is not set. Using default values.");
        }
    }

    /// <summary>
    /// Generate stars
    /// </summary>
    private void GenerateStars()
    {
        distantStars?.GenerateStars();
    }

    /// <summary>
    /// Generates a random number of planets with randomized physical and orbital properties, positions them around the
    /// sun, and initializes their orbits and moons.
    /// </summary>
    /// <remarks>Ensures planets are spaced to avoid overlap and logs a warning if placement is not possible
    /// due to insufficient space.</remarks>
    private void GeneratePlanets()
    {
        int count = stellarRNG.Next(minPlanets, maxPlanets);
        if(debug)
            Debug.Log($"[SolarSystemGenerator] Generating {count} planets.");

        for (int i = 0; i < count; i++)
        {
            // Set physical properties first to compute radius for spacing
            float mass              = SeedManager.Range(minPlanetMass, maxPlanetMass, planetaryRNG);
            float density           = SeedManager.Range(minPlanetDensity, maxPlanetDensity, planetaryRNG);
            float visualRadius      = CelestialBody.ComputeRadius(mass, density, planetScale);
            float footprint         = visualRadius + maxMoonDistance; // worst case moon orbit
            float rotationSpeed     = SeedManager.Range(minRotationSpeed, maxRotationSpeed, planetaryRNG); 

            float distance = FindSafePlanetDistance(footprint);
            if (distance < 0f)
            {
                if(debug)
                    Debug.LogWarning($"Cannot place planet {i} : Not enough space");
                continue;
            }

            // Random angle around the sun
            float angle     = SeedManager.Range(0f, Mathf.PI * 2f, planetaryRNG);
            // Inclination with respect to the ecliptic plane, to avoid all bodies being aligned
            float incline   = SeedManager.Range(-3f, 3f, planetaryRNG);

            // Position based on the distance from the sun
            Vector3 pos = sun.position + new Vector3(
                Mathf.Cos(angle) * distance,
                incline,
                Mathf.Sin(angle) * distance
            );

            // Slight incline on itself
            Vector3 rot = new Vector3(
                SeedManager.Range(-10f, 10f, planetaryRNG),
                0f,
                SeedManager.Range(-10, 10, planetaryRNG)
            );

            GameObject planet = Instantiate(planetPrefab, pos, Quaternion.Euler(rot));
            CelestialBody body = planet.GetComponent<CelestialBody>();
            OrbitDrawer orbitDrawer = planet.GetComponentInChildren<OrbitDrawer>();
            
            string name = planetData != null 
                ? GetUniqueName(planetData, planetaryRNG, $"Planet_{i}")
                : $"Planet_{i}";

            // Body
            body.SetMass(mass);
            body.SetDensity(density);
            body.SetRotationSpeed(rotationSpeed);
            body.SetName(name);

            body.ApplyScale(planetScale);
            body.ApplyColor(maxPlanetDensity);

            // pre-roll moons count so the context is complet before gen.
            int moonCount = (int)SeedManager.Range(minMoons, maxMoons, lunarRNG);

            // Ring : Only if no moon, random chance to have it.
            bool hasRing = false;
            if(moonCount == 0 && SeedManager.Range(0f, 1, planetaryRNG) < ringChance)
            {
                body.SpawnRing();
                hasRing = true;
            }

            // Build context  and randomise resources
            PlanetContext ctx = new PlanetContext(moonCount, density, hasRing);
            body.RandomizeResource(planetaryRNG, ctx);

            // Orbital properties
            float orbitSpeed    = SeedManager.Range(minOrbitalSpeed, maxOrbitalSpeed, planetaryRNG) / distance;
            float inclination   = SeedManager.Range(-10f, 10f, planetaryRNG);

            // Orbit
            OrbitBody orbit = planet.AddComponent<OrbitBody>();
            orbit.SetCenter(sun);
            orbit.SetSeed(stellarRNG);
            orbit.SetOrbitColor(planetOrbitColor);
            orbit.SetOrbitRadius(distance);
            orbit.SetOrbitSpeed(orbitSpeed);
            orbit.SetOrbitInclination(inclination);

            // Orbit drawer
            body.SetCenter(sun);
            if(orbitDrawer != null)
            {
                orbitDrawer.Setup(distance, inclination, planetOrbitColor, stellarMapManager);
            }

            // Save influence footprint before generating moons
            usedPlanetOrbits.Add((distance, footprint));

            GenerateMoons(planet, moonCount);
        }
    }

    /// <summary>
    /// Generates and places a random number of moons in orbit around the specified planet, assigning physical and
    /// orbital properties to each moon.
    /// </summary>
    /// <param name="planet">The planet GameObject around which moons are generated.</param>
    private void GenerateMoons(GameObject planet, int moonCount)
    {
        // Store the orbital radius of each moon already placed around this planet
        List<(float orbit, float radius)> usedMoonOrbits = new();

        if(debug)
            Debug.Log($"[SolarSystemGenerator] Generating {moonCount} moons for {planet.name}.");

        CelestialBody planetBody = planet.GetComponent<CelestialBody>();
        if(planetBody == null)
        {
            Debug.LogError($"Planet {planet.name} does not have a CelestialBody component.");
            return;
        }
        float planetVisualRadius = planetBody.GetRadius(planetScale);

        for (int i = 0; i < moonCount; i++)
        {
            float mass              = SeedManager.Range(minMoonMass, maxMoonMass, lunarRNG);
            float density           = SeedManager.Range(minMoonDensity, maxMoonDensity, lunarRNG);
            float visualRadius      = CelestialBody.ComputeRadius(mass, density, moonScale);
            float rotationSpeed     = SeedManager.Range(minMoonRotationSpeed, maxMoonRotationSpeed, lunarRNG);

            // Minimum gap between two orbits : sum of the radii + margin
            float distance = FindSafeMoonOrbit(planetVisualRadius, visualRadius, usedMoonOrbits);
            if(distance < 0f)
            {
                // Debug.LogWarning($"Cannot place the moon_{i} around {planet.name}.");
                continue;
            }

            float angle = SeedManager.Range(0f, Mathf.PI * 2f, lunarRNG);
            float incline = SeedManager.Range(-10f, 10f, lunarRNG);

            Vector3 pos = planet.transform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                incline,
                Mathf.Sin(angle) * distance
            );

            GameObject moon = Instantiate(moonPrefab, pos, Quaternion.identity);
            CelestialBody body = moon.GetComponent<CelestialBody>();
            OrbitDrawer moonDrawer = moon.GetComponentInChildren<OrbitDrawer>();

            string name = moonData != null
                ? GetUniqueName(moonData, lunarRNG, $"Moon_{i}")
                : $"Moon_{i}";

            // Body
            body.SetMass(mass);
            body.SetDensity(density);
            body.SetRotationSpeed(rotationSpeed);
            body.SetName(name);
            body.ApplyScale(moonScale);
            body.ApplyColor(maxMoonDensity);

            // Moon get a neutral context  - no ring of their own 
            body.RandomizeResource(lunarRNG, new PlanetContext(0, density, false));

            float orbitalSpeed = SeedManager.Range(minMoonOrbitalSpeed, maxMoonOrbitalSpeed, lunarRNG) / distance;
            float inclination = SeedManager.Range(-20f, 20f, lunarRNG);

            // Orbit
            OrbitBody orbit = moon.AddComponent<OrbitBody>();
            orbit.SetCenter(planet.transform);
            orbit.SetSeed(stellarRNG);
            orbit.SetOrbitColor(moonOrbitColor);
            orbit.SetOrbitRadius(distance);
            orbit.SetOrbitSpeed(orbitalSpeed);
            orbit.SetOrbitInclination(inclination);

            // Orbit Drawer
            body.SetCenter(planet.transform);
            if(moonDrawer != null)
            {
                moonDrawer.Setup(distance, inclination, moonOrbitColor, stellarMapManager);
            }

            usedMoonOrbits.Add((distance, visualRadius));
        }
    }

    /// <summary>
    /// Finds a valid orbital distance for a new planet that does not overlap with existing planet orbits.
    /// </summary>
    /// <param name="newFootprint">The footprint radius of the new planet.</param>
    /// <param name="maxAttempts">The maximum number of attempts to find a valid distance.</param>
    /// <returns>A valid orbital distance if found; otherwise, -1.</returns>
    private float FindSafePlanetDistance(float newFootprint, int maxAttempts = 100)
    {
        for(int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float candidate = SeedManager.Range(minDistance, maxDistance, stellarRNG);
            bool valid = true;

            foreach(var (existingDist, existingFootprint) in usedPlanetOrbits)
            {
                // Both zones must not overlap another
                // |d1 - d2| > footprint2 + margin
                float requiredGap = existingFootprint + newFootprint + planetSafetyMargin;

                if(Mathf.Abs(candidate - existingDist) < requiredGap)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                return candidate;
            }
        }

        return -1f;
    }

    /// <summary>
    /// Finds a valid moon orbit radius that does not overlap with existing orbits.
    /// </summary>
    /// <param name="newMoonRadius">The radius of the moon to consider when determining orbit spacing.</param>
    /// <param name="usedOrbits">A list of existing orbit radii to avoid overlapping.</param>
    /// <param name="maxAttempts">The maximum number of attempts to find a valid orbit radius. Defaults to 100.</param>
    /// <returns>A valid orbit radius if found; otherwise, -1.</returns>
    private float FindSafeMoonOrbit(float planetRadius, float newMoonRadius, List<(float orbit, float visualRadius)> usedOrbits, int maxAttempts = 100)
    {
        for(int attempts = 0; attempts < maxAttempts; attempts++)
        {
            float candidate = planetRadius + SeedManager.Range(minMoonDistance, maxMoonDistance, lunarRNG);
            bool valid = true;

            foreach(var (existingOrbit, existingRadius) in usedOrbits)
            {
                // Both orbits must not cross if the difference of radius
                // is greater than the body size + margin
                float requiredGap = newMoonRadius + existingRadius + moonOrbitGap;

                if(Mathf.Abs(candidate - existingOrbit) < requiredGap)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                return candidate;
            }
        }

        return -1f;
    }

    /// <summary>
    /// Returns a unique name from the available list, or generates one with a numeric suffix if needed.
    /// Falls back to a default value if no valid data is provided.
    /// </summary>
    /// <param name="data">Source of possible names.</param>
    /// <param name="rng">Random generator used for selection.</param>
    /// <param name="fallback">Default name if no data is available.</param>
    /// <returns>A unique name.</returns>
    private string GetUniqueName(CelestialObjectDataSO data, System.Random rng, string fallback)
    {
        if(data == null || data.names == null || data.names.Count == 0)
        {
            return fallback;
        }

        // Collects all the available names (unused names)
        List<string> available = new List<string>();
        foreach(string n in data.names)
        {
            if (!usedNames.Contains(n))
            {
                available.Add(n);
            }
        }

        // If all names are used, we allow double with a numerical added to it
        if(available.Count == 0)
        {
            string baseName = data.GetRandomName(rng);
            int suffix = 2;
            string unique = $"{baseName} {suffix}";
            while (usedNames.Contains(unique))
            {
                suffix++;
                unique = $"{baseName} {suffix}";
            }

            usedNames.Add(unique);
            return unique;
        }

        int index = rng.Next(0, available.Count);
        string chosen = available[index];
        usedNames.Add(chosen);
        return chosen;
    }
}