using System;
using UnityEngine;

public enum ToonMaterialPresetKind
{
    SoftAnimeCel = 0,
    HardInkCel = 1,
    HighContrastMono = 2,
    WarmComicCel = 3,
    ProceduralBackgroundCel = 4,
    BoldInkComic = 5,
    GraphicActionAnime = 6,
}

public static class ToonMaterialPreset
{
    private static readonly string[] PresetNames =
    {
        "Soft Anime Cel（柔和赛璐璐）",
        "Hard Ink Cel（硬边墨线）",
        "High Contrast Mono（黑白高对比）",
        "Warm Comic Cel（暖色漫画）",
        "Procedural Background Cel（程序化背景）",
        "Bold Ink Comic（粗线彩色漫画）",
        "Graphic Action Anime（热血图形动画）",
    };

    public static string[] GetPresetNames()
    {
        return (string[])PresetNames.Clone();
    }

    public static string GetPresetName(ToonMaterialPresetKind kind)
    {
        int index = (int)kind;
        if (index < 0 || index >= PresetNames.Length)
        {
            return kind.ToString();
        }

        return PresetNames[index];
    }

    public static void Apply(ToonMaterialPresetKind kind, Material material)
    {
        if (material == null)
        {
            throw new ArgumentNullException(nameof(material));
        }

        switch (kind)
        {
            case ToonMaterialPresetKind.SoftAnimeCel:
                ApplySoftAnimeCel(material);
                break;
            case ToonMaterialPresetKind.HardInkCel:
                ApplyHardInkCel(material);
                break;
            case ToonMaterialPresetKind.HighContrastMono:
                ApplyHighContrastMono(material);
                break;
            case ToonMaterialPresetKind.WarmComicCel:
                ApplyWarmComicCel(material);
                break;
            case ToonMaterialPresetKind.ProceduralBackgroundCel:
                ApplyProceduralBackgroundCel(material);
                break;
            case ToonMaterialPresetKind.BoldInkComic:
                NPRStylePreset.CreateBoldInkComicMaterialSettings().ApplyTo(material);
                break;
            case ToonMaterialPresetKind.GraphicActionAnime:
                NPRStylePreset.CreateGraphicActionAnimeMaterialSettings().ApplyTo(material);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown toon material preset.");
        }
    }

    private static void ApplySoftAnimeCel(Material material)
    {
        SetColor(material, "_BaseColor", new Color(1.0f, 0.98f, 0.95f, 1.0f));
        SetColor(material, "_1stShadeColor", new Color(0.82f, 0.80f, 0.88f, 1.0f));
        SetColor(material, "_2ndShadeColor", new Color(0.48f, 0.44f, 0.62f, 1.0f));
        SetFloat(material, "_AutoShadeColor", 1.0f);
        SetColor(material, "_1stShadeTint", new Color(0.86f, 0.78f, 0.95f, 1.0f));
        SetColor(material, "_2ndShadeTint", new Color(0.55f, 0.48f, 0.72f, 1.0f));
        SetFloat(material, "_ShadeSaturation", 0.28f);
        SetFloat(material, "_LitColorBoost", 0.08f);
        SetFloat(material, "_BaseStep", 0.53f);
        SetFloat(material, "_BaseFeather", 0.025f);
        SetFloat(material, "_ShadeStep", 0.31f);
        SetFloat(material, "_ShadeFeather", 0.018f);
        SetVirtualLighting(material, new Vector4(0.35f, 0.75f, 0.55f, 0.0f), 0.18f, 0.18f, 0.08f);
        SetSpecular(material, Color.white, 0.08f, 0.012f, 0.75f);
        SetRim(material, new Color(0.92f, 0.95f, 1.0f, 1.0f), 3.4f, 0.58f, 0.045f, 0.5f, 0.22f);
        SetOutline(material, 0.006f, new Color(0.06f, 0.055f, 0.06f, 1.0f), 2.0f, 0.0001f, 0.08f);
    }

    private static void ApplyHardInkCel(Material material)
    {
        SetColor(material, "_BaseColor", Color.white);
        SetColor(material, "_1stShadeColor", new Color(0.72f, 0.70f, 0.78f, 1.0f));
        SetColor(material, "_2ndShadeColor", new Color(0.34f, 0.31f, 0.45f, 1.0f));
        SetFloat(material, "_AutoShadeColor", 1.0f);
        SetColor(material, "_1stShadeTint", new Color(0.78f, 0.74f, 0.92f, 1.0f));
        SetColor(material, "_2ndShadeTint", new Color(0.42f, 0.38f, 0.66f, 1.0f));
        SetFloat(material, "_ShadeSaturation", 0.38f);
        SetFloat(material, "_LitColorBoost", 0.06f);
        SetFloat(material, "_BaseStep", 0.57f);
        SetFloat(material, "_BaseFeather", 0.006f);
        SetFloat(material, "_ShadeStep", 0.34f);
        SetFloat(material, "_ShadeFeather", 0.004f);
        SetVirtualLighting(material, new Vector4(0.25f, 0.86f, 0.44f, 0.0f), 0.08f, 0.1f, 0.04f);
        SetSpecular(material, Color.white, 0.06f, 0.004f, 1.15f);
        SetRim(material, new Color(0.92f, 0.93f, 1.0f, 1.0f), 2.6f, 0.48f, 0.018f, 0.35f, 0.48f);
        SetOutline(material, 0.008f, Color.black, 2.0f, 0.0002f, 0.09f);
    }

    private static void ApplyHighContrastMono(Material material)
    {
        SetColor(material, "_BaseColor", Color.white);
        SetColor(material, "_1stShadeColor", new Color(0.62f, 0.62f, 0.62f, 1.0f));
        SetColor(material, "_2ndShadeColor", new Color(0.08f, 0.08f, 0.08f, 1.0f));
        SetFloat(material, "_AutoShadeColor", 0.0f);
        SetColor(material, "_1stShadeTint", Color.white);
        SetColor(material, "_2ndShadeTint", Color.black);
        SetFloat(material, "_ShadeSaturation", 0.0f);
        SetFloat(material, "_LitColorBoost", 0.02f);
        SetFloat(material, "_BaseStep", 0.6f);
        SetFloat(material, "_BaseFeather", 0.012f);
        SetFloat(material, "_ShadeStep", 0.36f);
        SetFloat(material, "_ShadeFeather", 0.006f);
        SetVirtualLighting(material, new Vector4(0.25f, 0.85f, 0.45f, 0.0f), 0.05f, 0.12f, 0.05f);
        SetSpecular(material, Color.white, 0.05f, 0.005f, 1.5f);
        SetRim(material, new Color(0.9f, 0.9f, 1.0f, 1.0f), 2.5f, 0.45f, 0.03f, 0.25f, 0.8f);
        SetOutline(material, 0.008f, Color.black, 2.0f, 0.0002f, 0.09f);
    }

    private static void ApplyWarmComicCel(Material material)
    {
        SetColor(material, "_BaseColor", new Color(1.0f, 0.95f, 0.85f, 1.0f));
        SetColor(material, "_1stShadeColor", new Color(0.9f, 0.7f, 0.6f, 1.0f));
        SetColor(material, "_2ndShadeColor", new Color(0.6f, 0.4f, 0.3f, 1.0f));
        SetFloat(material, "_AutoShadeColor", 0.65f);
        SetColor(material, "_1stShadeTint", new Color(0.95f, 0.72f, 0.55f, 1.0f));
        SetColor(material, "_2ndShadeTint", new Color(0.7f, 0.35f, 0.25f, 1.0f));
        SetFloat(material, "_ShadeSaturation", 0.35f);
        SetFloat(material, "_LitColorBoost", 0.12f);
        SetFloat(material, "_BaseStep", 0.5f);
        SetFloat(material, "_BaseFeather", 0.01f);
        SetFloat(material, "_ShadeStep", 0.25f);
        SetFloat(material, "_ShadeFeather", 0.01f);
        SetVirtualLighting(material, new Vector4(0.45f, 0.75f, 0.45f, 0.0f), 0.15f, 0.15f, 0.1f);
        SetSpecular(material, Color.white, 0.1f, 0.01f, 0.8f);
        SetRim(material, new Color(1.0f, 1.0f, 0.9f, 1.0f), 4.0f, 0.6f, 0.04f, 0.35f, 0.4f);
        SetOutline(material, 0.007f, new Color(0.1f, 0.05f, 0.0f, 1.0f), 2.0f, 0.0001f, 0.085f);
    }

    private static void ApplyProceduralBackgroundCel(Material material)
    {
        SetColor(material, "_BaseColor", new Color(0.68f, 0.9f, 1.0f, 1.0f));
        SetColor(material, "_1stShadeColor", new Color(0.48f, 0.68f, 0.86f, 1.0f));
        SetColor(material, "_2ndShadeColor", new Color(0.26f, 0.42f, 0.66f, 1.0f));
        SetFloat(material, "_AutoShadeColor", 0.75f);
        SetColor(material, "_1stShadeTint", new Color(0.72f, 0.86f, 0.96f, 1.0f));
        SetColor(material, "_2ndShadeTint", new Color(0.42f, 0.60f, 0.84f, 1.0f));
        SetFloat(material, "_ShadeSaturation", 0.18f);
        SetFloat(material, "_LitColorBoost", 0.1f);
        SetFloat(material, "_BaseStep", 0.48f);
        SetFloat(material, "_BaseFeather", 0.08f);
        SetFloat(material, "_ShadeStep", 0.24f);
        SetFloat(material, "_ShadeFeather", 0.06f);
        SetVirtualLighting(material, new Vector4(0.2f, 0.9f, 0.35f, 0.0f), 0.25f, 0.28f, 0.12f);
        SetSpecular(material, new Color(0.9f, 1.0f, 1.0f, 1.0f), 0.18f, 0.04f, 0.35f);
        SetRim(material, new Color(0.8f, 0.95f, 1.0f, 1.0f), 4.8f, 0.68f, 0.08f, 0.45f, 0.18f);
        SetOutline(material, 0.002f, new Color(0.18f, 0.38f, 0.52f, 1.0f), 0.0f, 0.0f, 0.035f);
    }

    private static void SetVirtualLighting(Material material, Vector4 direction, float lightColorInfluence, float ambientStrength, float additionalLightInfluence)
    {
        SetFloat(material, "_UseVirtualLight", 1.0f);
        SetVector(material, "_VirtualLightDirection", direction);
        SetFloat(material, "_LightColorInfluence", lightColorInfluence);
        SetFloat(material, "_AmbientStrength", ambientStrength);
        SetFloat(material, "_AdditionalLightInfluence", additionalLightInfluence);
    }

    private static void SetSpecular(Material material, Color color, float size, float feather, float intensity)
    {
        SetColor(material, "_SpecularColor", color);
        SetFloat(material, "_SpecularSize", size);
        SetFloat(material, "_SpecularFeather", feather);
        SetFloat(material, "_SpecularIntensity", intensity);
    }

    private static void SetRim(Material material, Color color, float power, float threshold, float feather, float lightAlign, float intensity)
    {
        SetColor(material, "_RimColor", color);
        SetFloat(material, "_RimPower", power);
        SetFloat(material, "_RimThreshold", threshold);
        SetFloat(material, "_RimFeather", feather);
        SetFloat(material, "_RimLightAlign", lightAlign);
        SetFloat(material, "_RimIntensity", intensity);
    }

    private static void SetOutline(Material material, float width, Color color, float mode, float minWidth, float maxWidth)
    {
        SetFloat(material, "_OutlineWidth", width);
        SetColor(material, "_OutlineColor", color);
        SetFloat(material, "_OutlineMode", mode);
        SetFloat(material, "_OutlineMinWidth", minWidth);
        SetFloat(material, "_OutlineMaxWidth", maxWidth);
        SetFloat(material, "_OutlineMiterStrength", 1.0f);
        SetFloat(material, "_OutlineMiterMinDot", 0.35f);
        SetFloat(material, "_OutlineMiterMaxScale", 1.8f);
        SetFloat(material, "_OutlineUseVertexColorNormals", 1.0f);
        SetFloat(material, "_OutlineVertexColorNormalFormat", 0.0f);
        SetFloat(material, "_OutlineVertexNormalBlend", 1.0f);
        SetFloat(material, "_OutlineRenderReversePass", 1.0f);
        SetFloat(material, "_OutlineReverseWidthScale", 0.55f);
        SetFloat(material, "_OutlineReverseZOffset", 0.0f);
        SetFloat(material, "_OutlineUseWorldShell", 1.0f);
        SetFloat(material, "_OutlineDirectionalBias", 0.0f);
        SetVector(material, "_OutlineBiasDirection", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
        SetFloat(material, "_OutlineZOffset", 0.02f);
    }

    private static void SetFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    private static void SetColor(Material material, string propertyName, Color value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }

    private static void SetVector(Material material, string propertyName, Vector4 value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetVector(propertyName, value);
        }
    }
}
