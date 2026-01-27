using UnityEngine;

public static partial class MeshTools
{
    public static (int ia, int ib, int ic) GetTriangleIndices(
        Mesh mesh,
        int triangleIndex)
    {
        var i0 = mesh.triangles[triangleIndex * 3 + 0];
        var i1 = mesh.triangles[triangleIndex * 3 + 1];
        var i2 = mesh.triangles[triangleIndex * 3 + 2];
        return (i0, i1, i2);
    }

    public static (Vector3 A, Vector3 B, Vector3 C) GetTriangleVertices(
        Mesh mesh,
        int triangleIndex)
    {
        var i0 = mesh.triangles[triangleIndex * 3 + 0];
        var i1 = mesh.triangles[triangleIndex * 3 + 1];
        var i2 = mesh.triangles[triangleIndex * 3 + 2];
        var v0 = mesh.vertices[i0];
        var v1 = mesh.vertices[i1];
        var v2 = mesh.vertices[i2];
        return (v0, v1, v2);
    }

    public enum ToNonIndexedNormalsOption
    {
        Preserve,
        Recalculate
    }
    public static Mesh ToNonIndexed(
        Mesh mesh,
        ToNonIndexedNormalsOption normalsOption = ToNonIndexedNormalsOption.Recalculate)
    {
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var uvs = mesh.uv;

        var newVertices = new Vector3[triangles.Length];
        var newNormals = new Vector3[triangles.Length];
        var newUVs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            int index = triangles[i];
            newVertices[i] = vertices[index];
            if (normals.Length > 0)
                newNormals[i] = normals[index];
            if (uvs.Length > 0)
                newUVs[i] = uvs[index];
        }

        switch (normalsOption)
        {
            case ToNonIndexedNormalsOption.Recalculate:
                for (int i = 0; i < newNormals.Length; i += 3)
                {
                    var v0 = newVertices[i + 0];
                    var v1 = newVertices[i + 1];
                    var v2 = newVertices[i + 2];
                    var normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                    newNormals[i + 0] = normal;
                    newNormals[i + 1] = normal;
                    newNormals[i + 2] = normal;
                }
                break;
            case ToNonIndexedNormalsOption.Preserve:
            default:
                // Do nothing
                break;
        }

        mesh.vertices = newVertices;
        if (normals.Length > 0)
            mesh.normals = newNormals;
        if (uvs.Length > 0)
            mesh.uv = newUVs;

        var newTriangles = new int[triangles.Length];
        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = i;
        }
        mesh.triangles = newTriangles;

        return mesh;
    }

}