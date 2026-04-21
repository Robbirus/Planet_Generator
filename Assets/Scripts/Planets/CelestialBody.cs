using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [Header("Physical properties")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float density = 1f;
    [SerializeField] private ResourceSO resourceSO;

    [Header("Resource Distribution")]
    [SerializeField] private List<ResourceDistribution> resourceDistribution = new();

    [Header("visual")]
    [SerializeField] private Renderer planetRenderer;
    public const float VISUAL_SCALE = 100f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Rings")]
    [SerializeField] private GameObject ringPrefab;
    private bool hasRing = false;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private readonly HashSet<ResourceType> usedResources = new HashSet<ResourceType>();

    /// <summary>
    /// Returns the full resources of the planet
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<ResourceDistribution> GetResourceDistributions()
    { 
        return resourceDistribution; 
    }

    /// <summary>
    /// Returns the percentage (0–100) of a given resource on this planet.
    /// Returns 0 if the resource is absent.
    /// </summary>
    public float GetResourcePercentage(ResourceType type)
    {
        foreach (var entry in resourceDistribution)
            if (entry.resourceType == type)
                return entry.percentage;
        return 0f;
    }

    /// <summary>
    /// Generates a random resource distribution from the pool defined in the assigned ResourceSO.
    /// Uses a seed for reproducibility (same seed = same planet resources every time).
    /// Does nothing if no ResourceSO is assigned.
    /// </summary>
    /// <param name="rng">Rng used for the random generation.</param>
    public void RandomizeResource(System.Random rng, PlanetContext ctx)
    {
        if (resourceSO == null || resourceSO.availableResources.Count == 0)
            return;

        // number of resources the planet will have
        int numberOfRessources = rng.Next(1, resourceSO.availableResources.Count);

        // Shuffles the source list so that the selection is random,
        // then we take the first 'numberOfRessources' without duplicates
        List<ResourceType> shuffled = resourceSO.availableResources;

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            ResourceType temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        // Takes the first 'numberOfResources' from the shuffled
        List<ResourceType> chosenResources = new List<ResourceType>();
        for(int i = 0; i < shuffled.Count && chosenResources.Count < numberOfRessources; i++)
        {
            ResourceType type = shuffled[i];
            if (!usedResources.Contains(type))
            {
                chosenResources.Add(type);
                usedResources.Add(type);
            }
        }

        if(chosenResources.Count == 0)
        {
            return;
        }

        // Compute weight multiplier
        Dictionary<ResourceType, float> multipliers = BuildMultipliers(ctx, chosenResources);

        // Assign weight (x1 - x9) multipliers
        List<float> weights = new();
        foreach(ResourceType type in chosenResources)
        {
            float baseWeight = (float)SeedManager.Range(1, 9, rng);
            float multiplier = multipliers.TryGetValue(type, out float m) ? m : 1f;
            weights.Add(multiplier * baseWeight);
        }

        // Normalise to percentage summing to 100
        float total = weights.Sum();
        List<int> percentages = weights.Select(w => Mathf.RoundToInt(w / total * 100f)).ToList();

        // Absorb rounding drift into last entry
        int drift = 100 - percentages.Sum();
        percentages[^1] += drift;

        resourceDistribution = chosenResources.Select((t, i ) => new ResourceDistribution(t, percentages[i])).ToList();

        if (debug) { LogDistribution(); }
    }

    /// <summary>
    /// Evaluates all the rules against the context and accumulates multipliers
    /// for each boosted resource. Multiple rule stack additively.
    /// </summary>
    private Dictionary<ResourceType, float> BuildMultipliers(PlanetContext ctx, List<ResourceType> chosen)
    {
        Dictionary<ResourceType, float> multipliers = new Dictionary<ResourceType, float>();

        if(resourceSO.rules == null) { return multipliers; }

        foreach(PlanetResourceRuleSO rule in  resourceSO.rules)
        {
            if(rule == null || !rule.Evaluate(ctx)) continue;
            
            foreach(ResourceType boosted in rule.boostedResources)
            {
                // Only boost resources that are actually present on this planet
                if(!chosen.Contains(boosted)) continue;

                if(multipliers.TryGetValue(boosted, out float existing)) 
                {
                    // Additive stacking following this rule (a + b - 1) to avoid exponential growth
                    multipliers[boosted] = existing + (rule.weightMultiplier - 1);
                }
                else
                {
                    multipliers[boosted] = rule.weightMultiplier;
                }
            }
        }

        return multipliers;
    }

    /// <summary>Spawns a ring visual and marks the planet as having a ring.</summary>
    public void SpawnRing()
    {
        if(ringPrefab == null)
        {
            Debug.LogWarning($"[CelestialBody] {gameObject.name} : ringPrefab not assigned.", this);
            return;
        }

        Instantiate(ringPrefab, transform.position, transform.rotation, transform);
        hasRing = true;
    }

    /// <summary>
    /// Logs the final distribution to the Unity console.
    /// </summary>
    private void LogDistribution()
    {
        foreach (var entry in resourceDistribution)
            Debug.Log($"[{gameObject.name}] {entry.resourceType} → {entry.percentage}%");
    }

    /// <summary>
    /// Calculates the radius of a sphere given its mass and density.
    /// Used for safe spacing
    /// </summary>
    /// <param name="mass">The mass of the sphere. Must be a non-negative value.</param>
    /// <param name="density">The density of the sphere. Must be a positive value.</param>
    /// <param name="scale"></param>
    /// <returns>The radius of the sphere, calculated based on the provided mass and density.</returns>
    public static float ComputeRadius(float mass, float density, float scale)
    {
        return Mathf.Pow((3f * mass) / (4f * Mathf.PI * density), 1f / 3f) * VISUAL_SCALE * scale;
    }

    public bool HasRing() {  return hasRing; }

    public float GetRadius(float scale)
    {
        return ComputeRadius(mass, density, scale);
    }

    public void SetMass(float m)
    {
        this.mass = m;
    }

    public void SetDensity(float d)
    {
        this.density = d;
    }

    public float GetMass()
    {
        return mass;
    }

    public void SetRotationSpeed(float rotationSpeed)
    {
        this.rotationSpeed = rotationSpeed;
    }

    public void ApplyScale(float scale)
    {
        float diameter = GetRadius(scale) * 2f;
        transform.localScale = Vector3.one * diameter;
    }

    public void SetName(string name)
    {
        gameObject.name = name;
    }

    public void ApplyColor(float maxDensity)
    {
        if (planetRenderer == null)
        {
            planetRenderer = GetComponent<Renderer>();
        }

        if (planetRenderer == null) return;

        // Base color in HSV
        Color baseColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);

        // Darken based on density
        float darknessFactor = Mathf.Clamp01(1f - (density / maxDensity));

        Color finalColor = baseColor * darknessFactor;

        // Apply to material
        planetRenderer.material = new Material(planetRenderer.sharedMaterial);
        planetRenderer.material.color = finalColor;
    }

    private void Update()
    {
        // Rotation on itself
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}