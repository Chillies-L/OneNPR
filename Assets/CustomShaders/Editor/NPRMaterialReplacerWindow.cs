using UnityEditor;
using UnityEngine;

public class NPRMaterialReplacerWindow : EditorWindow
{
    private GameObject root;
    private NPRMaterialReplacementProfile profile;
    private bool includeInactive = true;
    private bool createMaterialAssets = true;
    private bool processOutlineNormals = true;
    private string scanSummary = "No root selected.";

    [MenuItem("Tools/NPR/Material Replacer")]
    public static void ShowWindow()
    {
        GetWindow<NPRMaterialReplacerWindow>("NPR Material Replacer");
    }

    private void OnEnable()
    {
        if (root == null && Selection.activeGameObject != null)
        {
            root = Selection.activeGameObject;
        }

        RefreshScanSummary();
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Root Model", EditorStyles.boldLabel);
        root = (GameObject)EditorGUILayout.ObjectField("Root", root, typeof(GameObject), true);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Use Selection"))
            {
                root = Selection.activeGameObject;
                RefreshScanSummary();
            }

            if (GUILayout.Button("Scan"))
            {
                RefreshScanSummary();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Replacement Profile", EditorStyles.boldLabel);
        profile = (NPRMaterialReplacementProfile)EditorGUILayout.ObjectField("Profile", profile, typeof(NPRMaterialReplacementProfile), false);

        if (GUILayout.Button("Create Default Profile Asset"))
        {
            profile = CreateDefaultProfileAsset();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Apply Options", EditorStyles.boldLabel);
        includeInactive = EditorGUILayout.Toggle("Include Inactive Children", includeInactive);
        createMaterialAssets = EditorGUILayout.Toggle("Create Material Assets", createMaterialAssets);
        processOutlineNormals = EditorGUILayout.Toggle("Process MeshFilter Normals", processOutlineNormals);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(scanSummary, MessageType.Info);

        using (new EditorGUI.DisabledScope(root == null || profile == null))
        {
            if (GUILayout.Button("Apply NPR To Root"))
            {
                ApplyReplacement();
            }

            if (GUILayout.Button("Update Existing NPR Materials"))
            {
                UpdateExistingMaterials();
            }
        }
    }

    private void RefreshScanSummary()
    {
        NPRMaterialReplacementScan scan = NPRMaterialReplacementUtility.Scan(root, includeInactive);
        scanSummary = $"Renderers: {scan.RendererCount}\nMaterial slots: {scan.MaterialSlotCount}\nUnique source materials: {scan.UniqueMaterialCount}";
    }

    private void ApplyReplacement()
    {
        NPRMaterialReplacementOptions options = new NPRMaterialReplacementOptions
        {
            IncludeInactive = includeInactive,
            CreateMaterialAssets = createMaterialAssets,
            UseUndo = true,
            ProcessOutlineNormals = processOutlineNormals,
        };

        NPRMaterialReplacementResult result = NPRMaterialReplacementUtility.ReplaceMaterials(root, profile, options);
        RefreshScanSummary();

        string message =
            $"Applied NPR materials to {result.RendererCount} renderer(s).\n" +
            $"Material slots: {result.MaterialSlotCount}\n" +
            $"Created materials: {result.CreatedMaterialCount}\n" +
            $"Reused generated materials: {result.ReusedMaterialCount}\n" +
            $"Explicit replacements: {result.ExplicitReplacementCount}\n" +
            $"Skipped slots: {result.SkippedMaterialCount}\n" +
            $"Processed normals: {result.ProcessedNormalsCount}";

        Debug.Log($"NPRMaterialReplacerWindow: {message}");
        EditorUtility.DisplayDialog("NPR Material Replacer", message, "OK");
    }

    private void UpdateExistingMaterials()
    {
        NPRMaterialManagementOptions options = new NPRMaterialManagementOptions
        {
            IncludeInactive = includeInactive,
            UseUndo = true,
            ProcessOutlineNormals = processOutlineNormals,
        };

        NPRMaterialManagementResult result = NPRMaterialReplacementUtility.UpdateExistingNPRMaterials(root, profile, options);
        RefreshScanSummary();

        string message =
            $"Updated existing NPR materials under {result.RendererCount} renderer(s).\n" +
            $"Material slots: {result.MaterialSlotCount}\n" +
            $"Managed unique NPR materials: {result.ManagedMaterialCount}\n" +
            $"Repeated NPR material slots: {result.ReusedMaterialSlotCount}\n" +
            $"Skipped non-NPR/null slots: {result.SkippedMaterialCount}\n" +
            $"Created materials: {result.CreatedMaterialCount}\n" +
            $"Processed normals: {result.ProcessedNormalsCount}";

        Debug.Log($"NPRMaterialReplacerWindow: {message}");
        EditorUtility.DisplayDialog("NPR Material Replacer", message, "OK");
    }

    private static NPRMaterialReplacementProfile CreateDefaultProfileAsset()
    {
        const string folder = "Assets/CustomShaders/Presets";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/CustomShaders", "Presets");
        }

        NPRMaterialReplacementProfile newProfile = CreateInstance<NPRMaterialReplacementProfile>();
        string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/NPRMaterialReplacementProfile.asset");
        AssetDatabase.CreateAsset(newProfile, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = newProfile;
        EditorGUIUtility.PingObject(newProfile);
        return newProfile;
    }
}
