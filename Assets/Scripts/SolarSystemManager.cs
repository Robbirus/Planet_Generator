using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    [SerializeField] private Rigidbody sun;
    [SerializeField] private float G = 25f;

    private GameObject[] planets;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        planets = GameObject.FindGameObjectsWithTag("Celestials");
        SetInitialVelocities();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (planets == null || sun == null) return;

        foreach (GameObject planet in planets)
        {
            Rigidbody rb = planet.GetComponent<Rigidbody>();

            Vector3 direction = sun.position - rb.position;
            float distance = direction.magnitude;
            float forceMagnitude = 0;

            if(distance > 0.1f)
            {
                forceMagnitude = G * (sun.mass * rb.mass) / (distance * distance);
            }


            rb.AddForce(direction.normalized * forceMagnitude);
        }
    }

    private void SetInitialVelocities()
    {
        foreach (GameObject planet in planets)
        {
            Rigidbody rb = planet.GetComponent<Rigidbody>();

            Vector3 direction = planet.transform.position - sun.position;
            float distance = direction.magnitude;
            float orbitalSpeed = 0f;

            if (distance > 0.1f)
            {
                orbitalSpeed = Mathf.Sqrt(G * sun.mass / distance);
            }

            Vector3 tangent = Vector3.Cross(direction.normalized, Vector3.up);

            Debug.Log($"Setting initial velocity for {planet.name}: {tangent * orbitalSpeed}");

            rb.linearVelocity = tangent * orbitalSpeed;
        }
    }
}