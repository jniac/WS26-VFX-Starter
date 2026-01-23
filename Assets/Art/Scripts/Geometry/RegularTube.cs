using UnityEngine;

public enum DimensionMode { Radius, SideLength }

[System.Serializable]
public struct RegularTubeParameters
{
    public DimensionMode dimensionMode;
    public float dimension;
    public float turnFactor;
    public int sides;
    public float length;
    public float spreadFactor;
    public float align;
    public Color vertexColor;

    public static RegularTubeParameters Default => new RegularTubeParameters
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

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteAlways]
public class RegularTube : MonoBehaviour
{
    public RegularTubeParameters parameters = RegularTubeParameters.Default;

    public DimensionMode dimensionMode = DimensionMode.Radius;
    DimensionMode _cachedDimensionMode = DimensionMode.Radius;

    public float dimension = 1f;
    float _cachedDimension = 1f;

    [UnclampedRange(-1f, 1f)]
    public float turnFactor = 0f;
    float _cachedTurnFactor = 0f;

    public int sides = 3;
    int _cachedSides = 0;

    public float length = 10f;
    float _cachedLength = 10f;

    public float spreadFactor = 0f;
    float _cachedSpreadFactor = 0f;

    [UnclampedRange(-1f, 1f)]
    public float align = 0f;
    float _cachedAlign = 0f;

    public Color vertexColor = Color.white;
    Color _cachedVertexColor = Color.white;

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
        return
            _cachedDimension != dimension ||
            _cachedTurnFactor != turnFactor ||
            _cachedSides != sides ||
            _cachedLength != length ||
            _cachedSpreadFactor != spreadFactor ||
            _cachedAlign != align ||
            _cachedVertexColor != vertexColor ||
            _cachedDimensionMode != dimensionMode;
    }

    bool ConsumeDirty()
    {
        bool dirty = IsDirty();

        if (!dirty)
            return false;

        _cachedDimension = dimension;
        _cachedTurnFactor = turnFactor;
        _cachedSides = sides;
        _cachedLength = length;
        _cachedSpreadFactor = spreadFactor;
        _cachedAlign = align;
        _cachedVertexColor = vertexColor;
        _cachedDimensionMode = dimensionMode;

        return true;
    }

    void LateUpdate()
    {
        parameters.dimensionMode = dimensionMode;
        parameters.dimension = dimension;
        parameters.turnFactor = turnFactor;
        parameters.sides = sides;
        parameters.length = length;
        parameters.spreadFactor = spreadFactor;
        parameters.align = align;
        parameters.vertexColor = vertexColor;

        if (ConsumeDirty())
        {
            ComputeMesh();
        }
    }
}
