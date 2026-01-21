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

    public Color vertexColor = Color.white;

    public float Radius =>
        dimensionMode == DimensionMode.Radius ? dimension : dimension / (2 * Mathf.Sin(Mathf.PI / sides));

    public void ComputeMesh()
    {
        var mesh = new Mesh();

        float radius = Radius;

        Vector3[] vertices = new Vector3[sides * 4];
        Vector3[] normals = new Vector3[vertices.Length];
        int[] triangles = new int[sides * 12];
        Vector2[] uv = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < sides; i++)
        {
            float angle = 2 * Mathf.PI * (turnFactor + 0.25f + i / (float)sides);
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            vertices[i * 4] = new Vector3(x, y, -length / 2);
            vertices[i * 4 + 1] = new Vector3(x, y, length / 2);

            normals[i * 4] = new Vector3(x, y, 0).normalized * -1f;
            normals[i * 4 + 1] = new Vector3(x, y, 0).normalized * -1f;

            uv[i * 4] = new Vector2(i / (float)sides, 0);
            uv[i * 4 + 1] = new Vector2(i / (float)sides, 1);

            colors[i * 4] = vertexColor;
            colors[i * 4 + 1] = vertexColor;

            int nextI = (i + 1) % sides;

            triangles[i * 12] = i * 4;
            triangles[i * 12 + 1] = i * 4 + 1;
            triangles[i * 12 + 2] = nextI * 4;

            triangles[i * 12 + 3] = nextI * 4;
            triangles[i * 12 + 4] = i * 4 + 1;
            triangles[i * 12 + 5] = nextI * 4 + 1;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uv;

        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    void OnValidate()
    {
        ComputeMesh();
    }
}
