using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NPRRuntimeUIPanel : MonoBehaviour
{
    private const string DefaultPresetName = "Custom（自定义）";

    [Header("Targets")]
    [SerializeField] private Renderer[] targetRenderers = Array.Empty<Renderer>();
    [SerializeField] private Material[] targetMaterials = Array.Empty<Material>();
    [SerializeField] private bool useRuntimeMaterialInstances = true;
    [SerializeField] private bool editSelectedRendererOnly = true;
    [SerializeField] private Camera selectionCamera;
    [SerializeField] private LayerMask selectableLayers = ~0;
    [SerializeField] private float selectedOutlineMultiplier = 1.35f;

    [Header("Presets")]
    [SerializeField] private NPRStylePreset[] stylePresets = Array.Empty<NPRStylePreset>();
    [SerializeField] private NPRStylePostProcessFeature stylePostProcessFeature;
    [SerializeField] private bool applyFirstPresetOnStart;

    [Header("Generated UI")]
    [SerializeField] private bool buildUiOnStart = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private Vector2 panelSize = new Vector2(430f, 720f);
    [SerializeField] private Vector2 collapsedPanelSize = new Vector2(190f, 64f);
    [SerializeField] private Vector2 panelOffset = new Vector2(-18f, -18f);
    [SerializeField] private bool startCollapsed;
    [SerializeField] private bool ignoreClicksOverUi = true;

    [Header("Optional Existing UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Text selectedObjectText;
    [SerializeField] private Text presetNameText;
    [SerializeField] private Text fpsText;
    [SerializeField] private Slider autoShadeColorSlider;
    [SerializeField] private Slider shadeSaturationSlider;
    [SerializeField] private Slider litColorBoostSlider;
    [SerializeField] private Slider baseShadowStepSlider;
    [SerializeField] private Slider shadowFeatherSlider;
    [SerializeField] private Slider shadeStepSlider;
    [SerializeField] private Slider shadeMapStrengthSlider;
    [SerializeField] private Slider controlMapStrengthSlider;
    [SerializeField] private Slider rampTextureStrengthSlider;
    [SerializeField] private Slider shadowAntiFlickerSlider;
    [SerializeField] private Slider useVirtualLightSlider;
    [SerializeField] private Slider lightColorInfluenceSlider;
    [SerializeField] private Slider ambientStrengthSlider;
    [SerializeField] private Slider additionalLightInfluenceSlider;
    [SerializeField] private Slider specularSizeSlider;
    [SerializeField] private Slider specularFeatherSlider;
    [SerializeField] private Slider outlineWidthSlider;
    [SerializeField] private Slider outlineMinWidthSlider;
    [SerializeField] private Slider outlineMaxWidthSlider;
    [SerializeField] private Slider outlineMiterStrengthSlider;
    [SerializeField] private Slider outlineVertexNormalBlendSlider;
    [SerializeField] private Slider outlineDirectionalBiasSlider;
    [SerializeField] private Slider rimPowerSlider;
    [SerializeField] private Slider rimThresholdSlider;
    [SerializeField] private Slider rimFeatherSlider;
    [SerializeField] private Slider rimLightAlignSlider;
    [SerializeField] private Slider rimIntensitySlider;
    [SerializeField] private Slider specularIntensitySlider;
    [SerializeField] private Slider posterizeStrengthSlider;
    [SerializeField] private Slider postProcessContrastSlider;
    [SerializeField] private Slider postProcessSaturationSlider;
    [SerializeField] private Slider shadowCrushSlider;
    [SerializeField] private Slider halftoneStrengthSlider;
    [SerializeField] private Slider halftoneScaleSlider;
    [SerializeField] private Toggle postProcessToggle;

    private readonly List<Material> runtimeMaterials = new List<Material>();
    private readonly List<Text> valueLabels = new List<Text>();
    private readonly Dictionary<Slider, Text> valueLabelsBySlider = new Dictionary<Slider, Text>();
    private readonly Dictionary<Renderer, float> baseOutlineWidthsByRenderer = new Dictionary<Renderer, float>();
    private readonly Dictionary<Renderer, Material[]> originalSharedMaterialsByRenderer = new Dictionary<Renderer, Material[]>();
    private GameObject expandedContentRoot;
    private GameObject collapsedSummaryRoot;
    private Text collapsedFpsText;
    private Renderer selectedRenderer;
    private int currentPresetIndex = -1;
    private float fpsTimer;
    private float fpsDelta;
    private bool isCollapsed;
    private bool uiBuilt;

    public string CurrentPresetName { get; private set; } = DefaultPresetName;
    public bool IsCollapsed => isCollapsed;
    public string SelectedObjectName => selectedRenderer != null ? selectedRenderer.gameObject.name : "None（未选择）";

    public void ConfigureForRuntime(Material[] materials, NPRStylePreset[] presets, NPRStylePostProcessFeature postProcessFeature)
    {
        targetMaterials = materials ?? Array.Empty<Material>();
        stylePresets = presets ?? Array.Empty<NPRStylePreset>();
        stylePostProcessFeature = postProcessFeature;
        currentPresetIndex = -1;
        CurrentPresetName = DefaultPresetName;

        RefreshTargetMaterials();
        SyncControlValuesFromFirstMaterial();
        UpdateReadout();
    }

    public void ConfigureForRuntime(Renderer[] renderers, NPRStylePreset[] presets, NPRStylePostProcessFeature postProcessFeature)
    {
        targetRenderers = renderers ?? Array.Empty<Renderer>();
        targetMaterials = Array.Empty<Material>();
        stylePresets = presets ?? Array.Empty<NPRStylePreset>();
        stylePostProcessFeature = postProcessFeature;
        currentPresetIndex = -1;
        CurrentPresetName = DefaultPresetName;

        CaptureOriginalSharedMaterials();
        RefreshTargetMaterials();
        SyncControlValuesFromFirstMaterial();
        UpdateReadout();
    }

    public void ApplyPreset(int presetIndex)
    {
        if (stylePresets == null || presetIndex < 0 || presetIndex >= stylePresets.Length || stylePresets[presetIndex] == null)
        {
            Debug.LogWarning($"NPRRuntimeUIPanel: Invalid preset index {presetIndex}.");
            return;
        }

        NPRStylePreset preset = stylePresets[presetIndex];
        RefreshTargetMaterials();

        foreach (Material material in runtimeMaterials)
        {
            preset.ApplyTo(material);
        }

        if (stylePostProcessFeature != null)
        {
            bool wasEnabled = stylePostProcessFeature.settings.enabled;
            preset.ApplyTo(stylePostProcessFeature.settings);
            stylePostProcessFeature.settings.enabled = wasEnabled;
        }

        currentPresetIndex = presetIndex;
        CurrentPresetName = preset.DisplayName;
        SyncControlValuesFromFirstMaterial();
        UpdateReadout();
    }

    public void SelectRenderer(Renderer rendererToSelect)
    {
        if (selectedRenderer == rendererToSelect)
        {
            return;
        }

        ClearSelectionHighlight();
        selectedRenderer = rendererToSelect;
        ApplySelectionHighlight();
        SyncControlValuesFromFirstMaterial();
        UpdateReadout();
    }

    public Material CreateMaterialForSelection()
    {
        if (selectedRenderer == null)
        {
            Debug.LogWarning("NPRRuntimeUIPanel: Select a renderer before creating a material.");
            return null;
        }

        Material[] editableMaterials = GetEditableMaterials(selectedRenderer);
        Material source = editableMaterials.Length > 0 ? editableMaterials[0] : selectedRenderer.sharedMaterial;
        Shader toonShader = Shader.Find("Custom/NPR/Toon_Character_ImprovedOutline");
        Material createdMaterial = source != null ? new Material(source) : new Material(toonShader);

        if (toonShader != null && createdMaterial.shader != toonShader)
        {
            createdMaterial.shader = toonShader;
        }

        createdMaterial.name = selectedRenderer.gameObject.name + "_RuntimeNPRMaterial";
        if (editableMaterials.Length > 0)
        {
            editableMaterials[0] = createdMaterial;
            if (useRuntimeMaterialInstances)
            {
                selectedRenderer.materials = editableMaterials;
            }
            else
            {
                selectedRenderer.sharedMaterials = editableMaterials;
            }

            Material[] assignedMaterials = GetEditableMaterials(selectedRenderer);
            if (assignedMaterials.Length > 0 && assignedMaterials[0] != null)
            {
                createdMaterial = assignedMaterials[0];
            }
        }
        else
        {
            selectedRenderer.sharedMaterial = createdMaterial;
        }

        baseOutlineWidthsByRenderer[selectedRenderer] = GetMaterialFloat(createdMaterial, "_OutlineWidth", 0.008f);

        RefreshTargetMaterials();
        SyncControlValuesFromFirstMaterial();
        UpdateReadout();
        return createdMaterial;
    }

    public void ClearSelection()
    {
        SelectRenderer(null);
    }

    public void ApplyNextPreset()
    {
        if (stylePresets == null || stylePresets.Length == 0)
        {
            return;
        }

        int nextIndex = currentPresetIndex + 1;
        if (nextIndex >= stylePresets.Length)
        {
            nextIndex = 0;
        }

        ApplyPreset(nextIndex);
    }

    public void SetAutoShadeColor(float value)
    {
        SetClampedMaterialFloat("_AutoShadeColor", value, 0f, 1f, autoShadeColorSlider);
    }

    public void SetShadeSaturation(float value)
    {
        SetClampedMaterialFloat("_ShadeSaturation", value, 0f, 1f, shadeSaturationSlider);
    }

    public void SetLitColorBoost(float value)
    {
        SetClampedMaterialFloat("_LitColorBoost", value, 0f, 0.5f, litColorBoostSlider);
    }

    public void SetBaseShadowStep(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        SetFloatOnTargets("_BaseStep", clampedValue);
        SetSliderValue(baseShadowStepSlider, clampedValue);
        UpdateReadout();
    }

    public void SetShadowFeather(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0f, 0.5f);
        SetFloatOnTargets("_BaseFeather", clampedValue);
        SetFloatOnTargets("_ShadeFeather", clampedValue);
        SetSliderValue(shadowFeatherSlider, clampedValue);
        UpdateReadout();
    }

    public void SetShadeStep(float value)
    {
        SetClampedMaterialFloat("_ShadeStep", value, 0f, 1f, shadeStepSlider);
    }

    public void SetShadeMapStrength(float value)
    {
        SetClampedMaterialFloat("_ShadeMapStrength", value, 0f, 1f, shadeMapStrengthSlider);
    }

    public void SetControlMapStrength(float value)
    {
        SetClampedMaterialFloat("_ControlMapStrength", value, 0f, 1f, controlMapStrengthSlider);
    }

    public void SetRampTextureStrength(float value)
    {
        SetClampedMaterialFloat("_RampTextureStrength", value, 0f, 1f, rampTextureStrengthSlider);
    }

    public void SetShadowAntiFlicker(float value)
    {
        SetClampedMaterialFloat("_ShadowAntiFlicker", value, 0f, 1f, shadowAntiFlickerSlider);
    }

    public void SetUseVirtualLight(float value)
    {
        SetClampedMaterialFloat("_UseVirtualLight", value, 0f, 1f, useVirtualLightSlider);
    }

    public void SetLightColorInfluence(float value)
    {
        SetClampedMaterialFloat("_LightColorInfluence", value, 0f, 1f, lightColorInfluenceSlider);
    }

    public void SetAmbientStrength(float value)
    {
        SetClampedMaterialFloat("_AmbientStrength", value, 0f, 1f, ambientStrengthSlider);
    }

    public void SetAdditionalLightInfluence(float value)
    {
        SetClampedMaterialFloat("_AdditionalLightInfluence", value, 0f, 1f, additionalLightInfluenceSlider);
    }

    public void SetOutlineWidth(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0f, 0.1f);
        SetFloatOnTargets("_OutlineWidth", clampedValue);
        SetSliderValue(outlineWidthSlider, clampedValue);
        UpdateReadout();
    }

    public void SetOutlineMinWidth(float value)
    {
        SetClampedMaterialFloat("_OutlineMinWidth", value, 0f, 0.01f, outlineMinWidthSlider);
    }

    public void SetOutlineMaxWidth(float value)
    {
        SetClampedMaterialFloat("_OutlineMaxWidth", value, 0f, 0.1f, outlineMaxWidthSlider);
    }

    public void SetOutlineMiterStrength(float value)
    {
        SetClampedMaterialFloat("_OutlineMiterStrength", value, 0f, 1f, outlineMiterStrengthSlider);
    }

    public void SetOutlineVertexNormalBlend(float value)
    {
        SetClampedMaterialFloat("_OutlineVertexNormalBlend", value, 0f, 1f, outlineVertexNormalBlendSlider);
    }

    public void SetOutlineDirectionalBias(float value)
    {
        SetClampedMaterialFloat("_OutlineDirectionalBias", value, -1f, 1f, outlineDirectionalBiasSlider);
    }

    public void SetSpecularSize(float value)
    {
        SetClampedMaterialFloat("_SpecularSize", value, 0f, 1f, specularSizeSlider);
    }

    public void SetSpecularFeather(float value)
    {
        SetClampedMaterialFloat("_SpecularFeather", value, 0f, 0.25f, specularFeatherSlider);
    }

    public void SetRimIntensity(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0f, 2f);
        SetFloatOnTargets("_RimIntensity", clampedValue);
        SetSliderValue(rimIntensitySlider, clampedValue);
        UpdateReadout();
    }

    public void SetRimPower(float value)
    {
        SetClampedMaterialFloat("_RimPower", value, 0.1f, 8f, rimPowerSlider);
    }

    public void SetRimThreshold(float value)
    {
        SetClampedMaterialFloat("_RimThreshold", value, 0f, 1f, rimThresholdSlider);
    }

    public void SetRimFeather(float value)
    {
        SetClampedMaterialFloat("_RimFeather", value, 0f, 0.5f, rimFeatherSlider);
    }

    public void SetRimLightAlign(float value)
    {
        SetClampedMaterialFloat("_RimLightAlign", value, 0f, 1f, rimLightAlignSlider);
    }

    public void SetSpecularIntensity(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0f, 2f);
        SetFloatOnTargets("_SpecularIntensity", clampedValue);
        SetSliderValue(specularIntensitySlider, clampedValue);
        UpdateReadout();
    }

    public void SetPosterizeStrength(float value)
    {
        if (stylePostProcessFeature == null)
        {
            return;
        }

        float clampedValue = Mathf.Clamp01(value);
        stylePostProcessFeature.settings.posterizeStrength = clampedValue;
        SetSliderValue(posterizeStrengthSlider, clampedValue);
        UpdateReadout();
    }

    public void SetPostProcessContrast(float value)
    {
        if (stylePostProcessFeature == null)
        {
            return;
        }

        float clampedValue = Mathf.Clamp(value, 0.5f, 2.5f);
        stylePostProcessFeature.settings.contrast = clampedValue;
        SetSliderValue(postProcessContrastSlider, clampedValue);
        UpdateReadout();
    }

    public void SetPostProcessSaturation(float value)
    {
        if (stylePostProcessFeature == null)
        {
            return;
        }

        float clampedValue = Mathf.Clamp(value, 0f, 2f);
        stylePostProcessFeature.settings.saturation = clampedValue;
        SetSliderValue(postProcessSaturationSlider, clampedValue);
        UpdateReadout();
    }

    public void SetShadowCrush(float value)
    {
        if (stylePostProcessFeature == null)
        {
            return;
        }

        float clampedValue = Mathf.Clamp01(value);
        stylePostProcessFeature.settings.shadowCrush = clampedValue;
        SetSliderValue(shadowCrushSlider, clampedValue);
        UpdateReadout();
    }

    public void SetHalftoneStrength(float value)
    {
        if (stylePostProcessFeature == null)
        {
            return;
        }

        float clampedValue = Mathf.Clamp01(value);
        stylePostProcessFeature.settings.halftoneStrength = clampedValue;
        stylePostProcessFeature.settings.halftoneEnabled = clampedValue > 0f;
        if (clampedValue > 0f && stylePostProcessFeature.settings.patternType == NPRStylePostProcessFeature.PatternShadingType.Off)
        {
            stylePostProcessFeature.settings.patternType = NPRStylePostProcessFeature.PatternShadingType.Dots;
        }
        SetSliderValue(halftoneStrengthSlider, clampedValue);
        UpdateReadout();
    }

    public void SetHalftoneScale(float value)
    {
        if (stylePostProcessFeature == null)
        {
            return;
        }

        float clampedValue = Mathf.Clamp(value, 1f, 50f);
        stylePostProcessFeature.settings.halftoneScale = clampedValue;
        SetSliderValue(halftoneScaleSlider, clampedValue);
        UpdateReadout();
    }

    public void SetPostProcessEnabled(bool enabled)
    {
        if (stylePostProcessFeature == null)
        {
            return;
        }

        stylePostProcessFeature.settings.enabled = enabled;
        stylePostProcessFeature.SetActive(enabled);
        if (postProcessToggle != null)
        {
            postProcessToggle.SetIsOnWithoutNotify(enabled);
        }

        UpdateReadout();
    }

    public void TogglePanel()
    {
        ToggleCollapsed();
    }

    public void ToggleCollapsed()
    {
        SetCollapsed(!isCollapsed);
    }

    public void SetCollapsed(bool collapsed)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        isCollapsed = collapsed;

        if (expandedContentRoot != null)
        {
            SetChildGraphicsEnabled(expandedContentRoot, !collapsed);
            expandedContentRoot.SetActive(!collapsed);
        }

        if (collapsedSummaryRoot != null)
        {
            SetChildGraphicsEnabled(collapsedSummaryRoot, collapsed);
            collapsedSummaryRoot.SetActive(collapsed);
        }

        if (panelRoot == null)
        {
            return;
        }

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = collapsed ? collapsedPanelSize : panelSize;
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            Canvas.ForceUpdateCanvases();
        }
    }

    private void Start()
    {
        CaptureOriginalSharedMaterials();
        RefreshTargetMaterials();

        if (buildUiOnStart && panelRoot == null)
        {
            BuildGeneratedUi();
        }

        WireExistingControls();

        if (applyFirstPresetOnStart && stylePresets != null && stylePresets.Length > 0)
        {
            ApplyPreset(0);
        }
        else
        {
            SyncControlValuesFromFirstMaterial();
            UpdateReadout();
        }
    }

    private void Update()
    {
        if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }

        HandleSelectionInput();
        UpdateFps();
    }

    private void RefreshTargetMaterials()
    {
        runtimeMaterials.Clear();

        if (editSelectedRendererOnly && selectedRenderer != null)
        {
            Material[] selectedMaterials = GetEditableMaterials(selectedRenderer);
            foreach (Material material in selectedMaterials)
            {
                AddRuntimeMaterial(material);
            }

            return;
        }

        if (targetMaterials != null)
        {
            foreach (Material material in targetMaterials)
            {
                AddRuntimeMaterial(material);
            }
        }

        if (targetRenderers == null)
        {
            return;
        }

        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            Material[] materials = GetEditableMaterials(targetRenderer);
            foreach (Material material in materials)
            {
                AddRuntimeMaterial(material);
            }
        }
    }

    private void CaptureOriginalSharedMaterials()
    {
        if (targetRenderers == null)
        {
            return;
        }

        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null || originalSharedMaterialsByRenderer.ContainsKey(targetRenderer))
            {
                continue;
            }

            originalSharedMaterialsByRenderer.Add(targetRenderer, targetRenderer.sharedMaterials);
        }
    }

    private void AddRuntimeMaterial(Material material)
    {
        if (material == null || runtimeMaterials.Contains(material))
        {
            return;
        }

        runtimeMaterials.Add(material);
    }

    private Material[] GetEditableMaterials(Renderer renderer)
    {
        if (renderer == null)
        {
            return Array.Empty<Material>();
        }

        return useRuntimeMaterialInstances && Application.isPlaying ? renderer.materials : renderer.sharedMaterials;
    }

    private void HandleSelectionInput()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (ignoreClicksOverUi && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera activeCamera = selectionCamera != null ? selectionCamera : Camera.main;
        if (activeCamera == null)
        {
            return;
        }

        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, selectableLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        Renderer hitRenderer = hitInfo.collider.GetComponentInParent<Renderer>();
        if (hitRenderer != null)
        {
            SelectRenderer(hitRenderer);
        }
    }

    private void ClearSelectionHighlight()
    {
        if (selectedRenderer == null)
        {
            return;
        }

        if (baseOutlineWidthsByRenderer.TryGetValue(selectedRenderer, out float baseWidth))
        {
            foreach (Material material in GetEditableMaterials(selectedRenderer))
            {
                if (material != null && material.HasProperty("_OutlineWidth"))
                {
                    material.SetFloat("_OutlineWidth", baseWidth);
                }
            }
        }
    }

    private void ApplySelectionHighlight()
    {
        if (selectedRenderer == null)
        {
            return;
        }

        Material[] materials = GetEditableMaterials(selectedRenderer);
        foreach (Material material in materials)
        {
            if (material == null || !material.HasProperty("_OutlineWidth"))
            {
                continue;
            }

            float currentWidth = material.GetFloat("_OutlineWidth");
            if (!baseOutlineWidthsByRenderer.ContainsKey(selectedRenderer))
            {
                baseOutlineWidthsByRenderer[selectedRenderer] = currentWidth;
            }

            material.SetFloat("_OutlineWidth", Mathf.Clamp(currentWidth * selectedOutlineMultiplier, 0f, 0.1f));
        }
    }

    private void SetFloatOnTargets(string propertyName, float value)
    {
        RefreshTargetMaterials();

        foreach (Material material in runtimeMaterials)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }
    }

    private void SetClampedMaterialFloat(string propertyName, float value, float min, float max, Slider slider)
    {
        float clampedValue = Mathf.Clamp(value, min, max);
        SetFloatOnTargets(propertyName, clampedValue);
        SetSliderValue(slider, clampedValue);
        UpdateReadout();
    }

    private void SyncControlValuesFromFirstMaterial()
    {
        Material material = runtimeMaterials.Count > 0 ? runtimeMaterials[0] : null;
        if (material == null)
        {
            return;
        }

        SetSliderValue(autoShadeColorSlider, GetMaterialFloat(material, "_AutoShadeColor", 1f));
        SetSliderValue(shadeSaturationSlider, GetMaterialFloat(material, "_ShadeSaturation", 0.25f));
        SetSliderValue(litColorBoostSlider, GetMaterialFloat(material, "_LitColorBoost", 0.08f));
        SetSliderValue(baseShadowStepSlider, GetMaterialFloat(material, "_BaseStep", 0.55f));
        SetSliderValue(shadowFeatherSlider, GetMaterialFloat(material, "_BaseFeather", 0.01f));
        SetSliderValue(shadeStepSlider, GetMaterialFloat(material, "_ShadeStep", 0.3f));
        SetSliderValue(shadeMapStrengthSlider, GetMaterialFloat(material, "_ShadeMapStrength", 0f));
        SetSliderValue(controlMapStrengthSlider, GetMaterialFloat(material, "_ControlMapStrength", 0f));
        SetSliderValue(rampTextureStrengthSlider, GetMaterialFloat(material, "_RampTextureStrength", 0f));
        SetSliderValue(shadowAntiFlickerSlider, GetMaterialFloat(material, "_ShadowAntiFlicker", 0.35f));
        SetSliderValue(useVirtualLightSlider, GetMaterialFloat(material, "_UseVirtualLight", 1f));
        SetSliderValue(lightColorInfluenceSlider, GetMaterialFloat(material, "_LightColorInfluence", 0.25f));
        SetSliderValue(ambientStrengthSlider, GetMaterialFloat(material, "_AmbientStrength", 0.18f));
        SetSliderValue(additionalLightInfluenceSlider, GetMaterialFloat(material, "_AdditionalLightInfluence", 0.15f));
        SetSliderValue(outlineWidthSlider, GetMaterialFloat(material, "_OutlineWidth", 0.008f));
        SetSliderValue(outlineMinWidthSlider, GetMaterialFloat(material, "_OutlineMinWidth", 0.0001f));
        SetSliderValue(outlineMaxWidthSlider, GetMaterialFloat(material, "_OutlineMaxWidth", 0.08f));
        SetSliderValue(outlineMiterStrengthSlider, GetMaterialFloat(material, "_OutlineMiterStrength", 1f));
        SetSliderValue(outlineVertexNormalBlendSlider, GetMaterialFloat(material, "_OutlineVertexNormalBlend", 1f));
        SetSliderValue(outlineDirectionalBiasSlider, GetMaterialFloat(material, "_OutlineDirectionalBias", 0f));
        SetSliderValue(specularSizeSlider, GetMaterialFloat(material, "_SpecularSize", 0.1f));
        SetSliderValue(specularFeatherSlider, GetMaterialFloat(material, "_SpecularFeather", 0.02f));
        SetSliderValue(rimPowerSlider, GetMaterialFloat(material, "_RimPower", 3f));
        SetSliderValue(rimThresholdSlider, GetMaterialFloat(material, "_RimThreshold", 0.55f));
        SetSliderValue(rimFeatherSlider, GetMaterialFloat(material, "_RimFeather", 0.08f));
        SetSliderValue(rimLightAlignSlider, GetMaterialFloat(material, "_RimLightAlign", 0.45f));
        SetSliderValue(rimIntensitySlider, GetMaterialFloat(material, "_RimIntensity", 0.5f));
        SetSliderValue(specularIntensitySlider, GetMaterialFloat(material, "_SpecularIntensity", 1f));

        if (stylePostProcessFeature != null)
        {
            SetSliderValue(posterizeStrengthSlider, stylePostProcessFeature.settings.posterizeStrength);
            SetSliderValue(postProcessContrastSlider, stylePostProcessFeature.settings.contrast);
            SetSliderValue(postProcessSaturationSlider, stylePostProcessFeature.settings.saturation);
            SetSliderValue(shadowCrushSlider, stylePostProcessFeature.settings.shadowCrush);
            SetSliderValue(halftoneStrengthSlider, stylePostProcessFeature.settings.halftoneStrength);
            SetSliderValue(halftoneScaleSlider, stylePostProcessFeature.settings.halftoneScale);
            if (postProcessToggle != null)
            {
                postProcessToggle.SetIsOnWithoutNotify(stylePostProcessFeature.settings.enabled);
            }
        }
    }

    private static float GetMaterialFloat(Material material, string propertyName, float fallback)
    {
        return material.HasProperty(propertyName) ? material.GetFloat(propertyName) : fallback;
    }

    private void SetSliderValue(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
            UpdateSliderLabel(slider);
        }
    }

    private void BuildGeneratedUi()
    {
        if (uiBuilt)
        {
            return;
        }

        Canvas canvas = EnsureCanvasInfrastructure();

        panelRoot = CreateUiObject("NPR Runtime UI Panel", canvas.transform);
        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = panelOffset;
        panelRect.sizeDelta = panelSize;

        Image panelImage = panelRoot.AddComponent<Image>();
        panelImage.color = new Color(0.045f, 0.05f, 0.06f, 0.93f);

        collapsedSummaryRoot = CreateUiObject("Collapsed Summary", panelRoot.transform);
        RectTransform collapsedRect = collapsedSummaryRoot.GetComponent<RectTransform>();
        collapsedRect.anchorMin = Vector2.zero;
        collapsedRect.anchorMax = Vector2.one;
        collapsedRect.offsetMin = new Vector2(10f, 10f);
        collapsedRect.offsetMax = new Vector2(-10f, -10f);
        HorizontalLayoutGroup collapsedLayout = collapsedSummaryRoot.AddComponent<HorizontalLayoutGroup>();
        collapsedLayout.spacing = 8f;
        collapsedLayout.childAlignment = TextAnchor.MiddleCenter;
        collapsedLayout.childControlWidth = true;
        collapsedLayout.childControlHeight = true;
        collapsedLayout.childForceExpandWidth = false;
        collapsedLayout.childForceExpandHeight = false;
        Button expandButton = CreateButton(collapsedSummaryRoot.transform, "Open", ToggleCollapsed);
        AddLayoutElement(expandButton.gameObject, 60f, 34f);
        collapsedFpsText = CreateText(collapsedSummaryRoot.transform, "FPS: --", 13, FontStyle.Bold, TextAnchor.MiddleLeft, 30f);
        collapsedSummaryRoot.SetActive(false);

        expandedContentRoot = CreateUiObject("Expanded Content", panelRoot.transform);
        RectTransform expandedRect = expandedContentRoot.GetComponent<RectTransform>();
        expandedRect.anchorMin = Vector2.zero;
        expandedRect.anchorMax = Vector2.one;
        expandedRect.offsetMin = new Vector2(14f, 12f);
        expandedRect.offsetMax = new Vector2(-14f, -12f);
        VerticalLayoutGroup layout = expandedContentRoot.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        GameObject headerRow = CreateUiObject("Header Row", expandedContentRoot.transform);
        HorizontalLayoutGroup headerLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 8f;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = false;
        AddLayoutElement(headerRow, -1f, 34f);
        CreateText(headerRow.transform, "NPR Runtime UI", 18, FontStyle.Bold, TextAnchor.MiddleLeft, 30f);
        Button collapseButton = CreateButton(headerRow.transform, "Hide", ToggleCollapsed);
        AddLayoutElement(collapseButton.gameObject, 72f, 34f);

        selectedObjectText = CreateText(expandedContentRoot.transform, "Selected: " + SelectedObjectName, 13, FontStyle.Normal, TextAnchor.MiddleLeft, 22f);
        presetNameText = CreateText(expandedContentRoot.transform, "Preset: " + CurrentPresetName, 14, FontStyle.Normal, TextAnchor.MiddleLeft, 24f);

        GameObject commandRow = CreateUiObject("Command Row", expandedContentRoot.transform);
        HorizontalLayoutGroup commandLayout = commandRow.AddComponent<HorizontalLayoutGroup>();
        commandLayout.spacing = 6f;
        commandLayout.childControlWidth = true;
        commandLayout.childControlHeight = true;
        commandLayout.childForceExpandWidth = true;
        AddLayoutElement(commandRow, -1f, 34f);
        CreateButton(commandRow.transform, "Next Preset", ApplyNextPreset);
        CreateButton(commandRow.transform, "New Material", () => CreateMaterialForSelection());
        CreatePresetButtons(expandedContentRoot.transform);

        Transform controlsRoot = CreateScrollContent(expandedContentRoot.transform);
        CreateSectionLabel(controlsRoot, "Palette");
        autoShadeColorSlider = CreateSlider(controlsRoot, "Auto Shade", 0f, 1f, 1f, SetAutoShadeColor);
        shadeSaturationSlider = CreateSlider(controlsRoot, "Shade Saturation", 0f, 1f, 0.25f, SetShadeSaturation);
        litColorBoostSlider = CreateSlider(controlsRoot, "Lit Boost", 0f, 0.5f, 0.08f, SetLitColorBoost);

        CreateSectionLabel(controlsRoot, "Asset Maps");
        shadeMapStrengthSlider = CreateSlider(controlsRoot, "Shade Map", 0f, 1f, 0f, SetShadeMapStrength);
        controlMapStrengthSlider = CreateSlider(controlsRoot, "Control Map", 0f, 1f, 0f, SetControlMapStrength);
        rampTextureStrengthSlider = CreateSlider(controlsRoot, "Ramp Texture", 0f, 1f, 0f, SetRampTextureStrength);
        shadowAntiFlickerSlider = CreateSlider(controlsRoot, "Shadow Anti-Flicker", 0f, 1f, 0.35f, SetShadowAntiFlicker);

        CreateSectionLabel(controlsRoot, "Shading");
        baseShadowStepSlider = CreateSlider(controlsRoot, "Base Shadow Step", 0f, 1f, 0.55f, SetBaseShadowStep);
        shadowFeatherSlider = CreateSlider(controlsRoot, "Shadow Feather", 0f, 0.5f, 0.01f, SetShadowFeather);
        shadeStepSlider = CreateSlider(controlsRoot, "2nd Shadow Step", 0f, 1f, 0.3f, SetShadeStep);
        useVirtualLightSlider = CreateSlider(controlsRoot, "Virtual Light", 0f, 1f, 1f, SetUseVirtualLight);
        lightColorInfluenceSlider = CreateSlider(controlsRoot, "Light Color Influence", 0f, 1f, 0.25f, SetLightColorInfluence);
        ambientStrengthSlider = CreateSlider(controlsRoot, "Ambient", 0f, 1f, 0.18f, SetAmbientStrength);
        additionalLightInfluenceSlider = CreateSlider(controlsRoot, "Additional Lights", 0f, 1f, 0.15f, SetAdditionalLightInfluence);

        CreateSectionLabel(controlsRoot, "Specular");
        specularSizeSlider = CreateSlider(controlsRoot, "Specular Size", 0f, 1f, 0.1f, SetSpecularSize);
        specularFeatherSlider = CreateSlider(controlsRoot, "Specular Feather", 0f, 0.25f, 0.02f, SetSpecularFeather);
        specularIntensitySlider = CreateSlider(controlsRoot, "Specular Intensity", 0f, 2f, 1f, SetSpecularIntensity);

        CreateSectionLabel(controlsRoot, "Rim Light");
        rimPowerSlider = CreateSlider(controlsRoot, "Rim Power", 0.1f, 8f, 3f, SetRimPower);
        rimThresholdSlider = CreateSlider(controlsRoot, "Rim Threshold", 0f, 1f, 0.55f, SetRimThreshold);
        rimFeatherSlider = CreateSlider(controlsRoot, "Rim Feather", 0f, 0.5f, 0.08f, SetRimFeather);
        rimLightAlignSlider = CreateSlider(controlsRoot, "Rim Light Align", 0f, 1f, 0.45f, SetRimLightAlign);
        rimIntensitySlider = CreateSlider(controlsRoot, "Rim Intensity", 0f, 2f, 0.5f, SetRimIntensity);

        CreateSectionLabel(controlsRoot, "Outline");
        outlineWidthSlider = CreateSlider(controlsRoot, "Outline Width", 0f, 0.1f, 0.008f, SetOutlineWidth);
        outlineMinWidthSlider = CreateSlider(controlsRoot, "Outline Min", 0f, 0.01f, 0.0001f, SetOutlineMinWidth);
        outlineMaxWidthSlider = CreateSlider(controlsRoot, "Outline Max", 0f, 0.1f, 0.08f, SetOutlineMaxWidth);
        outlineMiterStrengthSlider = CreateSlider(controlsRoot, "Miter Strength", 0f, 1f, 1f, SetOutlineMiterStrength);
        outlineVertexNormalBlendSlider = CreateSlider(controlsRoot, "Vertex Normal Blend", 0f, 1f, 1f, SetOutlineVertexNormalBlend);
        outlineDirectionalBiasSlider = CreateSlider(controlsRoot, "Directional Bias", -1f, 1f, 0f, SetOutlineDirectionalBias);

        CreateSectionLabel(controlsRoot, "Post Process");
        postProcessToggle = CreateToggle(controlsRoot, "Post FX", SetPostProcessEnabled);
        posterizeStrengthSlider = CreateSlider(controlsRoot, "Posterize", 0f, 1f, 0.82f, SetPosterizeStrength);
        postProcessContrastSlider = CreateSlider(controlsRoot, "Contrast", 0.5f, 2.5f, 1.34f, SetPostProcessContrast);
        postProcessSaturationSlider = CreateSlider(controlsRoot, "Saturation", 0f, 2f, 1.38f, SetPostProcessSaturation);
        shadowCrushSlider = CreateSlider(controlsRoot, "Shadow Crush", 0f, 1f, 0.22f, SetShadowCrush);
        halftoneStrengthSlider = CreateSlider(controlsRoot, "Pattern Blend", 0f, 1f, 0f, SetHalftoneStrength);
        halftoneScaleSlider = CreateSlider(controlsRoot, "Pattern Scale", 1f, 50f, 10f, SetHalftoneScale);

        fpsText = CreateText(expandedContentRoot.transform, "FPS: --", 13, FontStyle.Normal, TextAnchor.MiddleLeft, 24f);
        uiBuilt = true;
        SetCollapsed(startCollapsed || isCollapsed);
    }

    private Canvas EnsureCanvasInfrastructure()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (canvas.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(canvas.transform.root, false);
        }

        return canvas;
    }

    private Transform CreateScrollContent(Transform parent)
    {
        GameObject scrollObject = CreateUiObject("Controls Scroll View", parent);
        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0.025f, 0.028f, 0.035f, 0.72f);

        float preferredScrollHeight = Mathf.Max(240f, panelSize.y - 220f);
        LayoutElement scrollLayout = AddLayoutElement(scrollObject, -1f, preferredScrollHeight);
        scrollLayout.flexibleHeight = 1f;

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 28f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewportObject = CreateUiObject("Viewport", scrollObject.transform);
        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(8f, 8f);
        viewportRect.offsetMax = new Vector2(-8f, -8f);

        GameObject contentObject = CreateUiObject("Content", viewportObject.transform);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup contentLayout = contentObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 6f;
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        return contentObject.transform;
    }

    private Text CreateSectionLabel(Transform parent, string label)
    {
        Text text = CreateText(parent, label, 12, FontStyle.Bold, TextAnchor.MiddleLeft, 22f);
        text.color = new Color(0.65f, 0.82f, 0.92f, 1f);
        return text;
    }

    private void CreatePresetButtons(Transform parent)
    {
        if (stylePresets == null || stylePresets.Length == 0)
        {
            return;
        }

        GameObject row = CreateUiObject("Preset Buttons", parent);
        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 6f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = false;
        AddLayoutElement(row, -1f, 36f);

        for (int i = 0; i < stylePresets.Length; i++)
        {
            int presetIndex = i;
            string label = stylePresets[i] != null ? ShortenPresetName(stylePresets[i].DisplayName) : $"Preset {i + 1}";
            CreateButton(row.transform, label, () => ApplyPreset(presetIndex));
        }
    }

    private static string ShortenPresetName(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            return "Preset";
        }

        int localizedPartIndex = displayName.IndexOf('（');
        return localizedPartIndex > 0 ? displayName.Substring(0, localizedPartIndex).Trim() : displayName;
    }

    private Text CreateText(Transform parent, string content, int fontSize, FontStyle fontStyle, TextAnchor alignment, float height)
    {
        GameObject textObject = CreateUiObject("Text", parent);
        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.alignment = alignment;
        AddLayoutElement(textObject, -1f, height);
        return text;
    }

    private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateUiObject("Button_" + label, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.22f, 0.28f, 0.96f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        Text labelText = CreateText(buttonObject.transform, label, 13, FontStyle.Normal, TextAnchor.MiddleCenter, 30f);
        RectTransform labelRect = labelText.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        AddLayoutElement(buttonObject, -1f, 34f);
        return button;
    }

    private Slider CreateSlider(Transform parent, string label, float min, float max, float initialValue, UnityEngine.Events.UnityAction<float> onValueChanged)
    {
        GameObject row = CreateUiObject("Slider_" + label, parent);
        VerticalLayoutGroup rowLayout = row.AddComponent<VerticalLayoutGroup>();
        rowLayout.spacing = 2f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        AddLayoutElement(row, -1f, 48f);

        GameObject labelRow = CreateUiObject("Label", row.transform);
        HorizontalLayoutGroup labelLayout = labelRow.AddComponent<HorizontalLayoutGroup>();
        labelLayout.childControlWidth = true;
        labelLayout.childForceExpandWidth = true;
        AddLayoutElement(labelRow, -1f, 18f);

        CreateText(labelRow.transform, label, 12, FontStyle.Normal, TextAnchor.MiddleLeft, 18f);
        Text valueText = CreateText(labelRow.transform, initialValue.ToString("0.###"), 12, FontStyle.Normal, TextAnchor.MiddleRight, 18f);

        GameObject sliderObject = CreateUiObject("Control", row.transform);
        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = initialValue;
        valueLabels.Add(valueText);
        valueLabelsBySlider[slider] = valueText;
        slider.onValueChanged.AddListener(value =>
        {
            valueText.text = value.ToString("0.###");
            onValueChanged(value);
        });

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(0f, 22f);
        AddLayoutElement(sliderObject, -1f, 22f);

        Image background = CreateSliderImage(sliderObject.transform, "Background", new Color(0.13f, 0.15f, 0.18f, 1f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image fill = CreateSliderImage(sliderObject.transform, "Fill", new Color(0.35f, 0.75f, 0.95f, 1f));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0.25f);
        fillRect.anchorMax = new Vector2(0.5f, 0.75f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        slider.fillRect = fillRect;

        Image handle = CreateSliderImage(sliderObject.transform, "Handle", new Color(0.95f, 0.96f, 0.98f, 1f));
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(16f, 20f);
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;
        return slider;
    }

    private static Image CreateSliderImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = CreateUiObject(name, parent);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static void SetChildGraphicsEnabled(GameObject root, bool enabled)
    {
        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
        {
            graphic.enabled = enabled;
        }
    }

    private Toggle CreateToggle(Transform parent, string label, UnityEngine.Events.UnityAction<bool> onValueChanged)
    {
        GameObject toggleObject = CreateUiObject("Toggle_" + label, parent);
        HorizontalLayoutGroup layout = toggleObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        AddLayoutElement(toggleObject, -1f, 28f);

        GameObject boxObject = CreateUiObject("Box", toggleObject.transform);
        Image boxImage = boxObject.AddComponent<Image>();
        boxImage.color = new Color(0.18f, 0.22f, 0.28f, 1f);
        AddLayoutElement(boxObject, 22f, 22f);

        GameObject checkObject = CreateUiObject("Checkmark", boxObject.transform);
        Image checkImage = checkObject.AddComponent<Image>();
        checkImage.color = new Color(0.35f, 0.95f, 0.65f, 1f);
        RectTransform checkRect = checkObject.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.targetGraphic = boxImage;
        toggle.graphic = checkImage;
        toggle.onValueChanged.AddListener(onValueChanged);

        CreateText(toggleObject.transform, label, 12, FontStyle.Normal, TextAnchor.MiddleLeft, 24f);
        return toggle;
    }

    private void WireExistingControls()
    {
        WireSlider(autoShadeColorSlider, SetAutoShadeColor);
        WireSlider(shadeSaturationSlider, SetShadeSaturation);
        WireSlider(litColorBoostSlider, SetLitColorBoost);
        WireSlider(baseShadowStepSlider, SetBaseShadowStep);
        WireSlider(shadowFeatherSlider, SetShadowFeather);
        WireSlider(shadeStepSlider, SetShadeStep);
        WireSlider(shadeMapStrengthSlider, SetShadeMapStrength);
        WireSlider(controlMapStrengthSlider, SetControlMapStrength);
        WireSlider(rampTextureStrengthSlider, SetRampTextureStrength);
        WireSlider(shadowAntiFlickerSlider, SetShadowAntiFlicker);
        WireSlider(useVirtualLightSlider, SetUseVirtualLight);
        WireSlider(lightColorInfluenceSlider, SetLightColorInfluence);
        WireSlider(ambientStrengthSlider, SetAmbientStrength);
        WireSlider(additionalLightInfluenceSlider, SetAdditionalLightInfluence);
        WireSlider(specularSizeSlider, SetSpecularSize);
        WireSlider(specularFeatherSlider, SetSpecularFeather);
        WireSlider(specularIntensitySlider, SetSpecularIntensity);
        WireSlider(rimPowerSlider, SetRimPower);
        WireSlider(rimThresholdSlider, SetRimThreshold);
        WireSlider(rimFeatherSlider, SetRimFeather);
        WireSlider(rimLightAlignSlider, SetRimLightAlign);
        WireSlider(rimIntensitySlider, SetRimIntensity);
        WireSlider(outlineWidthSlider, SetOutlineWidth);
        WireSlider(outlineMinWidthSlider, SetOutlineMinWidth);
        WireSlider(outlineMaxWidthSlider, SetOutlineMaxWidth);
        WireSlider(outlineMiterStrengthSlider, SetOutlineMiterStrength);
        WireSlider(outlineVertexNormalBlendSlider, SetOutlineVertexNormalBlend);
        WireSlider(outlineDirectionalBiasSlider, SetOutlineDirectionalBias);
        WireSlider(posterizeStrengthSlider, SetPosterizeStrength);
        WireSlider(postProcessContrastSlider, SetPostProcessContrast);
        WireSlider(postProcessSaturationSlider, SetPostProcessSaturation);
        WireSlider(shadowCrushSlider, SetShadowCrush);
        WireSlider(halftoneStrengthSlider, SetHalftoneStrength);
        WireSlider(halftoneScaleSlider, SetHalftoneScale);

        if (postProcessToggle != null)
        {
            postProcessToggle.onValueChanged.RemoveListener(SetPostProcessEnabled);
            postProcessToggle.onValueChanged.AddListener(SetPostProcessEnabled);
        }
    }

    private static void WireSlider(Slider slider, UnityEngine.Events.UnityAction<float> action)
    {
        if (slider == null)
        {
            return;
        }

        slider.onValueChanged.RemoveListener(action);
        slider.onValueChanged.AddListener(action);
    }

    private void UpdateReadout()
    {
        if (selectedObjectText != null)
        {
            selectedObjectText.text = "Selected: " + SelectedObjectName;
        }

        if (presetNameText != null)
        {
            presetNameText.text = "Preset: " + CurrentPresetName;
        }

        UpdateAllSliderLabels();
    }

    private void UpdateAllSliderLabels()
    {
        foreach (KeyValuePair<Slider, Text> entry in valueLabelsBySlider)
        {
            if (entry.Key != null && entry.Value != null)
            {
                entry.Value.text = entry.Key.value.ToString("0.###");
            }
        }
    }

    private void UpdateSliderLabel(Slider slider)
    {
        if (slider != null && valueLabelsBySlider.TryGetValue(slider, out Text valueLabel) && valueLabel != null)
        {
            valueLabel.text = slider.value.ToString("0.###");
        }
    }

    private void UpdateFps()
    {
        if (fpsText == null && collapsedFpsText == null)
        {
            return;
        }

        fpsTimer += Time.unscaledDeltaTime;
        fpsDelta += (Time.unscaledDeltaTime - fpsDelta) * 0.1f;

        if (fpsTimer < 0.25f)
        {
            return;
        }

        fpsTimer = 0f;
        float fps = fpsDelta > 0f ? 1f / fpsDelta : 0f;
        string fpsLabel = $"FPS: {fps:0}";
        if (fpsText != null)
        {
            fpsText.text = fpsLabel;
        }

        if (collapsedFpsText != null)
        {
            collapsedFpsText.text = fpsLabel;
        }
    }

    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private static LayoutElement AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
    {
        LayoutElement element = target.GetComponent<LayoutElement>();
        if (element == null)
        {
            element = target.AddComponent<LayoutElement>();
        }

        if (preferredWidth >= 0f)
        {
            element.preferredWidth = preferredWidth;
        }

        if (preferredHeight >= 0f)
        {
            element.preferredHeight = preferredHeight;
        }

        return element;
    }
}
