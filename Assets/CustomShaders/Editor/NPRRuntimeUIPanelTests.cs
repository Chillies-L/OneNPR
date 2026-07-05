using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class NPRRuntimeUIPanelTests
{
    [Test]
    public void ApplyPresetWritesTargetMaterialsAndPostProcessSettings()
    {
        GameObject host = new GameObject("Runtime UI Host");
        NPRRuntimeUIPanel panel = host.AddComponent<NPRRuntimeUIPanel>();
        Material material = CreateToonMaterial();
        NPRStylePreset preset = ScriptableObject.CreateInstance<NPRStylePreset>();
        NPRStylePostProcessFeature postProcessFeature = ScriptableObject.CreateInstance<NPRStylePostProcessFeature>();

        preset.UseGraphicActionAnimeDefaults();
        panel.ConfigureForRuntime(new[] { material }, new[] { preset }, postProcessFeature);

        panel.ApplyPreset(0);

        Assert.That(panel.CurrentPresetName, Is.EqualTo("Graphic Action Anime（热血图形动画）"));
        Assert.That(material.GetFloat("_BaseStep"), Is.EqualTo(0.54f).Within(0.0001f));
        Assert.That(material.GetFloat("_OutlineWidth"), Is.EqualTo(0.0095f).Within(0.0001f));
        Assert.That(material.GetFloat("_RimIntensity"), Is.EqualTo(1.35f).Within(0.0001f));
        Assert.That(postProcessFeature.settings.enabled, Is.True);
        Assert.That(postProcessFeature.settings.shadowCrush, Is.EqualTo(0.36f).Within(0.0001f));
        Assert.That(postProcessFeature.settings.patternType, Is.EqualTo(NPRStylePostProcessFeature.PatternShadingType.Hatching));

        Object.DestroyImmediate(postProcessFeature);
        Object.DestroyImmediate(preset);
        Object.DestroyImmediate(material);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void RuntimeControlsClampAndWriteMaterialParameters()
    {
        GameObject host = new GameObject("Runtime UI Host");
        NPRRuntimeUIPanel panel = host.AddComponent<NPRRuntimeUIPanel>();
        Material material = CreateToonMaterial();

        panel.ConfigureForRuntime(new[] { material }, new NPRStylePreset[0], null);

        panel.SetBaseShadowStep(2.0f);
        panel.SetShadowFeather(-1.0f);
        panel.SetOutlineWidth(0.2f);
        panel.SetRimIntensity(3.0f);
        panel.SetSpecularIntensity(-0.5f);

        Assert.That(material.GetFloat("_BaseStep"), Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_BaseFeather"), Is.EqualTo(0.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_OutlineWidth"), Is.EqualTo(0.1f).Within(0.0001f));
        Assert.That(material.GetFloat("_RimIntensity"), Is.EqualTo(2.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_SpecularIntensity"), Is.EqualTo(0.0f).Within(0.0001f));

        Object.DestroyImmediate(material);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void RuntimeControlsClampAndEnablePatternShadingParameters()
    {
        GameObject host = new GameObject("Runtime UI Host");
        NPRRuntimeUIPanel panel = host.AddComponent<NPRRuntimeUIPanel>();
        NPRStylePostProcessFeature postProcessFeature = ScriptableObject.CreateInstance<NPRStylePostProcessFeature>();
        postProcessFeature.settings.halftoneEnabled = false;
        postProcessFeature.settings.patternType = NPRStylePostProcessFeature.PatternShadingType.Off;

        panel.ConfigureForRuntime(new Material[0], new NPRStylePreset[0], postProcessFeature);

        panel.SetHalftoneStrength(2.0f);
        panel.SetHalftoneScale(99.0f);

        Assert.That(postProcessFeature.settings.halftoneEnabled, Is.True);
        Assert.That(postProcessFeature.settings.patternType, Is.EqualTo(NPRStylePostProcessFeature.PatternShadingType.Dots));
        Assert.That(postProcessFeature.settings.halftoneStrength, Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(postProcessFeature.settings.halftoneScale, Is.EqualTo(50.0f).Within(0.0001f));

        panel.SetHalftoneScale(-5.0f);

        Assert.That(postProcessFeature.settings.halftoneScale, Is.EqualTo(1.0f).Within(0.0001f));

        Object.DestroyImmediate(postProcessFeature);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void RuntimeControlsExposeInspectorLevelMaterialParameters()
    {
        GameObject host = new GameObject("Runtime UI Host");
        NPRRuntimeUIPanel panel = host.AddComponent<NPRRuntimeUIPanel>();
        Material material = CreateToonMaterial();
        panel.ConfigureForRuntime(new[] { material }, new NPRStylePreset[0], null);

        InvokeSetter(panel, "SetShadeStep", 2.0f);
        InvokeSetter(panel, "SetShadeMapStrength", 2.0f);
        InvokeSetter(panel, "SetControlMapStrength", -1.0f);
        InvokeSetter(panel, "SetRampTextureStrength", 2.0f);
        InvokeSetter(panel, "SetShadowAntiFlicker", 2.0f);
        InvokeSetter(panel, "SetAmbientStrength", 2.0f);
        InvokeSetter(panel, "SetAdditionalLightInfluence", -1.0f);
        InvokeSetter(panel, "SetSpecularSize", 2.0f);
        InvokeSetter(panel, "SetRimThreshold", -1.0f);
        InvokeSetter(panel, "SetOutlineMinWidth", 0.2f);

        Assert.That(material.GetFloat("_ShadeStep"), Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_ShadeMapStrength"), Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_ControlMapStrength"), Is.EqualTo(0.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_RampTextureStrength"), Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_ShadowAntiFlicker"), Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_AmbientStrength"), Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_AdditionalLightInfluence"), Is.EqualTo(0.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_SpecularSize"), Is.EqualTo(1.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_RimThreshold"), Is.EqualTo(0.0f).Within(0.0001f));
        Assert.That(material.GetFloat("_OutlineMinWidth"), Is.EqualTo(0.01f).Within(0.0001f));

        Object.DestroyImmediate(material);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void SelectingRendererRestrictsEditingToSelectedObject()
    {
        GameObject host = new GameObject("Runtime UI Host");
        NPRRuntimeUIPanel panel = host.AddComponent<NPRRuntimeUIPanel>();
        Material firstMaterial = CreateToonMaterial();
        Material secondMaterial = CreateToonMaterial();
        MeshRenderer firstRenderer = new GameObject("First Target").AddComponent<MeshRenderer>();
        MeshRenderer secondRenderer = new GameObject("Second Target").AddComponent<MeshRenderer>();
        firstRenderer.sharedMaterial = firstMaterial;
        secondRenderer.sharedMaterial = secondMaterial;

        panel.ConfigureForRuntime(new[] { firstRenderer, secondRenderer }, new NPRStylePreset[0], null);
        panel.SelectRenderer(secondRenderer);
        panel.SetOutlineWidth(0.04f);

        Assert.That(panel.SelectedObjectName, Is.EqualTo("Second Target"));
        Assert.That(firstMaterial.GetFloat("_OutlineWidth"), Is.Not.EqualTo(0.04f).Within(0.0001f));
        Assert.That(secondRenderer.sharedMaterial.GetFloat("_OutlineWidth"), Is.EqualTo(0.04f).Within(0.0001f));

        Object.DestroyImmediate(firstMaterial);
        Object.DestroyImmediate(secondMaterial);
        Object.DestroyImmediate(firstRenderer.gameObject);
        Object.DestroyImmediate(secondRenderer.gameObject);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void CreateMaterialForSelectionClonesSelectedRendererMaterial()
    {
        GameObject host = new GameObject("Runtime UI Host");
        NPRRuntimeUIPanel panel = host.AddComponent<NPRRuntimeUIPanel>();
        Material sourceMaterial = CreateToonMaterial();
        MeshRenderer renderer = new GameObject("Selected Target").AddComponent<MeshRenderer>();
        renderer.sharedMaterial = sourceMaterial;

        panel.ConfigureForRuntime(new[] { renderer }, new NPRStylePreset[0], null);
        panel.SelectRenderer(renderer);
        Material createdMaterial = panel.CreateMaterialForSelection();
        panel.SetBaseShadowStep(0.25f);

        Assert.NotNull(createdMaterial);
        Assert.That(createdMaterial, Is.Not.SameAs(sourceMaterial));
        Assert.That(renderer.sharedMaterial, Is.SameAs(createdMaterial));
        Assert.That(sourceMaterial.GetFloat("_BaseStep"), Is.Not.EqualTo(0.25f).Within(0.0001f));
        Assert.That(createdMaterial.GetFloat("_BaseStep"), Is.EqualTo(0.25f).Within(0.0001f));

        Object.DestroyImmediate(sourceMaterial);
        Object.DestroyImmediate(createdMaterial);
        Object.DestroyImmediate(renderer.gameObject);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void GeneratedRuntimeUiCanCollapseWhileKeepingFpsVisible()
    {
        GameObject host = new GameObject("Runtime UI Host");
        NPRRuntimeUIPanel panel = host.AddComponent<NPRRuntimeUIPanel>();

        InvokePrivate(panel, "BuildGeneratedUi");
        MethodInfo setCollapsed = typeof(NPRRuntimeUIPanel).GetMethod("SetCollapsed", BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(setCollapsed, "Expected NPRRuntimeUIPanel to expose SetCollapsed(bool).");

        setCollapsed.Invoke(panel, new object[] { true });

        PropertyInfo isCollapsed = typeof(NPRRuntimeUIPanel).GetProperty("IsCollapsed", BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(isCollapsed, "Expected NPRRuntimeUIPanel to expose IsCollapsed.");
        Assert.That(isCollapsed.GetValue(panel), Is.EqualTo(true));

        GameObject panelRoot = GetPrivateField<GameObject>(panel, "panelRoot");
        Transform expandedContent = FindChildRecursive(panelRoot.transform, "Expanded Content");
        Transform collapsedSummary = FindChildRecursive(panelRoot.transform, "Collapsed Summary");

        Assert.That(panelRoot.activeSelf, Is.True);
        Assert.NotNull(expandedContent, "Expected generated UI to group full controls under Expanded Content.");
        Assert.NotNull(collapsedSummary, "Expected generated UI to expose a collapsed summary view.");
        Assert.That(expandedContent.gameObject.activeSelf, Is.False);
        Assert.That(collapsedSummary.gameObject.activeSelf, Is.True);
        Assert.That(HasVisibleFpsText(collapsedSummary), Is.True, "Collapsed summary should keep an FPS readout visible.");

        Object.DestroyImmediate(host);
    }

    private static Material CreateToonMaterial()
    {
        Shader shader = Shader.Find("Custom/NPR/Toon_Character_ImprovedOutline");
        Assert.NotNull(shader, "Expected the project toon shader to be available.");
        return new Material(shader);
    }

    private static void InvokeSetter(NPRRuntimeUIPanel panel, string methodName, float value)
    {
        MethodInfo method = typeof(NPRRuntimeUIPanel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(method, $"Expected NPRRuntimeUIPanel to expose {methodName}.");
        method.Invoke(panel, new object[] { value });
    }

    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method, $"Expected {target.GetType().Name} to define {methodName}.");
        method.Invoke(target, null);
    }

    private static T GetPrivateField<T>(object target, string fieldName) where T : class
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field, $"Expected {target.GetType().Name} to define {fieldName}.");
        return field.GetValue(target) as T;
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            Transform nestedMatch = FindChildRecursive(child, name);
            if (nestedMatch != null)
            {
                return nestedMatch;
            }
        }

        return null;
    }

    private static bool HasVisibleFpsText(Transform root)
    {
        foreach (Text text in root.GetComponentsInChildren<Text>(false))
        {
            if (text.text.StartsWith("FPS:"))
            {
                return true;
            }
        }

        return false;
    }
}
