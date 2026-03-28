using System;
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
    [Space(10)]

    [Header("Planets")]
    [SerializeField] private int minPlanets = 3;
    [SerializeField] private int maxPlanets = 8;

    [SerializeField] private float minDistance = 20f;
    [SerializeField] private float maxDistance = 120f;

    [SerializeField] private float minOrbitalSpeed = 10f; 
    [SerializeField] private float maxOrbitalSpeed = 100f;

    [SerializeField] private float minRotationSpeed = 10f;
    [SerializeField] private float maxRotationSpeed = 100f;
    [Space(5)]

    [Header("Planet Properties")]
    [SerializeField] private float minPlanetMass = 5f;
    [SerializeField] private float maxPlanetMass = 50f;
    [SerializeField] private float minPlanetDensity = 0.5f;
    [SerializeField] private float maxPlanetDensity = 2f;
    [SerializeField] private Color planetOrbitColor = Color.blue;
    [Space(5)]

    [Header("Planet Spacing")]
    [Tooltip("Additional safety margin between two planetary paths")]
    [SerializeField] private float planetSafetyMargin = 100f;
    [Space(10)]

    [Header("Moons")]
    [SerializeField] private int minMoons = 0;
    [SerializeField] private int maxMoons = 3;
    [Space(5)]

    [SerializeField] private float moonDistanceMin = 3f;
    [SerializeField] private float moonDistanceMax = 12f;
    [Space(5)]

    [SerializeField] private float minMoonMass = 0.05f;
    [SerializeField] private float maxMoonMass = 1f;
    [SerializeField] private float minMoonDensity = 0.5f;
    [SerializeField] private float maxMoonDensity = 2f;

    [SerializeField] private Color moonOrbitColor = Color.cyan;
    [Space(10)]

    [Header("Moon Spacing")]
    [Tooltip("Minimum margin between two moon orbits")]
    [SerializeField] private float moonOrbitGap = 15f;
    [Space(10)]

    // For each planet: orbital distance + total influence (body radius + moonDistanceMax)
    private readonly List<(float distance, float footprint)> usedPlanetOrbits = new();

    private void Start()
    {
        UnityEngine.Random.InitState(seed);
        GeneratePlanets();
    }

    /// <summary>
    /// Generates a random number of planets with randomized physical and orbital properties, positions them around the
    /// sun, and initializes their orbits and moons.
    /// </summary>
    /// <remarks>Ensures planets are spaced to avoid overlap and logs a warning if placement is not possible
    /// due to insufficient space.</remarks>
    private void GeneratePlanets()
    {
        int count = UnityEngine.Random.Range(minPlanets, maxPlanets);

        for (int i = 0; i < count; i++)
        {
            // Set physical properties first to compute radius for spacing
            float mass          = UnityEngine.Random.Range(minPlanetMass, maxPlanetMass) * 1000;
            float density       = UnityEngine.Random.Range(minPlanetDensity, maxPlanetDensity);
            float radius        = ComputeRadius(mass, density);
            float footprint     = radius + moonDistanceMax; // worst case moon orbit
            float rotationSpeed = UnityEngine.Random.Range(minRotationSpeed, maxRotationSpeed);

            float distance = FindSafePlanetDistance(footprint);
            if(distance < 0)
            {
                Debug.LogWarning($"Cannot place planet {i} : Not enough space");
                continue;
            }

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float incline = UnityEngine.Random.Range(-3f, 3f); 

            Vector3 pos = sun.position + new Vector3(
                Mathf.Cos(angle) * distance,
                incline,
                Mathf.Sin(angle) * distance
            );

            GameObject planet = Instantiate(planetPrefab, pos, Quaternion.identity);
            CelestialBody body = planet.GetComponent<CelestialBody>();

            // Body
            body.SetMass(mass);
            body.SetDensity(density);
            body.SetRotationSpeed(rotationSpeed);
            body.ApplyScale();
            body.ApplyColor(maxPlanetDensity);

            float orbitSpeed = UnityEngine.Random.Range(minOrbitalSpeed, maxOrbitalSpeed) / distance;
            float inclination = UnityEngine.Random.Range(-10f, 10f);

            // Orbit
            OrbitBody orbit = planet.AddComponent<OrbitBody>();
            orbit.SetCenter(sun);
            orbit.SetSeed(seed);
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
        List<float> usedMoonOrbits = new();

        int moonCount = UnityEngine.Random.Range(minMoons, maxMoons);

        for (int i = 0; i < moonCount; i++)
        {
            float mass      = UnityEngine.Random.Range(minMoonMass, maxMoonMass) * 1000;
            float density   = UnityEngine.Random.Range(minMoonDensity, maxMoonDensity);
            float radius    = ComputeRadius(mass, density);

            CelestialBody planetBody = planet.GetComponent<CelestialBody>();
            float planetRadius = planetBody.GetRadius();

            // Minimum gap between two orbits : sum of the radii + margin
            float distance = FindSafeMoonOrbit(planetRadius, radius, usedMoonOrbits);
            if(distance < 0f)
            {
                Debug.LogWarning($"Cannot place the moon{i} around {planet.name}.");
                continue;
            }

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

            Vector3 pos = planet.transform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                UnityEngine.Random.Range(-1f, 1f),
                Mathf.Sin(angle) * distance
            );

            GameObject moon = Instantiate(moonPrefab, pos, Quaternion.identity);
            CelestialBody body = moon.GetComponent<CelestialBody>();

            body.SetMass(mass);
            body.SetDensity(density);
            body.ApplyScale();
            body.ApplyColor(maxMoonDensity);

            float orbitSpeed = UnityEngine.Random.Range(minOrbitalSpeed, maxOrbitalSpeed) / distance;
            float inclination = UnityEngine.Random.Range(-20f, 20f);

            OrbitBody orbit = moon.AddComponent<OrbitBody>();
            orbit.SetCenter(planet.transform);
            orbit.SetSeed(seed + i + planet.GetInstanceID());
            orbit.SetOrbitColor(moonOrbitColor);
            orbit.SetOrbitRadius(distance);
            orbit.SetOrbitSpeed(orbitSpeed);
            orbit.SetOrbitInclination(inclination);

            usedMoonOrbits.Add(distance);
        }
    }

    /// <summary>
    /// Calculates the radius of a sphere given its mass and density.
    /// </summary>
    /// <param name="mass">The mass of the sphere.</param>
    /// <param name="density">The density of the sphere.</param>
    /// <returns>The computed radius of the sphere.</returns>
    private float ComputeRadius(float mass, float density)
    {
        return Mathf.Pow((3f * mass) / (4f * Mathf.PI * density), 1f / 3f);
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
            float candidate = UnityEngine.Random.Range(minDistance, maxDistance);
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
    /// <param name="radius">The radius of the moon to consider when determining orbit spacing.</param>
    /// <param name="usedOrbits">A list of existing orbit radii to avoid overlapping.</param>
    /// <param name="maxAttempts">The maximum number of attempts to find a valid orbit radius. Defaults to 100.</param>
    /// <returns>A valid orbit radius if found; otherwise, -1.</returns>
    private float FindSafeMoonOrbit(float planetRadius, float radius, List<float> usedOrbits, int maxAttempts = 100)
    {
        for(int attempts = 0; attempts < maxAttempts; attempts++)
        {
            float candidate = planetRadius + UnityEngine.Random.Range(moonDistanceMin, moonDistanceMax);
            bool valid = true;

            foreach(float existingOrbit in usedOrbits)
            {
                // Both orbits must not cross if the difference of radius
                // is greater than the body size + margin
                float requiredGap = radius + moonOrbitGap;

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

}