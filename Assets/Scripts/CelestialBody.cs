using System;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [Header("Celestial Body Properties")]

    [Tooltip("Density")]
    [SerializeField] private float density = 1f;

    [Tooltip("Mass in game units")]
    [SerializeField] private float massUnity = 1f;

    [HideInInspector]
    private Rigidbody rb;

    // Safety clamps
    private const float MIN_DENSITY = 0.1f;
    private const float MIN_MASS = 0.1f;

    private double cachedRadius = -1;

    public double GetRadius()
    {
            if (cachedRadius < 0)
            {
                double m = GetMass();
                double rho = GetDensity();

                double r = Math.Pow((3 * m) / (4 * Math.PI * rho), 1.0 / 3.0);

                if (double.IsNaN(r) || double.IsInfinity(r))
                    r = 0.1;

                cachedRadius = r;
            }
            return cachedRadius;
        
    }

    public double GetMass()
    {
        return Math.Max(massUnity, MIN_MASS);
    }

    public double GetDensity()
    {
        return Math.Max(density, MIN_DENSITY);
    }

    public Rigidbody GetRigidbody()
    {
        return rb;
    }

    public void SetMass(float mass)
    {
        massUnity = mass;
        cachedRadius = -1; // Invalidate radius cache
    }

    public void SetDensity(float density)
    {
        this.density = density;
        cachedRadius = -1; // Invalidate radius cache
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.mass = 1f; // we ignore Unity physics mass
        }
    }

    public void ApplyScale()
    {
        float diameter = (float)(GetRadius() * 2f);

        if (float.IsNaN(diameter) || float.IsInfinity(diameter) || diameter < 0.01f)
        {
            diameter = 0.01f;
        }
        
        transform.localScale = Vector3.one * diameter;
    }
}