using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CelestialBody body;
    [Space(10)]

    [Header("Orbit Shape")]
    [Tooltip("Radius of the orbit.")]
    [SerializeField] private float radius = 5000f;
    [Tooltip("Orbit inclination in degrees.")]
    [SerializeField] private float inclination = 0f;
    [Space(10)]

    [Header("Appearence")]
    [SerializeField] private Color color = new Color(1f, 1f, 1f, 0.25f);
    [Tooltip("Base width in world units before adaptive scaling.")]
    [SerializeField] private float baseWidth = 50f;
    [Space(10)]

    [Header("Quality")]
    [Tooltip("Numbers of segements for the circle.")]
    [SerializeField] private int segments = 128;
    [Space(10)]

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private LineRenderer line;
    private Camera adaptiveCamera;
    private StellarMapManager stellarMapManager;

    private void OnDestroy()
    {
        if(stellarMapManager != null)
        {
            stellarMapManager.OnMapChanged -= OnMapChanged;
        }
    }

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        ConfigureRenderer();
    }

    private void LateUpdate()
    {
        // Adapt line width to camera ortho size every frame
        // so the line stays visible at any zoom level on the stellar map
        AdaptWidth();
    }

    private void ConfigureRenderer()
    {
        line.useWorldSpace = true;

        line.alignment = LineAlignment.View;

        line.loop = true;

        line.positionCount = segments;

        // Material : use simple unlit/additive material
        // Assign a proper material in the Inspector for best result
        if(line.sharedMaterial == null)
        {
            line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        // Color
        line.startColor = color;
        line.endColor = color;

        // Initial Width
        line.widthMultiplier = baseWidth;
    }

    /// <summary>
    /// Computes orbit circle points in world space.
    /// The positions are recalculated every frame in Update so the ring follows
    /// its center if the center moves.
    /// </summary>
    private void BuildOrbitPoint()
    {
        Vector3 center = body.GetCenter().position;

        Quaternion titl = Quaternion.Euler(inclination, 0f, 0f);

        for(int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 local = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            line.SetPosition(i, center + titl * local);
        }
    }

    private void Update()
    {
        // Rebuild each frame so the orbit follows a moving center (e.g. moon around planet)
        if (!line.enabled) return;
        BuildOrbitPoint();
    }

    private void AdaptWidth()
    {
        if(adaptiveCamera == null)
        {
            adaptiveCamera = Camera.main;
        }

        if (adaptiveCamera == null) return;

        // Scale width proportionnally to ortho size so it looks the same at all zooms
        float targetWidth = adaptiveCamera.orthographic
            ? adaptiveCamera.orthographicSize * 0.015f
            : baseWidth;

        line.widthMultiplier = Mathf.Max(targetWidth, 1f);
    }

    /// <summary>Set orbit parameters from SolarSystemGenerator.</summary>
    public void Setup(float radius, float inclination, Color color, StellarMapManager stellarMapManager)
    {
        this.radius = radius;
        this.inclination = inclination;
        this.color = color;
        this.stellarMapManager = stellarMapManager;

        if(line == null) line = GetComponent<LineRenderer>();

        line.startColor = color;
        line.endColor = color;

        if (stellarMapManager != null)
        {
            stellarMapManager.OnMapChanged += OnMapChanged;
            line.enabled = false;
        }
        else
        {
            Debug.LogWarning("[OrbitDrawer] stellarMapManager is null", this);
        }

        BuildOrbitPoint();
    }

    /// <summary>
    /// Assigns the camera used for adaptive width.
    /// Call this from StellarMapManager when opening the map so
    /// the width adapts to the stellar camera's ortho size.
    /// </summary>
    public void SetAdaptiveCamera(Camera cam)
    {
        adaptiveCamera = cam;
    }

    private void OnMapChanged(bool isMapOpen)
    {
        line.enabled = isMapOpen;
        if(debug)
            Debug.Log($"map open : {isMapOpen}");
    }
}