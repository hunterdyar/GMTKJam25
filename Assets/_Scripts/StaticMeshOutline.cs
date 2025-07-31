using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class StaticMeshOutline : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineScale = 1.03f;

    private GameObject outlineObject;

    void Start()
    {
        // Create outline mesh
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform, false);

        MeshFilter originalFilter = GetComponent<MeshFilter>();
        MeshRenderer originalRenderer = GetComponent<MeshRenderer>();

        MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
        MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();

        outlineFilter.sharedMesh = originalFilter.sharedMesh;
        outlineRenderer.material = outlineMaterial;

        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * outlineScale;
    }
}
