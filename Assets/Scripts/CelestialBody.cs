using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [Header("Physical properties")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float density = 1f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    public float GetRadius()
    {
        return Mathf.Pow((3f * mass) / (4f * Mathf.PI * density), 1f / 3f);
    }

    public void SetMass(float m) => mass = m;
    public void SetDensity(float d) => density = d;

    public float GetMass() => mass;

    public void SetRotationSpeed(float rotationSpeed)
    {
        this.rotationSpeed = rotationSpeed;
    }

    public void ApplyScale()
    {
        float diameter = GetRadius() * 2f;
        transform.localScale = Vector3.one * diameter;
    }

    private void Update()
    {
        // Rotation on itself
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}