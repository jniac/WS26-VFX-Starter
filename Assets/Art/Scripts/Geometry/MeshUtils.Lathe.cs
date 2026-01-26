using UnityEngine;
using UnityEngine.Rendering;

public static partial class MeshUtils
{
  public enum LatheCapMode { None, Bottom, Top, Both }

  [System.Serializable]
  public struct LatheParameters
  {
    /// <summary>
    /// Profile to be revolved around the Y axis (Must be in the X-Y plane).
    /// </summary>
    public Vector3[] profile;

    /// <summary>
    /// Normals corresponding to each point in the profile. If null or invalid, they will be computed automatically.
    /// </summary>
    public Vector3[] normals;

    public int latheSegments;

    public LatheCapMode capMode;

    public static LatheParameters GetDefault()
    {
      int angleSegments = 8;
      var profile = new Vector3[angleSegments + 3];
      for (int i = 0; i <= angleSegments; i++)
      {
        float t = i / (float)angleSegments;
        float angle = t * Mathf.PI / 2f;
        float x = Mathf.Cos(angle) * 0.5f + 0.25f;
        float y = Mathf.Sin(angle) * 0.5f;
        profile[i + 1] = new Vector3(x, y, 0f);
      }
      profile[0] = profile[1] + new Vector3(0f, -0.1f, 0f);
      profile[angleSegments + 2] = profile[angleSegments + 1] + new Vector3(-0.1f, 0f, 0f);
      return new LatheParameters
      {
        profile = profile,
        latheSegments = 16,
        capMode = LatheCapMode.None,
      }.Validate();
    }

    public void ComputeProfileNormals()
    {
      var profileCount = profile.Length;
      Vector3[] normals = new Vector3[profileCount];
      for (int i = 0; i < profileCount; i++)
      {
        Vector3 p0 = i > 0 ? profile[i - 1] : profile[i];
        Vector3 p1 = i < profileCount - 1 ? profile[i + 1] : profile[i];
        Vector3 tangent = (p1 - p0).normalized;
        normals[i] = new Vector3(tangent.y, -tangent.x, 0f);
      }
      this.normals = normals;
    }

    public LatheParameters Validate()
    {
      if (profile == null || profile.Length < 2)
        throw new System.Exception("Profile must have at least 2 points.");
      if (latheSegments < 3)
        throw new System.Exception("Segments must be at least 3.");
      if (normals == null || normals.Length != profile.Length)
        ComputeProfileNormals();
      return this;
    }
  }

  public static Mesh LatheMesh(LatheParameters parameters)
  {
    parameters.Validate();

    // for (int i = 0; i < parameters.profileNormals.Length; i++)
    // {
    //   Debug.Log($"Normal {i}: {parameters.profileNormals[i]}");
    // }

    var profile = parameters.profile;
    var profileCount = profile.Length;
    var profileNormals = parameters.normals;
    var lathSegments = parameters.latheSegments;
    var capmode = parameters.capMode;

    bool capEnd = capmode == LatheCapMode.Top || capmode == LatheCapMode.Both;
    bool capStart = capmode == LatheCapMode.Bottom || capmode == LatheCapMode.Both;

    int vertexCount = profileCount * (lathSegments + 1);
    if (capEnd)
      vertexCount += 1 + lathSegments;
    if (capStart)
      vertexCount += 1 + lathSegments;

    Vector3[] vertices = new Vector3[vertexCount];
    Vector3[] normals = new Vector3[vertexCount];
    Vector2[] uvs = new Vector2[vertexCount];

    int vOffset = 0;

    // Generate vertices
    for (int i = 0; i <= lathSegments; i++)
    {
      float t = i / (float)lathSegments;

      for (int j = 0; j < profileCount; j++)
      {
        var p = profile[j];
        var n = profileNormals[j];
        var q = Quaternion.AngleAxis(t * 360f, Vector3.up);

        int index = vOffset + j;
        vertices[index] = q * p;
        normals[index] = (q * n).normalized;
        uvs[index] = new Vector2(t, j / (float)(profileCount - 1));
      }

      vOffset += profileCount;
    }

    // Caps

    if (capEnd)
    {
      int topCenterIndex = vOffset;
      vertices[topCenterIndex] = new(0f, profile[profileCount - 1].y, 0f);
      normals[topCenterIndex] = Vector3.up;
      uvs[topCenterIndex] = new Vector2(0.5f, 1f);
      vOffset += 1;

      for (int i = 0; i < lathSegments; i++)
      {
        float t = i / (float)lathSegments;
        var q = Quaternion.AngleAxis(t * 360f, Vector3.up);

        int index = vOffset + i;
        vertices[index] = q * profile[profileCount - 1];
        normals[index] = Vector3.up;
        uvs[index] = new Vector2(t, 1f);
      }
      vOffset += lathSegments;
    }

    if (capStart)
    {
      int bottomCenterIndex = vOffset;
      vertices[bottomCenterIndex] = new(0f, profile[0].y, 0f);
      normals[bottomCenterIndex] = Vector3.down;
      uvs[bottomCenterIndex] = new Vector2(0.5f, 0f);
      vOffset += 1;

      for (int i = 0; i < lathSegments; i++)
      {
        float t = i / (float)lathSegments;
        var q = Quaternion.AngleAxis(t * 360f, Vector3.up);

        int index = vOffset + i;
        vertices[index] = q * profile[0];
        normals[index] = Vector3.down;
        uvs[index] = new Vector2(t, 0f);
      }
      vOffset += lathSegments;
    }

    // Generate indices
    int quadCount = lathSegments * (profileCount - 1);
    int indexCount = quadCount * 6;
    if (capEnd)
      indexCount += lathSegments * 3;
    if (capStart)
      indexCount += lathSegments * 3;
    int[] indices = new int[indexCount];
    int indexOffset = 0;

    // Side quads
    for (int i = 0; i < lathSegments; i++)
    {
      for (int j = 0; j < profileCount - 1; j++)
      {
        int v0 = i * profileCount + j;
        int v1 = v0 + profileCount;
        int v2 = v0 + 1;
        int v3 = v1 + 1;

        indices[indexOffset++] = v0;
        indices[indexOffset++] = v1;
        indices[indexOffset++] = v2;

        indices[indexOffset++] = v1;
        indices[indexOffset++] = v3;
        indices[indexOffset++] = v2;
      }
    }

    // Caps
    if (capEnd)
    {
      int topCenterIndex = vertexCount - (capStart ? (1 + lathSegments) : 0) - (1 + lathSegments);
      int topStartIndex = topCenterIndex + 1;

      for (int i = 0; i < lathSegments; i++)
      {
        int v0 = topCenterIndex;
        int v1 = topStartIndex + i;
        int v2 = topStartIndex + ((i + 1) % lathSegments);

        indices[indexOffset++] = v0;
        indices[indexOffset++] = v1;
        indices[indexOffset++] = v2;
      }
    }

    if (capStart)
    {
      int bottomCenterIndex = vertexCount - (1 + lathSegments);
      int bottomStartIndex = bottomCenterIndex + 1;

      for (int i = 0; i < lathSegments; i++)
      {
        int v0 = bottomCenterIndex;
        int v1 = bottomStartIndex + ((i + 1) % lathSegments);
        int v2 = bottomStartIndex + i;

        indices[indexOffset++] = v0;
        indices[indexOffset++] = v1;
        indices[indexOffset++] = v2;
      }
    }


    // Create mesh
    Mesh mesh = new()
    {
      vertices = vertices,
      normals = normals,
      uv = uvs,
      triangles = indices
    };

    return mesh;
  }
}