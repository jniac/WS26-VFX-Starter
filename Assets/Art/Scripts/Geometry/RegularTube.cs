using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RegularTube : MonoBehaviour
{
    public enum DimensionMode { Radius, SideLength }

    public DimensionMode dimensionMode = DimensionMode.Radius;

    public float dimension = 1f;

    [UnclampedRange(-1f, 1f)]
    public float turnFactor = 0f;

    public int sides = 3;

    public float length = 10f;

    [UnclampedRange(-1f, 1f)]
    public float align = 0f;

    public Color vertexColor = Color.white;

    public float Radius =>
        dimensionMode == DimensionMode.Radius ? dimension : dimension / (2 * Mathf.Sin(Mathf.PI / sides));

    public void ComputeMesh()
    {
        var mesh = new Mesh();

        float radius = Radius;

        // Non-indexed: 2 triangles per side, 3 vertices per triangle = 6 vertices per side
        Vector3[] vertices = new Vector3[sides * 6];
        Vector3[] normals = new Vector3[vertices.Length];
        int[] triangles = new int[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];

        int vi = 0;

        for (int i = 0; i < sides; i++)
        {
            float angle = 2 * Mathf.PI * (turnFactor + 0.25f + i / (float)sides);
            float nextAngle = 2 * Mathf.PI * (turnFactor + 0.25f + (i + 1) / (float)sides);
            float normalAngle = 2 * Mathf.PI * (turnFactor + 0.25f + (i + 0.5f) / sides);

            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            float z = length / 2f * align;
            float nextX = radius * Mathf.Cos(nextAngle);
            float nextY = radius * Mathf.Sin(nextAngle);
            float normalX = Mathf.Cos(normalAngle);
            float normalY = Mathf.Sin(normalAngle);

            var normal = new Vector3(-normalX, -normalY, 0);

            float u = i / (float)sides;
            float nextU = (i + 1) / (float)sides;

            // First triangle of the quad
            vertices[vi] = new(x, y, z - length / 2);
            normals[vi] = normal;
            uv[vi] = new(u, 0);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            vertices[vi] = new(x, y, z + length / 2);
            normals[vi] = normal;
            uv[vi] = new(u, 1);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            vertices[vi] = new(nextX, nextY, z - length / 2);
            normals[vi] = normal;
            uv[vi] = new(nextU, 0);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            // Second triangle of the quad
            vertices[vi] = new(nextX, nextY, z - length / 2);
            normals[vi] = normal;
            uv[vi] = new(nextU, 0);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            vertices[vi] = new(x, y, z + length / 2);
            normals[vi] = normal;
            uv[vi] = new(u, 1);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            vertices[vi] = new(nextX, nextY, z + length / 2);
            normals[vi] = normal;
            uv[vi] = new(nextU, 1);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uv;

        mesh.RecalculateBounds();

        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    void OnValidate()
    {
        ComputeMesh();
    }
}
