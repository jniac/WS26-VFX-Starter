using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public static partial class MeshTools
{
    public class SurfaceMeshTopology
    {
        public readonly struct EdgeKey
        {
            public readonly int A;
            public readonly int B;

            public EdgeKey(int a, int b)
            {
                if (a < b)
                {
                    A = a;
                    B = b;
                }
                else
                {
                    A = b;
                    B = a;
                }
            }
        }

        readonly Mesh mesh;

        readonly HashSet<EdgeKey> edges = new();
        readonly Dictionary<EdgeKey, List<int>> edgeToTriangleMap = new();

        public SurfaceMeshTopology(Mesh mesh)
        {
            this.mesh = mesh;

            for (int i = 0; i < mesh.triangles.Length / 3; i++)
            {
                int i0 = mesh.triangles[i * 3 + 0];
                int i1 = mesh.triangles[i * 3 + 1];
                int i2 = mesh.triangles[i * 3 + 2];

                var e0 = new EdgeKey(i0, i1);
                var e1 = new EdgeKey(i1, i2);
                var e2 = new EdgeKey(i2, i0);

                edges.Add(e0);
                edges.Add(e1);
                edges.Add(e2);

                if (!edgeToTriangleMap.ContainsKey(e0))
                    edgeToTriangleMap[e0] = new List<int>();
                edgeToTriangleMap[e0].Add(i);

                if (!edgeToTriangleMap.ContainsKey(e1))
                    edgeToTriangleMap[e1] = new List<int>();
                edgeToTriangleMap[e1].Add(i);

                if (!edgeToTriangleMap.ContainsKey(e2))
                    edgeToTriangleMap[e2] = new List<int>();
                edgeToTriangleMap[e2].Add(i);
            }
        }

        public bool GetAdjacentTriangle(int triangleIndex, int edgeIndex, out int adjacentTriangleIndex)
        {
            adjacentTriangleIndex = -1;

            int i0 = mesh.triangles[triangleIndex * 3 + 0];
            int i1 = mesh.triangles[triangleIndex * 3 + 1];
            int i2 = mesh.triangles[triangleIndex * 3 + 2];

            EdgeKey edge;
            switch (edgeIndex)
            {
                case 0:
                    edge = new EdgeKey(i0, i1);
                    break;
                case 1:
                    edge = new EdgeKey(i1, i2);
                    break;
                case 2:
                    edge = new EdgeKey(i2, i0);
                    break;
                default:
                    Debug.LogError("Invalid edge index");
                    return false;
            }

            if (edgeToTriangleMap.TryGetValue(edge, out var triangles))
            {
                foreach (var tri in triangles)
                {
                    if (tri != triangleIndex)
                    {
                        adjacentTriangleIndex = tri;
                        return true;
                    }
                }
            }

            return false;
        }

        public IEnumerable<int> GetAllAdjacentTriangles(int triangleIndex)
        {
            int i0 = mesh.triangles[triangleIndex * 3 + 0];
            int i1 = mesh.triangles[triangleIndex * 3 + 1];
            int i2 = mesh.triangles[triangleIndex * 3 + 2];

            var edges = new EdgeKey[]
            {
                new(i0, i1),
                new(i1, i2),
                new(i2, i0)
            };

            foreach (var edge in edges)
            {
                if (edgeToTriangleMap.TryGetValue(edge, out var triangles))
                {
                    foreach (var tri in triangles)
                    {
                        if (tri != triangleIndex)
                        {
                            yield return tri;
                        }
                    }
                }
            }
        }
    }

    public class SurfaceState
    {
        public Mesh mesh;

        public int triangleIndex;

        /// <summary>
        /// Barycentric coordinates within the triangle (AB|AC basis), w = 1 - u - v
        /// </summary>
        public Vector2 uv;

        public SurfaceState(
            Mesh mesh,
            int triangleIndex,
            Vector2 uv)
        {
            Set(mesh, triangleIndex, uv);
        }

        public SurfaceState()
        {
            Set(null, -1, Vector2.zero);
        }

        public SurfaceState Set(
            Mesh mesh,
            int triangleIndex,
            Vector2 uv)
        {
            this.mesh = mesh;
            this.triangleIndex = triangleIndex;
            this.uv = uv;
            return this;
        }

        public SurfaceState Copy(SurfaceState other)
        {
            mesh = other.mesh;
            triangleIndex = other.triangleIndex;
            uv = other.uv;
            return this;
        }

        public (Vector3 A, Vector3 B, Vector3 C) GetTriangleVertices()
        {
            int i0 = mesh.triangles[triangleIndex * 3 + 0];
            int i1 = mesh.triangles[triangleIndex * 3 + 1];
            int i2 = mesh.triangles[triangleIndex * 3 + 2];
            Vector3 v0 = mesh.vertices[i0];
            Vector3 v1 = mesh.vertices[i1];
            Vector3 v2 = mesh.vertices[i2];
            return (v0, v1, v2);
        }

        public Vector3 GetPosition(Vector2 uv)
        {
            var (A, B, C) = GetTriangleVertices();
            return A * (1.0f - uv.x - uv.y) + B * uv.x + C * uv.y;
        }

        public Vector3 GetPosition() => GetPosition(uv);

        /// <summary>
        /// Computes which edge will be crossed first when moving from uv by deltaUV.
        /// </summary>
        public static bool FindFirstEdge(Vector2 uv, Vector2 deltaUV, out int edgeIndex, out float t, out Vector2 I)
        {
            edgeIndex = -1;
            t = float.MaxValue;
            I = uv;

            float u = uv.x;
            float v = uv.y;
            float du = deltaUV.x;
            float dv = deltaUV.y;

            float t0 = du != 0f ? -u / du : float.MaxValue;
            if (t0 < 0f || t0 > 1f) t0 = float.MaxValue;

            float t1 = dv != 0f ? -v / dv : float.MaxValue;
            if (t1 < 0f || t1 > 1f) t1 = float.MaxValue;

            float t2 = (du + dv) != 0f ? (1f - u - v) / (du + dv) : float.MaxValue;
            if (t2 < 0f || t2 > 1f) t2 = float.MaxValue;

            float tMin = Mathf.Min(t0, Mathf.Min(t1, t2));

            if (tMin == float.MaxValue)
                // Still inside the triangle
                return false;

            t = tMin;
            edgeIndex = t0 == tMin ? 2 : (t1 == tMin ? 0 : 1);
            I.x = u + du * t;
            I.y = v + dv * t;
            return true;
        }

        public SurfaceState Move(Vector2 deltaUV)
        {
            if (FindFirstEdge(uv, deltaUV, out var edge, out var t, out var I))
            {
                uv = I;
            }
            else
            {
                uv += deltaUV;
            }
            return this;
        }
    }
}
