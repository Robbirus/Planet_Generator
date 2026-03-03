using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SolarSystemGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private Transform sun;

    [Header("Parameters")]
    [SerializeField] private int numberOfPlanets = 5;

    [Header("Seed parameters")]
    [SerializeField] private int seed = 0;
    [SerializeField] private float2 planetsMinMax;
    [SerializeField] private float2 planetSizeMinMax;
    [SerializeField] private float2 planetMassMinMax;
    [SerializeField] private float2 distanceFromSunMinMax;
    [SerializeField] private float2 angleFromSunMinMax;
    [SerializeField] private float2 inclineMinMax;

    [SerializeField] private float safeSpacing = 3f;

    private List<float> usedDistances = new List<float>();

    private void Start()
    {
        GeneratePlanets();
    }

    private void GeneratePlanets()
    {
        UnityEngine.Random.InitState(seed);
        RandomisePlanetsValue();

        for (int i = 0; i < numberOfPlanets; i++)
        {
            float size = UnityEngine.Random.Range(
                planetSizeMinMax.x,
                planetSizeMinMax.y
            );

            float distance = FindValidDistance(size);

            // Random Angle aroud the sun
            float angle = UnityEngine.Random.Range(
                angleFromSunMinMax.x,
                angleFromSunMinMax.y
            );

            float rad = angle * Mathf.Deg2Rad;

            Vector3 position = new Vector3(
                Mathf.Cos(rad) * distance,
                UnityEngine.Random.Range(
                    inclineMinMax.x,
                    inclineMinMax.y
                ), // Slight Incline
                Mathf.Sin(rad) * distance
            );

            GameObject planet = Instantiate(planetPrefab, position, Quaternion.identity);

            planet.transform.localScale = Vector3.one * size;

            Rigidbody rb = planet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.mass = UnityEngine.Random.Range(
                    planetMassMinMax.x,
                    planetMassMinMax.y
                );
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero; // IMPORTANT : No Speed Here
            }

            planet.tag = "Celestials";

            usedDistances.Add(distance);
        }
    }

    private void RandomisePlanetsValue()
    {
        numberOfPlanets = (int)UnityEngine.Random.Range(
            planetsMinMax.x,
            planetsMinMax.y
        );
    }

    private float FindValidDistance(float newScale)
    {
        float newDistance;
        bool valid;

        do
        {
            newDistance = UnityEngine.Random.Range(
                distanceFromSunMinMax.x, 
                distanceFromSunMinMax.y
            );

            valid = true;

            foreach (float existingDistance in usedDistances)
            {
                float minAllowed = newScale + safeSpacing;

                if (Mathf.Abs(newDistance - existingDistance) < minAllowed)
                {
                    valid = false;
                    break;
                }
            }

        } while (!valid);

        return newDistance;
    }
}