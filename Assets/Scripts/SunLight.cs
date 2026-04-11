using UnityEngine;

[RequireComponent(typeof(Light))]
public class SunLight : MonoBehaviour
{
    [Header("Point Light (close visual effect)")]
    [SerializeField] private float pointIntensity = 5f;
    [SerializeField] private float pointRange = 500f;
    [SerializeField] private Color sunColor = new Color(1f, 0.95f, 0.8f);

    [Header("Directional Light (system lighting)")]
    [Tooltip("Assign the Directional Light of the stage")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private float directionalIntensity = 1.2f;

    [Header("Flicker")]
    [SerializeField] private bool flicker = true;
    [SerializeField] private float flickerSpeed = 2f;
    [SerializeField] private float flickerAmount = 0.05f;

    private Light pointLight;
    private Transform camTransform;

    private void Awake()
    {
        pointLight = GetComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.range = pointRange;
        pointLight.color = sunColor;
        pointLight.intensity = pointIntensity;
        pointLight.shadows = LightShadows.Soft;

        if (directionalLight != null)
        {
            directionalLight.type = LightType.Directional;
            directionalLight.color = sunColor;
            directionalLight.intensity = directionalIntensity;
            directionalLight.shadows = LightShadows.Soft;
        }
        else
        {
            Debug.LogWarning("[SunLight] Unassigned Directional Light - distant planets will not be illuminated.", this);
        }

        camTransform = Camera.main?.transform;
    }

    private void LateUpdate()
    {
        // The Directional Light points FROM the sun TO the camera
        //   all the planets seen by the camera are illuminated
        // from the right direction
        if (directionalLight != null && camTransform != null)
        {
            Vector3 dirToCamera = (camTransform.position - transform.position).normalized;
            directionalLight.transform.rotation = Quaternion.LookRotation(dirToCamera);
        }

        HandleFlicker();
    }

    private void HandleFlicker()
    {
        if (!flicker) return;
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        pointLight.intensity = pointIntensity + (noise - 0.5f) * flickerAmount * pointIntensity;
    }

    private void OnValidate()
    {
        if (pointLight == null) pointLight = GetComponent<Light>();
        if (pointLight == null) return;
        pointLight.range = pointRange;
        pointLight.color = sunColor;
        pointLight.intensity = pointIntensity;

        if (directionalLight != null)
        {
            directionalLight.color = sunColor;
            directionalLight.intensity = directionalIntensity;
        }
    }
}