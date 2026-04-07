using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [Header("Physical properties")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float density = 1f;

    [Header("visual")]
    [SerializeField] private Renderer planetRenderer;
    public const float VISUAL_SCALE = 100f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    /// <summary>
    /// Calculates the radius of a sphere given its mass and density.
    /// Used for safe spacing
    /// </summary>
    /// <param name="mass">The mass of the sphere. Must be a non-negative value.</param>
    /// <param name="density">The density of the sphere. Must be a positive value.</param>
    /// <param name="scale"></param>
    /// <returns>The radius of the sphere, calculated based on the provided mass and density.</returns>
    public static float ComputeRadius(float mass, float density, float scale)
    {
        return Mathf.Pow((3f * mass) / (4f * Mathf.PI * density), 1f / 3f) * VISUAL_SCALE * scale;
    }

    public float GetRadius(float scale)
    {
        return ComputeRadius(mass, density, scale);
    }

    public void SetMass(float m)
    {
        this.mass = m;
    }

    public void SetDensity(float d)
    {
        this.density = d;
    }

    public float GetMass()
    {
        return mass;
    }

    public void SetRotationSpeed(float rotationSpeed)
    {
        this.rotationSpeed = rotationSpeed;
    }

    public void ApplyScale(float scale)
    {
        float diameter = GetRadius(scale) * 2f;
        transform.localScale = Vector3.one * diameter;
    }

    public void SetName(string name)
    {
        gameObject.name = name;
    }

    public void ApplyColor(float maxDensity)
    {
        if (planetRenderer == null)
        {
            planetRenderer = GetComponent<Renderer>();
        }

        if (planetRenderer == null) return;

        // Base color in HSV
        Color baseColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);

        // Darken based on density
        float darknessFactor = Mathf.Clamp01(1f - (density / maxDensity));

        Color finalColor = baseColor * darknessFactor;

        // Apply to material
        planetRenderer.material = new Material(planetRenderer.sharedMaterial);
        planetRenderer.material.color = finalColor;
    }

    private void Update()
    {
        // Rotation on itself
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}