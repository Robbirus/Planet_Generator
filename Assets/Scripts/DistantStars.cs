using UnityEngine;

[ExecuteAlways]
public class DistantStars : MonoBehaviour
{
    [Header("Star Settings")]
    [SerializeField] private int starAmount = 1000;
    [SerializeField] private Vector2 starDistance = new Vector2(1500f, 3000f);
    [SerializeField] private Material starMaterial;

    private System.Random stellarRNG;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        // Built-in pipeline
        Camera.onPreCull += OnCameraPreCull;

        // URP / HDRP
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        Camera.onPreCull -= OnCameraPreCull;
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    // Built-in pipeline
    private void OnCameraPreCull(Camera cam)
    {
        FollowCamera(cam);
    }

    // URP / HDRP
    private void OnBeginCameraRendering(UnityEngine.Rendering.ScriptableRenderContext ctx, Camera cam)
    {
        FollowCamera(cam);
    }

    private void FollowCamera(Camera cam)
    {
        // Ignorer les caméras de réflexion, preview, etc.
        if (cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView)
            return;

        transform.position = cam.transform.position;
    }

    // ─── Generation ───────────────────────────────────────────────────────────

    public void GenerateStars()
    {
        EnsureComponents();

        Mesh mesh = BuildStarMesh();

        // Very wide bounds: the mesh will NEVER be culled by the engine
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100000f);

        meshFilter.sharedMesh = mesh;

        if (starMaterial != null)
            meshRenderer.sharedMaterial = starMaterial;
    }

    // ─── Mesh building ────────────────────────────────────────────────────────

    /// <summary>
    /// Construit un unique mesh composé de quads billboard pour chaque étoile.
    /// Un seul draw call, zéro problème de culling individuel.
    /// </summary>
    private Mesh BuildStarMesh()
    {
        // Pré-allocation des tableaux
        int vertCount = starAmount * 4;
        int triCount = starAmount * 6;

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[triCount];
        Color[] colors = new Color[vertCount];

        // Coins locaux d'un quad centré en (0,0)
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f),
        };

        Vector2[] quadUVs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        };

        for (int i = 0; i < starAmount; i++)
        {
            Vector3 dir = RandomOnUnitSphere();
            float distance = Range(starDistance.x, starDistance.y);
            float scale = Range(0.03f, 0.3f) * distance * 0.01f;
            float bright = Range(0.6f, 1.0f);

            Vector3 center = dir * distance;

            // Calcul d'une base orthogonale pour orienter le quad face à l'origine
            // (billboard sphérique statique, économique en GPU)
            Vector3 forward = -dir;                                        // face vers le centre
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.Cross(Vector3.forward, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right);

            int vBase = i * 4;
            for (int c = 0; c < 4; c++)
            {
                Vector3 corner = corners[c];
                vertices[vBase + c] = center
                    + right * (corner.x * scale)
                    + up * (corner.y * scale);
                uvs[vBase + c] = quadUVs[c];
                colors[vBase + c] = new Color(bright, bright, bright, 1f);
            }

            int tBase = i * 6;
            triangles[tBase + 0] = vBase + 0;
            triangles[tBase + 1] = vBase + 2;
            triangles[tBase + 2] = vBase + 1;
            triangles[tBase + 3] = vBase + 0;
            triangles[tBase + 4] = vBase + 3;
            triangles[tBase + 5] = vBase + 2;
        }

        Mesh mesh = new Mesh();
        mesh.name = "DistantStarsMesh";

        // Nécessaire si starAmount > ~16 000
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        return mesh;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void EnsureComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }
    }

    private float Range(float min, float max)
    {
        return (float)(stellarRNG.NextDouble() * (max - min) + min);
    }

    private Vector3 RandomOnUnitSphere()
    {
        float u = (float)stellarRNG.NextDouble();
        float v = (float)stellarRNG.NextDouble();

        float theta = u * 2f * Mathf.PI;
        float phi = Mathf.Acos(2f * v - 1f);

        float x = Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = Mathf.Sin(phi) * Mathf.Sin(theta);
        float z = Mathf.Cos(phi);

        return new Vector3(x, y, z);
    }

    public void SetSeed(System.Random newSeed)
    {
        stellarRNG = newSeed;
    }
}