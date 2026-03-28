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

    [Header("Acceleration")]
    [SerializeField] private float forwardAcceleration = 2.5f;
    [SerializeField] private float strafAcceleration = 2f;
    [SerializeField] private float hoverAcceleration = 2f;
    [Space(5)]

    [Header("Current Speeds")]
    [SerializeField] private float activeForwardSpeed;
    [SerializeField] private float activeStrafSpeed;
    [SerializeField] private float activeHoverSpeed;
    [Space(5)]

    [Header("Rotation")]
    [SerializeField] private float lookRateSpeed = 90f;
    [Space(5)]

    private Vector2 lookInput;
    private Vector2 screenCenter;
    private Vector2 mouseDistance;

    private float rollInput;

    [Header("Roll")]
    [SerializeField] private float rollSpeed = 90f;
    [SerializeField] private float rollAcceleration = 3.5f;
    [Space(5)]

    [Header("Input")]
    [SerializeField] private InputActionReference movementActionReference;
    [SerializeField] private InputActionReference rollActionReference;
    [SerializeField] private InputActionReference boostActionReference;

    private float forwardInput;
    private float strafeInput;
    private float hoverInput;

    private void Start()
    {
        screenCenter.x = Screen.width / 2f;
        screenCenter.y = Screen.height / 2f;

        /*
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        */
    }

    private void OnEnable()
    {
        movementActionReference.action.Enable();
    }

    private void OnDisable()
    {
        movementActionReference.action.Disable();
    }

    private void Update()
    {
        DetectInput();
        HandleRoll();
        HandleMovement();
    }

    private void HandleMovement()
    {
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, forwardInput * forwardSpeed, forwardAcceleration * Time.deltaTime);
        activeStrafSpeed = Mathf.Lerp(activeStrafSpeed, strafeInput * strafSpeed, strafAcceleration * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, hoverInput * hoverSpeed, hoverAcceleration * Time.deltaTime);

        if(activeForwardSpeed < 0.01f) activeForwardSpeed = 0f;
        if(activeStrafSpeed < 0.01f) activeStrafSpeed = 0f;

        transform.position += transform.forward * activeForwardSpeed * Time.deltaTime;
        transform.position += (transform.right * activeStrafSpeed * Time.deltaTime) + (transform.up * activeHoverSpeed * Time.deltaTime);
    }

    private void HandleRoll()
    {
        transform.Rotate(-mouseDistance.y * lookRateSpeed * Time.deltaTime, mouseDistance.x * lookRateSpeed * Time.deltaTime, rollInput * rollSpeed * Time.deltaTime, Space.Self);
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

        Vector2 mousePos = Mouse.current.position.ReadValue();

        lookInput = mousePos;

        mouseDistance.x = (lookInput.x - screenCenter.x) / screenCenter.x;
        mouseDistance.y = (lookInput.y - screenCenter.y) / screenCenter.y;

        mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);

        // Rool Input
        rollInput = Mathf.Lerp(rollInput, rollActionReference.action.ReadValue<Vector2>().y, Time.deltaTime * rollAcceleration);

        // Movement Input
        Vector3 movement = movementActionReference.action.ReadValue<Vector3>();
        hoverInput = movement.x;
        strafeInput = movement.y;
        forwardInput = movement.z;
    }
}