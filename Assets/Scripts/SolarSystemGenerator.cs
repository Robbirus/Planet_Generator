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
    [SerializeField] private Vector2 massRange = new Vector2(5f, 50f);
    [SerializeField] private Vector2 densityRange = new Vector2(0.5f, 2f);
    [SerializeField] private Color planetOrbitColor = Color.blue;

    [Header("Moons")]
    [SerializeField] private int minMoons = 0;
    [SerializeField] private int maxMoons = 3;

    [SerializeField] private float moonDistanceMin = 3f;
    [SerializeField] private float moonDistanceMax = 12f;

    [SerializeField] private Color moonOrbitColor = Color.cyan;

    private List<float> usedDistances = new();

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
            body.SetMass(Random.Range(massRange.x, massRange.y));
            body.SetDensity(Random.Range(densityRange.x, densityRange.y));
            body.SetRotationSpeed(Random.Range(minRotationSpeed, maxRotationSpeed));
            body.ApplyScale();

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

            usedDistances.Add(distance);

            GenerateMoons(planet);
        }
    }

    private void GenerateMoons(GameObject planet)
    {
        int moonCount = Random.Range(minMoons, maxMoons);

        for (int i = 0; i < moonCount; i++)
        {
            float distance = Random.Range(moonDistanceMin, moonDistanceMax);

            float angle = Random.Range(0f, Mathf.PI * 2f);

            Vector3 pos = planet.transform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                Random.Range(-1f, 1f),
                Mathf.Sin(angle) * distance
            );

            GameObject moon = Instantiate(moonPrefab, pos, Quaternion.identity);

            CelestialBody body = moon.GetComponent<CelestialBody>();

            body.SetMass(Random.Range(0.5f, 5f));
            body.SetDensity(Random.Range(0.5f, 2f));
            body.ApplyScale();

            float orbitSpeed = Random.Range(minOrbitalSpeed, maxOrbitalSpeed) / distance;
            float inclination = Random.Range(-20f, 20f);

            OrbitBody orbit = moon.AddComponent<OrbitBody>();
            orbit.SetCenter(planet.transform);
            orbit.SetSeed(seed);
            orbit.SetOrbitColor(moonOrbitColor);
            orbit.SetOrbitRadius(distance);
            orbit.SetOrbitSpeed(orbitSpeed);
            orbit.SetOrbitInclination(inclination);
        }
    }

    private float FindSafeDistance()
    {
        float distance;
        bool valid;

        do
        {
            distance = Random.Range(minDistance, maxDistance);
            valid = true;

            foreach (float d in usedDistances)
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
}