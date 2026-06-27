using UnityEngine;

public class NPR_OutlineVertexColorNormalsBootstrap : MonoBehaviour
{
    [SerializeField] private string targetShaderName = "Custom/NPR/Toon_Character_ImprovedOutline";
    [SerializeField] private NPR_NormalsSmoother.VertexNormalColorFormat vertexColorFormat = NPR_NormalsSmoother.VertexNormalColorFormat.YSADirect;
    [SerializeField] private bool includeInactive;
    [SerializeField] private bool debugMode;

    private void Start()
    {
        ProcessSceneMeshes();
    }

    [ContextMenu("Process Scene Meshes")]
    public void ProcessSceneMeshes()
    {
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>(includeInactive);
        int processedCount = 0;

        foreach (MeshRenderer meshRenderer in renderers)
        {
            if (!UsesTargetShader(meshRenderer))
            {
                continue;
            }

            MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                continue;
            }

            NPR_NormalsSmoother smoother = meshRenderer.GetComponent<NPR_NormalsSmoother>();
            if (smoother == null)
            {
                smoother = meshRenderer.gameObject.AddComponent<NPR_NormalsSmoother>();
            }

            smoother.storeInVertexColors = true;
            smoother.vertexColorFormat = vertexColorFormat;
            smoother.debugMode = debugMode;
            smoother.ProcessMesh();
            processedCount++;
        }

        if (debugMode)
        {
            Debug.Log($"NPR_OutlineVertexColorNormalsBootstrap: Processed {processedCount} mesh renderer(s).");
        }
    }

    private bool UsesTargetShader(MeshRenderer meshRenderer)
    {
        foreach (Material material in meshRenderer.sharedMaterials)
        {
            if (material != null && material.shader != null && material.shader.name == targetShaderName)
            {
                return true;
            }
        }

        return false;
    }
}
