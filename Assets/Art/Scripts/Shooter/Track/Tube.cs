using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteAlways]
public class Tube : MonoBehaviour
{
    void OnValidate()
    {
        var trackPoints = FindObjectsByType<TrackPoint>(FindObjectsSortMode.None);

        var combineInstances = new List<CombineInstance>();
        foreach (var trackPoint in trackPoints)
        {
            if (trackPoint.nextPoints != null)
            {
                for (int i = 0; i < trackPoint.nextPoints.Length; i++)
                {
                    var end = trackPoint.nextPoints[i];
                    if (end != null)
                    {
                        var mesh = new Mesh();
                        Track.SetTubeMesh(
                            mesh,
                            trackPoint.transform.localToWorldMatrix,
                            end.transform.localToWorldMatrix,
                            trackPoint.parameters,
                            end.parameters
                        );
                        combineInstances.Add(new CombineInstance
                        {
                            mesh = mesh,
                            transform = Matrix4x4.identity
                        });
                    }
                }
            }
        }

        var finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        finalMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        finalMesh.name = "Track Combined Tube Mesh";
        GetComponent<MeshFilter>().sharedMesh = finalMesh;
    }
}
