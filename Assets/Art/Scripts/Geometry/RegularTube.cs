using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteAlways]
public class RegularTube : MonoBehaviour
{
    // STATIC:
    public enum DimensionMode { Radius, SideLength }

    [System.Serializable]
    public struct Parameters
    {
        public DimensionMode dimensionMode;

        public float dimension;

        [UnclampedRange(-0.5f, 0.5f)]
        public float turnFactor;

        public int sides;

        public float length;

        public float spreadFactor;

        [UnclampedRange(-1f, 1f)]
        public float align;

        public Color vertexColor;

        public static Parameters Default => new()
        {
            dimensionMode = DimensionMode.Radius,
            dimension = 1f,
            turnFactor = 0f,
            sides = 3,
            length = 10f,
            spreadFactor = 0f,
            align = 0f,
            vertexColor = Color.white
        };
    }





    // INSTANCE:
    public Parameters parameters = Parameters.Default;
    Parameters _cachedParameters = Parameters.Default;

    public float Radius =>
        parameters.dimensionMode == DimensionMode.Radius ? parameters.dimension : parameters.dimension / (2 * Mathf.Sin(Mathf.PI / parameters.sides));

    public void ComputeMesh()
    {
        var mesh = new Mesh();

        float radius = Radius;

        var sides = parameters.sides;
        var length = parameters.length;
        var turnFactor = parameters.turnFactor;
        var spreadFactor = parameters.spreadFactor;
        var align = parameters.align;
        var vertexColor = parameters.vertexColor;

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
            vertices[vi] = new Vector3(x, y, z - length / 2) - spreadFactor * normal;
            normals[vi] = normal;
            uv[vi] = new(u, 0);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            vertices[vi] = new Vector3(x, y, z + length / 2) - spreadFactor * normal;
            normals[vi] = normal;
            uv[vi] = new(u, 1);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            vertices[vi] = new Vector3(nextX, nextY, z - length / 2) - spreadFactor * normal;
            normals[vi] = normal;
            uv[vi] = new(nextU, 0);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            // Second triangle of the quad
            vertices[vi] = new Vector3(nextX, nextY, z - length / 2) - spreadFactor * normal;
            normals[vi] = normal;
            uv[vi] = new(nextU, 0);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            vertices[vi] = new Vector3(x, y, z + length / 2) - spreadFactor * normal;
            normals[vi] = normal;
            uv[vi] = new(u, 1);
            colors[vi] = vertexColor;
            triangles[vi] = vi;
            vi++;

            vertices[vi] = new Vector3(nextX, nextY, z + length / 2) - spreadFactor * normal;
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

    bool IsDirty()
    {
        return _cachedParameters.Equals(parameters) == false;
    }

    bool ConsumeDirty()
    {
        bool dirty = IsDirty();

        if (!dirty)
            return false;

        _cachedParameters = parameters;

        return true;
    }

    void LateUpdate()
    {
        if (ConsumeDirty())
        {
            ComputeMesh();
        }
    }
}
