using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class NPRAIAssetGenerationTests
{
    [Test]
    public void PromptBuilderCreatesAssetSpecificPromptsForAllToonTextureKinds()
    {
        Type promptBuilderType = GetProjectType("NPRAIAssetPromptBuilder");
        Type assetKindType = GetProjectType("NPRAIAssetKind");
        Type optionsType = GetProjectType("NPRAIAssetPromptOptions");
        MethodInfo buildMethod = promptBuilderType.GetMethod("BuildPrompt", BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(buildMethod, "Expected NPRAIAssetPromptBuilder.BuildPrompt to be public and static.");

        object options = Activator.CreateInstance(optionsType);
        SetField(optionsType, options, "subject", "anime character face");
        SetField(optionsType, options, "style", "clean cel shaded game asset");
        SetField(optionsType, options, "materialNotes", "skin and cloth material zones");

        string shadePrompt = BuildPrompt(buildMethod, assetKindType, "ShadeMap", options);
        string controlPrompt = BuildPrompt(buildMethod, assetKindType, "ControlMap", options);
        string rampPrompt = BuildPrompt(buildMethod, assetKindType, "RampTexture", options);

        Assert.That(shadePrompt, Does.Contain("Shade Map"));
        Assert.That(shadePrompt, Does.Contain("no text"));
        Assert.That(controlPrompt, Does.Contain("Control Map"));
        Assert.That(controlPrompt, Does.Contain("R channel"));
        Assert.That(controlPrompt, Does.Contain("G channel"));
        Assert.That(controlPrompt, Does.Contain("B channel"));
        Assert.That(rampPrompt, Does.Contain("Ramp Texture"));
        Assert.That(rampPrompt, Does.Contain("horizontal"));
    }

    [Test]
    public void RequestBuilderUsesConfigurableBaseUrlAndImageGenerationEndpoint()
    {
        Type requestBuilderType = GetProjectType("NPRAIImageGenerationRequest");
        MethodInfo endpointMethod = requestBuilderType.GetMethod("BuildEndpoint", BindingFlags.Public | BindingFlags.Static);
        MethodInfo bodyMethod = requestBuilderType.GetMethod("BuildJsonBody", BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(endpointMethod, "Expected BuildEndpoint to be public and static.");
        Assert.NotNull(bodyMethod, "Expected BuildJsonBody to be public and static.");

        string endpoint = (string)endpointMethod.Invoke(null, new object[] { "https://example.test/v1/" });
        string body = (string)bodyMethod.Invoke(null, new object[] { "gpt-image-2", "asset prompt", "1024x1024", "high" });

        Assert.That(endpoint, Is.EqualTo("https://example.test/v1/images/generations"));
        Assert.That(body, Does.Contain("\"model\":\"gpt-image-2\""));
        Assert.That(body, Does.Contain("\"prompt\":\"asset prompt\""));
        Assert.That(body, Does.Contain("\"size\":\"1024x1024\""));
        Assert.That(body, Does.Contain("\"output_format\":\"png\""));
        Assert.That(body, Does.Contain("\"n\":1"));
        Assert.That(body, Does.Not.Contain("response_format"));
    }

    [Test]
    public void MaterialClassifierRecognizesCommonCharacterMaterialNames()
    {
        Assert.That(NPRAIMaterialClassifier.GuessCategory("Body_SKIN"), Is.EqualTo(NPRAIMaterialCategory.Skin));
        Assert.That(NPRAIMaterialClassifier.GuessCategory("Hair_01"), Is.EqualTo(NPRAIMaterialCategory.Hair));
        Assert.That(NPRAIMaterialClassifier.GuessCategory("EyeIris"), Is.EqualTo(NPRAIMaterialCategory.Eye));
        Assert.That(NPRAIMaterialClassifier.GuessCategory("Shoes_CLOTH"), Is.EqualTo(NPRAIMaterialCategory.Cloth));
    }

    [Test]
    public void RecipeBuilderCreatesCategorySpecificSettingsWithoutChangingBaseIdentity()
    {
        Color baseColor = new Color(0.82f, 0.58f, 0.44f, 1f);
        NPRAIMaterialRecipe skinRecipe = NPRAIMaterialRecipeBuilder.Build(baseColor, NPRAIMaterialCategory.Skin);
        NPRAIMaterialRecipe hairRecipe = NPRAIMaterialRecipeBuilder.Build(baseColor, NPRAIMaterialCategory.Hair);
        NPRAIMaterialRecipe clothRecipe = NPRAIMaterialRecipeBuilder.Build(baseColor, NPRAIMaterialCategory.Cloth);
        Material material = CreateToonMaterial();
        Texture2D baseMap = new Texture2D(1, 1);

        material.SetColor("_BaseColor", baseColor);
        material.SetTexture("_BaseMap", baseMap);
        material.SetTextureScale("_BaseMap", new Vector2(2f, 3f));
        material.SetTextureOffset("_BaseMap", new Vector2(0.1f, 0.2f));
        skinRecipe.ApplyTo(material);

        Assert.That(skinRecipe.firstShadeColor, Is.Not.EqualTo(hairRecipe.firstShadeColor));
        Assert.That(skinRecipe.rimIntensity, Is.Not.EqualTo(clothRecipe.rimIntensity));
        Assert.That(material.GetColor("_BaseColor"), Is.EqualTo(baseColor));
        Assert.That(material.GetTexture("_BaseMap"), Is.SameAs(baseMap));
        Assert.That(material.GetTextureScale("_BaseMap"), Is.EqualTo(new Vector2(2f, 3f)));
        Assert.That(material.GetTextureOffset("_BaseMap"), Is.EqualTo(new Vector2(0.1f, 0.2f)));

        UnityEngine.Object.DestroyImmediate(material);
        UnityEngine.Object.DestroyImmediate(baseMap);
    }

    [Test]
    public void MaterialScannerCollapsesSharedMaterialsAcrossRendererSlots()
    {
        GameObject root = new GameObject("AI NPR Root");
        GameObject childA = new GameObject("Part A");
        GameObject childB = new GameObject("Part B");
        childA.transform.SetParent(root.transform);
        childB.transform.SetParent(root.transform);
        MeshRenderer rendererA = childA.AddComponent<MeshRenderer>();
        MeshRenderer rendererB = childB.AddComponent<MeshRenderer>();
        Material sharedSkin = CreateToonMaterial();
        Material hair = CreateToonMaterial();
        Shader targetShader = sharedSkin.shader;

        sharedSkin.name = "Body_SKIN";
        hair.name = "Hair_01";
        rendererA.sharedMaterials = new[] { sharedSkin, hair };
        rendererB.sharedMaterials = new[] { sharedSkin };

        List<NPRAIMaterialContext> contexts = NPRAIMaterialContextScanner.ScanUniqueMaterials(root, targetShader);

        Assert.That(contexts.Count, Is.EqualTo(2));
        Assert.That(contexts.Single(context => context.Material == sharedSkin).SharedSlotCount, Is.EqualTo(2));
        Assert.That(contexts.Single(context => context.Material == hair).SharedSlotCount, Is.EqualTo(1));
        Assert.That(contexts.All(context => context.IsTargetNPRShader), Is.True);

        UnityEngine.Object.DestroyImmediate(root);
        UnityEngine.Object.DestroyImmediate(sharedSkin);
        UnityEngine.Object.DestroyImmediate(hair);
    }

    [Test]
    public void RecipeApplierOnlyUpdatesTargetNprMaterials()
    {
        Material nprMaterial = CreateToonMaterial();
        Material nonNprMaterial = CreateNonToonMaterial();
        Shader targetShader = nprMaterial.shader;
        Color nprBaseColor = new Color(0.2f, 0.35f, 0.9f, 1f);
        Color nonNprBaseColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        NPRAIMaterialRecipe recipe = NPRAIMaterialRecipeBuilder.Build(nprBaseColor, NPRAIMaterialCategory.Cloth);

        nprMaterial.SetColor("_BaseColor", nprBaseColor);
        nonNprMaterial.SetColor("_BaseColor", nonNprBaseColor);

        List<NPRAIMaterialContext> contexts = new List<NPRAIMaterialContext>
        {
            NPRAIMaterialContext.FromMaterial(nprMaterial, targetShader, 1),
            NPRAIMaterialContext.FromMaterial(nonNprMaterial, targetShader, 1),
        };

        NPRAIMaterialRecipeApplyResult result = NPRAIMaterialRecipeApplier.ApplyRecipe(contexts, recipe, false);

        Assert.That(result.AppliedCount, Is.EqualTo(1));
        Assert.That(result.SkippedCount, Is.EqualTo(1));
        Assert.That(nprMaterial.GetFloat("_RimIntensity"), Is.EqualTo(recipe.rimIntensity).Within(0.0001f));
        Assert.That(nprMaterial.GetColor("_BaseColor"), Is.EqualTo(nprBaseColor));
        Assert.That(nonNprMaterial.GetColor("_BaseColor"), Is.EqualTo(nonNprBaseColor));

        UnityEngine.Object.DestroyImmediate(nprMaterial);
        UnityEngine.Object.DestroyImmediate(nonNprMaterial);
    }

    [Test]
    public void RampGeneratorCreatesSmallClampReadyRampAndAssignsItToMaterial()
    {
        Material material = CreateToonMaterial();
        NPRAIMaterialRecipe recipe = NPRAIMaterialRecipeBuilder.Build(new Color(0.85f, 0.62f, 0.48f, 1f), NPRAIMaterialCategory.Skin);

        Texture2D ramp = NPRAIRampTextureGenerator.CreateRampTexture(recipe);
        NPRAIRampTextureGenerator.ApplyRamp(material, ramp);

        Assert.That(ramp.width, Is.EqualTo(256));
        Assert.That(ramp.height, Is.EqualTo(16));
        Assert.That(ramp.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
        Assert.That(material.GetTexture("_RampTexture"), Is.SameAs(ramp));
        Assert.That(material.GetFloat("_RampTextureStrength"), Is.EqualTo(1f).Within(0.0001f));

        UnityEngine.Object.DestroyImmediate(material);
        UnityEngine.Object.DestroyImmediate(ramp);
    }

    [Test]
    public void MaterialAwarePromptsIncludeCategoryBaseColorAndUtilityMapConstraints()
    {
        Material material = CreateToonMaterial();
        material.name = "Body_SKIN";
        material.SetColor("_BaseColor", new Color(0.8f, 0.6f, 0.45f, 1f));
        NPRAIMaterialContext context = NPRAIMaterialContext.FromMaterial(material, material.shader, 1);
        NPRAIAssetPromptOptions options = new NPRAIAssetPromptOptions
        {
            subject = "anime character material",
            style = "clean cel shaded game asset",
            materialNotes = "skin zone",
        };

        string shadePrompt = NPRAIAssetPromptBuilder.BuildPrompt(NPRAIAssetKind.ShadeMap, options, context);
        string controlPrompt = NPRAIAssetPromptBuilder.BuildPrompt(NPRAIAssetKind.ControlMap, options, context);

        Assert.That(shadePrompt, Does.Contain("Material category: Skin"));
        Assert.That(shadePrompt, Does.Contain("Base Color: #CC9973"));
        Assert.That(shadePrompt, Does.Contain("not a character render"));
        Assert.That(controlPrompt, Does.Contain("R channel controls local shadow threshold"));
        Assert.That(controlPrompt, Does.Contain("A channel controls rim mask"));

        UnityEngine.Object.DestroyImmediate(material);
    }

    private static Type GetProjectType(string typeName)
    {
        Type type = typeof(NPRRuntimeUIPanel).Assembly.GetType(typeName);
        Assert.NotNull(type, $"Expected project type {typeName} to exist.");
        return type;
    }

    private static string BuildPrompt(MethodInfo buildMethod, Type assetKindType, string kindName, object options)
    {
        object kind = Enum.Parse(assetKindType, kindName);
        return (string)buildMethod.Invoke(null, new[] { kind, options });
    }

    private static void SetField(Type type, object target, string fieldName, object value)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(field, $"Expected prompt options field {fieldName}.");
        field.SetValue(target, value);
    }

    private static Material CreateToonMaterial()
    {
        Shader shader = Shader.Find("Custom/NPR/Toon_Character_ImprovedOutline");
        Assert.NotNull(shader, "Expected the project toon shader to be available.");
        return new Material(shader);
    }

    private static Material CreateNonToonMaterial()
    {
        string[] shaderNames =
        {
            "Universal Render Pipeline/Lit",
            "Standard",
            "Sprites/Default",
            "UI/Default",
        };

        foreach (string shaderName in shaderNames)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
            {
                return new Material(shader);
            }
        }

        Assert.Fail("Expected at least one non-toon built-in shader to be available.");
        return null;
    }
}
