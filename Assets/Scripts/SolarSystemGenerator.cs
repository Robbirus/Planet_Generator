using System.Collections.Generic;
using UnityEngine;

public class SolarSystemGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private Transform sun;

    [Header("Seed")]
    [SerializeField] private int seed = 0;

    [Header("Planets")]
    [SerializeField] private int minPlanets = 3;
    [SerializeField] private int maxPlanets = 8;

    [SerializeField] private float minDistance = 20f;
    [SerializeField] private float maxDistance = 120f;

    [SerializeField] private float minOrbitalSpeed = 10f; 
    [SerializeField] private float maxOrbitalSpeed = 100f;

    [SerializeField] private float minRotationSpeed = 10f;
    [SerializeField] private float maxRotationSpeed = 100f;

    [Header("Planet Properties")]
    [SerializeField] private float minPlanetMass = 5f;
    [SerializeField] private float maxPlanetMass = 50f;
    [SerializeField] private float minPlanetDensity = 0.5f;
    [SerializeField] private float maxPlanetDensity = 2f;
    [SerializeField] private Color planetOrbitColor = Color.blue;
    [Space(10)]

    [Header("Moons")]
    [SerializeField] private int minMoons = 0;
    [SerializeField] private int maxMoons = 3;

    [SerializeField] private float moonDistanceMin = 3f;
    [SerializeField] private float moonDistanceMax = 12f;

    [SerializeField] private float minMoonMass = 0.05f;
    [SerializeField] private float maxMoonMass = 1f;
    [SerializeField] private float minMoonDensity = 0.5f;
    [SerializeField] private float maxMoonDensity = 2f;

    [SerializeField] private Color moonOrbitColor = Color.cyan;

    private List<float> usedPlanetDistances = new();

    private void Start()
    {
        Random.InitState(seed);
        GeneratePlanets();
    }

    private void GeneratePlanets()
    {
        int count = Random.Range(minPlanets, maxPlanets);

        for (int i = 0; i < count; i++)
        {
            float distance = FindSafeDistance();

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float incline = UnityEngine.Random.Range(-3f, 3f); 

            Vector3 pos = sun.position + new Vector3(
                Mathf.Cos(angle) * distance,
                incline,
                Mathf.Sin(angle) * distance
            );

            GameObject planet = Instantiate(planetPrefab, pos, Quaternion.identity);

            CelestialBody body = planet.GetComponent<CelestialBody>();

            // Body
            body.SetMass(Random.Range(minPlanetMass, maxPlanetMass));
            body.SetDensity(Random.Range(minPlanetDensity, maxPlanetDensity));
            body.SetRotationSpeed(Random.Range(minRotationSpeed, maxRotationSpeed));
            body.ApplyScale();
            body.ApplyColor(maxPlanetDensity);

            float orbitSpeed = Random.Range(minOrbitalSpeed, maxOrbitalSpeed) / distance;
            float inclination = Random.Range(-10f, 10f);

            // Orbit
            OrbitBody orbit = planet.AddComponent<OrbitBody>();
            orbit.SetCenter(sun);
            orbit.SetSeed(seed);
            orbit.SetOrbitColor(planetOrbitColor);
            orbit.SetOrbitRadius(distance);
            orbit.SetOrbitSpeed(orbitSpeed);
            orbit.SetOrbitInclination(inclination);

            usedPlanetDistances.Add(distance);

            GenerateMoons(planet);
        }
    }

    private void GenerateMoons(GameObject planet)
    {
        List<(float distance, float radius)> usedMoonDistances = new();
        int moonCount = Random.Range(minMoons, maxMoons);

        for (int i = 0; i < moonCount; i++)
        {
            float mass = Random.Range(minMoonMass, maxMoonMass);
            float density = Random.Range(minMoonDensity, maxMoonDensity);
            float radius = ComputeRadius(mass, density);

            float distance = FindSafeMoonOrbit(radius, usedMoonDistances, moonDistanceMin, moonDistanceMax);
            distance += Random.Range(-0.2f, 0.2f);

            float angle = Random.Range(0f, Mathf.PI * 2f);

            foreach (var usedOrbit in usedMoonDistances)
            {
                if (Mathf.Abs(distance - usedOrbit.distance) < 0.1f)
                {
                    angle += Mathf.PI / 4f;
                }
            }

            Vector3 pos = planet.transform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                Random.Range(-1f, 1f),
                Mathf.Sin(angle) * distance
            );

            GameObject moon = Instantiate(moonPrefab, pos, Quaternion.identity);
            CelestialBody body = moon.GetComponent<CelestialBody>();

            body.SetMass(mass);
            body.SetDensity(density);
            body.ApplyScale();
            body.ApplyColor(maxMoonDensity);

            float orbitSpeed = Random.Range(minOrbitalSpeed, maxOrbitalSpeed) / distance;
            float inclination = Random.Range(-20f, 20f);

            OrbitBody orbit = moon.AddComponent<OrbitBody>();
            orbit.SetCenter(planet.transform);
            orbit.SetSeed(seed + i + planet.GetInstanceID());
            orbit.SetOrbitColor(moonOrbitColor);
            orbit.SetOrbitRadius(distance);
            orbit.SetOrbitSpeed(orbitSpeed);
            orbit.SetOrbitInclination(inclination);

            usedMoonDistances.Add((distance, radius));
        }
    }

    private float ComputeRadius(float mass, float density)
    {
        return Mathf.Pow((3f * mass) / (4f * Mathf.PI * density), 1f / 3f);
    }

    private float FindSafeDistance()
    {
        float distance;
        bool valid;

        do
        {
            distance = Random.Range(minDistance, maxDistance);
            valid = true;

            foreach (float d in usedPlanetDistances)
            {
                if (Mathf.Abs(distance - d) < 10f)
                {
                    valid = false;
                    break;
                }
            }

        } while (!valid);

        return distance;
    }

    private float FindSafeMoonOrbit(float newRadius, List<(float distance, float radius)> used, float min, float max)
    {
        float distance;
        bool valid;

        int safety = 0;

        do
        {
            distance = Random.Range(min, max);
            valid = true;

            foreach (var orbit in used)
            {
                float existingDist = orbit.distance;
                float existingRadius = orbit.radius;

                float minGap = (existingRadius + newRadius) * 2.5f;

                if (Mathf.Abs(distance - existingDist) < minGap)
                {
                    valid = false;
                    break;
                }
            }

            safety++;
            if (safety > 50) break;

        } while (!valid);

        if (!valid)
        {
            distance = min * 2f;
        }

        return distance;
    }
}