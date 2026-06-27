using UnityEngine;
using System.Linq;

/// <summary>
/// Fix outline breaks by smoothing normals and storing them in vertex colors
/// Usage: Attach this script to your character model GameObject
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class NPR_NormalsSmoother : MonoBehaviour
{
    public enum VertexNormalColorFormat
    {
        YSADirect,
        EncodedZeroToOne
    }

    [Header("Settings")]
    [Tooltip("If true, smoothed normals will be stored in vertex colors")]
    public bool storeInVertexColors = true;

    [Tooltip("YSA Direct stores normal.xyz directly in vertex RGB. Encoded 0-1 keeps the older remapped format.")]
    public VertexNormalColorFormat vertexColorFormat = VertexNormalColorFormat.YSADirect;

    [Tooltip("If true, will show debug info in console")]
    public bool debugMode = false;

    [Header("Info (Read Only)")]
    [SerializeField] private int originalVertexCount = 0;
    [SerializeField] private int uniquePositionCount = 0;
    [SerializeField] private bool isProcessed = false;

    void Start()
    {
        if (!isProcessed)
        {
            ProcessMesh();
        }
    }

    public void ProcessMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("NPR_NormalsSmoother: MeshFilter not found!");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        if (mesh == null)
        {
            Debug.LogError("NPR_NormalsSmoother: Mesh is null!");
            return;
        }

        originalVertexCount = mesh.vertexCount;

        var vertexGroups = mesh.vertices
            .Select((position, index) => new { Position = position, Index = index })
            .GroupBy(v => v.Position)
            .Select(group => new
            {
                Position = group.Key,
                Indices = group.Select(v => v.Index).ToList()
            })
            .ToList();

        uniquePositionCount = vertexGroups.Count;

        if (debugMode)
        {
            Debug.Log($"NPR_NormalsSmoother: Processing mesh '{mesh.name}'");
            Debug.Log($"  Original vertices: {originalVertexCount}");
            Debug.Log($"  Unique positions: {uniquePositionCount}");
            Debug.Log($"  Duplicate vertices: {originalVertexCount - uniquePositionCount}");
        }

        Color[] smoothedNormals = new Color[mesh.vertexCount];

        foreach (var group in vertexGroups)
        {
            Vector3 averagedNormal = Vector3.zero;

            foreach (int index in group.Indices)
            {
                averagedNormal += mesh.normals[index];
            }

            averagedNormal = averagedNormal.normalized;

            foreach (int index in group.Indices)
            {
                smoothedNormals[index] = EncodeNormalColor(averagedNormal);
            }
        }

        if (storeInVertexColors)
        {
            mesh.colors = smoothedNormals;

            if (debugMode)
            {
                Debug.Log($"NPR_NormalsSmoother: Smoothed normals stored in vertex colors");
            }
        }

        isProcessed = true;

        if (debugMode)
        {
            Debug.Log($"NPR_NormalsSmoother: Processing complete!");
        }
    }

    private Color EncodeNormalColor(Vector3 normal)
    {
        if (vertexColorFormat == VertexNormalColorFormat.EncodedZeroToOne)
        {
            return new Color(
                normal.x * 0.5f + 0.5f,
                normal.y * 0.5f + 0.5f,
                normal.z * 0.5f + 0.5f,
                1f
            );
        }

        return new Color(normal.x, normal.y, normal.z, 1f);
    }

    [ContextMenu("Process Mesh Now")]
    public void ProcessMeshManually()
    {
        ProcessMesh();
    }

    [ContextMenu("Reset Vertex Colors")]
    public void ResetVertexColors()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null)
        {
            Mesh mesh = meshFilter.mesh;
            Color[] colors = new Color[mesh.vertexCount];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            mesh.colors = colors;
            isProcessed = false;

            if (debugMode)
            {
                Debug.Log("NPR_NormalsSmoother: Vertex colors reset to white");
            }
        }
    }
}
