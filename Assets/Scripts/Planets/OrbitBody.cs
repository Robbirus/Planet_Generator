using UnityEngine;

public class OrbitBody : MonoBehaviour
{
    [SerializeField] private Transform center;

    [SerializeField] private float orbitRadius;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float orbitInclination;

    [SerializeField] private Color orbitColor = Color.yellow;

    private float angle;
    private System.Random orbitRNG;

    private void Awake()
    {
        // Fall back to a default RNG if none is provided
        if (orbitRNG == null)
        {
            orbitRNG = SeedManager.GetRNG("orbitBody");
        }
    }

    private void Start()
    {
        angle = SeedManager.Range(0f, Mathf.PI * 2f, orbitRNG);
    }

    private void Update()
    {
        if (center == null) return;

        angle += orbitSpeed * Time.deltaTime;

        Vector3 pos = new Vector3(
            Mathf.Cos(angle) * orbitRadius,
            0,
            Mathf.Sin(angle) * orbitRadius
        );

        // Inclinaison
        pos = Quaternion.Euler(orbitInclination, 0, 0) * pos;

        transform.position = center.position + pos;
    }

    private void OnDrawGizmos()
    {
        if (center == null) return;

        Gizmos.color = Color.white;

        int segments = 100;

        CelestialBody centerBody = center.GetComponent<CelestialBody>();

        Vector3 previousPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * orbitRadius,
                0,
                Mathf.Sin(angle) * orbitRadius
            );

            // Appliquer l'inclinaison
            point = Quaternion.Euler(orbitInclination, 0, 0) * point;

            point += center.position;

            if(centerBody != null)
            {
                Gizmos.color = orbitColor;
            }

            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, point);
            }

            previousPoint = point;
        }
    }

    public float GetOrbitRadius()
    {
        return orbitRadius;
    }

    public float GetOrbitSpeed()
    {
        return orbitSpeed;
    }

    public float GetOrbitInclination()
    {
        return orbitInclination;
    }

    public Transform GetCenter()
    {
        return center;
    }

    public void SetOrbitRadius(float r)
    {
        orbitRadius = r;
    }

    public void SetOrbitSpeed(float s)
    {
        orbitSpeed = s;
    }

    public void SetOrbitInclination(float i)
    {
        orbitInclination = i;
    }

   public void SetCenter(Transform c)
   {
            center = c;
   }

    public void SetSeed(System.Random newSeed)
    {
        orbitRNG = newSeed;
    }

    public void SetOrbitColor(Color c)
    {
        orbitColor = c;
    }
}