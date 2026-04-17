using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{

    [Range(2, 256)]
    public int resolution = 10;

    public ShapeSettings shapeSettings; // link shape editor
    public ColourSettings colourSettings; // link color editor

    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;

    ShapeGenerator shapeGenerator; // use shapeGenerator object to update shape

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters; // a mesh filter holds a reference to a mesh
    TerrainFace[] terrainFaces;

    private void OnValidate()
    {
        GeneratePlanet();
    }

    // Initialize mesh filters for each terrain face
    void Initialize()
    {
        shapeGenerator = new ShapeGenerator(shapeSettings); // Create shape generator from current shape settings
        
        if (meshFilters == null || meshFilters.Length == 0) // check if the mesh filter needs to be initialized
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard")); // Add mesh renderer and assign default material (standard shader)
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
        }
    }

    // Generate a planet including shape and color settings
    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColours();
    }

    // Update shape (mesh) if shape settings have changed
    public void OnShapeSettingsUpdated()
    {
        Initialize();
        GenerateMesh();
    }

    // Update colors if color settings have changed
    public void OnColourSettingsUpdated()
    {
        Initialize();
        GenerateColours();
    }

    // Generate mesh for each terrain face
    void GenerateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }

    // Generate color from colourSettings editor
    void GenerateColours()
    {
        foreach (MeshFilter m in meshFilters)
        {
            m.GetComponent<MeshRenderer>().sharedMaterial.color = colourSettings.planetColour;
        }
    }
}