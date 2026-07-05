using NUnit.Framework;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class ToonMaterialPresetTests
{
    [Test]
    public void SoftAnimeCelPresetWritesCelShadingParameters()
    {
        Material material = CreateToonMaterial();

        ToonMaterialPreset.Apply(ToonMaterialPresetKind.SoftAnimeCel, material);

        Assert.That(material.GetFloat("_AutoShadeColor"), Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_BaseStep"), Is.EqualTo(0.53f).Within(0.0001f));
        Assert.That(material.GetFloat("_BaseFeather"), Is.EqualTo(0.025f).Within(0.0001f));
        Assert.That(material.GetFloat("_ShadeFeather"), Is.EqualTo(0.018f).Within(0.0001f));
        Assert.That(material.GetFloat("_OutlineWidth"), Is.EqualTo(0.006f).Within(0.0001f));
        Assert.That(material.GetColor("_1stShadeTint").r, Is.EqualTo(0.86f).Within(0.0001f));
        Assert.That(material.GetColor("_OutlineColor").r, Is.EqualTo(0.06f).Within(0.0001f));

        Object.DestroyImmediate(material);
    }

    [Test]
    public void PresetNamesExposeReadableInspectorOptions()
    {
        string[] presetNames = ToonMaterialPreset.GetPresetNames();

        Assert.That(presetNames, Does.Contain("Soft Anime Cel（柔和赛璐璐）"));
        Assert.That(presetNames, Does.Contain("Hard Ink Cel（硬边墨线）"));
        Assert.That(presetNames, Does.Contain("Bold Ink Comic（粗线彩色漫画）"));
        Assert.That(presetNames, Does.Contain("Graphic Action Anime（热血图形动画）"));

        foreach (string presetName in presetNames)
        {
            Assert.That(presetName, Does.Not.Contain("鏌"));
            Assert.That(presetName, Does.Not.Contain("纭"));
            Assert.That(presetName, Does.Not.Contain("绋"));
        }
    }

    [Test]
    public void BoldInkComicStylePresetWritesMaterialAndPostProcessParameters()
    {
        NPRStylePreset preset = ScriptableObject.CreateInstance<NPRStylePreset>();
        preset.UseBoldInkComicDefaults();
        Material material = CreateToonMaterial();
        NPRStylePostProcessFeature.StylePostProcessSettings postProcess = new NPRStylePostProcessFeature.StylePostProcessSettings();

        preset.ApplyTo(material);
        preset.ApplyTo(postProcess);

        Assert.That(preset.DisplayName, Is.EqualTo("Bold Ink Comic（粗线彩色漫画）"));
        Assert.That(material.GetFloat("_BaseFeather"), Is.EqualTo(0.004f).Within(0.0001f));
        Assert.That(material.GetFloat("_ShadeFeather"), Is.EqualTo(0.003f).Within(0.0001f));
        Assert.That(material.GetFloat("_OutlineWidth"), Is.EqualTo(0.011f).Within(0.0001f));
        Assert.That(material.GetFloat("_RimIntensity"), Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(postProcess.enabled, Is.True);
        Assert.That(postProcess.posterizeStrength, Is.EqualTo(0.82f).Within(0.0001f));
        Assert.That(postProcess.patternType, Is.EqualTo(NPRStylePostProcessFeature.PatternShadingType.Dots));
        Assert.That(postProcess.halftoneStrength, Is.EqualTo(0.65f).Within(0.0001f));
        Assert.That(postProcess.halftoneScale, Is.EqualTo(10f).Within(0.0001f));
        Assert.That(postProcess.patternLumaThreshold, Is.EqualTo(0.5f).Within(0.0001f));

        Object.DestroyImmediate(material);
        Object.DestroyImmediate(preset);
    }

    [Test]
    public void GraphicActionAnimeStylePresetWritesMaterialAndPostProcessParameters()
    {
        NPRStylePreset preset = ScriptableObject.CreateInstance<NPRStylePreset>();
        preset.UseGraphicActionAnimeDefaults();
        Material material = CreateToonMaterial();
        NPRStylePostProcessFeature.StylePostProcessSettings postProcess = new NPRStylePostProcessFeature.StylePostProcessSettings();

        preset.ApplyTo(material);
        preset.ApplyTo(postProcess);

        Assert.That(preset.DisplayName, Is.EqualTo("Graphic Action Anime（热血图形动画）"));
        Assert.That(material.GetFloat("_BaseFeather"), Is.EqualTo(0.006f).Within(0.0001f));
        Assert.That(material.GetFloat("_SpecularIntensity"), Is.EqualTo(1.65f).Within(0.0001f));
        Assert.That(material.GetFloat("_RimIntensity"), Is.EqualTo(1.35f).Within(0.0001f));
        Assert.That(material.GetColor("_RimColor").r, Is.GreaterThan(0.95f));
        Assert.That(postProcess.enabled, Is.True);
        Assert.That(postProcess.shadowCrush, Is.EqualTo(0.36f).Within(0.0001f));
        Assert.That(postProcess.halftoneStrength, Is.EqualTo(0.0f).Within(0.0001f));
        Assert.That(postProcess.patternType, Is.EqualTo(NPRStylePostProcessFeature.PatternShadingType.Hatching));
        Assert.That(postProcess.accentColor.r, Is.GreaterThan(0.95f));

        Object.DestroyImmediate(material);
        Object.DestroyImmediate(preset);
    }

    [Test]
    public void StylePostProcessSettingsExposeIndependentEffectToggles()
    {
        System.Type settingsType = typeof(NPRStylePostProcessFeature.StylePostProcessSettings);
        System.Type presetType = typeof(NPRStylePreset.PostProcessSettings);

        Assert.That(settingsType.GetField("outlineEnabled"), Is.Not.Null);
        Assert.That(settingsType.GetField("halftoneEnabled"), Is.Not.Null);
        Assert.That(settingsType.GetField("colorGradingEnabled"), Is.Not.Null);
        Assert.That(settingsType.GetField("outlineColor"), Is.Not.Null);
        Assert.That(settingsType.GetField("outlineIntensity"), Is.Not.Null);
        Assert.That(settingsType.GetField("outlineThickness"), Is.Not.Null);
        Assert.That(settingsType.GetField("depthThreshold"), Is.Not.Null);
        Assert.That(settingsType.GetField("normalThreshold"), Is.Not.Null);
        Assert.That(settingsType.GetField("colorThreshold"), Is.Not.Null);
        Assert.That(settingsType.GetField("useDepth"), Is.Not.Null);
        Assert.That(settingsType.GetField("useNormals"), Is.Not.Null);
        Assert.That(settingsType.GetField("useColor"), Is.Not.Null);

        Assert.That(presetType.GetField("outlineEnabled"), Is.Not.Null);
        Assert.That(presetType.GetField("halftoneEnabled"), Is.Not.Null);
        Assert.That(presetType.GetField("colorGradingEnabled"), Is.Not.Null);
    }

    [Test]
    public void StylePostProcessSettingsExposeCelLookPatternControls()
    {
        System.Type settingsType = typeof(NPRStylePostProcessFeature.StylePostProcessSettings);
        System.Type presetType = typeof(NPRStylePreset.PostProcessSettings);

        Assert.That(settingsType.GetField("patternType"), Is.Not.Null);
        Assert.That(settingsType.GetField("patternAngle"), Is.Not.Null);
        Assert.That(settingsType.GetField("patternIntensity"), Is.Not.Null);
        Assert.That(settingsType.GetField("patternColor"), Is.Not.Null);
        Assert.That(settingsType.GetField("patternLumaThreshold"), Is.Not.Null);

        Assert.That(presetType.GetField("patternType"), Is.Not.Null);
        Assert.That(presetType.GetField("patternAngle"), Is.Not.Null);
        Assert.That(presetType.GetField("patternIntensity"), Is.Not.Null);
        Assert.That(presetType.GetField("patternColor"), Is.Not.Null);
        Assert.That(presetType.GetField("patternLumaThreshold"), Is.Not.Null);
    }

    [Test]
    public void StylePostProcessShaderDeclaresScreenSpaceOutlineInputs()
    {
        string source = ReadStylePostProcessShaderSource();

        Assert.That(source, Does.Contain("DeclareDepthTexture.hlsl"));
        Assert.That(source, Does.Contain("DeclareNormalsTexture.hlsl"));
        Assert.That(source, Does.Contain("_OutlineEnabled"));
        Assert.That(source, Does.Contain("_OutlineIntensity"));
        Assert.That(source, Does.Contain("_UseDepth"));
        Assert.That(source, Does.Contain("_UseNormals"));
        Assert.That(source, Does.Contain("_UseColor"));
    }

    [Test]
    public void StylePostProcessShaderUsesReferenceStylePatternShadingInputs()
    {
        string source = ReadStylePostProcessShaderSource();

        Assert.That(source, Does.Contain("_PatternType"));
        Assert.That(source, Does.Contain("_PatternAngle"));
        Assert.That(source, Does.Contain("_PatternIntensity"));
        Assert.That(source, Does.Contain("_PatternColor"));
        Assert.That(source, Does.Contain("_PatternLumaThreshold"));
        Assert.That(source, Does.Contain("ApplyPatternShading"));
        Assert.That(source, Does.Not.Contain("HalftoneMask"));
    }

    [Test]
    public void StylePostProcessShaderCompilesWithoutErrors()
    {
        Shader shader = Shader.Find("Hidden/NPR/StylePostProcess");
        Assert.NotNull(shader, "Expected the NPR style postprocess shader to be available.");

        string shaderPath = AssetDatabase.GetAssetPath(shader);
        Assert.That(shaderPath, Is.Not.Empty);
        AssetDatabase.ImportAsset(shaderPath, ImportAssetOptions.ForceUpdate);

        string[] errors = ShaderUtil.GetShaderMessages(shader)
            .Where(message => message.severity == ShaderCompilerMessageSeverity.Error)
            .Select(message => $"{message.message} ({message.file}:{message.line})")
            .ToArray();

        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }

    [Test]
    public void ToonShaderExposesAiAssistedAssetMapAndShadowStabilityProperties()
    {
        Material material = CreateToonMaterial();

        Assert.That(material.HasProperty("_ShadeMap"), Is.True);
        Assert.That(material.HasProperty("_ShadeMapStrength"), Is.True);
        Assert.That(material.HasProperty("_ControlMap"), Is.True);
        Assert.That(material.HasProperty("_ControlMapStrength"), Is.True);
        Assert.That(material.HasProperty("_ControlShadowStepRange"), Is.True);
        Assert.That(material.HasProperty("_ControlFeatherRange"), Is.True);
        Assert.That(material.HasProperty("_RampTexture"), Is.True);
        Assert.That(material.HasProperty("_RampTextureStrength"), Is.True);
        Assert.That(material.HasProperty("_ShadowAntiFlicker"), Is.True);

        Object.DestroyImmediate(material);
    }

    [Test]
    public void MaterialReplacementPreservesRendererSlotsAndSourceAlbedo()
    {
        GameObject root = new GameObject("NPR Material Replacement Root");
        MeshRenderer renderer = root.AddComponent<MeshRenderer>();
        Texture2D sourceTexture = new Texture2D(1, 1);
        Material sourceA = CreateSourceMaterial("Skin Source", new Color(0.8f, 0.55f, 0.45f, 1f), sourceTexture);
        Material sourceB = CreateSourceMaterial("Cloth Source", new Color(0.2f, 0.3f, 0.7f, 1f), null);
        NPRMaterialReplacementProfile profile = ScriptableObject.CreateInstance<NPRMaterialReplacementProfile>();

        renderer.sharedMaterials = new[] { sourceA, sourceB };

        NPRMaterialReplacementResult result = NPRMaterialReplacementUtility.ReplaceMaterials(
            root,
            profile,
            NPRMaterialReplacementOptions.CreateInMemoryForTests());

        Material[] replacedMaterials = renderer.sharedMaterials;
        Assert.That(result.RendererCount, Is.EqualTo(1));
        Assert.That(result.MaterialSlotCount, Is.EqualTo(2));
        Assert.That(result.CreatedMaterialCount, Is.EqualTo(2));
        Assert.That(replacedMaterials.Length, Is.EqualTo(2));
        Assert.That(replacedMaterials[0], Is.Not.SameAs(sourceA));
        Assert.That(replacedMaterials[1], Is.Not.SameAs(sourceB));
        Assert.That(replacedMaterials[0].shader.name, Is.EqualTo("Custom/NPR/Toon_Character_ImprovedOutline"));
        Assert.That(replacedMaterials[1].shader.name, Is.EqualTo("Custom/NPR/Toon_Character_ImprovedOutline"));
        Assert.That(replacedMaterials[0].GetTexture("_BaseMap"), Is.SameAs(sourceTexture));
        Assert.That(replacedMaterials[0].GetColor("_BaseColor").r, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(replacedMaterials[1].GetColor("_BaseColor").b, Is.EqualTo(0.7f).Within(0.0001f));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(sourceA);
        Object.DestroyImmediate(sourceB);
        Object.DestroyImmediate(replacedMaterials[0]);
        Object.DestroyImmediate(replacedMaterials[1]);
        Object.DestroyImmediate(sourceTexture);
        Object.DestroyImmediate(profile);
    }

    [Test]
    public void MaterialReplacementReusesGeneratedMaterialAcrossChildRenderers()
    {
        GameObject root = new GameObject("NPR Root");
        GameObject childA = new GameObject("Part A");
        GameObject childB = new GameObject("Part B");
        childA.transform.SetParent(root.transform);
        childB.transform.SetParent(root.transform);
        MeshRenderer rendererA = childA.AddComponent<MeshRenderer>();
        MeshRenderer rendererB = childB.AddComponent<MeshRenderer>();
        Material sharedSource = CreateSourceMaterial("Shared Source", Color.white, null);
        NPRMaterialReplacementProfile profile = ScriptableObject.CreateInstance<NPRMaterialReplacementProfile>();

        rendererA.sharedMaterials = new[] { sharedSource };
        rendererB.sharedMaterials = new[] { sharedSource };

        NPRMaterialReplacementResult result = NPRMaterialReplacementUtility.ReplaceMaterials(
            root,
            profile,
            NPRMaterialReplacementOptions.CreateInMemoryForTests());

        Assert.That(result.RendererCount, Is.EqualTo(2));
        Assert.That(result.MaterialSlotCount, Is.EqualTo(2));
        Assert.That(result.CreatedMaterialCount, Is.EqualTo(1));
        Assert.That(rendererA.sharedMaterial, Is.SameAs(rendererB.sharedMaterial));
        Assert.That(rendererA.sharedMaterial.shader.name, Is.EqualTo("Custom/NPR/Toon_Character_ImprovedOutline"));

        Material generatedMaterial = rendererA.sharedMaterial;
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(sharedSource);
        Object.DestroyImmediate(generatedMaterial);
        Object.DestroyImmediate(profile);
    }

    [Test]
    public void MaterialManagementUpdatesExistingNPRMaterialsWithoutOverwritingSurfaceIdentity()
    {
        GameObject root = new GameObject("NPR Management Root");
        MeshRenderer renderer = root.AddComponent<MeshRenderer>();
        Material nprMaterial = CreateToonMaterial();
        Texture2D sourceTexture = new Texture2D(1, 1);
        NPRMaterialReplacementProfile profile = ScriptableObject.CreateInstance<NPRMaterialReplacementProfile>();
        Color perMaterialColor = new Color(0.25f, 0.5f, 0.85f, 1f);

        nprMaterial.name = "Managed_NPR";
        nprMaterial.SetColor("_BaseColor", perMaterialColor);
        nprMaterial.SetTexture("_BaseMap", sourceTexture);
        nprMaterial.SetTextureScale("_BaseMap", new Vector2(2f, 3f));
        nprMaterial.SetTextureOffset("_BaseMap", new Vector2(0.1f, 0.2f));
        nprMaterial.SetFloat("_OutlineWidth", 0.001f);
        nprMaterial.SetFloat("_RimIntensity", 0.1f);
        renderer.sharedMaterials = new[] { nprMaterial };

        NPRMaterialManagementResult result = NPRMaterialReplacementUtility.UpdateExistingNPRMaterials(
            root,
            profile,
            NPRMaterialManagementOptions.CreateInMemoryForTests());

        Assert.That(result.RendererCount, Is.EqualTo(1));
        Assert.That(result.MaterialSlotCount, Is.EqualTo(1));
        Assert.That(result.ManagedMaterialCount, Is.EqualTo(1));
        Assert.That(result.SkippedMaterialCount, Is.EqualTo(0));
        Assert.That(result.CreatedMaterialCount, Is.EqualTo(0));
        Assert.That(renderer.sharedMaterial, Is.SameAs(nprMaterial));
        Assert.That(nprMaterial.GetFloat("_OutlineWidth"), Is.EqualTo(0.0095f).Within(0.0001f));
        Assert.That(nprMaterial.GetFloat("_RimIntensity"), Is.EqualTo(1.35f).Within(0.0001f));
        Assert.That(nprMaterial.GetColor("_BaseColor").r, Is.EqualTo(perMaterialColor.r).Within(0.0001f));
        Assert.That(nprMaterial.GetColor("_BaseColor").g, Is.EqualTo(perMaterialColor.g).Within(0.0001f));
        Assert.That(nprMaterial.GetColor("_BaseColor").b, Is.EqualTo(perMaterialColor.b).Within(0.0001f));
        Assert.That(nprMaterial.GetTexture("_BaseMap"), Is.SameAs(sourceTexture));
        Assert.That(nprMaterial.GetTextureScale("_BaseMap"), Is.EqualTo(new Vector2(2f, 3f)));
        Assert.That(nprMaterial.GetTextureOffset("_BaseMap"), Is.EqualTo(new Vector2(0.1f, 0.2f)));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(nprMaterial);
        Object.DestroyImmediate(sourceTexture);
        Object.DestroyImmediate(profile);
    }

    [Test]
    public void MaterialReplacementProfileCanSwitchToEveryBuiltInToonPreset()
    {
        NPRMaterialReplacementProfile profile = ScriptableObject.CreateInstance<NPRMaterialReplacementProfile>();
        Material material = CreateToonMaterial();

        profile.UseBuiltInPreset(ToonMaterialPresetKind.WarmComicCel);
        profile.ApplyStyleTo(material);

        Assert.That(profile.StylePreset, Is.Null);
        Assert.That(profile.FallbackPreset, Is.EqualTo(ToonMaterialPresetKind.WarmComicCel));
        Assert.That(profile.ActivePresetName, Is.EqualTo(ToonMaterialPreset.GetPresetName(ToonMaterialPresetKind.WarmComicCel)));
        Assert.That(material.GetFloat("_BaseStep"), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(material.GetFloat("_RimIntensity"), Is.EqualTo(0.4f).Within(0.0001f));

        Object.DestroyImmediate(material);
        Object.DestroyImmediate(profile);
    }

    private static Material CreateToonMaterial()
    {
        Shader shader = Shader.Find("Custom/NPR/Toon_Character_ImprovedOutline");
        Assert.NotNull(shader, "Expected the project toon shader to be available.");
        return new Material(shader);
    }

    private static Material CreateSourceMaterial(string materialName, Color baseColor, Texture texture)
    {
        Material material = CreateToonMaterial();
        material.name = materialName;
        material.SetColor("_BaseColor", baseColor);
        if (texture != null)
        {
            material.SetTexture("_BaseMap", texture);
        }

        return material;
    }

    private static string ReadImprovedToonShaderSource()
    {
        Shader shader = Shader.Find("Custom/NPR/Toon_Character_ImprovedOutline");
        Assert.NotNull(shader, "Expected the project toon shader to be available.");

        string shaderPath = AssetDatabase.GetAssetPath(shader);
        Assert.That(shaderPath, Is.Not.Empty);

        return File.ReadAllText(shaderPath);
    }

    private static string ReadStylePostProcessShaderSource()
    {
        Shader shader = Shader.Find("Hidden/NPR/StylePostProcess");
        Assert.NotNull(shader, "Expected the NPR style postprocess shader to be available.");

        string shaderPath = AssetDatabase.GetAssetPath(shader);
        Assert.That(shaderPath, Is.Not.Empty);

        return File.ReadAllText(shaderPath);
    }
}
