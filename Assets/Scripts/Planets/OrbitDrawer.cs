using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitDrawer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int segments = 10000;
    [SerializeField] private float radius = 5000f;

    private void Start()
    {
        LineRenderer line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.useWorldSpace = false;

        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            line.SetPosition(i, new Vector3(x, 0, z));
            angle += (360f / segments);
        }
    }
}