using UnityEngine;

public static class GizmosUtils
{
    public static void DrawPolyline(Vector3[] points, bool closed = false)
    {
        if (points.Length < 2)
            return;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }

        if (closed)
        {
            Gizmos.DrawLine(points[points.Length - 1], points[0]);
        }
    }

    public static void DrawRegularPolygon(Vector3 center, Vector3 normal, Vector3 right, float radius, int sides)
    {
        Vector3[] points = new Vector3[sides];
        var up = Vector3.Cross(normal, right).normalized;
        for (int i = 0; i < sides; i++)
        {
            float angle = i * 2 * Mathf.PI / sides;
            points[i] = center + radius * (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up);
        }

        DrawPolyline(points, closed: true);
    }

    public static void DrawCircle(Vector3 center, Vector3 normal, float radius, int segments = 36)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
        Vector3 previousPoint = center + rotation * new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            Vector3 nextPoint = center + rotation * new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }

    public static void DrawCircle(Vector3 center, float radius, int segments = 36) =>
        DrawCircle(center, Vector3.forward, radius, segments);

    public static void DrawCircle(float radius, int segments = 36) =>
        DrawCircle(Vector3.zero, Vector3.forward, radius, segments);

    public static void DrawCircle(Transform transform, float radius, int segments = 36)
    {
        var previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        DrawCircle(Vector3.zero, Vector3.forward, radius, segments);
        Gizmos.matrix = previousMatrix;
    }
}
