using UnityEngine;

public class ShipCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Vector3 freeModeCameraPosition = new Vector3(0f, 80f, -300f);
    [SerializeField] private Vector3 orbitalModeCameraPosition = new Vector3(0f, 1000f, 0f);

    [Header("Transition")]
    [Tooltip("Speed transition between both modes")]
    [SerializeField] private float transitionSpeed = 3f;

    private Vector3 targetLocalPosition;
    private Quaternion targetLocalRotation;

    private bool transitioning = false;

    private void Awake()
    {
        // Initialize the target on free mode on default
        targetLocalPosition = freeModeCameraPosition;
        targetLocalRotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if(!transitioning) return;

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

        if(posClose && rotClose)
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
