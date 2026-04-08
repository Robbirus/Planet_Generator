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
    [SerializeField] private int seed = 0;
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


    // For each planet: orbital distance + total influence (body radius + moonDistanceMax)
    private readonly List<(float distance, float footprint)> usedPlanetOrbits = new();

    private void Start()
    {
        if(sun == null)
        {
            Debug.LogError("[SolarSystemGenerator] 'sun' reference is not set.", this);
            enabled = false;
            return;
        }

        GenerateSeeds(seed);
        SetPlanetData(planetData);
        SetMoonData(moonData);
        GenerateStars();
        GeneratePlanets();
    }

    private void GenerateSeeds(int seed)
    {
        UnityEngine.Random.InitState(seed);
        stellarRNG = new(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        planetaryRNG = new(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        lunarRNG = new(UnityEngine.Random.Range(int.MinValue, int.MaxValue));

        if(distantStars != null)
        {
            distantStars.SetSeed(stellarRNG);
        }
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
        distantStars.GenerateStars();
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
        Debug.Log("nb of planet : " + count);

        for (int i = 0; i < count; i++)
        {
            // Set physical properties first to compute radius for spacing
            float mass              = Range(minPlanetMass, maxPlanetMass, planetaryRNG);
            float density           = Range(minPlanetDensity, maxPlanetDensity, planetaryRNG);
            float visualRadius      = CelestialBody.ComputeRadius(mass, density, planetScale);
            float footprint         = visualRadius + maxMoonDistance; // worst case moon orbit
            float rotationSpeed     = Range(minRotationSpeed, maxRotationSpeed, planetaryRNG); 

            float distance = FindSafePlanetDistance(footprint);
            if (distance < 0)
            {
                Debug.LogWarning($"Cannot place planet {i} : Not enough space");
                continue;
            }

            // Random angle around the sun
            float angle     = Range(0f, Mathf.PI * 2f, planetaryRNG);
            // Inclination with respect to the ecliptic plane, to avoid all bodies being aligned
            float incline   = Range(-3f, 3f, planetaryRNG);

            // Position based on the distance from the sun
            Vector3 pos = sun.position + new Vector3(
                Mathf.Cos(angle) * distance,
                incline,
                Mathf.Sin(angle) * distance
            );

            // Slight incline on itself
            Vector3 rot = new Vector3(
                Range(-10f, 10f, planetaryRNG),
                0f,
                Range(-10, 10, planetaryRNG)
            );

            GameObject planet = Instantiate(planetPrefab, pos, Quaternion.Euler(rot));
            CelestialBody body = planet.GetComponent<CelestialBody>();
            
            System.Random nameRNG = new(seed + i);
            string name = planetData != null ? planetData.GetRandomName(nameRNG) : $"Planet_{i}";

            // Body
            body.SetMass(mass);
            body.SetDensity(density);
            body.SetRotationSpeed(rotationSpeed);
            body.SetName(name);
            body.ApplyScale(planetScale);
            body.ApplyColor(maxPlanetDensity);

            // Orbital properties
            float orbitSpeed    = (float)(planetaryRNG.NextDouble() * (maxOrbitalSpeed - minOrbitalSpeed) + minOrbitalSpeed) / distance; 
            float inclination   = (float)(planetaryRNG.NextDouble() * 20f - 10f);

            // Orbit
            OrbitBody orbit = planet.AddComponent<OrbitBody>();
            orbit.SetCenter(sun);
            orbit.SetSeed(stellarRNG);
            orbit.SetOrbitColor(planetOrbitColor);
            orbit.SetOrbitRadius(distance);
            orbit.SetOrbitSpeed(orbitSpeed);
            orbit.SetOrbitInclination(inclination);

            // Save influence footprint before generating moons
            usedPlanetOrbits.Add((distance, footprint));

            GenerateMoons(planet);
        }
    }

    /// <summary>
    /// Generates and places a random number of moons in orbit around the specified planet, assigning physical and
    /// orbital properties to each moon.
    /// </summary>
    /// <param name="planet">The planet GameObject around which moons are generated.</param>
    private void GenerateMoons(GameObject planet)
    {
        // Store the orbital radius of each moon already placed around this planet
        List<(float orbit, float radius)> usedMoonOrbits = new();

        int moonCount = stellarRNG.Next(minMoons, maxMoons);
        Debug.Log("nb of moons : " + moonCount);
        CelestialBody planetBody = planet.GetComponent<CelestialBody>();
        if(planetBody == null)
        {
            Debug.LogError($"Planet {planet.name} does not have a CelestialBody component.");
            return;
        }
        float planetVisualRadius = planetBody.GetRadius(planetScale);

        for (int i = 0; i < moonCount; i++)
        {
            float mass              = Range(minMoonMass, maxMoonMass, lunarRNG);
            float density           = Range(minMoonDensity, maxMoonDensity, lunarRNG);
            float visualRadius      = CelestialBody.ComputeRadius(mass, density, moonScale);
            float rotationSpeed     = Range(minMoonRotationSpeed, maxMoonRotationSpeed, lunarRNG);

            // Minimum gap between two orbits : sum of the radii + margin
            float distance = FindSafeMoonOrbit(planetVisualRadius, visualRadius, usedMoonOrbits);
            if(distance < 0f)
            {
                Debug.LogWarning($"Cannot place the moon_{i} around {planet.name}.");
                continue;
            }

            float angle = Range(0f, Mathf.PI * 2f, lunarRNG);
            float incline = Range(-10f, 10f, lunarRNG);

            Vector3 pos = planet.transform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                incline,
                Mathf.Sin(angle) * distance
            );

            GameObject moon = Instantiate(moonPrefab, pos, Quaternion.identity);
            CelestialBody body = moon.GetComponent<CelestialBody>();

            System.Random nameRNG = new(seed + i * 2);
            string name = planetData != null ? moonData.GetRandomName(nameRNG) : $"Moon_{i}";

            // Body
            body.SetMass(mass);
            body.SetDensity(density);
            body.SetRotationSpeed(rotationSpeed);
            body.SetName(name);
            body.ApplyScale(moonScale);
            body.ApplyColor(maxMoonDensity);

            float orbitSpeed = Range(minMoonOrbitalSpeed, maxMoonOrbitalSpeed, lunarRNG) / distance;
            float inclination = Range(-20f, 20f, lunarRNG);

            OrbitBody orbit = moon.AddComponent<OrbitBody>();
            orbit.SetCenter(planet.transform);
            orbit.SetSeed(stellarRNG);
            orbit.SetOrbitColor(moonOrbitColor);
            orbit.SetOrbitRadius(distance);
            orbit.SetOrbitSpeed(orbitSpeed);
            orbit.SetOrbitInclination(inclination);

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
            float candidate = Range(minDistance, maxDistance, stellarRNG);
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
            float candidate = planetRadius + Range(minMoonDistance, maxMoonDistance, lunarRNG);
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
    /// Generates a random float value within the specified range using the provided random number generator.
    /// </summary>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <param name="rng">The random number generator</param>
    /// <returns></returns>
    private float Range(float min, float max, System.Random rng)
    {
        return (float)(rng.NextDouble() * (max - min) + min);
    }

}