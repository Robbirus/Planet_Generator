using UnityEngine;

/// <summary>
/// Pilot one or more TrailRenderer placed on the shipÅfs engines.
/// The length and width of the drag adapt to forward speed.
/// To be placed on the same GameObject as SpaceshipController.
/// </summary>
[RequireComponent(typeof(SpaceshipController))]
public class EngineTrail : MonoBehaviour
{
    [Header("Engine references")]
    [Tooltip("One TrailRenderer per motor nozzle (children of the ship)")]
    [SerializeField] private TrailRenderer[] trails;

    [Header("Length of trail")]
    [Tooltip("Lifetime of trail points at rest (speed = 0)")]
    [SerializeField] private float minTime = 0.05f;
    [Tooltip("Lifetime of trail points at maximum speed")]
    [SerializeField] private float maxTime = 0.6f;

    [Header("Trailing width")]
    [Tooltip("Width of the trail at rest")]
    [SerializeField] private float minWidth = 0.05f;
    [Tooltip("Width of trail at maximum speed")]
    [SerializeField] private float maxWidth = 0.35f;

    [Header("Boost")]
    [Tooltip("Extra width during the boost (adds to maxWidth)")]
    [SerializeField] private float boostExtraWidth = 0.25f;
    [Tooltip("Additional lifespan during the boost")]
    [SerializeField] private float boostExtraTime = 0.4f;

    [Header("Smoothing")]
    [Tooltip("Interpolation speed of the trail parameters")]
    [SerializeField] private float smoothSpeed = 4f;

    private SpaceshipController ship;

    // Interpolated current values
    private float currentTime;
    private float currentWidth;


    private void Awake()
    {
        ship = GetComponent<SpaceshipController>();

        // Initializes the current values to a minimum
        currentTime = minTime;
        currentWidth = minWidth;
    }

    private void LateUpdate()
    {
        // t = 0 at rest, 1 at max forward speed
        float speedRatio = Mathf.Clamp01(ship.GetForwardSpeedRatio());
        bool boosting = ship.IsBoosting();

        float targetTime = Mathf.Lerp(minTime, maxTime, speedRatio)
                          + (boosting ? boostExtraTime : 0f);
        float targetWidth = Mathf.Lerp(minWidth, maxWidth, speedRatio)
                          + (boosting ? boostExtraWidth : 0f);

        currentTime = Mathf.Lerp(currentTime, targetTime, smoothSpeed * Time.deltaTime);
        currentWidth = Mathf.Lerp(currentWidth, targetWidth, smoothSpeed * Time.deltaTime);

        foreach (TrailRenderer trail in trails)
        {
            if (trail == null) continue;
            trail.time = currentTime;
            trail.startWidth = currentWidth;
            // endWidth remains at 0 (defined in the material/inspector) for the fade
        }
    }
}