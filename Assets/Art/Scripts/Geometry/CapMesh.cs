using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CapMesh : MonoBehaviour
{
    public MeshUtils.LatheParameters parameters = MeshUtils.LatheParameters.GetDefault();

    void OnValidate()
    {
        var mesh = MeshUtils.LatheMesh(parameters);
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
