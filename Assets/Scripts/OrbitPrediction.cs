using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class OrbitPredictionVerlet : MonoBehaviour
{
    public float G = 0.01f;
    public int simulationSteps = 800;
    public float timeStep = 0.02f;
    public Color orbitColor = Color.yellow;

    private Rigidbody[] bodies;

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        bodies = FindObjectsOfType<Rigidbody>();

        Rigidbody current = GetComponent<Rigidbody>();
        if (current == null)
            return;

        List<Vector3> positions = PredictOrbit(current);

        Gizmos.color = orbitColor;

        for (int i = 0; i < positions.Count - 1; i++)
        {
            Gizmos.DrawLine(positions[i], positions[i + 1]);
        }
    }

    List<Vector3> PredictOrbit(Rigidbody body)
    {
        List<Vector3> result = new List<Vector3>();

        Vector3 currentPos = body.position;
        Vector3 velocity = body.linearVelocity;

        // Calcul position précédente pour Verlet
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

    Vector3 ComputeAcceleration(Rigidbody body, Vector3 simulatedPos)
    {
        Vector3 totalForce = Vector3.zero;

        foreach (Rigidbody other in bodies)
        {
            if (other == body)
                continue;

            Vector3 direction = other.position - simulatedPos;
            float distance = direction.magnitude;

            if (distance < 0.1f)
                continue;

            float forceMagnitude = G * (body.mass * other.mass) / (distance * distance);
            totalForce += direction.normalized * forceMagnitude;
        }

        return totalForce / body.mass;
    }
}