using System;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Top down camera used for stellar map.
/// </summary>
[RequireComponent (typeof(Camera))]
public class StellarCamera : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera cam;
    [Space(10)]

    [Header("Height")]
    [Tooltip("Y position of the map camera.")]
    [SerializeField] private float height = 15000f;
    [Space(10)]

    [Header("Pan")]
    [Tooltip("Pan speed in world unit per pixel moved.")]
    [SerializeField] private float panSensitivity = 0.5f;
    [Tooltip("Smoothing pan.")]
    [SerializeField] private float panSmoothing = 8f;
    [Space(10)]

    [Header("Zoom")]
    [SerializeField] private float minOrthoSize = 30000f;
    [SerializeField] private float maxOrthoSize = 60000f;
    [SerializeField] private float zoomSpeed    = 500f;
    [SerializeField] private float zoomSmoothing = 8f;
    [Space(10)]

    [Header("Start position")]
    [Tooltip("World Space XZ target to focus on when the map opens.")]
    [SerializeField] private Vector3 targetPosition;

    private Vector3 currentVelocity;
    private float targetOrthoSize;
    private bool isPanning;
    private Vector2 lastMousePost;


    private void Awake()
    {
        if(cam == null)
        {
            cam = GetComponent<Camera>();
        }

        cam.orthographic = true;
        targetOrthoSize = cam.orthographicSize;
        targetPosition = transform.position;
    }

    private void Update()
    {   
        HandlePan();
        HandleZoom();
        ApplyMovement();
    }

    private void HandlePan()
    {
        // Start Pan on left mouse button press
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isPanning = true;
            lastMousePost = Mouse.current.position.ReadValue();
            return;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isPanning = false;
            return;
        }

        if (!isPanning) return;

        Vector2 currentMousePos = Mouse.current.position.ReadValue();
        Vector2 delta = currentMousePos - lastMousePost;
        lastMousePost = currentMousePos;

        // Convert pixel delta to world XZ (Camera looks down so X = X and Y = Z)
        float worldScale = cam.orthographicSize * 2f / Screen.height;
        Vector3 worldDelta = new Vector3(-delta.x, 0f, -delta.y) * worldScale * panSensitivity;

        targetPosition += worldDelta;
    }

    private void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Approximately(scroll, 0f)) return;

        targetOrthoSize = Mathf.Clamp(
            targetOrthoSize - scroll * zoomSpeed,
            minOrthoSize,
            maxOrthoSize);
    }

    private void ApplyMovement()
    {
        // Smooth camera position
        Vector3 target = new Vector3(targetPosition.x, height, targetPosition.z);
        transform.position = Vector3.SmoothDamp(
            transform.position, target,
            ref currentVelocity,
            1f / panSmoothing);

        // Smooth range
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetOrthoSize,
            zoomSmoothing * Time.unscaledDeltaTime);
    }


    /// <summary>Center the view on the provided world position</summary>
    public void FocusOn(Vector3 worldPosition, float initialOrthoSize = 300f)
    {
        targetPosition = worldPosition;
        transform.position = new Vector3(worldPosition.x, height, worldPosition.z);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        targetOrthoSize = Mathf.Clamp(initialOrthoSize, minOrthoSize, maxOrthoSize);
        cam.orthographicSize = targetOrthoSize;
    }
}
