using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPRMaterialReplacementProfile", menuName = "NPR/Material Replacement Profile")]
public class NPRMaterialReplacementProfile : ScriptableObject
{
    public const string DefaultToonShaderName = "Custom/NPR/Toon_Character_ImprovedOutline";

    [Header("Target NPR Shader")]
    [SerializeField] private Shader targetShader;
    [SerializeField] private NPRStylePreset stylePreset;
    [SerializeField] private ToonMaterialPresetKind fallbackPreset = ToonMaterialPresetKind.GraphicActionAnime;

    [Header("Generated Material Assets")]
    [SerializeField] private string outputFolder = "Assets/CustomShaders/GeneratedMaterials";

    [Header("Preserve Source Material Identity")]
    [SerializeField] private bool preserveBaseMap = true;
    [SerializeField] private bool preserveBaseColor = true;
    [SerializeField] private bool preserveTextureScaleAndOffset = true;

    [Header("Outline Normal Preparation")]
    [SerializeField] private bool processMeshFilterNormals = true;
    [SerializeField] private NPR_NormalsSmoother.VertexNormalColorFormat vertexColorFormat = NPR_NormalsSmoother.VertexNormalColorFormat.YSADirect;
    [SerializeField] private bool debugNormalsProcessing;

    [Header("Partial Replacement Rules")]
    [SerializeField] private List<NPRMaterialReplacementRule> rules = new List<NPRMaterialReplacementRule>();

    public Shader TargetShader => targetShader;
    public NPRStylePreset StylePreset => stylePreset;
    public ToonMaterialPresetKind FallbackPreset => fallbackPreset;
    public string ActivePresetName => stylePreset != null ? stylePreset.DisplayName : ToonMaterialPreset.GetPresetName(fallbackPreset);
    public string OutputFolder => string.IsNullOrWhiteSpace(outputFolder) ? "Assets/CustomShaders/GeneratedMaterials" : outputFolder;
    public bool PreserveBaseMap => preserveBaseMap;
    public bool PreserveBaseColor => preserveBaseColor;
    public bool PreserveTextureScaleAndOffset => preserveTextureScaleAndOffset;
    public bool ProcessMeshFilterNormals => processMeshFilterNormals;
    public NPR_NormalsSmoother.VertexNormalColorFormat VertexColorFormat => vertexColorFormat;
    public bool DebugNormalsProcessing => debugNormalsProcessing;
    public IReadOnlyList<NPRMaterialReplacementRule> Rules => rules;

    public Shader ResolveTargetShader()
    {
        return targetShader != null ? targetShader : Shader.Find(DefaultToonShaderName);
    }

    public void ApplyStyleTo(Material material)
    {
        if (material == null)
        {
            throw new ArgumentNullException(nameof(material));
        }

        if (stylePreset != null)
        {
            stylePreset.ApplyTo(material);
            return;
        }

        ToonMaterialPreset.Apply(fallbackPreset, material);
    }

    public void UseBuiltInPreset(ToonMaterialPresetKind preset)
    {
        stylePreset = null;
        fallbackPreset = preset;
    }

    public void UseStylePreset(NPRStylePreset preset)
    {
        stylePreset = preset;
    }

    public bool TryResolveRule(Material sourceMaterial, out Material replacementMaterial, out bool skip)
    {
        replacementMaterial = null;
        skip = false;

        if (sourceMaterial == null)
        {
            return false;
        }

        for (int i = 0; i < rules.Count; i++)
        {
            NPRMaterialReplacementRule rule = rules[i];
            if (rule == null || !rule.Matches(sourceMaterial))
            {
                continue;
            }

            skip = rule.Skip;
            replacementMaterial = rule.ReplacementMaterial;
            return true;
        }

        return false;
    }
}

[Serializable]
public class NPRMaterialReplacementRule
{
    [SerializeField] private Material sourceMaterial;
    [SerializeField] private Material replacementMaterial;
    [SerializeField] private bool skip;

    public Material SourceMaterial => sourceMaterial;
    public Material ReplacementMaterial => replacementMaterial;
    public bool Skip => skip;

    public bool Matches(Material material)
    {
        return material != null && material == sourceMaterial;
    }
}
