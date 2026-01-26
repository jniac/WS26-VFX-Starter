using UnityEngine;

public class TrackPoint : MonoBehaviour
{
    public Track.TrackPointParameters parameters = Track.TrackPointParameters.Default;

    public TrackPoint[] nextPoints;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        GizmosUtils.DrawCircle(transform, radius: parameters.radius);

        foreach (var nextPoint in nextPoints)
        {
            if (nextPoint != null)
            {
                Vector3 v = nextPoint.transform.position - transform.position;
                Track.CubicBezierSegment segment = new()
                {
                    p0 = transform.position,
                    p1 = transform.position + transform.forward * (v.magnitude / 3f),
                    p2 = nextPoint.transform.position - nextPoint.transform.forward * (v.magnitude / 3f),
                    p3 = nextPoint.transform.position
                };
                Gizmos.DrawSphere(segment.p1, 0.1f);
                Gizmos.DrawSphere(segment.p2, 0.1f);
                for (int i = 0; i < 20; i++)
                {
                    float t0 = i / 20f;
                    float t1 = (i + 1) / 20f;
                    Gizmos.DrawLine(segment.GetPoint(t0), segment.GetPoint(t1));
                }
            }
        }
    }
}
