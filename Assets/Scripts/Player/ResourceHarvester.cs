using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Harvest resources from the locked Planet over time.
/// 
/// HOW IT WORKS:
///     - While locked, each resource is harvested every tick.
///     - The harvest amount per second is : baseHarvestRate * (resourcePercentage / 100).
///     A resource at 60% yields 3x more than one at 20%.
///     - Harvesting stops automatically when the inventory is full or the ship unlocks.
/// </summary>
[RequireComponent(typeof(PlanetLockSystem))]
[RequireComponent(typeof(ShipInventory))]
public class ResourceHarvester : MonoBehaviour
{
    [Header("Harvest Settings")]
    [Tooltip("Total units harvested per second at 100% resource concentration.")]
    [SerializeField] private float baseHarvestRate = 10f;
    [Space(5)]

    [Tooltip("Seconds between each harvest tick. Lower = smoother but more CPU.")]
    [SerializeField] private float tickInterval = 0.5f;
    [Space(5)]

    [Header("Events (UI Feedback)")]
    [Tooltip("Fires every tick with the list of resources just harvested this tick.")]
    public System.Action<List<(ResourceType type, float amount)>> OnHarvestTick;

    private PlanetLockSystem lockSystem;
    private ShipInventory inventory;

    private float tickTimer = 0f;

    // Last planet we harvested from - used to cache resource data
    private Transform lastHarvestedPlanet;
    private List<ResourceDistribution> cacheResouces = new();

    private void Awake()
    {
        lockSystem = GetComponent<PlanetLockSystem>();
        inventory = GetComponent<ShipInventory>();
    }

    private void Update()
    {
        // Only Harvest while orbiting a planet
        if (lockSystem.GetState() != PlanetLockSystem.LockState.Locked) return;
        if (inventory.IsCompletlyFull()) return;

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;

        tickTimer = 0f;
        Harvest(lockSystem.GetLockedPlanet());
    }

    private void Harvest(Transform planet)
    {
        if (planet == null) return;

        // Refresh resource cache when the planet changes
        if(planet != lastHarvestedPlanet)
        {
            lastHarvestedPlanet = planet;
            cacheResouces.Clear();

            CelestialBody body = planet.GetComponent<CelestialBody>();
            if (body == null) return;

            cacheResouces.AddRange(body.GetResourceDistributions());
        }

        if (cacheResouces.Count == 0) return;

        // Build the list of what we harvest this tick
        List<(ResourceType, float)> tickResults = new List<(ResourceType, float)>();

        foreach (ResourceDistribution resource in cacheResouces) 
        {
            // Amount for this tick = rate * concentration * tickInterval
            float amount = baseHarvestRate * (resource.percentage / 100f) * tickInterval;

            float stored = inventory.Add(resource.resourceType, amount);

            if(stored > 0f)
            {
                tickResults.Add((resource.resourceType, amount));
            }
        }

        // Notify Listeners (HUD, Sound Effects, etc)
        if(tickResults.Count > 0)
        {
            OnHarvestTick?.Invoke(tickResults);
        }

        LogTick(tickResults, planet.name);
    }

    // Debug

    private void LogTick(List<(ResourceType, float)> tickResults, string name)
    {
        if(tickResults.Count == 0) return;

        System.Text.StringBuilder sb = new();
        sb.Append($"[ResourceHarvester] Tick from {name} -> ");
        foreach(var (type, amount) in tickResults)
        {
            sb.Append($"{type}: +{amount:0.00} ");
        }

        Debug.Log(sb.ToString());
    }

    // Getters
    public float GetBaseHarvestRate()
    {
        return baseHarvestRate;
    }

    public float GetTickInterval()
    {
        return tickInterval;
    }
}
