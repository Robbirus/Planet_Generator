using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipController : MonoBehaviour
{
    [Header("Free Movement")]
    private float forwardSpeed = 25f;
    private float strafSpeed = 7.5f;
    private float hoverSpeed = 5f;
    [Space(5)]

    [Header("Orbital Movement")]
    private float rotationSpeed = 25f;
    private Vector2 heightRange = new Vector2(50, 100); 
    [Space(5)]

    [Header("Boost")]
    private float boostMultiplier = 3f;
    private float boostAcceleration = 4f;
    private float boostDuration = 10f;
    [Space(5)]

    [Header("Boost Regeneration")]
    private float boostRegenDelay = 2f;
    private float boostRegenRate = 1f;
    [Space(5)]

    [Header("Boost Time Management (debug)")]
    [SerializeField] private float boostTimeRemaining;
    [SerializeField] private float boostTimeToAdd;
    [SerializeField] private float activeBoostMultiplier = 1f;

    [Header("Acceleration")]
    private float forwardAcceleration = 2.5f;
    private float strafAcceleration = 2f;
    private float hoverAcceleration = 2f;
    [Space(5)]

    [Header("Current Speeds (debug)")]
    [SerializeField] private float activeForwardSpeed;
    [SerializeField] private float activeStrafSpeed;
    [SerializeField] private float activeHoverSpeed;
    [Space(5)]

    [Header("Rotation")]
    private float lookRateSpeed = 90f;
    [Space(5)]

    [Header("Dead zone")]
    private float deadZoneRadius = 50f;
    [Space(5)]

    [Header("Roll")]
    private float rollSpeed = 90f;
    private float rollAcceleration = 3.5f;
    [Space(5)]

    [Header("Input")]
    [SerializeField] private InputActionReference movementActionReference;
    [SerializeField] private InputActionReference rollActionReference;
    [SerializeField] private InputActionReference boostActionReference;
    [Space(5)]

    [Header("Reference")]
    [SerializeField] private PlanetLockSystem planetLockSystem;
    [SerializeField] private SpaceshipSO spaceshipData;

    private Vector2 screenCenter;
    private Vector2 mouseDistance;
    private Vector2 virtualMousePos;

    private float rollInput;
    private float forwardInput;
    private float strafeInput;
    private float hoverInput;

    private float timeSinceLastBoost = 0f;

    private bool lockedMode = false;
    private void Awake()
    {
        GameManager.instance.RegisterShip(this);

        SetMovementData(spaceshipData);
    }

    private void SetMovementData(SpaceshipSO spaceshipData)
    {
        if(spaceshipData != null)
        {
            // Acceleration
            forwardAcceleration = spaceshipData.forwardAcceleration;
            strafAcceleration   = spaceshipData.strafAcceleration;
            hoverAcceleration   = spaceshipData.hoverAcceleration;

            // Roll
            rollAcceleration    = spaceshipData.rollAcceleration;
            rollSpeed           = spaceshipData.rollSpeed;

            // Orbital Movement
            forwardSpeed        = spaceshipData.forwardSpeed;
            strafSpeed          = spaceshipData.strafSpeed;
            hoverSpeed          = spaceshipData.hoverSpeed;

            // Free Movement
            rotationSpeed       = spaceshipData.rotationSpeed;
            heightRange         = spaceshipData.heightRange;

            // Boost
            boostAcceleration   = spaceshipData.boostAcceleration;
            boostMultiplier     = spaceshipData.boostMultiplier;
            boostDuration       = spaceshipData.boostDuration;

            // Look speed
            lookRateSpeed       = spaceshipData.lookRateSpeed;
        }
        else
        {
            Debug.LogWarning("Movement Data is not set. Using default values.");
        }
    }

    private void OnDestroy()
    {
        GameManager.instance.UnregisterShip(this);
    }

    private void Start()
    {
        boostTimeRemaining = boostDuration;

        screenCenter.x = Screen.width / 2f;
        screenCenter.y = Screen.height / 2f;

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
        // Mouse Input is still detected in locked mode to update the mouseDistance for the planet lock system
        DetectInput();

        if (lockedMode)
        {
            UpdateOrbitalMovement();
        }
        else
        {
            UpdateFreeMovement();
        }
    }

    /// <summary>
    /// Updates the spaceship's position and rotation based on player input when in orbital movement mode (planet-locked).
    /// </summary>
    private void UpdateOrbitalMovement()
    {
        if(planetLockSystem == null)
        {
            Debug.LogError("PlanetLockSystem reference is missing. Please assign it in the inspector.");
            return;
        }

        Vector3 orbitDir = planetLockSystem.GetOrbitDir();

        // Boost handling
        bool isBoosting = boostActionReference.action.IsPressed();
        float targetBoost = isBoosting ? boostMultiplier : 1f;
        activeBoostMultiplier = Mathf.Lerp(activeBoostMultiplier, targetBoost, boostAcceleration * Time.deltaTime);
        if (Mathf.Abs(activeBoostMultiplier) < 0.01f) activeForwardSpeed = 0f;

        // Z/S - advance on the surface: we move along transform.forward
        // PlanetlockSystem re-snaps the position on the frame at each frame
        float boostedForwardSpeed = forwardSpeed * activeBoostMultiplier;
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, forwardInput * boostedForwardSpeed, forwardAcceleration * Time.deltaTime);

        transform.position += transform.forward * activeForwardSpeed * Time.deltaTime;

        // Q/D - Rotation around orbitDir (The local Up of the planet)
        // It turns the ship on itself, like a gravitanonal yaw
        if(Mathf.Abs(strafeInput) > 0.01f)
        {
            float yaw = strafeInput * rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.AngleAxis(yaw, orbitDir) * transform.rotation;
        }

        // Space / Ctrl - orbital altitude clamped by heightRange
        if(Mathf.Abs(hoverInput) > 0.01f)
        {
            float altitudeDelta = hoverInput * hoverSpeed * Time.deltaTime;
            planetLockSystem.AdjustAltitude(altitudeDelta, heightRange);
        }

        UpdateBoostTimer();
    }

    /// <summary>
    /// Updates the spaceship's position and rotation based on player input when in free movement mode.
    /// </summary>
    private void UpdateFreeMovement()
    {
        HandleRoll();
        HandleMovement();
        UpdateBoostTimer();
    }

    /// <summary>
    /// Handles the spaceship's movement in free movement mode, applying acceleration and boost effects based on player input.
    /// It updates the position of the spaceship accordingly.
    /// </summary>
    private void HandleMovement()
    {
        // Boost handling
        bool isBoosting = IsBoosting();
        float targetBoost = isBoosting ? boostMultiplier : 1f;
        activeBoostMultiplier = Mathf.Lerp(activeBoostMultiplier, targetBoost, boostAcceleration * Time.deltaTime);

        float boostedForwardSpeed = forwardSpeed * activeBoostMultiplier;

        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, forwardInput * boostedForwardSpeed, forwardAcceleration * Time.deltaTime);
        activeStrafSpeed = Mathf.Lerp(activeStrafSpeed, strafeInput * strafSpeed, strafAcceleration * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, hoverInput * hoverSpeed, hoverAcceleration * Time.deltaTime);

        // Delete the micro speed when at rest
        if(Mathf.Abs(activeBoostMultiplier) < 0.01f) activeForwardSpeed = 0f;
        if(Mathf.Abs(strafeInput) < 0.01f) activeStrafSpeed = 0f;
        if(Mathf.Abs(hoverInput) < 0.01f) activeHoverSpeed = 0f;

        transform.position += transform.forward * activeForwardSpeed    * Time.deltaTime
                            + transform.right   * activeStrafSpeed      * Time.deltaTime
                            + transform.up      * activeHoverSpeed      * Time.deltaTime;
    }

    /// <summary>
    /// Handles the spaceship's rotation based on the mouse distance from the screen center and the roll input.
    /// </summary>
    private void HandleRoll()
    {
        transform.Rotate(
            -mouseDistance.y * lookRateSpeed * Time.deltaTime, 
             mouseDistance.x * lookRateSpeed * Time.deltaTime,
             rollInput       * rollSpeed     * Time.deltaTime,
            Space.Self
        );
    }

    /// <summary>
    /// Detects player input from the mouse and gamepad, updating the virtual mouse position and calculating the distance from the screen center for rotation purposes. 
    /// It also reads the roll and movement inputs from the assigned Input Actions.
    /// </summary>
    private void DetectInput()
    {
        // Mouse Input
        if(Mouse.current == null)
        {
            Debug.LogError("No mouse detected. Please ensure a mouse is connected and recognized by the system.");
            return;
        }

        // --- Accumulated virtual position via delta ---
        // The mouse never physically exits the screen (Confined),
        // but we accumulate the delta to simulate a free cursor in the window.
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        virtualMousePos += mouseDelta;
        virtualMousePos.x = Mathf.Clamp(virtualMousePos.x, 0f, Screen.width);
        virtualMousePos.y = Mathf.Clamp(virtualMousePos.y, 0f, Screen.height);

        // --- Distance from center in pixels ---
        Vector2 offset = virtualMousePos - screenCenter;
        float dist = offset.magnitude;

        // --- Dead zone handling ---
        if(dist < deadZoneRadius)
        {
            mouseDistance = Vector2.zero;
        }
        else
        {
            // We substract the radius for the rotation goes from 0 when existing the dead zone
            float activeRange = Mathf.Min(screenCenter.x, screenCenter.y) - deadZoneRadius;
            float t = Mathf.Clamp01((dist - deadZoneRadius) / activeRange);
            mouseDistance = offset.normalized * t;
        }

        // Roll Input
        rollInput = Mathf.Lerp(
            rollInput, 
            rollActionReference.action.ReadValue<Vector2>().x, 
            Time.deltaTime * rollAcceleration
        );

        // Movement Input
        Vector3 movement = movementActionReference.action.ReadValue<Vector3>();
        strafeInput = movement.x;
        hoverInput = movement.y;
        forwardInput = movement.z;
    }

    /// <summary>
    /// Updates the boost timer by consuming boost when active, regenerating boost after a delay, and applying any
    /// externally added boost time.
    /// </summary>
    private void UpdateBoostTimer()
    {
        if (boostActionReference.action.IsPressed() && boostTimeRemaining > 0)
        {
            // Consumes the boost
            boostTimeRemaining = Mathf.Max(0f, boostTimeRemaining - Time.deltaTime);
            timeSinceLastBoost = 0f;
        }
        else
        {
            // Passive regeneration after the delay
            timeSinceLastBoost += Time.deltaTime;
            if (timeSinceLastBoost >= boostRegenDelay)
            {
                boostTimeRemaining = Mathf.Min(boostDuration, boostTimeRemaining + boostRegenRate * Time.deltaTime);
            }
        }

        // Add external boost
        if (boostTimeToAdd > 0)
        {
            const float boostAddSpeed = 4f;
            float boostTimeToAddThisFrame = Mathf.Min(Time.deltaTime * boostAddSpeed, boostTimeToAdd);
            boostTimeRemaining = Mathf.Min(boostDuration, boostTimeRemaining + boostTimeToAddThisFrame);
            boostTimeToAdd -= boostTimeToAddThisFrame;
        }
    }

    /// <summary>
    /// Increases the remaining boost duration by the specified amount of time.
    /// </summary>
    /// <param name="time">The amount of time to add to the remaining boost duration, in seconds.</param>
    private void AddBoost(float time)
    {
        boostTimeRemaining += time;
    }

    /// <summary>
    /// Obtains the current forward speed as a ratio of the maximum forward speed (0 to 1).
    /// </summary>
    /// <returns>A value between 0 and 1</returns>
    public float GetForwardSpeedRatio()
    {
        return Mathf.Clamp01(Mathf.Abs(activeForwardSpeed) / forwardSpeed);
    }

    /// <summary>
    /// Determines whether the boost action is currently being performed.
    /// </summary>
    /// <returns>true if the boost action is pressed; otherwise, false.</returns>
    public bool IsBoosting()
    {
        return boostActionReference != null && boostActionReference.action.IsPressed() && boostTimeRemaining > 0;
    }

    /// <summary>
    /// Gets the current mouse distance from the screen center, which is used for determining the spaceship's rotation based on player input.
    /// </summary>
    /// <returns>Mouse distance form the center</returns>
    public Vector2 GetMouseDistance()
    {
        return mouseDistance;
    }

    /// <summary>
    /// Sets the locked mode of the spaceship. When locked, the spaceship will not respond to player input and will have its speeds set to zero.
    /// </summary>
    /// <param name="locked">Boolean</param>
    public void SetLockedMode(bool locked)
    {
        this.lockedMode = locked;
        if (locked)
        {
            activeForwardSpeed = 0f;
            activeStrafSpeed = 0f;
            activeHoverSpeed = 0f;
        }        
    }

    public float GetActiveForwardSpeed()
    {
        return activeForwardSpeed;
    }

    /// <summary>
    /// Gets the name of the currently selected planet.
    /// </summary>
    /// <returns>The name of the selected planet, or "None" if no planet is selected.</returns>
    public string GetCurrentPlanetName()
    {
        if(planetLockSystem != null && planetLockSystem.GetSelectablePlanet() != null)
        {
            return planetLockSystem.GetSelectablePlanet().name;
        }
        return "None";
    }

    /// <summary>
    /// Calculates the distance from the current object to the selectable planet.
    /// </summary>
    /// <returns>The distance to the selectable planet if available; otherwise, 0.</returns>
    public float GetPlanetDistance()
    {
        if(planetLockSystem != null && planetLockSystem.GetSelectablePlanet() != null)
        {
            return Vector3.Distance(transform.position, planetLockSystem.GetSelectablePlanet().position);
        }
        return 0f;
    }

    /// <summary>
    /// Retrieves the current state of the planet as a string.
    /// </summary>
    /// <returns>A string representing the planet's state, or "None" if the state is unavailable.</returns>
    public string GetPlanetState()
    {
        if(planetLockSystem != null)
        {
            return planetLockSystem.GetState().ToString();
        }
        return "None";
    }

    public Vector2 GetVirtualCursor()
    {
        return virtualMousePos;
    }

    /// <summary>
    /// Calculates the ratio of remaining boost time to the total boost duration.
    /// </summary>
    /// <returns>A value between 0 and 1 representing the proportion of boost time remaining.</returns>
    public float GetBoostTimeRatio()
    {
        return boostTimeRemaining / boostDuration;
    }
}