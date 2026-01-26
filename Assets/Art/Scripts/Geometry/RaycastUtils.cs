using UnityEngine;

public static class RaycastUtils
{
  public static bool RayIntersectsTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 hitPoint, out float distance)
  {
    hitPoint = Vector3.zero;
    distance = 0f;

    Vector3 edge1 = v1 - v0;
    Vector3 edge2 = v2 - v0;
    Vector3 h = Vector3.Cross(ray.direction, edge2);
    float a = Vector3.Dot(edge1, h);
    if (Mathf.Abs(a) < Mathf.Epsilon) return false; // Ray is parallel

    float f = 1f / a;
    Vector3 s = ray.origin - v0;
    float u = f * Vector3.Dot(s, h);
    if (u < 0f || u > 1f) return false;

    Vector3 q = Vector3.Cross(s, edge1);
    float v = f * Vector3.Dot(ray.direction, q);
    if (v < 0f || u + v > 1f) return false;

    distance = f * Vector3.Dot(edge2, q);
    if (distance < 0f) return false; // Triangle behind

    hitPoint = ray.origin + ray.direction * distance;
    return true;
  }

  public struct RaycastHit
  {
    public Vector3 localPoint;
    public Vector3 worldPoint;
    public Vector3 normal;
    public Vector3 uv;
    public float distance;
    public int triangleIndex;
  }

  public static bool RaycastMesh(Ray worldRay, Mesh mesh, Transform meshTransform, out RaycastHit hitInfo)
  {
    hitInfo = new RaycastHit();

    // Transform the ray into the mesh's local space
    var localRay = new Ray(
        meshTransform.InverseTransformPoint(worldRay.origin),
        meshTransform.InverseTransformDirection(worldRay.direction)
    );

    float closestDist = float.MaxValue;
    bool hit = false;

    if (mesh.bounds.IntersectRay(localRay) == false)
      return false;

    var vertices = mesh.vertices;
    var triangles = mesh.triangles;

    for (int i = 0; i < triangles.Length; i += 3)
    {
      Vector3 a = vertices[triangles[i]];
      Vector3 b = vertices[triangles[i + 1]];
      Vector3 c = vertices[triangles[i + 2]];

      if (RayIntersectsTriangle(localRay, a, b, c, out var point, out var dist) && dist < closestDist)
      {
        closestDist = dist;
        hitInfo.triangleIndex = i / 3;
        hitInfo.localPoint = point;
        hitInfo.worldPoint = meshTransform.TransformPoint(point);
        hitInfo.distance = Vector3.Distance(worldRay.origin, hitInfo.worldPoint);
        hitInfo.normal = meshTransform.TransformDirection(Vector3.Cross(b - a, c - a).normalized);
        hit = true;
      }
    }

    return hit;
  }
}