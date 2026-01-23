using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RegularPolygon : MonoBehaviour
{
    [System.Serializable]
    public struct Parameters
    {
        public GeometryUtils.DimensionMode dimensionMode;

        public float dimension;

        [UnclampedRange(-1f, 1f)]
        public float turnFactor;

        [UnclampedRange(0f, 2f)]
        public float ringFactor;

        [Range(3, 20)]
        public int sides;

        public Color vertexColor;

        public bool doubleSided;

        public static Parameters Default => new()
        {
            dimensionMode = GeometryUtils.DimensionMode.Radius,
            dimension = 1f,
            turnFactor = 0f,
            ringFactor = 0f,
            sides = 3,
            vertexColor = Color.white,
            doubleSided = true
        };
    }

    public Parameters parameters = Parameters.Default;

    public GeometryUtils.DimensionMode dimensionMode = GeometryUtils.DimensionMode.Radius;

    public float dimension = 1f;

    [UnclampedRange(-1f, 1f)]
    public float turnFactor = 0f;

    [UnclampedRange(0f, 2f)]
    public float ringFactor = 0f;

    [Range(3, 20)]
    public int sides = 3;

    public Color vertexColor = Color.white;

    public bool doubleSided = true;

    public bool createEmptyChildren = false;

    public float Radius =>
        dimensionMode == GeometryUtils.DimensionMode.Radius ? dimension : GeometryUtils.SideLengthToRadius(sides, dimension);

    float GetAngle(int index, float indexOffset = 0f)
    {
        return 2 * Mathf.PI * (turnFactor + 0.25f + (index + indexOffset) / sides);
    }

    void ComputeMesh()
    {
        var mesh = new Mesh();

        float radius = Radius;
        float innerRadius = radius * ringFactor;

        var vertices = new Vector3[sides * 2 * (doubleSided ? 2 : 1)];
        var normals = new Vector3[vertices.Length];
        var triangles = new int[sides * 6 * (doubleSided ? 2 : 1)];
        var uv = new Vector2[vertices.Length];
        var colors = new Color[vertices.Length];

        for (int i = 0; i < sides; i++)
        {
            float angle = GetAngle(i);
            float xOuter = radius * Mathf.Cos(angle);
            float yOuter = radius * Mathf.Sin(angle);
            float xInner = innerRadius * Mathf.Cos(angle);
            float yInner = innerRadius * Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(xOuter, yOuter, 0);
            vertices[i * 2 + 1] = new Vector3(xInner, yInner, 0);

            normals[i * 2] = Vector3.back;
            normals[i * 2 + 1] = Vector3.back;

            uv[i * 2] = new Vector2((xOuter / (2 * radius)) + 0.5f, (yOuter / (2 * radius)) + 0.5f);
            uv[i * 2 + 1] = new Vector2((xInner / (2 * radius)) + 0.5f, (yInner / (2 * radius)) + 0.5f);

            colors[i * 2] = vertexColor;
            colors[i * 2 + 1] = vertexColor;

            int nextI = (i + 1) % sides;

            triangles[i * 6] = i * 2;
            triangles[i * 6 + 1] = i * 2 + 1;
            triangles[i * 6 + 2] = nextI * 2;

            triangles[i * 6 + 3] = nextI * 2;
            triangles[i * 6 + 4] = i * 2 + 1;
            triangles[i * 6 + 5] = nextI * 2 + 1;

            if (doubleSided)
            {
                vertices[sides * 2 + i * 2] = vertices[i * 2];
                vertices[sides * 2 + i * 2 + 1] = vertices[i * 2 + 1];

                normals[sides * 2 + i * 2] = Vector3.forward;
                normals[sides * 2 + i * 2 + 1] = Vector3.forward;

                uv[sides * 2 + i * 2] = uv[i * 2];
                uv[sides * 2 + i * 2 + 1] = uv[i * 2 + 1];

                colors[sides * 2 + i * 2] = vertexColor;
                colors[sides * 2 + i * 2 + 1] = vertexColor;

                int baseIndex = sides * 6 + i * 6;
                int tri_offset = sides * 2;

                triangles[baseIndex + 0] = tri_offset + i * 2 + 1;
                triangles[baseIndex + 1] = tri_offset + i * 2;
                triangles[baseIndex + 2] = tri_offset + nextI * 2;

                triangles[baseIndex + 3] = tri_offset + i * 2 + 1;
                triangles[baseIndex + 4] = tri_offset + nextI * 2;
                triangles[baseIndex + 5] = tri_offset + nextI * 2 + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.RecalculateBounds();

        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    void EnsureEmptyChildren()
    {
        var desiredChildCount = createEmptyChildren ? sides : 0;
        while (transform.childCount < desiredChildCount)
        {
            var child = new GameObject();
            child.transform.parent = transform;
        }

        var extraChildren = transform.Cast<Transform>()
            .Skip(desiredChildCount)
            .ToList();
        if (Application.isPlaying)
        {
            foreach (var child in extraChildren)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            foreach (var child in extraChildren)
            {
                Destroy(child.gameObject);
            }
        }

        // \boxed{R' = R \cdot \cos\left(\frac{\pi}{n}\right)}
        float innerRadius = Radius * Mathf.Cos(Mathf.PI / sides);
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.name = $"Side-{i}";
            float angle = GetAngle(i, indexOffset: 0.5f);
            float x = innerRadius * Mathf.Cos(angle);
            float y = innerRadius * Mathf.Sin(angle);
            child.localPosition = new Vector3(x, y, 0);
            child.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        }
    }

    void OnValidate()
    {
        ComputeMesh();
        EnsureEmptyChildren();

        parameters.dimensionMode = dimensionMode;
        parameters.dimension = dimension;
        parameters.turnFactor = turnFactor;
        parameters.ringFactor = ringFactor;
        parameters.sides = sides;
        parameters.vertexColor = vertexColor;
        parameters.doubleSided = doubleSided;
        EditorUtility.SetDirty(this);
    }
}
