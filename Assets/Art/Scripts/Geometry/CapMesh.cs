using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CapMesh : MonoBehaviour
{
    public MeshTools.LatheParameters parameters = MeshTools.LatheParameters.GetDefault();

    void OnValidate()
    {
        var mesh = MeshTools.LatheMesh(parameters);
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
