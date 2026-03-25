using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class OrbitPredictionVerlet : MonoBehaviour
{
    [SerializeField] private float G = 0.1f;
    [SerializeField] private int simulationSteps = 800;
    [SerializeField] private float timeStep = 0.02f;
    [SerializeField] private Color orbitColor = Color.yellow;

    private CelestialBody[] bodies;

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

            bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID);

        CelestialBody current = GetComponent<CelestialBody>();
        if (current == null) return;

        List<Vector3> positions = PredictOrbit(current);

        Gizmos.color = orbitColor;

        for (int i = 0; i < positions.Count - 1; i++)
        {
            Gizmos.DrawLine(positions[i], positions[i + 1]);
        }
    }

    private List<Vector3> PredictOrbit(CelestialBody body)
    {
        List<Vector3> result = new List<Vector3>();

        Vector3 currentPos = body.GetRigidbody().position;
        Vector3 velocity = body.GetRigidbody().linearVelocity;

        // Calcul position presente pour Verlet
        Vector3 previousPos = currentPos - velocity * timeStep;

        for (int step = 0; step < simulationSteps; step++)
        {
            Vector3 acceleration = ComputeAcceleration(body, currentPos);

            Vector3 newPos = currentPos
                            + (currentPos - previousPos)
                            + acceleration * timeStep * timeStep;

            previousPos = currentPos;
            currentPos = newPos;

            result.Add(currentPos);
        }

        return result;
    }

    Vector3 ComputeAcceleration(CelestialBody body, Vector3 simulatedPos)
    {
        Vector3 totalForce = Vector3.zero;

        foreach (CelestialBody other in bodies)
        {
            if (other == body)
                continue;

            Vector3 direction = other.GetRigidbody().position - simulatedPos;
            float distance = direction.magnitude;

            if (distance < 0.1f)
                continue;

            double forceMagnitude = G * (body.GetMass() * other.GetMass()) / (distance * distance);
            totalForce += direction.normalized * (float)forceMagnitude;
        }

        return totalForce / 1f;
    }
}