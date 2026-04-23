using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all spaceship movement : free flight, orbital mode, boost and roll.
/// Pure movement logic — no knowledge of UI, inventory or other systems.
/// </summary>
[RequireComponent(typeof(SpaceshipController))]
public class SpaceshipMovement : MonoBehaviour
{
    // Stats
    [Header("Free Movement")]
    [SerializeField] private float forwardSpeed = 25f;
    [SerializeField] private float strafSpeed = 7.5f;
    [SerializeField] private float hoverSpeed = 5f;

    [Header("Orbital Movement")]
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private Vector2 heightRange = new Vector2(50f, 100f);

    [Header("Acceleration")]
    [SerializeField] private float forwardAcceleration = 2.5f;
    [SerializeField] private float strafAcceleration = 2f;
    [SerializeField] private float hoverAcceleration = 2f;

    [Header("Roll")]
    [SerializeField] private float rollSpeed = 90f;
    [SerializeField] private float rollAcceleration = 3.5f;

    [Header("Boost")]
    [SerializeField] private float boostMultiplier = 3f;
    [SerializeField] private float boostAcceleration = 4f;
    [SerializeField] private float boostDuration = 10f;

    [Header("Boost Regeneration")]
    [SerializeField] private float boostRegenDelay = 2f;
    [SerializeField] private float boostRegenRate = 1f;

    [Header("Rotation")]
    [SerializeField] private float lookRateSpeed = 90f;

    [Header("Dead Zone")]
    [SerializeField] private float deadZoneRadius = 50f;

    // InpuT
    [Header("Input")]
    [SerializeField] private InputActionReference movementActionReference;
    [SerializeField] private InputActionReference rollActionReference;
    [SerializeField] private InputActionReference boostActionReference;

    // Debug / runtime state
    [Header("Debug — Current Speeds")]
    [SerializeField] private float activeForwardSpeed;
    [SerializeField] private float activeStrafSpeed;
    [SerializeField] private float activeHoverSpeed;

    [Header("Debug — Boost")]
    [SerializeField] private float boostTimeRemaining;
    [SerializeField] private float boostTimeToAdd;
    [SerializeField] private float activeBoostMultiplier = 1f;

    // Reference 
    [Header("References")]
    [SerializeField] private PlanetLockSystem planetLockSystem;

    private Vector2 screenCenter;
    private Vector2 mouseDistance;
    private Vector2 virtualMousePos;

    private float rollInput;
    private float forwardInput;
    private float strafeInput;
    private float hoverInput;

    private float timeSinceLastBoost;
    private bool lockedMode;
    private bool isFrozen = false;

    
    private void Awake()
    {
        // Lazy loading
        if (planetLockSystem == null)
        {
            planetLockSystem = GetComponent<PlanetLockSystem>();
        }
    }

    private void Start()
    {
        boostTimeRemaining = boostDuration;
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        virtualMousePos = screenCenter;
    }

    private void OnEnable()
    {
        movementActionReference.action.Enable();
        rollActionReference.action.Enable();
        boostActionReference.action.Enable();
    }

    private void OnDisable()
    {
        movementActionReference.action.Disable();
        rollActionReference.action.Disable();
        boostActionReference.action.Disable();
    }

    private void Update()
    {
        if (isFrozen) return;

        // Input is always read so PlanetLockSystem gets mouseDistance even in orbital mode
        DetectInput();

        if (lockedMode)
            UpdateOrbitalMovement();
        else
            UpdateFreeMovement();
    }

   
    private void UpdateFreeMovement()
    {
        HandleRoll();
        HandleMovement();
        UpdateBoostTimer();
    }

    private void HandleMovement()
    {
        bool isBoosting = boostActionReference.action.IsPressed() && boostTimeRemaining > 0f;
        float targetBoost = isBoosting ? boostMultiplier : 1f;
        activeBoostMultiplier = Mathf.Lerp(activeBoostMultiplier, targetBoost, boostAcceleration * Time.deltaTime);

        float boostedForwardSpeed = forwardSpeed * activeBoostMultiplier;

        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, forwardInput * boostedForwardSpeed, forwardAcceleration * Time.deltaTime);
        activeStrafSpeed = Mathf.Lerp(activeStrafSpeed, strafeInput * strafSpeed, strafAcceleration * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, hoverInput * hoverSpeed, hoverAcceleration * Time.deltaTime);

        transform.position += transform.forward * activeForwardSpeed * Time.deltaTime
                            + transform.right * activeStrafSpeed * Time.deltaTime
                            + transform.up * activeHoverSpeed * Time.deltaTime;
    }

    private void HandleRoll()
    {
        transform.Rotate(
            -mouseDistance.y * lookRateSpeed * Time.deltaTime,
             mouseDistance.x * lookRateSpeed * Time.deltaTime,
             rollInput * rollSpeed * Time.deltaTime,
            Space.Self
        );
    }

    private void UpdateOrbitalMovement()
    {
        if (planetLockSystem == null)
        {
            Debug.LogError("[SpaceshipMovement] PlanetLockSystem is missing.", this);
            return;
        }

        Vector3 orbitDir = planetLockSystem.GetOrbitDir();

        bool isBoosting = boostActionReference.action.IsPressed() && boostTimeRemaining > 0f;
        float targetBoost = isBoosting ? boostMultiplier : 1f;
        activeBoostMultiplier = Mathf.Lerp(activeBoostMultiplier, targetBoost, boostAcceleration * Time.deltaTime);

        float boostedForwardSpeed = forwardSpeed * activeBoostMultiplier;
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, forwardInput * boostedForwardSpeed, forwardAcceleration * Time.deltaTime);
        transform.position += transform.forward * activeForwardSpeed * Time.deltaTime;

        // Strafe -> yaw around orbit normal (gravitational rotation)
        if (Mathf.Abs(strafeInput) > 0.01f)
        {
            float yaw = strafeInput * rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.AngleAxis(yaw, orbitDir) * transform.rotation;
        }

        // Hover -> orbital altitude
        if (Mathf.Abs(hoverInput) > 0.01f)
        {
            float altitudeDelta = hoverInput * hoverSpeed * Time.deltaTime;
            planetLockSystem.AdjustAltitude(altitudeDelta, heightRange);
        }

        UpdateBoostTimer();
    }

    private void DetectInput()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        virtualMousePos += mouseDelta;
        virtualMousePos.x = Mathf.Clamp(virtualMousePos.x, 0f, Screen.width);
        virtualMousePos.y = Mathf.Clamp(virtualMousePos.y, 0f, Screen.height);

        Vector2 offset = virtualMousePos - screenCenter;
        float dist = offset.magnitude;

        if (dist < deadZoneRadius)
        {
            mouseDistance = Vector2.zero;
        }
        else
        {
            float activeRange = Mathf.Min(screenCenter.x, screenCenter.y) - deadZoneRadius;
            float t = Mathf.Clamp01((dist - deadZoneRadius) / activeRange);
            mouseDistance = offset.normalized * t;
        }

        rollInput = Mathf.Lerp(
            rollInput,
            rollActionReference.action.ReadValue<Vector2>().x,
            Time.deltaTime * rollAcceleration
        );

        Vector3 movement = movementActionReference.action.ReadValue<Vector3>();
        strafeInput = movement.x;
        hoverInput = movement.y;
        forwardInput = movement.z;
    }

    private void UpdateBoostTimer()
    {
        if (boostActionReference.action.IsPressed() && boostTimeRemaining > 0f)
        {
            boostTimeRemaining = Mathf.Max(0f, boostTimeRemaining - Time.deltaTime);
            timeSinceLastBoost = 0f;
        }
        else
        {
            timeSinceLastBoost += Time.deltaTime;
            if (timeSinceLastBoost >= boostRegenDelay)
                boostTimeRemaining = Mathf.Min(boostDuration, boostTimeRemaining + boostRegenRate * Time.deltaTime);
        }

        // Externally added boost (from skills, pickups, etc.)
        if (boostTimeToAdd > 0f)
        {
            const float boostAddSpeed = 4f;
            float addThisFrame = Mathf.Min(Time.deltaTime * boostAddSpeed, boostTimeToAdd);
            boostTimeRemaining = Mathf.Min(boostDuration, boostTimeRemaining + addThisFrame);
            boostTimeToAdd -= addThisFrame;
        }
    }

    /// <summary>Switches between free-flight and orbital movement mode.</summary>
    public void SetLockedMode(bool locked)
    {
        lockedMode = locked;
        if (locked)
        {
            activeForwardSpeed = 0f;
            activeStrafSpeed = 0f;
            activeHoverSpeed = 0f;
        }
    }

    /// <summary>Applies stat overrides from the SkillTreeManager.</summary>
    public void SetStats(
        float forwardSpeed, float strafeSpeed, float hoverSpeed,
        float rotationSpeed, float rollSpeed, float lookRateSpeed,
        float boostMultiplier, float boostDuration,
        float boostRegenRate, float boostRegenDelay)
    {
        this.forwardSpeed = forwardSpeed;
        this.strafSpeed = strafeSpeed;
        this.hoverSpeed = hoverSpeed;
        this.rotationSpeed = rotationSpeed;
        this.rollSpeed = rollSpeed;
        this.lookRateSpeed = lookRateSpeed;
        this.boostMultiplier = boostMultiplier;
        this.boostDuration = boostDuration;
        this.boostRegenRate = boostRegenRate;
        this.boostRegenDelay = boostRegenDelay;
    }

    /// <summary>Loads base stats from a SpaceshipSO asset.</summary>
    public void SetMovementData(SpaceshipSO data)
    {
        if (data == null) { Debug.LogWarning("[SpaceshipMovement] SpaceshipSO is null.", this); return; }

        forwardAcceleration = data.forwardAcceleration;
        strafAcceleration = data.strafAcceleration;
        hoverAcceleration = data.hoverAcceleration;
        rollAcceleration = data.rollAcceleration;
        rollSpeed = data.rollSpeed;
        forwardSpeed = data.forwardSpeed;
        strafSpeed = data.strafSpeed;
        hoverSpeed = data.hoverSpeed;
        rotationSpeed = data.rotationSpeed;
        heightRange = data.heightRange;
        boostAcceleration = data.boostAcceleration;
        boostMultiplier = data.boostMultiplier;
        boostDuration = data.boostDuration;
        lookRateSpeed = data.lookRateSpeed;
        boostRegenDelay = data.boostRegenDelay;
        boostRegenRate = data.boostRegenRate;
    }

    /// <summary>Adds boost time (called from pickups, skills, etc.).</summary>
    public void AddBoostTime(float time)
    {
        boostTimeToAdd += time;
    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;

        if (frozen)
        {
            // Kill all active velocities so the ship stops instantly
            activeForwardSpeed = 0f;
            activeStrafSpeed = 0f;
            activeHoverSpeed = 0f;
            rollInput = 0f;
            activeBoostMultiplier = 1f;
        }
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }

    public float GetForwardSpeedRatio()
    {
        return Mathf.Clamp01(Mathf.Abs(activeForwardSpeed) / forwardSpeed);
    }

    public float GetActiveForwardSpeed() { return activeForwardSpeed; }
    public float GetBoostTimeRatio() { return boostDuration > 0f ? boostTimeRemaining / boostDuration : 0f; }
    public bool IsBoosting() { return boostActionReference != null && boostActionReference.action.IsPressed() && boostTimeRemaining > 0f; }
    public Vector2 GetMouseDistance() { return mouseDistance; }
    public Vector2 GetVirtualCursor() { return virtualMousePos; }
}