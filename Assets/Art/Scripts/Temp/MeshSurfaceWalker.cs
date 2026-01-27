using UnityEngine;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MeshSurfaceWalker : MonoBehaviour
{
    static Vector3 UV2Barycentric(Vector2 uv) =>
        new(uv.x, uv.y, 1.0f - uv.x - uv.y);

    public int triangleIndex = 0;
    public Vector2 uv = new(0.33f, 0.33f);
    public Vector2 moveAmount = new(1.0f, 0.0f);

    readonly MeshTools.SurfaceState state0 = new(), state1 = new();

    bool GetMeshFilterOrReturn(out MeshFilter meshFilter)
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshSurfaceWalker requires a MeshFilter component.");
            return false;
        }
        return true;
    }

    void Update()
    {
        if (GetMeshFilterOrReturn(out var meshFilter) == false)
            return;

        var mesh = meshFilter.sharedMesh;
        state0.Set(mesh, triangleIndex, uv);
        state1.Copy(state0).Move(moveAmount);
    }

    void OnDrawGizmos()
    {
        GetMeshFilterOrReturn(out var meshFilter);
        var mesh = meshFilter.sharedMesh;
        var transform = meshFilter.transform;

        var i0 = mesh.triangles[triangleIndex * 3 + 0];
        var i1 = mesh.triangles[triangleIndex * 3 + 1];
        var i2 = mesh.triangles[triangleIndex * 3 + 2];
        var v0 = transform.TransformPoint(mesh.vertices[i0]);
        var v1 = transform.TransformPoint(mesh.vertices[i1]);
        var v2 = transform.TransformPoint(mesh.vertices[i2]);
        var u = v1 - v0;
        var v = v2 - v0;
        var n = Vector3.Cross(u, v).normalized;
        var pointOnSurface = v0 + uv.x * u + uv.y * v;

        GizmosUtils.DrawMeshTriangle(meshFilter, triangleIndex);

        pointOnSurface += n * GizmosUtils.NORMAL_OFFSET;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(pointOnSurface, pointOnSurface + n * 0.2f);
        GizmosUtils.DrawCircle(pointOnSurface, n, 0.05f);

        var moveWorld = u * moveAmount.x + v * moveAmount.y;
        GizmosUtils.DrawWireArrow(pointOnSurface, pointOnSurface + moveWorld);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.TransformPoint(state1.GetPosition()), 0.02f);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MeshSurfaceWalker))]
    public class MeshSurfaceWalkerEditor : Editor
    {
        bool pointerRaycast = false;

        MeshSurfaceWalker Target => (MeshSurfaceWalker)target;

        public override void OnInspectorGUI()
        {
            // DrawDefaultInspector();

            Target.uv = EditorGUILayout.Vector2Field(
                "UV Coordinates",
                Target.uv);

            Target.moveAmount = EditorGUILayout.Vector2Field(
                "Move Amount (UV space)",
                Target.moveAmount);

            float moveMagnitude = Target.moveAmount.magnitude;
            EditorGUI.BeginChangeCheck();
            float newMoveMagnitude = EditorGUILayout.FloatField(
                "Move Magnitude",
                moveMagnitude);
            if (EditorGUI.EndChangeCheck())
            {
                if (moveMagnitude > 0f)
                {
                    Target.moveAmount *= newMoveMagnitude / moveMagnitude;
                }
                else
                {
                    Target.moveAmount = Target.moveAmount.normalized * newMoveMagnitude;
                }
                EditorUtility.SetDirty(Target);
            }

            float moveAngle = Mathf.Atan2(Target.moveAmount.y, Target.moveAmount.x) * Mathf.Rad2Deg;
            EditorGUI.BeginChangeCheck();
            float newMoveAngle = EditorGUILayout.Slider(
                "Move Angle (degrees)",
                moveAngle,
                -180f,
                180f);
            if (EditorGUI.EndChangeCheck())
            {
                float angleRad = newMoveAngle * Mathf.Deg2Rad;
                float magnitude = Target.moveAmount.magnitude;
                Target.moveAmount = new Vector2(
                    Mathf.Cos(angleRad),
                    Mathf.Sin(angleRad)) * magnitude;
                EditorUtility.SetDirty(Target);
            }

            GUILayout.Space(10);

            if (Target.TryGetComponent<MeshFilter>(out var meshFilter) == false
                || meshFilter.sharedMesh == null)
            {
                EditorGUILayout.HelpBox(
                    "MeshSurfaceWalker requires a MeshFilter with a valid mesh.",
                    MessageType.Error);
                return;
            }

            var mesh = meshFilter.sharedMesh;

            var maxTriangleIndex = mesh.triangles.Length / 3 - 1;
            Target.triangleIndex = EditorGUILayout.IntSlider(
                "Triangle Index",
                Target.triangleIndex,
                0, maxTriangleIndex);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous Triangle"))
            {
                Target.triangleIndex =
                    (Target.triangleIndex - 1 + (maxTriangleIndex + 1)) % (maxTriangleIndex + 1);
            }
            if (GUILayout.Button("Next Triangle"))
            {
                Target.triangleIndex =
                    (Target.triangleIndex + 1) % (maxTriangleIndex + 1);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                $"Current UV: {UV2Barycentric(Target.uv):F3}"
                + $"\nNew UV after move: {UV2Barycentric(Target.uv + Target.moveAmount):F3}",
                MessageType.None);

            if (pointerRaycast)
                GUI.backgroundColor = Color.red;
            if (GUILayout.Button($"{(pointerRaycast ? "🔴" : "⏺️")} Pointer Raycast"))
                pointerRaycast = !pointerRaycast;
        }

        void OnSceneGUI()
        {
            var e = Event.current;

            if (pointerRaycast && e.type == EventType.MouseMove)
                ComputeRaycast();

            if (pointerRaycast && e.type == EventType.MouseDown)
            {
                ComputeRaycast();
                e.Use();
                pointerRaycast = false;
            }
        }

        void ComputeRaycast()
        {
            var e = Event.current;
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            var meshFilter = ((MeshSurfaceWalker)target).GetComponent<MeshFilter>();
            var mesh = meshFilter.sharedMesh;
            var transform = meshFilter.transform;
            if (MeshTools.RaycastMesh(ray, mesh, transform, out var hitInfo))
            {
                Target.triangleIndex = hitInfo.triangleIndex;
                Target.uv = hitInfo.uv;

                EditorUtility.SetDirty(Target);
            }
        }
    }
#endif
}