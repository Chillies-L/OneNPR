using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class NPRAIAssetGeneratorWindow : EditorWindow
{
    private const string ApiKeyPrefKey = "NPR.AIAssetGenerator.ApiKey";
    private const string BaseUrlPrefKey = "NPR.AIAssetGenerator.BaseUrl";
    private const string ModelPrefKey = "NPR.AIAssetGenerator.Model";
    private const string SizePrefKey = "NPR.AIAssetGenerator.Size";
    private const string QualityPrefKey = "NPR.AIAssetGenerator.Quality";
    private const string OutputFolderPrefKey = "NPR.AIAssetGenerator.OutputFolder";
    private const string TargetShaderName = "Custom/NPR/Toon_Character_ImprovedOutline";

    private static readonly string[] CandidateKindLabels =
    {
        "Shade Map",
        "Control Map",
    };

    private string apiKey;
    private string baseUrl;
    private string model;
    private string size;
    private string quality;
    private string outputFolder;
    private bool includeInactive = true;
    private bool showApiSettings;
    private GameObject selectedRoot;
    private int selectedMaterialIndex;
    private Vector2 materialScroll;
    private readonly List<NPRAIMaterialContext> materialContexts = new List<NPRAIMaterialContext>();
    private readonly Dictionary<Material, NPRAIMaterialRecipe> recipesByMaterial = new Dictionary<Material, NPRAIMaterialRecipe>();
    private NPRAIAssetKind candidateKind = NPRAIAssetKind.ShadeMap;
    private NPRAIAssetPromptOptions promptOptions = new NPRAIAssetPromptOptions();
    private string promptPreview;
    private string status;
    private UnityWebRequest activeRequest;
    private string pendingOutputPath;
    private NPRAIAssetKind pendingGeneratedAssetKind;
    private bool isDownloadingImageUrl;
    private string lastCandidatePath;
    private NPRAIAssetKind lastCandidateKind;

    [MenuItem("NPR/AI Asset Generator")]
    public static void Open()
    {
        NPRAIAssetGeneratorWindow window = GetWindow<NPRAIAssetGeneratorWindow>("NPR AI Assets");
        window.minSize = new Vector2(720f, 680f);
        window.Show();
    }

    private void OnEnable()
    {
        apiKey = EditorPrefs.GetString(ApiKeyPrefKey, string.Empty);
        baseUrl = EditorPrefs.GetString(BaseUrlPrefKey, NPRAIImageGenerationRequest.DefaultBaseUrl);
        model = EditorPrefs.GetString(ModelPrefKey, NPRAIImageGenerationRequest.DefaultModel);
        size = EditorPrefs.GetString(SizePrefKey, "1024x1024");
        quality = EditorPrefs.GetString(QualityPrefKey, "high");
        outputFolder = EditorPrefs.GetString(OutputFolderPrefKey, "Assets/CustomShaders/GeneratedAIAssets");
        selectedRoot = Selection.activeGameObject;
        RefreshPromptPreview();
    }

    private void OnDisable()
    {
        EditorApplication.update -= PollActiveRequest;
        activeRequest?.Dispose();
        activeRequest = null;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("AI NPR Material Assistant", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Scans selected model materials, builds NPR material recipes from Base Color/Base Map context, and optionally saves AI-generated Shade/Control map candidates. Non-NPR materials are skipped instead of being replaced here.",
            MessageType.Info);

        DrawSelectionSection();
        DrawRecipeActions();
        DrawMaterialContextList();
        DrawCandidateGenerationSection();

        if (!string.IsNullOrEmpty(status))
        {
            EditorGUILayout.HelpBox(status, activeRequest == null ? MessageType.None : MessageType.Info);
        }
    }

    private void DrawSelectionSection()
    {
        Shader targetShader = ResolveTargetShader();
        if (targetShader == null)
        {
            EditorGUILayout.HelpBox($"Target shader not found: {TargetShaderName}", MessageType.Warning);
        }

        EditorGUI.BeginChangeCheck();
        selectedRoot = EditorGUILayout.ObjectField("Root GameObject", selectedRoot, typeof(GameObject), true) as GameObject;
        includeInactive = EditorGUILayout.Toggle("Include Inactive", includeInactive);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        if (EditorGUI.EndChangeCheck())
        {
            PersistSettings();
        }

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = selectedRoot != null && targetShader != null;
        if (GUILayout.Button("Scan Selection", GUILayout.Height(28f)))
        {
            ScanSelection();
        }

        if (GUILayout.Button("Use Active Selection", GUILayout.Height(28f)))
        {
            selectedRoot = Selection.activeGameObject;
            ScanSelection();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawRecipeActions()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Material Recipes", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = materialContexts.Count > 0;
        if (GUILayout.Button("Generate Recipes", GUILayout.Height(28f)))
        {
            GenerateRecipes();
        }

        GUI.enabled = recipesByMaterial.Count > 0;
        if (GUILayout.Button("Apply Recipes", GUILayout.Height(28f)))
        {
            ApplyRecipes();
        }

        NPRAIMaterialContext selectedContext = GetSelectedContext();
        GUI.enabled = selectedContext != null && selectedContext.IsTargetNPRShader && selectedContext.Material != null;
        if (GUILayout.Button("Create and Apply Ramp", GUILayout.Height(28f)))
        {
            CreateAndApplyRamp(selectedContext);
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawMaterialContextList()
    {
        if (materialContexts.Count == 0)
        {
            EditorGUILayout.HelpBox("No scanned materials yet. Select a model root and click Scan Selection.", MessageType.None);
            return;
        }

        EditorGUILayout.LabelField($"Scanned Materials ({materialContexts.Count})", EditorStyles.boldLabel);
        materialScroll = EditorGUILayout.BeginScrollView(materialScroll, GUILayout.MinHeight(180f), GUILayout.MaxHeight(260f));

        for (int i = 0; i < materialContexts.Count; i++)
        {
            DrawMaterialContextRow(i, materialContexts[i]);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawMaterialContextRow(int index, NPRAIMaterialContext context)
    {
        bool isSelected = selectedMaterialIndex == index;
        GUIStyle boxStyle = isSelected ? "OL SelectedRow" : "box";
        EditorGUILayout.BeginVertical(boxStyle);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Toggle(isSelected, "Select", "Button", GUILayout.Width(70f)))
        {
            selectedMaterialIndex = index;
            RefreshPromptPreview();
        }

        EditorGUILayout.LabelField(context.MaterialName, EditorStyles.boldLabel, GUILayout.MinWidth(160f));

        EditorGUI.BeginChangeCheck();
        NPRAIMaterialCategory category = (NPRAIMaterialCategory)EditorGUILayout.EnumPopup(context.Category, GUILayout.Width(110f));
        if (EditorGUI.EndChangeCheck())
        {
            context.Category = category;
            recipesByMaterial.Remove(context.Material);
            RefreshPromptPreview();
        }

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ColorField(context.BaseColor, GUILayout.Width(70f));
            EditorGUILayout.ToggleLeft("Base Map", context.HasBaseMap, GUILayout.Width(90f));
            EditorGUILayout.ToggleLeft("NPR", context.IsTargetNPRShader, GUILayout.Width(60f));
            EditorGUILayout.IntField("Slots", context.SharedSlotCount, GUILayout.Width(100f));
        }

        EditorGUILayout.EndHorizontal();

        if (recipesByMaterial.TryGetValue(context.Material, out NPRAIMaterialRecipe recipe))
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ColorField("1st Shade", recipe.firstShadeColor);
                EditorGUILayout.ColorField("2nd Shade", recipe.secondShadeColor);
                EditorGUILayout.FloatField("Rim", recipe.rimIntensity);
                EditorGUILayout.FloatField("Outline", recipe.outlineWidth);
                EditorGUILayout.EndHorizontal();
            }
        }

        if (!context.IsTargetNPRShader)
        {
            EditorGUILayout.HelpBox("Skipped by recipe apply. Convert this material with the NPR Material Replacer first.", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawCandidateGenerationSection()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("AI Candidate Maps", EditorStyles.boldLabel);

        showApiSettings = EditorGUILayout.Foldout(showApiSettings, "API Settings");
        if (showApiSettings)
        {
            EditorGUI.BeginChangeCheck();
            apiKey = EditorGUILayout.PasswordField("API Key", apiKey);
            baseUrl = EditorGUILayout.TextField("Base URL", baseUrl);
            model = EditorGUILayout.TextField("Model", model);
            size = EditorGUILayout.TextField("Size", size);
            quality = EditorGUILayout.TextField("Quality", quality);
            if (EditorGUI.EndChangeCheck())
            {
                PersistSettings();
            }
        }

        EditorGUI.BeginChangeCheck();
        int candidateIndex = candidateKind == NPRAIAssetKind.ControlMap ? 1 : 0;
        candidateIndex = EditorGUILayout.Popup("Candidate Kind", candidateIndex, CandidateKindLabels);
        candidateKind = candidateIndex == 0 ? NPRAIAssetKind.ShadeMap : NPRAIAssetKind.ControlMap;
        promptOptions.subject = EditorGUILayout.TextField("Subject", promptOptions.subject);
        promptOptions.style = EditorGUILayout.TextField("Style", promptOptions.style);
        EditorGUILayout.LabelField("Material Notes");
        promptOptions.materialNotes = EditorGUILayout.TextArea(promptOptions.materialNotes, GUILayout.MinHeight(42f));
        if (EditorGUI.EndChangeCheck())
        {
            RefreshPromptPreview();
        }

        EditorGUILayout.LabelField("Material-Aware Prompt", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextArea(promptPreview, GUILayout.MinHeight(130f));
        }

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = activeRequest == null && GetSelectedContext() != null;
        if (GUILayout.Button("Generate Candidate", GUILayout.Height(28f)))
        {
            StartGeneration(candidateKind);
        }

        GUI.enabled = !string.IsNullOrEmpty(lastCandidatePath) && GetSelectedContext() != null;
        if (GUILayout.Button("Apply Candidate", GUILayout.Height(28f)))
        {
            ApplyCandidateToSelectedMaterial();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void PersistSettings()
    {
        EditorPrefs.SetString(ApiKeyPrefKey, apiKey ?? string.Empty);
        EditorPrefs.SetString(BaseUrlPrefKey, baseUrl ?? string.Empty);
        EditorPrefs.SetString(ModelPrefKey, model ?? string.Empty);
        EditorPrefs.SetString(SizePrefKey, size ?? string.Empty);
        EditorPrefs.SetString(QualityPrefKey, quality ?? string.Empty);
        EditorPrefs.SetString(OutputFolderPrefKey, outputFolder ?? string.Empty);
    }

    private void ScanSelection()
    {
        Shader targetShader = ResolveTargetShader();
        materialContexts.Clear();
        recipesByMaterial.Clear();
        selectedMaterialIndex = 0;

        if (selectedRoot == null)
        {
            status = "No root GameObject selected.";
            RefreshPromptPreview();
            return;
        }

        if (targetShader == null)
        {
            status = $"Target shader not found: {TargetShaderName}";
            RefreshPromptPreview();
            return;
        }

        materialContexts.AddRange(NPRAIMaterialContextScanner.ScanUniqueMaterials(selectedRoot, targetShader, includeInactive));
        RefreshPromptPreview();
        status = $"Scanned {materialContexts.Count} unique materials under {selectedRoot.name}.";
    }

    private void GenerateRecipes()
    {
        recipesByMaterial.Clear();

        for (int i = 0; i < materialContexts.Count; i++)
        {
            NPRAIMaterialContext context = materialContexts[i];
            if (context.Material == null || !context.IsTargetNPRShader)
            {
                continue;
            }

            recipesByMaterial[context.Material] = NPRAIMaterialRecipeBuilder.Build(context.BaseColor, context.Category);
        }

        status = $"Generated {recipesByMaterial.Count} material recipes. Materials are unchanged until Apply Recipes.";
    }

    private void ApplyRecipes()
    {
        int applied = 0;
        int skipped = 0;

        for (int i = 0; i < materialContexts.Count; i++)
        {
            NPRAIMaterialContext context = materialContexts[i];
            if (context.Material == null || !context.IsTargetNPRShader || !recipesByMaterial.TryGetValue(context.Material, out NPRAIMaterialRecipe recipe))
            {
                skipped++;
                continue;
            }

            Undo.RecordObject(context.Material, "Apply AI NPR material recipe");
            recipe.ApplyTo(context.Material);
            EditorUtility.SetDirty(context.Material);
            applied++;
        }

        if (applied > 0)
        {
            AssetDatabase.SaveAssets();
        }

        status = $"Applied {applied} recipes. Skipped {skipped} materials.";
    }

    private void CreateAndApplyRamp(NPRAIMaterialContext context)
    {
        NPRAIMaterialRecipe recipe = GetOrCreateRecipe(context);
        if (recipe == null)
        {
            status = "No recipe available for selected material.";
            return;
        }

        Texture2D ramp = NPRAIRampTextureGenerator.CreateRampTexture(recipe);
        string assetPath = BuildOutputPath(NPRAIAssetKind.RampTexture, context);
        File.WriteAllBytes(assetPath, ramp.EncodeToPNG());
        DestroyImmediate(ramp);

        AssetDatabase.ImportAsset(assetPath);
        ConfigureImportedTexture(assetPath, NPRAIAssetKind.RampTexture);
        Texture2D importedRamp = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (importedRamp == null)
        {
            status = $"Ramp saved but could not be loaded: {assetPath}";
            return;
        }

        Undo.RecordObject(context.Material, "Apply AI NPR ramp texture");
        NPRAIRampTextureGenerator.ApplyRamp(context.Material, importedRamp);
        EditorUtility.SetDirty(context.Material);
        AssetDatabase.SaveAssets();
        status = $"Generated and applied ramp texture: {assetPath}";
    }

    private NPRAIMaterialRecipe GetOrCreateRecipe(NPRAIMaterialContext context)
    {
        if (context == null || context.Material == null || !context.IsTargetNPRShader)
        {
            return null;
        }

        if (!recipesByMaterial.TryGetValue(context.Material, out NPRAIMaterialRecipe recipe))
        {
            recipe = NPRAIMaterialRecipeBuilder.Build(context.BaseColor, context.Category);
            recipesByMaterial[context.Material] = recipe;
        }

        return recipe;
    }

    private void RefreshPromptPreview()
    {
        promptPreview = NPRAIAssetPromptBuilder.BuildPrompt(candidateKind, promptOptions, GetSelectedContext());
    }

    private void StartGeneration(NPRAIAssetKind kind)
    {
        NPRAIMaterialContext context = GetSelectedContext();
        if (context == null)
        {
            status = "Select a scanned material before generating an AI candidate map.";
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            status = "API key is empty.";
            return;
        }

        if (kind != NPRAIAssetKind.ShadeMap && kind != NPRAIAssetKind.ControlMap)
        {
            status = "AI generation is limited to Shade Map and Control Map candidates. Use Create and Apply Ramp for Ramp Texture.";
            return;
        }

        RefreshPromptPreview();
        PersistSettings();

        string endpoint = NPRAIImageGenerationRequest.BuildEndpoint(baseUrl);
        string body = NPRAIImageGenerationRequest.BuildJsonBody(model, promptPreview, size, quality);
        byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(body);

        activeRequest = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST);
        activeRequest.uploadHandler = new UploadHandlerRaw(bodyBytes);
        activeRequest.downloadHandler = new DownloadHandlerBuffer();
        activeRequest.SetRequestHeader("Content-Type", "application/json");
        activeRequest.SetRequestHeader("Authorization", "Bearer " + apiKey.Trim());

        pendingOutputPath = BuildOutputPath(kind, context);
        pendingGeneratedAssetKind = kind;
        isDownloadingImageUrl = false;
        status = "Generating AI candidate map...";
        activeRequest.SendWebRequest();
        EditorApplication.update -= PollActiveRequest;
        EditorApplication.update += PollActiveRequest;
    }

    private void PollActiveRequest()
    {
        if (activeRequest == null || !activeRequest.isDone)
        {
            return;
        }

        UnityWebRequest completed = activeRequest;
        activeRequest = null;

        if (completed.result != UnityWebRequest.Result.Success)
        {
            status = $"Request failed: {completed.responseCode} {completed.error}\n{completed.downloadHandler.text}";
            completed.Dispose();
            EditorApplication.update -= PollActiveRequest;
            Repaint();
            return;
        }

        if (isDownloadingImageUrl)
        {
            SaveCandidateBytes(completed.downloadHandler.data, pendingGeneratedAssetKind);
            completed.Dispose();
            EditorApplication.update -= PollActiveRequest;
            Repaint();
            return;
        }

        ImageGenerationResponse response = JsonUtility.FromJson<ImageGenerationResponse>(completed.downloadHandler.text);
        completed.Dispose();

        if (response == null || response.data == null || response.data.Length == 0)
        {
            status = "Image generation response did not contain data[0].";
            EditorApplication.update -= PollActiveRequest;
            Repaint();
            return;
        }

        ImageGenerationData imageData = response.data[0];
        if (!string.IsNullOrEmpty(imageData.b64_json))
        {
            SaveCandidateBytes(Convert.FromBase64String(imageData.b64_json), pendingGeneratedAssetKind);
            EditorApplication.update -= PollActiveRequest;
            Repaint();
            return;
        }

        if (!string.IsNullOrEmpty(imageData.url))
        {
            activeRequest = UnityWebRequest.Get(imageData.url);
            isDownloadingImageUrl = true;
            status = "Downloading generated image URL...";
            activeRequest.SendWebRequest();
            Repaint();
            return;
        }

        status = "Image generation response had neither b64_json nor url.";
        EditorApplication.update -= PollActiveRequest;
        Repaint();
    }

    private void SaveCandidateBytes(byte[] imageBytes, NPRAIAssetKind kind)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            status = "Generated image was empty.";
            return;
        }

        File.WriteAllBytes(pendingOutputPath, imageBytes);
        AssetDatabase.ImportAsset(pendingOutputPath);
        ConfigureImportedTexture(pendingOutputPath, kind);
        lastCandidatePath = pendingOutputPath;
        lastCandidateKind = kind;
        status = $"Candidate saved: {pendingOutputPath}. Click Apply Candidate to bind it to the selected material.";
    }

    private void ApplyCandidateToSelectedMaterial()
    {
        NPRAIMaterialContext context = GetSelectedContext();
        if (context == null || context.Material == null)
        {
            status = "No selected material for candidate apply.";
            return;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(lastCandidatePath);
        if (texture == null)
        {
            status = $"Candidate texture not found: {lastCandidatePath}";
            return;
        }

        string propertyName = lastCandidateKind == NPRAIAssetKind.ControlMap ? "_ControlMap" : "_ShadeMap";
        string strengthName = lastCandidateKind == NPRAIAssetKind.ControlMap ? "_ControlMapStrength" : "_ShadeMapStrength";

        Undo.RecordObject(context.Material, "Apply AI NPR candidate map");
        if (context.Material.HasProperty(propertyName))
        {
            context.Material.SetTexture(propertyName, texture);
        }

        if (context.Material.HasProperty(strengthName))
        {
            context.Material.SetFloat(strengthName, 1f);
        }

        EditorUtility.SetDirty(context.Material);
        AssetDatabase.SaveAssets();
        status = $"Applied {lastCandidateKind} candidate to {context.MaterialName}.";
    }

    private string BuildOutputPath(NPRAIAssetKind kind, NPRAIMaterialContext context)
    {
        string folder = string.IsNullOrWhiteSpace(outputFolder) ? "Assets/CustomShaders/GeneratedAIAssets" : outputFolder.Trim();
        if (!folder.StartsWith("Assets/", StringComparison.Ordinal) && folder != "Assets")
        {
            folder = "Assets/CustomShaders/GeneratedAIAssets";
        }

        Directory.CreateDirectory(folder);
        string stem = Path.GetFileNameWithoutExtension(NPRAIAssetPromptBuilder.GetRecommendedFileName(kind));
        string materialName = SanitizeFileName(context == null ? "material" : context.MaterialName);
        string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        return Path.Combine(folder, $"{materialName}-{stem}-{timestamp}.png").Replace('\\', '/');
    }

    private void ConfigureImportedTexture(string assetPath, NPRAIAssetKind kind)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.wrapMode = kind == NPRAIAssetKind.RampTexture ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
        importer.mipmapEnabled = kind != NPRAIAssetKind.RampTexture;
        importer.sRGBTexture = kind != NPRAIAssetKind.ControlMap;
        importer.SaveAndReimport();
    }

    private NPRAIMaterialContext GetSelectedContext()
    {
        if (materialContexts.Count == 0)
        {
            return null;
        }

        selectedMaterialIndex = Mathf.Clamp(selectedMaterialIndex, 0, materialContexts.Count - 1);
        return materialContexts[selectedMaterialIndex];
    }

    private static Shader ResolveTargetShader()
    {
        return Shader.Find(TargetShaderName);
    }

    private static string SanitizeFileName(string value)
    {
        string safeName = string.IsNullOrWhiteSpace(value) ? "material" : value.Trim();
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            safeName = safeName.Replace(invalidChar, '_');
        }

        return safeName;
    }

    [Serializable]
    private class ImageGenerationResponse
    {
        public ImageGenerationData[] data = Array.Empty<ImageGenerationData>();
    }

    [Serializable]
    private class ImageGenerationData
    {
        public string b64_json = string.Empty;
        public string url = string.Empty;
    }
}
