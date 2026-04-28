using UnityEngine;

/// <summary>
/// Central coordinator for all player subsystems.
/// Holds references, delegates to the right component, and exposes
/// a unified API consumed by UIShip, ShipCamera, SkillTreeManager, etc.
/// </summary>
public class SpaceshipController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private SpaceshipSO spaceshipData;

    [Header("Player Components")]
    [SerializeField] private SpaceshipMovement movement;
    [SerializeField] private PlanetLockSystem planetLockSystem;
    [SerializeField] private ShipInventory inventory;
    [SerializeField] private ResourceHarvester harvester;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Camera playerCamera;

    private void Awake()
    {
        GameManager.instance.RegisterShip(this);
        movement.SetMovementData(spaceshipData);
    }

    private void OnDestroy()
    {
        GameManager.instance.UnregisterShip(this);
    }

    // Delegation -> SpaceshipMovement 

    /// <summary>Switches between free-flight and orbital mode.</summary>
    public void SetLockedMode(bool locked) { movement.SetLockedMode(locked); }

    /// <summary>Called by SkillTreeManager after an upgrade is purchased.</summary>
    public void SetStats(
        float forwardSpeed, float strafeSpeed, float hoverSpeed,
        float rotationSpeed, float rollSpeed, float lookRateSpeed,
        float boostMultiplier, float boostDuration,
        float boostRegenRate, float boostRegenDelay)
    {
        movement.SetStats(
            forwardSpeed, strafeSpeed, hoverSpeed,
            rotationSpeed, rollSpeed, lookRateSpeed,
            boostMultiplier, boostDuration,
            boostRegenRate, boostRegenDelay);
    }

    // Getters for UIShip 

    public float GetForwardSpeedRatio() { return movement.GetForwardSpeedRatio(); }
    public float GetActiveForwardSpeed() { return movement.GetActiveForwardSpeed(); }
    public float GetBoostTimeRatio() { return movement.GetBoostTimeRatio(); }
    public bool IsBoosting() { return movement.IsBoosting(); }
    public Vector2 GetMouseDistance() { return movement.GetMouseDistance(); }
    public Vector2 GetVirtualCursor() { return movement.GetVirtualCursor(); }

    // GETTERS

    public string GetCurrentPlanetName()
    {
        if (planetLockSystem == null) return "None";
        Transform planet = planetLockSystem.GetActivePlanet();
        return planet != null ? planet.name : "None";
    }

    public float GetPlanetDistance()
    {
        if (planetLockSystem == null) return 0f;
        Transform planet = planetLockSystem.GetActivePlanet();
        return planet != null ? Vector3.Distance(transform.position, planet.position) : 0f;
    }

    public string GetPlanetState()
    {
        return planetLockSystem != null ? planetLockSystem.GetState().ToString() : "None";
    }

    // Getters

    public ShipInventory GetInventory() { return inventory; }
    public ResourceHarvester GetHarvester() { return harvester; }
    public PlanetLockSystem GetPlanetLock() { return planetLockSystem; }
    public SpaceshipMovement GetMovement() { return movement; }
    public WeaponManager GetWeaponManager() { return weaponManager; }
    public Camera GetPlayerCamera() {  return playerCamera; }
}