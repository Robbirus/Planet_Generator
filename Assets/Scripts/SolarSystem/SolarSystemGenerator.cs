using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SolarSystemGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private GameObject cometPrefab;
    [SerializeField] private Transform sun;
    [Tooltip("SunLight component on the Sun GO — receives the auto-computed point range after generation.")]
    [SerializeField] private SunLight sunLight;
    [Space(10)]

    [Header("Seed")]
    [SerializeField] private DistantStars distantStars;
    private System.Random stellarRNG;
    private System.Random planetaryRNG;
    private System.Random lunarRNG;
    private System.Random cometRNG;
    [Space(10)]

    [Header("Data")]
    [SerializeField] private CelestialObjectDataSO planetData;
    [SerializeField] private CelestialObjectDataSO moonData;
    [SerializeField] private CelestialObjectDataSO cometData;
    [Space(10)]

    [Header("Comets")]
    [SerializeField] private Vector2Int cometCountRange = new Vector2Int(1, 4);

    [Header("Orbit colors")]
    [SerializeField] private Color planetOrbitColor = Color.blue;
    [SerializeField] private Color moonOrbitColor = Color.cyan;
    [SerializeField] private Color cometOrbitColor = new Color(1f, 0.6f, 0f);
    [Space(10)]

    [Header("Spacing")]
    [Tooltip("Additional safety margin between two planetary paths")]
    [SerializeField] private float planetSafetyMargin = 100f;
    [Tooltip("Minimum margin between two moon orbits")]
    [SerializeField] private float moonOrbitGap = 1.5f;
    [Space(10)]

    [Header("Ring")]
    [Tooltip("Chance (0-1) that a planet with zero moon generate a ring.")]
    [Range(0f, 1f)]
    [SerializeField] private float ringChance = 0.4f;
    [Space(10)]

    [Header("Sun Scale")]
    [Tooltip("Sun world radius = max terrain planet radius x this value.\n" +
             "Applied automatically after planet generation.")]
    [SerializeField] private float sunRadiusMultiplier = 8f;
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

    public static float GenerationProgress { get; private set; }
    public static bool IsGenerationComplete { get; private set; }

    private void Awake()
    {
        GenerationProgress = 0f;
        IsGenerationComplete = false;
    }

    private IEnumerator Start()
    {
        if (sun == null)
        {
            Debug.LogError("[SolarSystemGenerator] 'sun' reference is not set.", this);
            enabled = false;
            yield break;
        }

        if (stellarMapManager == null)
        {
            stellarMapManager = GetComponent<StellarMapManager>();
        }

        GenerationProgress = 0f;
        IsGenerationComplete = false;

        ValidateShapePool();
        GenerateSeeds();
        GenerateStars();

        yield return StartCoroutine(GeneratePlanetAsync());

        GenerateComets();
        CalibrateSunLightRange();
        CalibrateSunScale();

        if (GameManager.instance != null)
        {
            SpaceshipController playerShip = GameManager.instance.GetSpaceshipController();
            
            if (playerShip != null)
            {
                // Case A: The ship is already known, we release it immediately
                playerShip.GetMovement().SetState(ShipState.FreeFlight);
            }
            else
            {
                // Case B: The ship isn’t here yet, we’re waiting for it to register
                GameManager.instance.OnPlayerRegistered += OnPlayerLateRegistration;
            }
        }
        GenerationProgress = 1f;
        IsGenerationComplete = true;

        if (debug) Debug.Log("[SolarSystemGenerator] Generation Done.", this);
    }

    private void OnPlayerLateRegistration(SpaceshipController playerShip)
    {
        if (playerShip != null)
        {
            playerShip.GetMovement().SetState(ShipState.FreeFlight);
        }

        // We unsubscribe immediately to avoid memory leaks.
        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlayerRegistered -= OnPlayerLateRegistration;
        }
    }

    /// <summary>
    /// Checks that all ShapeSettings in the pool share the same planetRadius.
    /// A mix causes the first (or any) planet to get the wrong
    /// collider radius and safe-orbit calculation.
    /// Logs a clear error for each offending asset so the user can fix it.
    /// </summary>
    private void ValidateShapePool()
    {
        if (planetData == null || planetData.shapeSettingsOptions == null) return;

        // Find the most common planetRadius in the pool to use as the reference
        float reference = -1f;
        foreach (ShapeSettings s in planetData.shapeSettingsOptions)
        {
            if (s == null) continue;
            if (reference < 0f) { reference = s.planetRadius; continue; }

            if (!Mathf.Approximately(s.planetRadius, reference))
            {
                Debug.LogError(
                    $"[SolarSystemGenerator] ShapeSettings '{s.name}' has planetRadius = {s.planetRadius} " +
                    $"but the pool reference is {reference}. " +
                    $"All shapes must share the same planetRadius (set them all to {reference} in the Inspector). " +
                    $"This causes incorrect collider size and safe-orbit distance on planets that pick this shape.",
                    s);
            }
        }
    }

    /// <summary>
    /// After all planets are placed, computes the farthest orbit distance and sets
    /// the SunLight's point range to (max distance + planet footprint) * 1.2f so the
    /// sun's point light always reaches the outermost body with a 20% margin.
    /// Falls back gracefully if sunLight is not assigned.
    /// </summary>
    private void CalibrateSunLightRange()
    {
        if (sunLight == null)
        {
            Debug.LogWarning("[SolarSystemGenerator] sunLight not assigned — point range not auto-calibrated.", this);
            return;
        }

        if (usedPlanetOrbits.Count == 0)
        {
            Debug.LogWarning("[SolarSystemGenerator] No planets generated — cannot calibrate sun range.", this);
            return;
        }

        // Farthest orbit edge = orbit distance + the planet's footprint (radius + max moon orbit)
        float maxReach = 0f;
        foreach (var (distance, footprint) in usedPlanetOrbits)
            maxReach = Mathf.Max(maxReach, distance + footprint);

        float calibratedRange = maxReach * 1.2f;
        sunLight.SetPointRange(calibratedRange);

        if (debug)
            Debug.Log($"[SolarSystemGenerator] Sun point range calibrated to {calibratedRange:0} u " +
                      $"(farthest reach {maxReach:0} u × 1.2).", this);
    }

    /// <summary>
    /// Scales the Sun GO so it is visually dominant over the planets.
    /// Sun world radius = terrainRadiusRange.y x sunRadiusMultiplier.
    /// The Unity built-in sphere mesh has local radius 0.5,
    /// so localScale = targetRadius x 2.
    /// Called after GeneratePlanets() so planetData is valid.
    /// </summary>
    private void CalibrateSunScale()
    {
        if (sun == null) return;
        if (planetData == null) return;

        // Use the largest possible planet radius as reference
        float maxPlanetRadius = planetData.terrainRadiusRange.y > 0f
            ? planetData.terrainRadiusRange.y
            : planetData.distanceRange.y * 0.05f; // fallback for non-terrain planets

        float targetSunRadius = maxPlanetRadius * sunRadiusMultiplier;

        // localScale x 0.5 (sphere local radius) = world radius
        float newScale = targetSunRadius * 2f;
        sun.localScale = Vector3.one * newScale;

        if (debug)
            Debug.Log($"[SolarSystemGenerator] Sun scaled to radius {targetSunRadius:0} u " +
                      $"(max planet {maxPlanetRadius:0} u x {sunRadiusMultiplier}x).", this);
    }

    private void GenerateSeeds()
    {
        stellarRNG = SeedManager.GetRNG("stellar");
        planetaryRNG = SeedManager.GetRNG("planetary");
        lunarRNG = SeedManager.GetRNG("lunar");
        cometRNG = SeedManager.GetRNG("comet");
    }

    /// <summary>Generate stars</summary>
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
    public IEnumerator GeneratePlanetAsync(System.Action<int, int> onFaceProgress = null)
    {
        if (planetData == null) { Debug.LogWarning("[Generator] planetData is not assigned.", this); yield break; }

        int count = planetaryRNG.Next((int)planetData.numberRange.x, (int)planetData.numberRange.y);
        if (debug) Debug.Log($"[Generator] Spawning {count} planets.", this);

        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(TrySpawnPlanetAsync(i));
            GenerationProgress = (float)(i + 1) / count * 0.9f;

            yield return null;
        }
    }

    private IEnumerator TrySpawnPlanetAsync(int index) // Changé en IEnumerator
    {
        float mass = SeedManager.Range(planetData.massRange, planetaryRNG);
        float density = SeedManager.Range(planetData.densityRange, planetaryRNG);

        // Draw the actual terrain radius FIRST, needed for accurate orbit spacing.
        float directRadius = planetData.GetRandomTerrainRadius(planetaryRNG);
        float visualRadius = CelestialBody.ComputeRadius(mass, density, planetData.visualScale);
        float effectiveRadius = directRadius > 0f ? directRadius : visualRadius;

        // Footprint = planet radius + worst-case moon orbit so no moon clips a neighbour
        float footprint = effectiveRadius + moonData.distanceRange.y;

        float distance = FindSafePlanetDistance(footprint);
        if (distance < 0f)
        {
            if (debug)
                Debug.LogWarning($"[Generator] Cannot place planet {index} : Not enough space");
            yield break; // Remplacé return par yield break
        }

        float rotationSpeed = SeedManager.Range(planetData.rotationSpeedRange.x, planetData.rotationSpeedRange.y, planetaryRNG);
        float orbitalSpeed = SeedManager.Range(planetData.orbitalSpeedRange.x, planetData.orbitalSpeedRange.y, planetaryRNG) / distance;
        float inclination = SeedManager.Range(planetData.inclinationRange.x, planetData.inclinationRange.y, planetaryRNG);
        float angle = SeedManager.Range(0f, Mathf.PI * 2f, planetaryRNG);

        Vector3 position = sun.position + new Vector3(Mathf.Cos(angle) * distance, inclination, Mathf.Sin(angle) * distance);
        Vector3 rotation = new Vector3(
            SeedManager.Range(-10f, 10f, planetaryRNG),
            0f,
            SeedManager.Range(-10f, 10f, planetaryRNG));

        string name = GetUniqueName(planetData, planetaryRNG, $"Planet_{index}");

        // Pre roll moon count so context is ready before resource gen
        int moonCount = moonData != null
            ? lunarRNG.Next((int)moonData.numberRange.x, (int)moonData.numberRange.y + 1)
            : 0;

        GameObject planet = SpawnBody(planetPrefab, position, Quaternion.Euler(rotation),
            mass, density, rotationSpeed, name,
            planetData.visualScale, planetData.densityRange.y);

        CelestialBody body = planet.GetComponent<CelestialBody>();
        OrbitDrawer drawer = planet.GetComponentInChildren<OrbitDrawer>();

        // Procedural terrain (new Planet.cs system)
        if (body.HasTerrain() && planetData.HasTerrainOptions)
        {
            ShapeSettings shape = planetData.GetRandomShapeSettings(planetaryRNG);
            ColourSettings colour = planetData.GetRandomColourSettings(planetaryRNG);

            // Call the asynchronous version to avoid blocking the thread
            yield return StartCoroutine(body.GenerateTerrainAsync(shape, colour));

            if (directRadius > 0f)
                body.ApplyTerrainScaleDirect(directRadius);
            else
                body.ApplyTerrainScale(planetData.visualScale);

            planet.GetComponent<SphereCollider>().radius = shape.planetRadius;
        }

        // Ring : Only if no moon
        bool hasRing = false;
        if (moonCount == 0 && SeedManager.Range(0f, 1f, planetaryRNG) < ringChance)
        {
            body.SpawnRing();
            hasRing = true;
        }

        body.SetCenter(sun);
        body.RandomizeResource(planetaryRNG, new PlanetContext(moonCount, density, hasRing));

        AddOrbitBody(planet, sun, distance, orbitalSpeed, inclination);
        drawer?.Setup(distance, inclination, planetOrbitColor, stellarMapManager, sun);

        usedPlanetOrbits.Add((distance, footprint));

        // Binary : Only if no moon, random chance
        bool isBinary = moonCount == 0 && SeedManager.Range(0f, 1f, planetaryRNG) < planetData.binaryChance;

        if (isBinary)
        {
            // Waiting for the binary planet’s generation
            yield return StartCoroutine(GenerateBinaryAsync(planet, distance, inclination, orbitalSpeed));
        }
        else
        {
            GenerateMoons(planet, body, moonCount);
        }
    }

    private IEnumerator GenerateBinaryAsync(GameObject primary, float solarOrbitDistance, float inclination, float solarOrbitSpeed)
    {
        float separation = SeedManager.Range(planetData.binarySeparationRange, planetaryRNG);
        float binarySpeed = SeedManager.Range(planetData.binaryOrbitSpeedRange, planetaryRNG);
        float massRatioA = SeedManager.Range(0.35f, 0.65f, planetaryRNG);

        // Barycenter orbits the Sun
        GameObject barycenter = new GameObject($"{primary.name}_Barycenter");
        barycenter.transform.position = primary.transform.position;

        OrbitBody baryOrbit = barycenter.AddComponent<OrbitBody>();
        baryOrbit.SetCenter(sun);
        baryOrbit.SetSeed(stellarRNG);
        baryOrbit.SetOrbitRadius(solarOrbitDistance);
        baryOrbit.SetOrbitSpeed(solarOrbitSpeed);
        baryOrbit.SetOrbitInclination(inclination);
        baryOrbit.SetOrbitColor(planetOrbitColor);

        // Companion Planet
        float compMass = SeedManager.Range(planetData.massRange, planetaryRNG);
        float compDensity = SeedManager.Range(planetData.densityRange, planetaryRNG);
        float compRot = SeedManager.Range(planetData.rotationSpeedRange, planetaryRNG);
        string compName = GetUniqueName(planetData, planetaryRNG, $"{primary.name}_B");

        GameObject companion = SpawnBody(planetPrefab, primary.transform.position, Quaternion.identity,
            compMass, compDensity, compRot, compName,
            planetData.visualScale, planetData.densityRange.y);

        CelestialBody companionBody = companion.GetComponent<CelestialBody>();
        companionBody.RandomizeResource(planetaryRNG, new PlanetContext(0, compDensity, false));

        if (companionBody.HasTerrain() && planetData.HasTerrainOptions)
        {
            ShapeSettings compShape = planetData.GetRandomShapeSettings(planetaryRNG);
            ColourSettings compColour = planetData.GetRandomColourSettings(planetaryRNG);

            // Call the asynchronous version for the companion planet
            yield return StartCoroutine(companionBody.GenerateTerrainAsync(compShape, compColour));

            float compDirectRadius = planetData.GetRandomTerrainRadius(planetaryRNG);
            if (compDirectRadius > 0f)
                companionBody.ApplyTerrainScaleDirect(compDirectRadius);
            else
                companionBody.ApplyTerrainScale(planetData.visualScale);
        }

        // Re-parent both under barycenter
        primary.transform.SetParent(barycenter.transform, true);
        companion.transform.SetParent(barycenter.transform, true);

        DisableOrbitDrawer(primary);
        DisableOrbitDrawer(companion);

        GameObject orbitLine = new GameObject("Orbit Line");
        orbitLine.transform.SetParent(barycenter.transform, false);

        if (!orbitLine.TryGetComponent<LineRenderer>(out _))
        {
            orbitLine.AddComponent<LineRenderer>();
        }

        OrbitDrawer baryDrawer = orbitLine.AddComponent<OrbitDrawer>();
        baryDrawer.Setup(solarOrbitDistance, inclination, planetOrbitColor, stellarMapManager, sun);

        BinaryOrbitBody binary = barycenter.AddComponent<BinaryOrbitBody>();
        binary.Setup(primary.transform, companion.transform,
            separation, binarySpeed, massRatioA, inclination);

        if (debug)
            Debug.Log($"[Generator] Binary: {primary.name} + {compName} sep={separation:0.0}, speed={binarySpeed:0.0}°/s");
    }
    
    private void GenerateMoons(GameObject planet, CelestialBody planetBody, int moonCount)
    {
        if (moonData == null || moonCount == 0) return;

        List<(float, float)> used = new();
        // For terrain planets the formula radius (mass/density) is ~160u while
        // the actual visual surface is 1200-4000u, moons would orbit inside the planet.
        // GetTerrainSurfaceRadius() returns SphereCollider.radius x localScale (world space).
        float planetRadius = planetBody.HasTerrain()
            ? planetBody.GetTerrainSurfaceRadius()
            : planetBody.GetRadius(planetData.visualScale);


        for (int i = 0; i < moonCount; i++)
        {
            float mass = SeedManager.Range(moonData.massRange.x, moonData.massRange.y, lunarRNG);
            float density = SeedManager.Range(moonData.densityRange.x, moonData.densityRange.y, lunarRNG);
            float visualRadius = CelestialBody.ComputeRadius(mass, density, moonData.visualScale);
            float rotationSpeed = SeedManager.Range(moonData.rotationSpeedRange.x, moonData.rotationSpeedRange.y, lunarRNG);

            float distance = FindSafeMoonOrbit(planetRadius, visualRadius, used);
            if (distance < 0f) continue;

            float angle = SeedManager.Range(0f, Mathf.PI * 2f, lunarRNG);
            float incline = SeedManager.Range(-10f, 10f, lunarRNG);
            float orbitSpeed = SeedManager.Range(moonData.orbitalSpeedRange.x, moonData.orbitalSpeedRange.y, lunarRNG) / distance;
            float inclination = SeedManager.Range(-20f, 20f, lunarRNG);

            Vector3 position = planet.transform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                incline,
                Mathf.Sin(angle) * distance);

            string name = GetUniqueName(moonData, lunarRNG, $"Moon_{i}");
            GameObject moon = SpawnBody(moonPrefab, position, Quaternion.identity,
                mass, density, rotationSpeed, name,
                moonData.visualScale, moonData.densityRange.y);

            CelestialBody body = moon.GetComponent<CelestialBody>();
            OrbitDrawer drawer = moon.GetComponentInChildren<OrbitDrawer>();

            body.SetCenter(planet.transform);
            body.RandomizeResource(lunarRNG, new PlanetContext(0, density, false));

            AddOrbitBody(moon, planet.transform, distance, orbitSpeed, inclination);
            drawer?.Setup(distance, inclination, moonOrbitColor, stellarMapManager, planet.transform);

            used.Add((distance, visualRadius));
        }
    }

    private void GenerateComets()
    {
        if (cometData == null || cometPrefab == null) return;

        int count = cometRNG.Next(cometCountRange.x, cometCountRange.y + 1);
        if (debug) Debug.Log($"[Generator] Spawning {count} comets.", this);

        for (int i = 0; i < count; i++)
        {
            float maxOrbit = usedPlanetOrbits.Count > 0
                ? usedPlanetOrbits[^1].distance * 1.5f : 200f;

            float semiMajorAxis = SeedManager.Range(maxOrbit * 0.3f, maxOrbit, cometRNG);
            float eccentricity = SeedManager.Range(cometData.eccentricityRange.x, cometData.eccentricityRange.y, cometRNG);
            float period = SeedManager.Range(cometData.periodRange.x, cometData.periodRange.y, cometRNG);
            float inclination = SeedManager.Range(cometData.inclinationRange.x, cometData.inclinationRange.y, cometRNG);
            float argPerihelion = SeedManager.Range(0f, 360f, cometRNG);
            float startAngle = SeedManager.Range(0, Mathf.PI * 2f, cometRNG);

            float mass = SeedManager.Range(cometData.massRange.x, cometData.massRange.y, cometRNG);
            float density = SeedManager.Range(cometData.densityRange.x, cometData.densityRange.y, cometRNG);
            float rotationSpeed = SeedManager.Range(cometData.rotationSpeedRange.x, cometData.rotationSpeedRange.y, cometRNG);

            string name = GetUniqueName(cometData, cometRNG, $"Comet_{i}");

            GameObject comet = SpawnBody(cometPrefab, sun.position, Quaternion.identity,
                mass, density, rotationSpeed, name, cometData.visualScale, cometData.densityRange.y);
            comet.name = name;

            CelestialBody body = comet.GetComponent<CelestialBody>();
            OrbitDrawer cometDrawer = null;

            body.SetCenter(sun);
            body.RandomizeResource(cometRNG, new PlanetContext(0, density, false));

            EllipticalOrbit ellipse = comet.AddComponent<EllipticalOrbit>();
            ellipse.Setup(sun, semiMajorAxis, eccentricity, period,
                startAngle, inclination, argPerihelion);

            foreach (Transform child in comet.transform)
            {
                cometDrawer = child.GetComponent<OrbitDrawer>();
                if (cometDrawer != null) break;
            }
            cometDrawer.SetEllipse(true);
            cometDrawer?.SetupEllipse(semiMajorAxis, eccentricity, inclination,
                                        argPerihelion, sun, cometOrbitColor, stellarMapManager);

            if (debug)
            {
                Debug.Log($"[Generator] Comet '{name}' a={semiMajorAxis:0.0}, e={eccentricity:0.00}, T={period:0.0}s");
            }
        }
    }

    private GameObject SpawnBody(GameObject prefab, Vector3 position, Quaternion rotation,
                                float mass, float density, float rotationSpeed,
                                string bodyName, float scale, float maxDensity)
    {
        GameObject go = Instantiate(prefab, position, rotation);
        CelestialBody b = go.GetComponent<CelestialBody>();
        b.SetMass(mass);
        b.SetDensity(density);
        b.SetRotationSpeed(rotationSpeed);
        b.SetName(bodyName);
        b.ApplyScale(scale);
        b.ApplyColor(maxDensity);

        return go;
    }

    private void AddOrbitBody(GameObject gameObject, Transform center,
                                float radius, float speed, float inclination)
    {
        OrbitBody ob = gameObject.AddComponent<OrbitBody>();
        ob.SetCenter(center);
        ob.SetSeed(stellarRNG);
        ob.SetOrbitRadius(radius);
        ob.SetOrbitSpeed(speed);
        ob.SetOrbitInclination(inclination);
        ob.SetOrbitColor(planetOrbitColor);
    }

    private void DisableOrbitDrawer(GameObject body)
    {
        OrbitDrawer drawer = null;
        foreach (Transform child in body.transform)
        {
            drawer = child.GetComponent<OrbitDrawer>();
            if (drawer != null)
            {
                drawer.gameObject.SetActive(false);
                break;
            }
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
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float candidate = SeedManager.Range(planetData.distanceRange, stellarRNG);
            bool valid = true;

            foreach (var (existingDist, existingFootprint) in usedPlanetOrbits)
            {
                // Both zones must not overlap another
                // |d1 - d2| > footprint2 + margin
                float requiredGap = existingFootprint + newFootprint + planetSafetyMargin;

                if (Mathf.Abs(candidate - existingDist) < requiredGap)
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
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            float candidate = planetRadius + SeedManager.Range(moonData.distanceRange, lunarRNG);
            bool valid = true;

            foreach (var (existingOrbit, existingRadius) in usedOrbits)
            {
                // Both orbits must not cross if the difference of radius
                // is greater than the body size + margin
                float requiredGap = newMoonRadius + existingRadius + moonOrbitGap;

                if (Mathf.Abs(candidate - existingOrbit) < requiredGap)
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
        if (data == null || data.names == null || data.names.Count == 0)
        {
            return fallback;
        }

        // Collects all the available names (unused names)
        List<string> available = new List<string>();
        foreach (string n in data.names)
        {
            if (!usedNames.Contains(n))
            {
                available.Add(n);
            }
        }

        // If all names are used, we allow double with a numerical added to it
        if (available.Count == 0)
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

    public CelestialObjectDataSO GetPlanetData()
    {
        return planetData;
    }

    public CelestialObjectDataSO GetMoonData()
    {
        return moonData;
    }

    public CelestialObjectDataSO GetCometData()
    {
        return cometData;
    }

    public float GetRingChance()
    {
        return ringChance;
    }

    public Vector2Int GetCometCountRange()
    {
        return cometCountRange;
    }
}