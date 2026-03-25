using System;
using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    [Tooltip("The Sun's rigidbody.")]
    [SerializeField] private Rigidbody sun;

    [SerializeField] private float G = 0.1f;

    private CelestialBody[] bodies;

    private void Start()
    {
        bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
        SetInitialVelocities();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        foreach (CelestialBody a in bodies)
        {
            Vector3 totalAcceleration = Vector3.zero;

            foreach (CelestialBody b in bodies)
            {
                if (a == b) continue;

                Vector3 dir = b.transform.position - a.transform.position;
                float dist = dir.magnitude + 0.1f;

                float mB = (float)b.GetMass();

                float accel = G * mB / (dist * dist);

                totalAcceleration += dir.normalized * accel;
            }

            a.GetRigidbody().linearVelocity += totalAcceleration * Time.fixedDeltaTime;
        }
    }

    /*
    private void ApplyGravity()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            for (int j = i + 1; j < bodies.Length; j++)
            {
                CelestialBody a = bodies[i];
                CelestialBody b = bodies[j];

                Rigidbody rbA = a.GetRigidbody();
                Rigidbody rbB = b.GetRigidbody();

                Vector3 dir = b.transform.position - a.transform.position;
                float dist = dir.magnitude + 0.1f;

                float mA = (float)a.GetMass();
                float mB = (float)b.GetMass();

                float force = G * (mA * mB) / (dist * dist);

                Vector3 forceVec = dir.normalized * force;

                // action / reaction
                rbA.AddForce(forceVec);
                rbB.AddForce(-forceVec);
            }
        }
    }
    */

    private void SetInitialVelocities()
    {
        CelestialBody sunBody = GetHeaviestBody();

        foreach(CelestialBody body in bodies)
        {
            if(body == sunBody) continue;

            Vector3 dir = body.GetRigidbody().position - sun.position;
            float r = dir.magnitude;


            float speed = Mathf.Sqrt((float)(G * sunBody.GetMass() / r));

            Vector3 tangent = Vector3.Cross(dir.normalized, Vector3.up);

            body.GetRigidbody().linearVelocity = tangent * speed;
        }
    }

    private CelestialBody GetHeaviestBody()
    {
        CelestialBody heaviest = bodies[0];

        foreach (CelestialBody body in bodies)
        {
            if (body.GetMass() > heaviest.GetMass())
            {
                heaviest = body;
            }
        }

        return heaviest;
    }
}