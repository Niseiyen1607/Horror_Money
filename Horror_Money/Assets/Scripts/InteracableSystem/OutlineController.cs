using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class OutlineController : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material[] originalMaterials;
    private bool isOutlineActive = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterials = meshRenderer.materials;
        DisableOutline(); 
    }

    public void EnableOutline()
    {
        if (isOutlineActive) return;

        Material[] mats = meshRenderer.materials;
        if (mats.Length > 1)
        {
            mats[1].SetFloat("_Outline_Thickness", 1.05f);
        }
        meshRenderer.materials = mats;
        isOutlineActive = true;
    }

    public void DisableOutline()
    {
        if (!isOutlineActive) return;

        Material[] mats = meshRenderer.materials;
        if (mats.Length > 1)
        {
            mats[1].SetFloat("_Outline_Thickness", 0f); 
        }
        meshRenderer.materials = mats;
        isOutlineActive = false;
    }
}
