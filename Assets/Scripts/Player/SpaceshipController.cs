using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 25f;
    [SerializeField] private float strafSpeed = 7.5f;
    [SerializeField] private float hoverSpeed = 5f;
    [Space(5)]

    [Header("Boost")]
    [SerializeField] private float boostMultiplier = 3f;
    [SerializeField] private float boostAcceleration = 4f;
    [SerializeField] private float boostDuration = 10f;
    [Space(5)]

    private float activeBoostMultiplier = 1f;

    [Header("Acceleration")]
    [SerializeField] private float forwardAcceleration = 2.5f;
    [SerializeField] private float strafAcceleration = 2f;
    [SerializeField] private float hoverAcceleration = 2f;
    [Space(5)]

    [Header("Current Speeds (debug)")]
    [SerializeField] private float activeForwardSpeed;
    [SerializeField] private float activeStrafSpeed;
    [SerializeField] private float activeHoverSpeed;
    [Space(5)]

    [Header("Rotation")]
    [SerializeField] private float lookRateSpeed = 90f;
    [Space(5)]

    [Header("Dead zone")]
    [Tooltip("Radius in px around the center where mouse input is ignored.")]
    [SerializeField] private float deadZoneRadius = 50f;
    [Space(5)]

    [Header("Roll")]
    [SerializeField] private float rollSpeed = 90f;
    [SerializeField] private float rollAcceleration = 3.5f;
    [Space(5)]

    [Header("Input")]
    [SerializeField] private InputActionReference movementActionReference;
    [SerializeField] private InputActionReference rollActionReference;
    [SerializeField] private InputActionReference boostActionReference; 
    
    private Vector2 screenCenter;
    private Vector2 mouseDistance;
    private Vector2 virtualMousePos;

    private float rollInput;
    private float forwardInput;
    private float strafeInput;
    private float hoverInput;

    private bool lockedMode = false;

    private void Start()
    {
        screenCenter.x = Screen.width / 2f;
        screenCenter.y = Screen.height / 2f;

        virtualMousePos = screenCenter;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Mouse Input is still detected in locked mode to update the mouseDistance for the planet lock system
        DetectInput();

        if(lockedMode) return;

        HandleRoll();
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Boost handling
        bool isBoosting = boostActionReference.action.IsPressed();
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
    /// 
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

        // Rool Input
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
        return boostActionReference != null && boostActionReference.action.IsPressed();
    }

    public void SetPlayerControlEnabled(bool enabled)
    {
        SetLockedMode(!enabled);
    }

    public Vector2 GetMouseDistance()
    {
        return mouseDistance;
    }

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
}