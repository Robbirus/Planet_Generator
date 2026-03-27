using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float drag = 2f;
    [SerializeField] private float currentSpeed = 0f;

    [Header("Input")]
    [SerializeField] private InputActionReference movementActionReference;

    private float forwardInput;
    private float rotationInput;


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
    }

    private void FixedUpdate()
    {
        UpdateSpeed();
        MovingPlayer();
        TurnPlayer();
    }

    private void TurnPlayer()
    {
        float turnInput = rotationInput * rotationSpeed * Time.fixedDeltaTime;
        transform.Rotate(0f, turnInput, 0f);
    }

    private void MovingPlayer()
    {
        Vector3 movement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        transform.position += movement;
    }

    private void UpdateSpeed()
    {
        // Acceleration
        currentSpeed += forwardInput * acceleration * Time.fixedDeltaTime;

        // Drag
        currentSpeed = Mathf.Lerp(currentSpeed, 0f, drag * Time.fixedDeltaTime);

        // Clamp speed
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
    }

    /// <summary>
    /// Detect the input from the player and update the forward and rotation speed accordingly.
    /// </summary>
    private void DetectInput()
    {
        Vector2 movement = movementActionReference.action.ReadValue<Vector2>();

        forwardInput = movement.y;
        rotationInput = movement.x;
    }
}