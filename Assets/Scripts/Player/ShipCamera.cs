using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine;

public class ShipCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Vector3 freeModeCameraPosition = new Vector3(0f, 80f, -300f);
    [SerializeField] private Vector3 orbitalModeCameraPosition = new Vector3(0f, 1000f, 0f);

    [Header("Transition")]
    [Tooltip("Speed transition between both modes")]
    [SerializeField] private float transitionSpeed = 3f;

    [Header("Speed Effect")]
    [SerializeField] private SpaceshipController ship;

    [Tooltip("Additional camera recoil on Z at max speed")]
    [SerializeField] private float maxSpeedPullBack = 80f;

    [Tooltip("Basic FOV")]
    [SerializeField] private float baseFOV = 60f;

    [Tooltip("FOV added at max speed")]
    [SerializeField] private float maxFOVBoost = 20f;

    [Tooltip("additional FOV during the boost")]
    [SerializeField] private float boostFOVBoost = 10f;

    [Tooltip("Interpolation speed of speed effects")]
    [SerializeField] private float speedEffectSmoothing = 4f;

    private Vector3 targetLocalPosition;
    private Quaternion targetLocalRotation;
    private bool transitioning = false;

    private Camera cam;
    private float currentFOV;
    private float currentPullback;

    private Volume      postVolume;
    private MotionBlur  motionBlur;

    private void Awake()
    {
        // Initialize the target on free mode on default
        targetLocalPosition = freeModeCameraPosition;
        targetLocalRotation = Quaternion.identity;

        cam = GetComponent<Camera>();
        currentFOV = baseFOV;

        if (cam != null )
        {
            cam.fieldOfView = baseFOV;
        }

        // Find the global volume from the scene
        postVolume = FindFirstObjectByType<Volume>();
        postVolume?.profile.TryGet(out motionBlur);

    }

    private void LateUpdate()
    {
        HandleModeTransition();
        HandleSpeedEffect();
    }

    private void HandleSpeedEffect()
    {
        if (ship == null || cam == null || transitioning) return;

        float speedRatio = ship.GetForwardSpeedRatio();
        bool boosting = ship.IsBoosting();

        // FOV : base + speed + boost
        float targetFOV = baseFOV
            + speedRatio * maxFOVBoost
            + (boosting ? boostFOVBoost : 0F);

        currentFOV = Mathf.Lerp(currentFOV, targetFOV, speedEffectSmoothing * Time.deltaTime);
        cam.fieldOfView = currentFOV;

        // Camera recoil : Further back at high speed
        float targetPullBack = speedRatio * maxSpeedPullBack
            + (boosting ? maxSpeedPullBack * 0.3f : 0F);

        currentPullback = Mathf.Lerp(currentPullback, targetPullBack, speedEffectSmoothing * Time.deltaTime);

        // We apply the pullback on local Z
        // in addition to the current mode position, without breaking the transition
        Vector3 basePos = transitioning ? transform.localPosition : targetLocalPosition;
        transform.localPosition = basePos + new Vector3(0f, 0f, -currentPullback);

        // Motion blur : 0 in calm, 0.35 at full speed, peak during the boost
        if(motionBlur != null)
        {
            float targetBlur = speedRatio * 0.35f + (boosting ? 0.15f : 0f);
            motionBlur.intensity.value = Mathf.Lerp(
                motionBlur.intensity.value,
                targetBlur,
                speedEffectSmoothing * Time.deltaTime
            );
        }
    }

    private void HandleModeTransition()
    {
        if (!transitioning) return;

        // Lerp position & Slerp rotation
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetLocalPosition,
            transitionSpeed * Time.deltaTime
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetLocalRotation,
            transitionSpeed * Time.deltaTime
        );

        // Stop the transition when close enough
        bool posClose = Vector3.Distance(transform.localPosition, targetLocalPosition) < 0.5f;
        bool rotClose = Quaternion.Angle(transform.localRotation, targetLocalRotation) < 0.5f;

        if (posClose && rotClose)
        {
            transform.localPosition = targetLocalPosition;
            transform.localRotation = targetLocalRotation;
            transitioning = false;
        }
    }

    public void SetCameraFreeMode()
    {
        targetLocalPosition = freeModeCameraPosition;
        targetLocalRotation = Quaternion.identity;
        transitioning       = true;
    }

    public void SetCameraOrbitalMode()
    {
        targetLocalPosition = orbitalModeCameraPosition;
        targetLocalRotation = Quaternion.Euler(90f, 0f, 0f);
        transitioning       = true;
    }
}
