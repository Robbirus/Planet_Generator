using UnityEngine;

public class OrbitBody : MonoBehaviour
{
    [SerializeField] private Transform center;

    [SerializeField] private float orbitRadius;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float orbitInclination;

    [SerializeField] private Color orbitColor = Color.yellow;

    [SerializeField] private int seed = 42;

    private float angle;

    private void Start()
    {
        UnityEngine.Random.InitState(seed);
        angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
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

            if(center.GetComponent<CelestialBody>() != null)
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

    public void SetSeed(int s)
    {
        seed = s;
        UnityEngine.Random.InitState(seed);
    }

    public void SetOrbitColor(Color c)
    {
        orbitColor = c;
    }
}