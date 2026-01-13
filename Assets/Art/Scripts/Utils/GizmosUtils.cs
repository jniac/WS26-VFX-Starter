using UnityEngine;

public static class GizmosUtils
{
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
}
