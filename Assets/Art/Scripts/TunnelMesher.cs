using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TunnelMesher : MonoBehaviour
{
    [Range(0, 18)]
    public int sides = 6;
    public float radius = 5f;
    public float length = 10f;
    public bool normalOut = false;

    void CreateGeometry()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new();
        mf.mesh = mesh;

        Vector3[] vertices = new Vector3[sides * 2];
        int[] triangles = new int[sides * 6];

        float angleStep = 360f / sides;

        int[] triangleOffsets = normalOut ? new int[] { 0, 1, 2, 3, 4, 5 } : new int[] { 0, 2, 1, 3, 5, 4 };

        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            vertices[i] = new Vector3(x, y, 0);
            vertices[i + sides] = new Vector3(x, y, length);

            int nextIndex = (i + 1) % sides;

            // First triangle
            triangles[i * 6 + triangleOffsets[0]] = i;
            triangles[i * 6 + triangleOffsets[1]] = nextIndex;
            triangles[i * 6 + triangleOffsets[2]] = i + sides;

            // Second triangle
            triangles[i * 6 + triangleOffsets[3]] = nextIndex;
            triangles[i * 6 + triangleOffsets[4]] = nextIndex + sides;
            triangles[i * 6 + triangleOffsets[5]] = i + sides;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void CreateGeometryWithSharpEdges()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        // We need vertices for both ends and separate normals
        Vector3[] vertices = new Vector3[sides * 2];
        Vector3[] normals = new Vector3[sides * 2];

        float angleStep = 360f / sides;

        // Create vertices
        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            vertices[i] = new Vector3(x, y, 0);            // Bottom vertex
            vertices[i + sides] = new Vector3(x, y, length); // Top vertex
        }

        // Create triangles and calculate normals
        List<int> triangles = new List<int>();

        for (int i = 0; i < sides; i++)
        {
            int nextIndex = (i + 1) % sides;

            // Add triangles for this face
            if (normalOut == false)
            {
                triangles.Add(i);
                triangles.Add(i + sides);
                triangles.Add(nextIndex);

                triangles.Add(nextIndex);
                triangles.Add(i + sides);
                triangles.Add(nextIndex + sides);
            }
            else
            {
                triangles.Add(i);
                triangles.Add(nextIndex);
                triangles.Add(i + sides);

                triangles.Add(nextIndex);
                triangles.Add(nextIndex + sides);
                triangles.Add(i + sides);
            }

            // Calculate normal for this face
            Vector3 faceNormal = Vector3.Cross(
                vertices[nextIndex] - vertices[i],
                vertices[i + sides] - vertices[i]
            ).normalized;

            if (!normalOut) faceNormal = -faceNormal;

            // Assign normals to vertices of this face
            // Note: vertices are shared, but normals will be averaged unless we duplicate
            normals[i] = faceNormal;
            normals[i + sides] = faceNormal;
            normals[nextIndex] = faceNormal;
            normals[nextIndex + sides] = faceNormal;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals;
        mesh.RecalculateBounds();
    }

    void OnValidate()
    {
        CreateGeometry();

        var mr = GetComponent<MeshRenderer>();
        if (mr.sharedMaterial == null)
            mr.sharedMaterial = new Material(Shader.Find("Standard"));
    }

    void Start()
    {
        CreateGeometry();
    }
}
