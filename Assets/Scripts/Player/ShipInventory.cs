using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all resources harvested by the ship.
/// </summary>
public class ShipInventory : MonoBehaviour
{
    [Header("Capacity per resource")]
    [Tooltip("Maximum units storable for each individual resource type.")]
    [SerializeField] private float maxCapacityPerResource = 2000f;
    [Space(5)]

    [Header("Debug Inventory")]
    [SerializeField] private List<ResourceStock> debugStock = new();

    // Key : ResourceType, Value : current amount stored
    private Dictionary<ResourceType, float> stock = new Dictionary<ResourceType, float>();

    /// <summary> Fires whenever any resource amount changes. Useful to refresh UI. </summary>
    public event Action OnInventoryChanged;

    public float Add(ResourceType type, float amount)
    {
        float current = Get(type);
        float remaining = maxCapacityPerResource - current;
        float toStore = Mathf.Min(amount, remaining);

        if (toStore <= 0) return 0f;

        stock[type] = current + toStore;

        SyncDebugStock();
        OnInventoryChanged?.Invoke();
        return toStore;
    }

    private void SyncDebugStock()
    {
        debugStock.Clear();
        foreach(var pair in stock)
        {
            debugStock.Add(new ResourceStock { type = pair.Key, amount = pair.Value });
        }
    }

    /// <summary>
    /// Returns the amount stored for a given resource type
    /// </summary>
    /// <param name="type">The resource type to get</param>
    /// <returns>The amount stored</returns>
    public float Get(ResourceType type)
    {
        return stock.TryGetValue(type, out float v) ? v : 0f;
    }

    /// <summary>
    /// Returns true if the slot for this specific resource type is full
    /// </summary>
    public bool IsFullFor(ResourceType type)
    {
        return Get(type) >= maxCapacityPerResource;
    }

    /// <summary>
    /// Returns true only if every known resource slot is full.
    /// </summary>
    public bool IsCompletlyFull()
    {
        foreach(ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (!IsFullFor(type)) return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the fill ratio (0-1) for a specific resource type.
    /// </summary>
    public float GetFillRatio(ResourceType type)
    {
        return Get(type) / maxCapacityPerResource;
    }

    /// <summary>
    /// Returns the sum of all stored resources across all slots.
    /// </summary>
    public float GetTotalStored()
    {
        float total = 0f;
        foreach (float v in stock.Values)
        {
            total += v;
        }
        return total;
    }

    /// <summary>
    /// Returns a view of the whole stock.
    /// </summary>
    public Dictionary<ResourceType, float> GetAll()
    {
        return stock;
    }

    /// <summary>
    /// Returns the max capacity per resource.
    /// </summary>
    public float GetMaxCapacityPerResource()
    {
        return maxCapacityPerResource;
    }
}

[Serializable]
public struct ResourceStock
{
    public ResourceType type;
    public float amount;
}
