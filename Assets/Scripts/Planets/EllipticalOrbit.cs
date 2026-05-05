using System;
using UnityEngine;

public class EllipticalOrbit : MonoBehaviour
{
    [Header("Focus (Sun)")]
    [SerializeField] private Transform focus;
    [Space(10)]

    [Header("Orbital Parameters")]
    [Tooltip("Semi-major axis: average of perihelion + aphelion distances.")]
    [SerializeField] private float semiMajorAxis = 150;

    [Tooltip("Eccentricity: 0 = circle, 0-1 = ellipse. Comets: 0.7 - 0.9")]
    [Range(0f, 1f)]
    [SerializeField] private float eccentricity = 0.85f;

    [Tooltip("Orbital period in seconds (how long on full lap takes).")]
    [SerializeField] private float period = 60f;

    [Tooltip("Starting angle in radians (0 = perihelion).")]
    [SerializeField] private float startAngle = 0f;

    [Tooltip("Inclination of the orbital plane in degrees.")]
    [SerializeField] private float inclination = 15f;

    [Tooltip("Argument of perihelion - rotates the ellipse in its plane.")]
    [SerializeField] private float argumentOfPerihelion = 0f;

    private float meanAnomaly = 0f; // increases linearly with time
    private float meanMotion = 0f; // radians per seconds

    private void Start()
    {
        meanMotion  = (2f * Mathf.PI) / Mathf.Max(period, 0.01f);
        meanAnomaly = startAngle; 
    }

    private void Update()
    {
        if (focus == null) return;

        meanAnomaly += meanMotion * Time.deltaTime;

        // Solve Kepler's equation M = E - e * sin(E) (Newton-Raphson, 5 iterations)
        float e = SolveKeplerEquation(meanAnomaly, eccentricity);

        // True anomaly
        float cosE = Mathf.Cos(e);
        float sinE = Mathf.Sin(e);
        float trueAnomaly = Mathf.Atan2(
            Mathf.Sqrt(1f - eccentricity * eccentricity) * sinE,
            cosE - eccentricity);

        // Radius at this true anomaly
        float r = semiMajorAxis * (1f - eccentricity * cosE);

        // Position in orbital plane (before inclination / argument of perihelion)
        float cosW = Mathf.Cos(argumentOfPerihelion * Mathf.Deg2Rad);
        float sinW = Mathf.Sin(argumentOfPerihelion * Mathf.Deg2Rad);
        float cosV = Mathf.Cos(trueAnomaly);
        float sinV = Mathf.Sin(trueAnomaly);

        // Rotate by argument of perihelion in XZ plane
        float x = r * (cosW * cosV - sinW * sinV);
        float z = r * (sinW * cosV + cosW * sinV);

        Vector3 localPos = new Vector3(x, 0f, z);

        // Apply orbital inclination
        localPos = Quaternion.Euler(inclination, 0f, 0f) * localPos;

        transform.position = focus.position + localPos;
    }

    private float SolveKeplerEquation(float meanAnomaly, float eccentricity, int iterations = 5)
    {
        float e = meanAnomaly; // initial guess
        for(int i = 0; i < iterations; i++)
        {
            e = e - (e - eccentricity * Mathf.Sin(e) - meanAnomaly) / (1f - eccentricity * Mathf.Cos(e));
        }

        return e;
    }

    public void Setup(Transform sun, float semiMajorAxis, float eccentricity, float period, float startAngle, float inclination, float argumentOfPerihelion = 0f)
    {
        this.focus                  = sun;
        this.semiMajorAxis          = semiMajorAxis;
        this.eccentricity           = Mathf.Clamp(eccentricity, 0f, 0.99f);
        this.period                 = period;
        this.startAngle             = startAngle;
        this.inclination            = inclination;
        this.argumentOfPerihelion   = argumentOfPerihelion;

        meanMotion  = (2f * Mathf.PI) / Mathf.Max(period, 0.01f);
        meanAnomaly = startAngle;
    }

    /// <summary>Returns the current distance from the focus</summary>
    public float GetCurrentRadius()
    {
        if (focus == null) return 0f;
        return Vector3.Distance(transform.position, focus.position);
    }

    public float GetPerihelionDistance()
    {
        return this.semiMajorAxis * (1f - this.eccentricity);
    }

    public float GetAphelionDistance()
    {
        return this.semiMajorAxis * (1f + this.eccentricity);
    }
}
