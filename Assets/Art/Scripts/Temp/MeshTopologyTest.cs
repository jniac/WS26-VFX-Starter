using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshTopologyTest : MonoBehaviour
{
    void OnValidate()
    {
        var mesh = new Mesh
        {
            name = "MeshTopologyTest_Mesh",
            vertices = new Vector3[]
            {
                new(0, 0, 0),
                new(1, 0, 0),
                new(0, 1, 0),
                new(1, 1, 0.5f),

                new(-1, 0, 0),
                new(-1, 1, 0),
            },
            triangles = new int[]
            {
                0, 2, 1,
                1, 2, 3,

                0, 4, 2,
                4, 5, 2,
            }
        };

        GetComponent<MeshFilter>().sharedMesh = MeshTools.ToNonIndexed(mesh, MeshTools.ToNonIndexedNormalsOption.Recalculate);
    }
}
