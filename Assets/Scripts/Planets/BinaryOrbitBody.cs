using UnityEngine;

/// <summary>
/// Makes two bodies orbit their common barycenter
/// Attach on the Barycenter GameObject - The two planet bodies are children.
/// 
/// The barycenter itself uses a normal OrbitBody to go around the Sun.
/// BinaryOrbitBody handles the internal dance of the two siblings.
/// </summary>
public class BinaryOrbitBody : MonoBehaviour
{
    [Header("Bodies")]
    [SerializeField] private Transform bodyA;
    [SerializeField] private Transform bodyB;
    [Space(10)]

    [Header("Orbit")]
    [Tooltip("Distance between the two bodies.")]
    [SerializeField] private float separation = 10f; 
    [Tooltip("Orbital speed of the pair in degrees per second.")]
    [SerializeField] private float orbitSpeed = 30f;
    [Tooltip("Orbit inclination of the binary pair relative to the ecliptic.")]
    [SerializeField] private float inclination = 0f;
    [Space(10)]

    [Header("Mass Ratio")]
    [Tooltip("Mass of body A / (mass A + mass B). " +
        "0.5 = equal masses (symmetric). " +
        "0.8 = A is heavier and closer to barycenter.")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float massRatioA = 0.5f;

    private float angle = 0f;
    private System.Random binaryRNG;

    private void Awake()
    {
        // Fall back to a default RNG if none is provided
        if (binaryRNG == null)
        {
            binaryRNG = SeedManager.GetRNG("binaryOrbitBody");
        }
    }
    private void Start()
    {
        angle = SeedManager.Range(0f, Mathf.PI * 2f, binaryRNG);
    }

    private void Update()
    {
        angle += orbitSpeed * Mathf.Deg2Rad * Time.deltaTime;

        Quaternion tilt = Quaternion.Euler(inclination, 0f, 0f);

        // Body A is closer to barycenter if massRatioA > 0.5
        // distA / distB = massB / massA -> massRatio drive the split
        float distA = separation * (1f - massRatioA); // Lighter body -> farther
        float distB = separation * massRatioA; // Heavier body -> closer

        Vector3 dirA = tilt * new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        Vector3 dirB = tilt * new Vector3(Mathf.Cos(angle + Mathf.PI), 0f, Mathf.Sin(angle + Mathf.PI));
    
        if(bodyA != null) bodyA.position = transform.position + dirA * distA;
        if(bodyB != null) bodyB.position = transform.position + dirB * distB;
    }

    public void Setup(Transform a, Transform b, float separation, float orbitSpeed, float massRatioA = 0.5f, float inclination = 0f)
    {
        this.bodyA = a;
        this.bodyB = b;
        this.separation = separation;
        this.orbitSpeed = orbitSpeed;
        this.massRatioA = massRatioA;
        this.inclination = inclination;
    }

    public float GetSeparation()
    {
        return this.separation;
    }
}
