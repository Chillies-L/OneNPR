using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPRMaterialReplacementProfile))]
public class NPRMaterialReplacementProfileEditor : Editor
{
    private SerializedProperty targetShader;
    private SerializedProperty stylePreset;
    private SerializedProperty fallbackPreset;
    private SerializedProperty outputFolder;
    private SerializedProperty preserveBaseMap;
    private SerializedProperty preserveBaseColor;
    private SerializedProperty preserveTextureScaleAndOffset;
    private SerializedProperty processMeshFilterNormals;
    private SerializedProperty vertexColorFormat;
    private SerializedProperty debugNormalsProcessing;
    private SerializedProperty rules;

    private void OnEnable()
    {
        targetShader = serializedObject.FindProperty("targetShader");
        stylePreset = serializedObject.FindProperty("stylePreset");
        fallbackPreset = serializedObject.FindProperty("fallbackPreset");
        outputFolder = serializedObject.FindProperty("outputFolder");
        preserveBaseMap = serializedObject.FindProperty("preserveBaseMap");
        preserveBaseColor = serializedObject.FindProperty("preserveBaseColor");
        preserveTextureScaleAndOffset = serializedObject.FindProperty("preserveTextureScaleAndOffset");
        processMeshFilterNormals = serializedObject.FindProperty("processMeshFilterNormals");
        vertexColorFormat = serializedObject.FindProperty("vertexColorFormat");
        debugNormalsProcessing = serializedObject.FindProperty("debugNormalsProcessing");
        rules = serializedObject.FindProperty("rules");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        NPRMaterialReplacementProfile profile = (NPRMaterialReplacementProfile)target;

        EditorGUILayout.LabelField("Active Preset", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            stylePreset.objectReferenceValue != null
                ? $"Using Style Preset asset: {profile.ActivePresetName}"
                : $"Using built-in toon preset: {profile.ActivePresetName}",
            MessageType.Info);

        EditorGUILayout.PropertyField(targetShader, new GUIContent("Target NPR Shader"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preset Source", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(stylePreset, new GUIContent("Style Preset Asset"));

        int selectedBuiltInPreset = fallbackPreset.enumValueIndex;
        string[] presetNames = ToonMaterialPreset.GetPresetNames();
        int nextBuiltInPreset = EditorGUILayout.Popup("Built-in Toon Preset", selectedBuiltInPreset, presetNames);
        if (nextBuiltInPreset != selectedBuiltInPreset)
        {
            fallbackPreset.enumValueIndex = nextBuiltInPreset;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Clear Style Asset / Use Built-in"))
            {
                stylePreset.objectReferenceValue = null;
            }
        }

        if (stylePreset.objectReferenceValue != null)
        {
            EditorGUILayout.HelpBox("Style Preset Asset takes priority. The built-in preset below is only used when Style Preset Asset is empty.", MessageType.None);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Generated Material Assets", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(outputFolder, new GUIContent("Output Folder"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preserve Source Material Identity", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(preserveBaseMap, new GUIContent("Preserve Base Map"));
        EditorGUILayout.PropertyField(preserveBaseColor, new GUIContent("Preserve Base Color"));
        EditorGUILayout.PropertyField(preserveTextureScaleAndOffset, new GUIContent("Preserve Texture Scale And Offset"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Outline Normal Preparation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(processMeshFilterNormals, new GUIContent("Process MeshFilter Normals"));
        EditorGUILayout.PropertyField(vertexColorFormat, new GUIContent("Vertex Color Format"));
        EditorGUILayout.PropertyField(debugNormalsProcessing, new GUIContent("Debug Normals Processing"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(rules, new GUIContent("Partial Replacement Rules"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
