using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpaceshipController))]
public class PlanetLockSystem : MonoBehaviour
{
    public enum LockState { None, Selectable, Locked }
    [SerializeField] private LockState state = LockState.None;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 80f;
    [SerializeField] private float detectionConeAngle = 30f;
    [SerializeField] private LayerMask planetLayer;

    [Header("Orbital tracking")]
    [SerializeField] private float surfaceMargin = 50f;
    [SerializeField] private float followSmoothSpeed = 6f;
    [SerializeField] private float lookSmoothSpeed = 4f;

    [Header("Input")]
    [SerializeField] private InputActionReference lockActionReference;
    [SerializeField] private InputActionReference movementActionReference;

    [Header("References (debug)")]
    [SerializeField] private SpaceshipController shipController;
    [SerializeField] private ShipCamera shipCamera;
    [SerializeField] private Camera mainCam;

    // Normalize direction from planet to ship (The local up gravity)
    private Vector3 orbitDir;

    private Transform selectablePlanet;
    private Transform lockedPlanet;
    private float lockedPlanetRadius;
    private float orbitDistance;
    private float currentAltitudeOffset = 0f;

    private readonly Collider[] overlapBuffer = new Collider[16];

    // Validity checks
    private bool hasShipController;
    private bool hasShipCamera;

    private bool hasMovementAction;
    private bool hasLockAction;

    private void Awake()
    {
        hasShipCamera = Validate(shipCamera != null, nameof(shipCamera));
        hasShipController = Validate(shipController != null, nameof(shipController));
        hasLockAction = Validate(lockActionReference != null && lockActionReference.action != null, nameof(lockActionReference));
        hasMovementAction = Validate(movementActionReference != null && movementActionReference.action != null, nameof(movementActionReference));

        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        if (!hasLockAction) return;
        if(!movementActionReference) return;

        lockActionReference.action.Enable();
        movementActionReference.action.Enable();
        lockActionReference.action.performed += OnLockPressed;
    }

    private void OnDisable()
    {
        if (!hasLockAction) return;
        if (!movementActionReference) return;

        lockActionReference.action.performed -= OnLockPressed;
        lockActionReference.action.Disable();
        movementActionReference.action.Disable();
    }

    private void Update()
    {
        if (mainCam == null) mainCam = Camera.main;

        switch (state)
        {
            case LockState.None:
            case LockState.Selectable:
                DetectBestPlanet();
                break;

            case LockState.Locked:
                if (lockedPlanet == null) 
                { 
                    Unlock(); 
                    break; 
                }
                ApplyOrbitalConstraint();
                CheckAutoUnlock();
                break;
        }
    }

    /// <summary>
    /// Detect the best candidate planet to lock on, based on distance and angle from the center of the screen.
    /// </summary>
    private void DetectBestPlanet()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, overlapBuffer, planetLayer);
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Transform bestCandidate = null;
        float bestScreenDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Transform candidate = overlapBuffer[i].transform;
            Vector3 toCandidate = (candidate.position - mainCam.transform.position).normalized;

            if (Vector3.Dot(mainCam.transform.forward, toCandidate) < Mathf.Cos(detectionConeAngle * Mathf.Deg2Rad * 0.5f))
                continue;

            Vector3 screenPos = mainCam.WorldToScreenPoint(candidate.position);
            if (screenPos.z < 0f) continue;

            float screenDist = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), screenCenter);
            if (screenDist < bestScreenDist)
            {
                bestScreenDist = screenDist;
                bestCandidate = candidate;
            }
        }

        selectablePlanet = bestCandidate;
        state = bestCandidate != null ? LockState.Selectable : LockState.None;
    }

    /// <summary>
    /// On lock input, either lock on the selectable planet or unlock if already locked.
    /// </summary>
    /// <param name="ctx"></param>
    private void OnLockPressed(InputAction.CallbackContext ctx)
    {
        if (state == LockState.Locked)
            Unlock();
        else if (state == LockState.Selectable && selectablePlanet != null)
            Lock(selectablePlanet);
    }

    /// <summary>
    /// Lock the ship to the given planet, initializing the orbital parameters and switching camera mode.
    /// </summary>
    /// <param name="planet"></param>
    private void Lock(Transform planet)
    {
        if(!hasShipController || !hasShipCamera) return;

        lockedPlanet = planet;

        float radius = 1f;
        Collider col = planet.GetComponent<Collider>();
        if (col != null)
        {
            Vector3 ext = col.bounds.extents;
            radius = Mathf.Max(ext.x, ext.y, ext.z);
        }

        lockedPlanetRadius = radius;
        orbitDistance = radius + surfaceMargin;
        currentAltitudeOffset = 0f;

        // Initial direction from the current position
        orbitDir = (transform.position - planet.position).normalized;

        state = LockState.Locked;
        shipController.SetLockedMode(true);
        shipCamera.SetCameraOrbitalMode();
    }

    /// <summary>
    /// Unlock the ship from the planet, clearing parameters and switching camera mode back to free.
    /// </summary>
    private void Unlock()
    {
        lockedPlanet = null;
        state = LockState.None;
        shipController.SetLockedMode(false);
        shipCamera.SetCameraFreeMode();
    }

    /// <summary>
    /// Checks the distance to the locked planet and unlocks if beyond the specified detection radius.
    /// </summary>
    private void CheckAutoUnlock()
    {
        if (Vector3.Distance(transform.position, lockedPlanet.position) > detectionRadius * 2.5f)
            Unlock();
    }

    /// <summary>
    /// Applies the orbital constraint by snapping the ship's position to the defined orbit distance from the planet and 
    /// smoothly aligning its rotation to face along the tangent of the orbit while keeping "up" towards the planet. 
    /// This creates a stable orbital movement around the locked planet.
    /// </summary>
    private void ApplyOrbitalConstraint()
    {
        // 1 Recompute orbitDir from real position of the ship
        // SpaceshipController was able to freely move
        Vector3 toShip = transform.position - lockedPlanet.position;
        orbitDir = toShip.normalized;

        // 2 Snap the position exactly on the orbital sphere
        transform.position = lockedPlanet.position + orbitDir * orbitDistance;

        // 3 Align the ship : up = orbitDir (gravity towards the planet)
        // We conserve the current forward projection on the tangent plane
        Vector3 currentForward = transform.forward;
        Vector3 tangentForward = Vector3.ProjectOnPlane(currentForward, orbitDir).normalized;

        if(tangentForward.sqrMagnitude < 0.001f)
        {
            tangentForward = Vector3.ProjectOnPlane(Vector3.forward, orbitDir).normalized;
        }

        Quaternion targetRot = Quaternion.LookRotation(tangentForward, orbitDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lookSmoothSpeed * Time.deltaTime);
    }

    public void AdjustAltitude(float delta, Vector2 heightRange)
    {
        currentAltitudeOffset = Mathf.Clamp(
            currentAltitudeOffset + delta,
            heightRange.x,
            heightRange.y
        );

        orbitDistance = lockedPlanetRadius + surfaceMargin + currentAltitudeOffset;
    }

    // ── Gizmos (debug) ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (Application.isPlaying && lockedPlanet != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(lockedPlanet.position, lockedPlanetRadius);
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawWireSphere(lockedPlanet.position, orbitDistance);
        }
    }

    public Transform GetSelectablePlanet()
    {
        return selectablePlanet;
    }

    public LockState GetLockState()
    {
        return state;
    }

    public Vector3 GetOrbitDir()
    {
        return orbitDir;
    }

    public LockState GetState()
    {
        return state;
    }


    // Utilities
    private bool Validate(bool isAsigned, string fieldName)
    {
        if (isAsigned) return true;

        Debug.LogWarning($"[UIShip] '{fieldName}' is not assigned in the inspector.", this);
        return false;

    }
}