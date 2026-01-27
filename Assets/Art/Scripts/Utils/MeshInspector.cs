using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MeshInspector : MonoBehaviour
{
    public enum MeshInspectorMode { Triangle, Vertex }

    public MeshInspectorMode mode = MeshInspectorMode.Triangle;

    public int triangleIndex = 0;
    public int vertexIndex = 0;

    public float normalOffset = 0.001f;

    bool pointerRaycast = false;

    MeshFilter meshFilter;

    (int, int, int) GetTriangleVertexIndices(int triangleIdx)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return (-1, -1, -1);

        Mesh mesh = meshFilter.sharedMesh;
        int[] triangles = mesh.triangles;

        if (triangleIdx < 0 || triangleIdx >= triangles.Length / 3)
            return (-1, -1, -1);

        int i0 = triangles[triangleIdx * 3 + 0];
        int i1 = triangles[triangleIdx * 3 + 1];
        int i2 = triangles[triangleIdx * 3 + 2];

        return (i0, i1, i2);
    }

    (int, int, int) GetTriangleVertexIndices() =>
        GetTriangleVertexIndices(triangleIndex);

    (Vector3, Vector3, Vector3) GetTriangleVertices(int triangleIdx)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return (Vector3.zero, Vector3.zero, Vector3.zero);

        Vector3[] vertices = meshFilter.sharedMesh.vertices;

        var (i0, i1, i2) = GetTriangleVertexIndices(triangleIdx);

        return (vertices[i0], vertices[i1], vertices[i2]);
    }

    (Vector3, Vector3, Vector3) GetTriangleVertices() =>
        GetTriangleVertices(triangleIndex);

    void OnValidate()
    {
        if (meshFilter == null)
            TryGetComponent(out meshFilter);
    }

    void GizmosTriangle()
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Mesh mesh = meshFilter.sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        if (triangleIndex < 0 || triangleIndex >= triangles.Length / 3)
            return;

        var (i0, i1, i2) = GetTriangleVertexIndices(triangleIndex);

        Vector3 v0 = meshFilter.transform.TransformPoint(vertices[i0]);
        Vector3 v1 = meshFilter.transform.TransformPoint(vertices[i1]);
        Vector3 v2 = meshFilter.transform.TransformPoint(vertices[i2]);

        Vector3 n0 = meshFilter.transform.TransformDirection(normals[i0]);
        Vector3 n1 = meshFilter.transform.TransformDirection(normals[i1]);
        Vector3 n2 = meshFilter.transform.TransformDirection(normals[i2]);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(v0 + normalOffset * n0, v1 + normalOffset * n1);
        Gizmos.DrawLine(v1 + normalOffset * n1, v2 + normalOffset * n2);
        Gizmos.DrawLine(v2 + normalOffset * n2, v0 + normalOffset * n0);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(v0, v0 + n0 * 0.2f);
        Gizmos.DrawLine(v1, v1 + n1 * 0.2f);
        Gizmos.DrawLine(v2, v2 + n2 * 0.2f);
    }

    void GizmosVertex()
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        if (vertexIndex < 0 || vertexIndex >= vertices.Length)
            return;

        Vector3 v = meshFilter.transform.TransformPoint(vertices[vertexIndex]);
        Vector3 n = meshFilter.transform.TransformDirection(normals[vertexIndex]);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(v, 0.01f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(v, v + n * 0.2f);
    }

    void OnDrawGizmosSelected()
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        switch (mode)
        {
            case MeshInspectorMode.Triangle:
                GizmosTriangle();
                break;
            case MeshInspectorMode.Vertex:
                GizmosVertex();
                break;
        }
    }

    void Update()
    {
        if (Pointer.current.press.wasPressedThisFrame)
        {
            Debug.Log("Pointer pressed");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MeshInspector))]
    public class MeshInspectorEditor : Editor
    {
        void OnSceneGUI()
        {
            var inspector = (MeshInspector)target;
            var e = Event.current;

            void DoRaycast()
            {
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                var mesh = inspector.meshFilter.sharedMesh;
                var transform = inspector.meshFilter.transform;
                if (MeshTools.RaycastMesh(ray, mesh, transform, out var hitInfo))
                {
                    inspector.triangleIndex = hitInfo.triangleIndex;

                    var (p0, p1, p2) = inspector.GetTriangleVertices();
                    float d0 = Vector3.Distance(hitInfo.localPoint, p0);
                    float d1 = Vector3.Distance(hitInfo.localPoint, p1);
                    float d2 = Vector3.Distance(hitInfo.localPoint, p2);
                    var (i0, i1, i2) = inspector.GetTriangleVertexIndices();
                    if (d0 <= d1 && d0 <= d2)
                        inspector.vertexIndex = i0;
                    else if (d1 <= d0 && d1 <= d2)
                        inspector.vertexIndex = i1;
                    else
                        inspector.vertexIndex = i2;

                    EditorUtility.SetDirty(inspector);
                }
            }

            if (inspector.pointerRaycast)
            {
                if (e.type == EventType.MouseMove)
                {
                    DoRaycast();
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    DoRaycast();
                    e.Use();
                    inspector.pointerRaycast = false;
                }
            }

        }

        void TriangleMode(MeshInspector inspector, Mesh mesh)
        {
            var triangleCount = mesh.triangles.Length / 3;
            var (i0, i1, i2) = inspector.GetTriangleVertexIndices();
            EditorGUILayout.HelpBox(
                $"Current Triangle Indices: {i0}, {i1}, {i2}"
                , MessageType.Info);

            inspector.triangleIndex = EditorGUILayout.IntSlider("Triangle Index", inspector.triangleIndex, 0, Mathf.Max(0, triangleCount - 1));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous Triangle"))
            {
                inspector.triangleIndex = Mathf.Max(0, inspector.triangleIndex - 1);
            }
            if (GUILayout.Button("Next Triangle"))
            {
                inspector.triangleIndex = Mathf.Min(triangleCount - 1, inspector.triangleIndex + 1);
            }
            GUILayout.EndHorizontal();
        }

        void VertexMode(MeshInspector inspector, Mesh mesh)
        {
            var vertexCount = mesh.vertexCount;
            EditorGUILayout.HelpBox(
                $"Current Vertex Index: {inspector.vertexIndex}"
                , MessageType.Info);

            inspector.vertexIndex = EditorGUILayout.IntSlider("Vertex Index", inspector.vertexIndex, 0, Mathf.Max(0, vertexCount - 1));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous Vertex"))
            {
                inspector.vertexIndex = Mathf.Max(0, inspector.vertexIndex - 1);
            }
            if (GUILayout.Button("Next Vertex"))
            {
                inspector.vertexIndex = Mathf.Min(vertexCount - 1, inspector.vertexIndex + 1);
            }
            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            MeshInspector inspector = (MeshInspector)target;

            if (inspector.meshFilter == null || inspector.meshFilter.sharedMesh == null)
                return;

            Mesh mesh = inspector.meshFilter.sharedMesh;

            var triangleCount = mesh.triangles.Length / 3;
            EditorGUILayout.HelpBox($"Mesh: {mesh.name}"
                + $"\nVertices: {mesh.vertexCount}"
                + $"\nTriangles: {triangleCount}"
                , MessageType.Info);

            inspector.mode = (MeshInspectorMode)EditorGUILayout.EnumPopup("Mode", inspector.mode);

            switch (inspector.mode)
            {
                case MeshInspectorMode.Triangle:
                    TriangleMode(inspector, mesh);
                    break;
                case MeshInspectorMode.Vertex:
                    VertexMode(inspector, mesh);
                    break;
            }

            if (inspector.pointerRaycast)
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button($"{(inspector.pointerRaycast ? "🔴" : "⚠️")} Pointer Raycast"))
            {
                inspector.pointerRaycast = !inspector.pointerRaycast;
            }
        }
    }
#endif
}
