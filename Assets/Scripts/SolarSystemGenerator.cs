using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SolarSystemGenerator : MonoBehaviour
{
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private Transform sun;

    [Header("Seed parameters")]
    [SerializeField] private int seed = 0;

    [Header("Planet count")]
    [SerializeField] private float2 planetsMinMax;
    [SerializeField] private int numberOfPlanets = 5;

    [Header("Mass & Density")]
    [SerializeField] private float2 massUnityScale;
    [SerializeField] private float2 densityScale;

    [Header("Orbital placement")]
    [SerializeField] private float2 distanceFromSun;
    [SerializeField] private float2 inclineMinMax;

    [SerializeField] private float safeSpacing = 4f;

    private List<(float distance, float radius)> usedDistances = new List<(float, float)>();

    private void Start()
    {
        GeneratePlanets();
    }

    private void GeneratePlanets()
    {
        UnityEngine.Random.InitState(seed);

        int numPlanets = (int)UnityEngine.Random.Range(
            planetsMinMax.x,
            planetsMinMax.y
        );

        for (int i = 0; i < numberOfPlanets; i++)
        {
            float mass = UnityEngine.Random.Range(massUnityScale.x, massUnityScale.y);
            float density = UnityEngine.Random.Range(densityScale.x, densityScale.y);

            float radius = Mathf.Pow((3 * mass) / (4 * Mathf.PI * density), 1f / 3f);

            float distance = FindSafeDistance(radius);

            float angle = UnityEngine.Random.Range(0f, Mathf.PI *2f);
            float incline = UnityEngine.Random.Range(inclineMinMax.x, inclineMinMax.y);

            Vector3 pos = sun.position + new Vector3(
                Mathf.Cos(angle) * distance, 
                incline, 
                Mathf.Sin(angle) * distance
            );

            GameObject planet = Instantiate(planetPrefab, pos, Quaternion.identity);

            CelestialBody body = planet.GetComponent<CelestialBody>();

            body.SetMass(mass);
            body.SetDensity(density);
            body.ApplyScale();

            usedDistances.Add((distance, radius));
        }
    }

    private float FindSafeDistance(float newRadius)
    {
        float distance;
        bool valid;

        int safety = 0;

        do
        {
            distance = UnityEngine.Random.Range(distanceFromSun.x, distanceFromSun.y);
            valid = true;

            foreach (var orbit in usedDistances)
            {
                float existingDistance = orbit.distance;
                float existingRadius = orbit.radius;

                float safeDistance = existingRadius + newRadius + 2f; // marge

                if (Mathf.Abs(distance - existingDistance) < safeDistance)
                {
                    valid = false;
                    break;
                }
            }

            safety++;
            if (safety > 100) break;

        } while (!valid);

        return distance;
    }
}