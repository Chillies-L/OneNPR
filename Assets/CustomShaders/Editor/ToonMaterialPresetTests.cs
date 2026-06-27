using NUnit.Framework;
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
        Assert.That(postProcess.halftoneStrength, Is.EqualTo(0.18f).Within(0.0001f));

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
        Assert.That(postProcess.accentColor.r, Is.GreaterThan(0.95f));

        Object.DestroyImmediate(material);
        Object.DestroyImmediate(preset);
    }

    private static Material CreateToonMaterial()
    {
        Shader shader = Shader.Find("Custom/NPR/Toon_Character_ImprovedOutline");
        Assert.NotNull(shader, "Expected the project toon shader to be available.");
        return new Material(shader);
    }
}
