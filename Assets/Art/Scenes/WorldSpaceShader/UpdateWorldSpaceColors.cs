using UnityEngine;

public class UpdateWorldSpaceColors : MonoBehaviour
{
    public Transform spot4Transform;
    public Color spot4Color = Color.blue;
    public float spot4RangeStart = 0f;
    public float spot4RangeEnd = 1f;
    [Range(0f, 20f)]
    public float spot4Intensity = 20f;

    void Update()
    {
        if (TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            meshRenderer.material.SetColor("_Spot4_Color", spot4Color);
            meshRenderer.material.SetVector("_Spot4_Position", spot4Transform.position);
            meshRenderer.material.SetVector("_Spot4_Props", new Vector4(spot4RangeStart, spot4RangeEnd, spot4Intensity, 0));
        }
    }
}
