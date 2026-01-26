using UnityEngine;

[ExecuteAlways]
public class OnHexaGrid : MonoBehaviour
{
    public Transform origin;
    public float radius = 1f;

    [Tooltip("If true, the hex grid is oriented horizontally (pointy top, flat sides, float row). If false, it's vertical (flat top, pointy sides, float column).")]
    public bool horizontal = true;

    void OnDrawGizmosSelected()
    {
        var originPos = origin != null ? origin.position : Vector3.zero;
        var cellCenter = GeometryUtils.GetHexaCellCenter(originPos, radius, transform.position, horizontal);

        Gizmos.color = Color.yellow;
        GizmosUtils.DrawRegularPolygon(originPos, Vector3.forward, Vector3.up, radius, 6);
        GizmosUtils.DrawRegularPolygon(cellCenter, Vector3.forward, Vector3.up, radius, 6);
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!UnityEditor.Selection.Contains(gameObject))
            return;

        var originPos = origin != null ? origin.position : Vector3.zero;
        var cellCenter = GeometryUtils.GetHexaCellCenter(originPos, radius, transform.position, horizontal);
        transform.position = cellCenter;
    }
#endif
}
