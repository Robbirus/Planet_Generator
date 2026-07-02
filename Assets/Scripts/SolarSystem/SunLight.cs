using UnityEngine;

[RequireComponent(typeof(Light))]
public class SunLight : MonoBehaviour
{
    [Header("Point Light (close visual effect)")]
    [SerializeField] private float pointIntensity = 5f;
    [Tooltip("Point Light range in Unity units.\n" +
             "If SolarSystemGenerator.sunLight is assigned, this value is\n" +
             "automatically overwritten after generation (max orbit × 1.2).\n" +
             "Otherwise, manually set the distance to the farthest orbit + 20%.")]
    [SerializeField] private float pointRange = 30000f;
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
        if (directionalLight != null && camTransform != null)
        {
            Vector3 dirToCamera = (camTransform.position - transform.position).normalized;
            directionalLight.transform.rotation = Quaternion.LookRotation(dirToCamera);
        }

        HandleFlicker();
    }

    /// <summary>
    /// Called by SolarSystemGenerator after all planets are placed.
    /// Overrides pointRange with the auto-computed value so the Point Light
    /// always reaches the outermost body.
    /// </summary>
    public void SetPointRange(float range)
    {
        pointRange = range;

        // pointLight might not exist yet if called before Awake (shouldn't happen
        // since SolarSystemGenerator runs in Start, after Awake, but guard anyway)
        if (pointLight == null) pointLight = GetComponent<Light>();
        if (pointLight != null) pointLight.range = pointRange;
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