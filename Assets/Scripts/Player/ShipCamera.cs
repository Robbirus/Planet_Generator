using UnityEngine;

public class ShipCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Vector3 freeModeCameraPosition = new Vector3(0f, 80f, -300f);
    [SerializeField] private Vector3 orbitalModeCameraPosition = new Vector3(0f, 1000f, 0f);

    public void SetCameraFreeMode()
    {
        transform.localPosition = freeModeCameraPosition;
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void SetCameraOrbitalMode()
    {
        transform.localPosition = orbitalModeCameraPosition;
        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
