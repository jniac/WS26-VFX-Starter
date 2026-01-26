using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CapMesh : MonoBehaviour
{
    [System.Serializable]
    public struct Parameters
    {
        public float radius;
        public float edgeRadius;
        public int resolution;
        public int edgeSegments;

        public static Parameters Default => new()
        {
            radius = 1f,
            edgeRadius = 0.1f,
            resolution = 4,
            edgeSegments = 5,
        };
    }

    struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }

    public static void SetupCylinderMesh(
        Mesh mesh,
        Parameters parameters
    )
    {
        int resolution = Mathf.Max(parameters.resolution, 4);
        float radius = parameters.radius;
        int edgeSegments = Mathf.Max(parameters.edgeSegments, 3);

        mesh.Clear();

        // Vertices:
        int vertexCount = (resolution + 1) * (edgeSegments + 1);
        var vertices = new Vertex[vertexCount];

        for (int y = 0; y <= edgeSegments; y++)
        {
            float v = y / (float)edgeSegments;

            float width = parameters.edgeRadius * (1f - Mathf.Cos(v * Mathf.PI / 2f));
            float height = parameters.edgeRadius * Mathf.Sin(v * Mathf.PI / 2f);

            for (int i = 0; i <= resolution; i++)
            {
                float u = i / (float)resolution;

                Vector3 center = new(0f, height, 0f);
                float angle = u * Mathf.PI * 2f;
                Vector3 vx = Mathf.Cos(angle) * new Vector3(radius - width, 0f, 0f);
                Vector3 vz = Mathf.Sin(angle) * new Vector3(0f, 0f, radius - width);

                int index = y * (resolution + 1) + i;
                vertices[index].position = center + (vx + vz);
                vertices[index].normal = vx + vz;
                vertices[index].uv = new Vector2(u, v);
            }
        }

        // Indices:
        int[] indices = new int[resolution * edgeSegments * 6];
        for (int y = 0; y < edgeSegments; y++)
        {
            for (int i = 0; i < resolution; i++)
            {
                int baseIndex = (y * resolution + i) * 6;
                int v0 = y * (resolution + 1) + i;
                int v1 = v0 + 1;
                int v2 = v0 + (resolution + 1);
                int v3 = v2 + 1;

                indices[baseIndex + 0] = v0;
                indices[baseIndex + 1] = v2;
                indices[baseIndex + 2] = v1;

                indices[baseIndex + 3] = v1;
                indices[baseIndex + 4] = v2;
                indices[baseIndex + 5] = v3;
            }
        }

        // Assign to mesh:
        mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
        mesh.SetVertexBufferParams(vertexCount, new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        });
        mesh.SetVertexBufferData(vertices, 0, 0, vertexCount);
        mesh.SetIndexBufferData(indices, 0, 0, indices.Length);
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    public Parameters parameters = Parameters.Default;

    void OnValidate()
    {
        var mesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        SetupCylinderMesh(mesh, parameters);
    }
}
