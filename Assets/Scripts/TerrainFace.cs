using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Constructs 6 terrain faces, each with its own mesh
// Here a planet = a modified cube
public class TerrainFace
{

    Mesh mesh;
    int resolution; // determines how detailed a terrain face is
    Vector3 localUp; // determines which way a terrain face is facing
    Vector3 axisA; // will be perpendicular to localUp and axisB
    Vector3 axisB; // will be perpendicular to localUp and axisA

    // Terrain face constructor
    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    // Constructs the mesh for a terrain face
    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution]; // resolution = total number of vertices along a single edge of a face
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6]; // calculate the number of triangles in our mesh
        int triIndex = 0; // index for triangles array

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution; // index for vertices array
                Vector2 percent = new Vector2(x, y) / (resolution - 1); // Defines how close we are to completing the mesh
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB; // Calculate how far along each axis (A, B, localUp) we are
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized; //Transform cube into sphere
                vertices[i] = pointOnUnitSphere;

                // Calculate the vertices for each triangle
                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }
        mesh.Clear(); // clear mesh data before reassigning the vertices and triangles in case the resolution changes
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Update the normals to reflect vertices changes
    }
}