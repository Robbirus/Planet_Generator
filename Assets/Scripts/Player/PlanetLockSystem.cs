using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpaceshipController))]
public class PlanetLockSystem : MonoBehaviour
{
    public enum LockState { None, Selectable, Locked }
    [SerializeField] private LockState State = LockState.None;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 80f;
    [SerializeField] private float detectionConeAngle = 30f;
    [SerializeField] private LayerMask planetLayer;

    [Header("Orbital tracking")]
    [SerializeField] private float surfaceMargin = 5f;
    [SerializeField] private float followSmoothSpeed = 6f;
    [SerializeField] private float lookSmoothSpeed = 4f;

    [Header("Orbital control")]
    [SerializeField] private float strafeOrbitSpeed = 60f;

    [Header("Input")]
    [SerializeField] private InputActionReference lockActionReference;
    [SerializeField] private InputActionReference movementActionReference;

    private SpaceshipController shipController;
    private Camera mainCam;

    private Transform selectablePlanet;
    private Transform lockedPlanet;
    private float lockedPlanetRadius;
    private float orbitAngle;
    private float orbitDistance;

    private readonly Collider[] overlapBuffer = new Collider[16];

    private void Awake()
    {
        shipController = GetComponent<SpaceshipController>();
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        lockActionReference.action.Enable();
        movementActionReference.action.Enable();
        lockActionReference.action.performed += OnLockPressed;
    }

    private void OnDisable()
    {
        lockActionReference.action.performed -= OnLockPressed;
        lockActionReference.action.Disable();
        movementActionReference.action.Disable();
    }

    private void Update()
    {
        if (mainCam == null) mainCam = Camera.main;

        switch (State)
        {
            case LockState.None:
            case LockState.Selectable:
                DetectBestPlanet();
                break;

            case LockState.Locked:
                if (lockedPlanet == null) { Unlock(); break; }
                HandleOrbitalInput();
                ApplyOrbitalFollow();
                CheckAutoUnlock();
                break;
        }
    }

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
        State = bestCandidate != null ? LockState.Selectable : LockState.None;
    }

    private void OnLockPressed(InputAction.CallbackContext ctx)
    {
        if (State == LockState.Locked)
            Unlock();
        else if (State == LockState.Selectable && selectablePlanet != null)
            Lock(selectablePlanet);
    }

    private void Lock(Transform planet)
    {
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

        // Initial angle from the current position (no jump)
        Vector3 flatOffset = transform.position - planet.position;
        flatOffset.y = 0f;
        orbitAngle = Mathf.Atan2(flatOffset.x, flatOffset.z) * Mathf.Rad2Deg;

        State = LockState.Locked;
        shipController.SetLockedMode(true);
    }

    private void Unlock()
    {
        lockedPlanet = null;
        State = LockState.None;
        shipController.SetLockedMode(false);
    }

    private void CheckAutoUnlock()
    {
        if (Vector3.Distance(transform.position, lockedPlanet.position) > detectionRadius * 2.5f)
            Unlock();
    }
    
    private void HandleOrbitalInput()
    {
        float strafe = movementActionReference.action.ReadValue<Vector3>().x;
        orbitAngle += strafe * strafeOrbitSpeed * Time.deltaTime;
    }

    private void ApplyOrbitalFollow()
    {
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 radialDir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));

        // Position: horizontal circle at planet Y level
        Vector3 targetPos = lockedPlanet.position + radialDir * orbitDistance;
        targetPos.y = lockedPlanet.position.y;

        transform.position = Vector3.Lerp(transform.position, targetPos, followSmoothSpeed * Time.deltaTime);

        // Tangent = direction of movement on the circle
        Vector3 tangent = new Vector3(Mathf.Cos(rad), 0f, -Mathf.Sin(rad));

        // +90° on local Z: the ship lies down on its side (wing towards the planet)
        Quaternion baseRot = Quaternion.LookRotation(tangent, Vector3.up);
        Quaternion targetRot = baseRot * Quaternion.Euler(0f, 0f, 90f);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lookSmoothSpeed * Time.deltaTime);
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

    public Transform GetLockedPlanet()
    {
        return lockedPlanet;
    }

    public LockState GetLockState()
    {
        return State;
    }
}