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

    public static void DrawWireArrow(
        Vector3 from,
        Vector3 to)
    {
        Gizmos.DrawLine(from, to);

        var move = to - from;
        var dir = move.normalized;
        var v = Vector3.Cross(dir, Vector3.up);
        if (v.sqrMagnitude < 0.001f)
            v = Vector3.Cross(dir, Vector3.right);
        v.Normalize();
        var u = Vector3.Cross(dir, v).normalized;

        const float BODY_RATIO = 0.9f;
        const float HEAD_WIDTH = 0.025f;

        var r = HEAD_WIDTH * move.magnitude;
        DrawCircle(from + move * BODY_RATIO, dir, r, 12);

        for (int i = 0; i < 12; i++)
        {
            float angle = i * 2 * Mathf.PI / 12;
            Vector3 circlePoint = from + move * BODY_RATIO + r * (Mathf.Cos(angle) * v + Mathf.Sin(angle) * u);
            Gizmos.DrawLine(to, circlePoint);
        }
    }

    public const float NORMAL_OFFSET = 0.002f;

    public static void DrawMeshTriangle(
        Vector3 A,
        Vector3 B,
        Vector3 C
    )
    {
        const float VERTICE_PADDING = 0.2f;
        const float NORMAL_INSET = -0.025f;

        var RED = new Color(1f, 0.0f, 0.1f);
        var GREEN = new Color(0f, 0.8f, 0.1f);
        var BLUE = new Color(0f, 0.2f, 1f);

        var AB = B - A;
        var AC = C - A;
        var BC = C - B;

        var cross_AB_AC = Vector3.Cross(AB, AC);
        var n = cross_AB_AC.normalized;
        var noff = n * NORMAL_OFFSET;

        var size = (AB.magnitude + AC.magnitude + BC.magnitude) / 3f;

        var nAB = NORMAL_INSET * size * Vector3.Cross(AB, n).normalized;
        var arrowStartAB = A + AB * VERTICE_PADDING + nAB;
        var arrowEndAB = A + AB * (1f - VERTICE_PADDING) + nAB;
        var basisArrowABStart = A + AB * VERTICE_PADDING - nAB;
        var basisArrowABEnd = A + AB * (1f - VERTICE_PADDING) - nAB;

        var nBC = NORMAL_INSET * size * Vector3.Cross(BC, n).normalized;
        var arrowStartBC = B + BC * VERTICE_PADDING + nBC;
        var arrowEndBC = B + BC * (1f - VERTICE_PADDING) + nBC;

        var nCA = NORMAL_INSET * size * Vector3.Cross(-AC, n).normalized;
        var arrowStartCA = C - AC * VERTICE_PADDING + nCA;
        var arrowEndCA = C - AC * (1f - VERTICE_PADDING) + nCA;
        var basisArrowACStart = A + AC * VERTICE_PADDING - nCA;
        var basisArrowACEnd = A + AC * (1f - VERTICE_PADDING) - nCA;

        // Computation done, now draw
        // Shift by "noff"
        A += noff;
        B += noff;
        C += noff;
        arrowStartAB += noff;
        arrowEndAB += noff;
        arrowStartBC += noff;
        arrowEndBC += noff;
        arrowStartCA += noff;
        arrowEndCA += noff;
        basisArrowABStart += noff;
        basisArrowABEnd += noff;
        basisArrowACStart += noff;
        basisArrowACEnd += noff;

        Gizmos.color = Color.yellow;
        DrawWireArrow(basisArrowABStart, basisArrowABEnd);
        DrawWireArrow(basisArrowACStart, basisArrowACEnd);
        DrawWireArrow(A, A + n * size);

        Gizmos.color = RED;
        Gizmos.DrawLine(A, B);
        DrawWireArrow(arrowStartAB, arrowEndAB);
        Gizmos.DrawSphere(A, 0.01f);

        Gizmos.color = GREEN;
        Gizmos.DrawLine(B, C);
        DrawWireArrow(arrowStartBC, arrowEndBC);
        Gizmos.DrawSphere(B, 0.01f);

        Gizmos.color = BLUE;
        Gizmos.DrawLine(C, A);
        DrawWireArrow(arrowStartCA, arrowEndCA);
        Gizmos.DrawSphere(C, 0.01f);
    }

    public static void DrawMeshTriangle(
        MeshFilter meshFilter,
        int triangleIndex
    )
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Mesh mesh = meshFilter.sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        if (triangleIndex < 0 || triangleIndex >= triangles.Length / 3)
            return;

        int i0 = triangles[triangleIndex * 3 + 0];
        int i1 = triangles[triangleIndex * 3 + 1];
        int i2 = triangles[triangleIndex * 3 + 2];

        Vector3 v0 = meshFilter.transform.TransformPoint(vertices[i0]);
        Vector3 v1 = meshFilter.transform.TransformPoint(vertices[i1]);
        Vector3 v2 = meshFilter.transform.TransformPoint(vertices[i2]);

        DrawMeshTriangle(v0, v1, v2);
    }
}
