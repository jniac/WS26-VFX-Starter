using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public static class Track
{
    public static class Settings
    {
        public const int CIRCLE_RESOLUTION = 4 * 8;
        // public const int CIRCLE_RESOLUTION = 4;
    }

    [System.Serializable]
    public struct TrackPointParameters
    {
        public float radius;
        public float angle;
        public float aperture;

        public static TrackPointParameters Default => new()
        {
            radius = 10f,
            angle = 0f,
            aperture = 1f
        };
    }

    public struct CubicBezierSegment
    {
        public Vector3 p0, p1, p2, p3;

        public readonly Vector3 GetPoint(float t)
        {
            float u = 1 - t;
            return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }

        public readonly Vector3 GetTangent(float t)
        {
            float u = 1 - t;
            return (-3 * u * u) * p0 + (3 * u * u - 6 * u * t) * p1 + (6 * u * t - 3 * t * t) * p2 + (3 * t * t) * p3;
        }
    }

    public struct VertexData
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector4 tangent;
        public Vector2 uv;
    }

    public static (NativeArray<VertexData>, Bounds) CreateTubeVertexDataArray(
        Matrix4x4 start,
        Matrix4x4 end,
        TrackPointParameters startParameters,
        TrackPointParameters endParameters
    )
    {
        int SUBDIVISION_T = 16;
        int SUBDIVISION_U = Settings.CIRCLE_RESOLUTION;

        int vertexCount = SUBDIVISION_T * SUBDIVISION_U * 2 * 3;

        var vertexData = new NativeArray<VertexData>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        var pStart = (Vector3)start.GetColumn(3);
        var pEnd = (Vector3)end.GetColumn(3);
        var qStart = Quaternion.LookRotation(start.GetColumn(2), start.GetColumn(1));
        var qEnd = Quaternion.LookRotation(end.GetColumn(2), end.GetColumn(1));
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        var forwardStart = (Vector3)start.GetColumn(2);
        var forwardEnd = (Vector3)end.GetColumn(2);
        var amount = (pStart - pEnd).magnitude / 3f;
        var segment = new CubicBezierSegment
        {
            p0 = pStart,
            p1 = pStart + forwardStart * amount,
            p2 = pEnd - forwardEnd * amount,
            p3 = pEnd
        };

        for (int i = 0; i < SUBDIVISION_T; i++)
        {
            var t0 = (float)i / SUBDIVISION_T;
            var t1 = (float)(i + 1) / SUBDIVISION_T;

            var r0 = Mathf.Lerp(startParameters.radius, endParameters.radius, t0);
            var r1 = Mathf.Lerp(startParameters.radius, endParameters.radius, t1);

            // var p0 = Vector3.Lerp(pStart, pEnd, t0);
            // var p1 = Vector3.Lerp(pStart, pEnd, t1);
            var p0 = segment.GetPoint(t0);
            var p1 = segment.GetPoint(t1);
            var v01n = (p1 - p0).normalized;

            var q0 = Quaternion.Slerp(qStart, qEnd, t0);
            var q1 = Quaternion.Slerp(qStart, qEnd, t1);

            for (int j = 0; j < Settings.CIRCLE_RESOLUTION; j++)
            {
                var angle0 = 2 * Mathf.PI * j / Settings.CIRCLE_RESOLUTION;
                var angle1 = 2 * Mathf.PI * (j + 1) / Settings.CIRCLE_RESOLUTION;

                var v0 = new Vector3(Mathf.Cos(angle0), Mathf.Sin(angle0), 0);
                var v1 = new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0);

                Vector3 vertex0 = p0 + q0 * v0 * r0;
                Vector3 vertex1 = p0 + q0 * v1 * r0;
                Vector3 vertex2 = p1 + q1 * v0 * r1;
                Vector3 vertex3 = p1 + q1 * v1 * r1;

                Vector3 normal0 = q0 * v0;
                Vector3 normal1 = q0 * v1;

                var tangent = new Vector4(v01n.x, v01n.y, v01n.z, 1f);

                int baseIndex = (i * Settings.CIRCLE_RESOLUTION + j) * 6;

                var uv0 = new Vector2(1f - (float)j / Settings.CIRCLE_RESOLUTION, t0);
                var uv1 = new Vector2(1f - (float)(j + 1) / Settings.CIRCLE_RESOLUTION, t0);
                var uv2 = new Vector2(1f - (float)j / Settings.CIRCLE_RESOLUTION, t1);
                var uv3 = new Vector2(1f - (float)(j + 1) / Settings.CIRCLE_RESOLUTION, t1);

                min = Vector3.Min(min, vertex0);
                min = Vector3.Min(min, vertex1);
                min = Vector3.Min(min, vertex2);
                min = Vector3.Min(min, vertex3);

                max = Vector3.Max(max, vertex0);
                max = Vector3.Max(max, vertex1);
                max = Vector3.Max(max, vertex2);
                max = Vector3.Max(max, vertex3);

                vertexData[baseIndex + 0] = new VertexData { position = vertex0, normal = normal0, tangent = tangent, uv = uv0 };
                vertexData[baseIndex + 1] = new VertexData { position = vertex1, normal = normal1, tangent = tangent, uv = uv1 };
                vertexData[baseIndex + 2] = new VertexData { position = vertex2, normal = normal0, tangent = tangent, uv = uv2 };

                vertexData[baseIndex + 3] = new VertexData { position = vertex1, normal = normal1, tangent = tangent, uv = uv1 };
                vertexData[baseIndex + 4] = new VertexData { position = vertex3, normal = normal1, tangent = tangent, uv = uv3 };
                vertexData[baseIndex + 5] = new VertexData { position = vertex2, normal = normal0, tangent = tangent, uv = uv2 };
            }
        }

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);

        return (vertexData, bounds);
    }

    /// <summary>
    /// Sets up a tube mesh between two given transforms.
    /// <br/>
    /// Note:
    /// <br/>
    /// - Data is non-indexed (for allowing non contiguous attributes).
    /// </summary>
    public static void SetTubeMesh(
        Mesh mesh,
        Matrix4x4 start,
        Matrix4x4 end,
        TrackPointParameters startParameters,
        TrackPointParameters endParameters
    )
    {
        var (vertexData, bounds) = CreateTubeVertexDataArray(start, end, startParameters, endParameters);
        int vertexCount = vertexData.Length;
        mesh.SetVertexBufferParams(vertexCount, new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 0),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 0),
        });


        mesh.bounds = bounds;
        mesh.SetVertexBufferData(vertexData, 0, 0, vertexCount);

        // Indices:
        int[] indices = new int[vertexCount];
        for (int i = 0; i < vertexCount; i++)
            indices[i] = i;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        mesh.name = "Track Tube Mesh";
    }

    public class TrackSegment
    {
        public (Matrix4x4, TrackPointParameters) start;
        public (Matrix4x4, TrackPointParameters) end;
    }
}
