using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StellarMapManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference openMapAction;
    [Space(10)]

    [Header("Ship References")]
    [SerializeField] private SpaceshipController controller;
    [SerializeField] private SpaceshipMovement movement;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Camera cam;
    [Space(10)]

    [Header("Map camera")]
    [SerializeField] private StellarCamera mapCamera;
    [Space(10)]

    [Header("Focus target")]
    [Tooltip("The sun transform, the map will be centered on it.")]
    [SerializeField] private Transform sun;
    [Tooltip("Initial orthographic size when the map open.")]
    [SerializeField] private float orthoSize = 300f;
    [Space(10)]

    [Header("UI")]
    [Tooltip("Map overlay panel (legends, labels...). Toggled with the map.")]
    [SerializeField] private GameObject mapPanel;
    [Tooltip("HUD that should be hidden while the map is open.")]
    [SerializeField] private GameObject hudContainer;
    [Space(10)]

    [Header("Labels Configuration")]
    [SerializeField] private TextMeshProUGUI labelPrefab;
    [SerializeField] private Transform labelContainer;
    [Space(10)]

    [Header("Debug")]
    [SerializeField] private bool isMapOpen = false;
    [SerializeField] private bool debug = true;

    private void Awake()
    {
        // Ensure map camera starts disabled 
        if (mapCamera != null)
        {
            mapCamera.gameObject.SetActive(false);
            mapCamera.enabled = true;
            Vector3 focusPoint = sun != null ? sun.position : Vector3.zero;
            mapCamera.FocusOn(focusPoint, orthoSize);
        }

        if (mapPanel != null)
        {
            mapPanel.gameObject.SetActive(false);
        }

        LazyLoading();
    }

    private void LazyLoading()
    {
        if(controller == null)
        {
            controller = GetComponent<SpaceshipController>();
        }

        if(movement == null)
        {
            movement = GetComponent<SpaceshipMovement>();
        }

        if(weaponManager == null)
        {
            weaponManager = GetComponent<WeaponManager>();
        }
    }


    private void Start()
    {
        StellarLabel[] allLabels = GameObject.FindObjectsByType<StellarLabel>(FindObjectsSortMode.None);
        foreach (StellarLabel label in allLabels)
        {
            label.Setup(labelPrefab, labelContainer, mapCamera.GetComponent<Camera>());
        }
    }

    private void OnEnable()
    {
        if (openMapAction == null) return;
        openMapAction.action.Enable();
        openMapAction.action.performed += OnMapPressed;
    }

    private void OnDisable()
    {
        if (openMapAction == null) return;
        openMapAction.action.Disable();
        openMapAction.action.performed -= OnMapPressed;
    }

    private void OnMapPressed(InputAction.CallbackContext ctx)
    {
        if (isMapOpen) CloseMap();
        else OpenMap();
    }

    private void OpenMap()
    {
        if (isMapOpen) return;
        isMapOpen = true;

        // Freeze the ship. No movement, no rotation
        if (movement != null) movement.SetFrozen(true);

        // Freeze weapons
        if (weaponManager != null) weaponManager.enabled = false;

        // Swap cameras
        if (cam != null) cam.enabled = false;

        if (mapCamera != null)
        {
            mapCamera.gameObject.SetActive(true);
            Vector3 focusPoint = sun != null ? sun.position : Vector3.zero;
            mapCamera.FocusOn(focusPoint, orthoSize);
        }

        // Toggle UI
        if(hudContainer != null) hudContainer.SetActive(false);
        if(mapPanel != null) mapPanel.SetActive(true);

        // Free the cursor for panning
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (debug)
            Debug.Log("[StellarMapManager] Map Opened", this);
    }

    private void CloseMap()
    {
        if (!isMapOpen) return;
        isMapOpen = false;

        // Unfreeze the ship
        if (movement != null) movement.SetFrozen(false);

        // Unfreeze weapon
        if (weaponManager != null) weaponManager.enabled = true;

        // Swap cameras back
        if (mapCamera != null) mapCamera.gameObject.SetActive(false);

        if (cam != null) cam.enabled = true;

        // Toggle UI
        if (mapPanel != null) mapPanel.SetActive(false);
        if (hudContainer != null) hudContainer.SetActive(true);

        // Re-lock cursor for flight
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        if(debug)
            Debug.Log("[StellarMap] Map closed.", this);
    }
}
