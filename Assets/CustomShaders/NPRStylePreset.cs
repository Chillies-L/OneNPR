using System;
using UnityEngine;

public enum NPRStylePresetKind
{
    BoldInkComic = 0,
    GraphicActionAnime = 1,
}

[CreateAssetMenu(fileName = "NPRStylePreset", menuName = "NPR/Style Preset")]
public class NPRStylePreset : ScriptableObject
{
    [SerializeField] private string displayName = "Bold Ink Comic（粗线彩色漫画）";
    [SerializeField] private NPRStylePresetKind styleKind = NPRStylePresetKind.BoldInkComic;
    [SerializeField] private ToonMaterialSettings material = ToonMaterialSettings.CreateBoldInkComic();
    [SerializeField] private PostProcessSettings postProcess = PostProcessSettings.CreateBoldInkComic();

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public NPRStylePresetKind StyleKind => styleKind;
    public ToonMaterialSettings Material => material;
    public PostProcessSettings PostProcess => postProcess;

    public void ApplyTo(Material target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        material.ApplyTo(target);
    }

    public void ApplyTo(NPRStylePostProcessFeature.StylePostProcessSettings target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        postProcess.ApplyTo(target);
    }

    public void UseBoldInkComicDefaults()
    {
        displayName = "Bold Ink Comic（粗线彩色漫画）";
        styleKind = NPRStylePresetKind.BoldInkComic;
        material = ToonMaterialSettings.CreateBoldInkComic();
        postProcess = PostProcessSettings.CreateBoldInkComic();
    }

    public void UseGraphicActionAnimeDefaults()
    {
        displayName = "Graphic Action Anime（热血图形动画）";
        styleKind = NPRStylePresetKind.GraphicActionAnime;
        material = ToonMaterialSettings.CreateGraphicActionAnime();
        postProcess = PostProcessSettings.CreateGraphicActionAnime();
    }

    public static ToonMaterialSettings CreateBoldInkComicMaterialSettings()
    {
        return ToonMaterialSettings.CreateBoldInkComic();
    }

    public static ToonMaterialSettings CreateGraphicActionAnimeMaterialSettings()
    {
        return ToonMaterialSettings.CreateGraphicActionAnime();
    }

    [Serializable]
    public class ToonMaterialSettings
    {
        public Color baseColor = Color.white;
        public Color firstShadeColor = new Color(0.72f, 0.7f, 0.78f, 1f);
        public Color secondShadeColor = new Color(0.34f, 0.31f, 0.45f, 1f);
        public float autoShadeColor = 1f;
        public Color firstShadeTint = new Color(0.78f, 0.74f, 0.92f, 1f);
        public Color secondShadeTint = new Color(0.42f, 0.38f, 0.66f, 1f);
        public float shadeSaturation = 0.4f;
        public float litColorBoost = 0.1f;
        public float baseStep = 0.56f;
        public float baseFeather = 0.01f;
        public float shadeStep = 0.33f;
        public float shadeFeather = 0.006f;
        public float useVirtualLight = 1f;
        public Vector4 virtualLightDirection = new Vector4(0.25f, 0.86f, 0.44f, 0f);
        public float lightColorInfluence = 0.08f;
        public float ambientStrength = 0.1f;
        public float additionalLightInfluence = 0.04f;
        public Color specularColor = Color.white;
        public float specularSize = 0.06f;
        public float specularFeather = 0.004f;
        public float specularIntensity = 1.15f;
        public Color rimColor = new Color(0.92f, 0.93f, 1f, 1f);
        public float rimPower = 2.6f;
        public float rimThreshold = 0.48f;
        public float rimFeather = 0.018f;
        public float rimLightAlign = 0.35f;
        public float rimIntensity = 0.48f;
        public float outlineWidth = 0.008f;
        public Color outlineColor = Color.black;
        public float outlineMode = 2f;
        public float outlineMinWidth = 0.0002f;
        public float outlineMaxWidth = 0.09f;
        public float outlineMiterStrength = 1f;
        public float outlineMiterMinDot = 0.35f;
        public float outlineMiterMaxScale = 1.8f;
        public float outlineUseVertexColorNormals = 1f;
        public float outlineVertexColorNormalFormat = 0f;
        public float outlineVertexNormalBlend = 1f;
        public float outlineRenderReversePass = 1f;
        public float outlineReverseWidthScale = 0.58f;
        public float outlineReverseZOffset = 0f;
        public float outlineUseWorldShell = 1f;
        public float outlineDirectionalBias = 0f;
        public Vector4 outlineBiasDirection = new Vector4(1f, 1f, 0f, 0f);
        public float outlineZOffset = 0.025f;

        public static ToonMaterialSettings CreateBoldInkComic()
        {
            return new ToonMaterialSettings
            {
                baseColor = new Color(1f, 0.92f, 0.72f, 1f),
                firstShadeColor = new Color(0.64f, 0.56f, 0.72f, 1f),
                secondShadeColor = new Color(0.22f, 0.18f, 0.34f, 1f),
                autoShadeColor = 1f,
                firstShadeTint = new Color(0.86f, 0.7f, 1f, 1f),
                secondShadeTint = new Color(0.38f, 0.26f, 0.68f, 1f),
                shadeSaturation = 0.72f,
                litColorBoost = 0.2f,
                baseStep = 0.58f,
                baseFeather = 0.004f,
                shadeStep = 0.33f,
                shadeFeather = 0.003f,
                useVirtualLight = 1f,
                virtualLightDirection = new Vector4(0.2f, 0.88f, 0.42f, 0f),
                lightColorInfluence = 0.05f,
                ambientStrength = 0.08f,
                additionalLightInfluence = 0.03f,
                specularColor = Color.white,
                specularSize = 0.075f,
                specularFeather = 0.003f,
                specularIntensity = 1.25f,
                rimColor = new Color(0.75f, 0.95f, 1f, 1f),
                rimPower = 2.2f,
                rimThreshold = 0.46f,
                rimFeather = 0.014f,
                rimLightAlign = 0.34f,
                rimIntensity = 0.75f,
                outlineWidth = 0.011f,
                outlineColor = Color.black,
                outlineMode = 2f,
                outlineMinWidth = 0.0002f,
                outlineMaxWidth = 0.095f,
                outlineReverseWidthScale = 0.62f,
                outlineZOffset = 0.035f,
            };
        }

        public static ToonMaterialSettings CreateGraphicActionAnime()
        {
            return new ToonMaterialSettings
            {
                baseColor = new Color(0.95f, 0.87f, 0.76f, 1f),
                firstShadeColor = new Color(0.55f, 0.32f, 0.3f, 1f),
                secondShadeColor = new Color(0.11f, 0.06f, 0.08f, 1f),
                autoShadeColor = 0.85f,
                firstShadeTint = new Color(1f, 0.45f, 0.28f, 1f),
                secondShadeTint = new Color(0.42f, 0.05f, 0.08f, 1f),
                shadeSaturation = 0.55f,
                litColorBoost = 0.16f,
                baseStep = 0.54f,
                baseFeather = 0.006f,
                shadeStep = 0.27f,
                shadeFeather = 0.004f,
                useVirtualLight = 1f,
                virtualLightDirection = new Vector4(-0.25f, 0.82f, 0.5f, 0f),
                lightColorInfluence = 0.06f,
                ambientStrength = 0.06f,
                additionalLightInfluence = 0.02f,
                specularColor = new Color(1f, 0.95f, 0.82f, 1f),
                specularSize = 0.052f,
                specularFeather = 0.004f,
                specularIntensity = 1.65f,
                rimColor = new Color(1f, 0.28f, 0.05f, 1f),
                rimPower = 2.0f,
                rimThreshold = 0.42f,
                rimFeather = 0.012f,
                rimLightAlign = 0.28f,
                rimIntensity = 1.35f,
                outlineWidth = 0.0095f,
                outlineColor = new Color(0.015f, 0.01f, 0.012f, 1f),
                outlineMode = 2f,
                outlineMinWidth = 0.0002f,
                outlineMaxWidth = 0.09f,
                outlineReverseWidthScale = 0.56f,
                outlineDirectionalBias = 0.12f,
                outlineBiasDirection = new Vector4(1f, -0.35f, 0f, 0f),
                outlineZOffset = 0.028f,
            };
        }

        public void ApplyTo(Material material)
        {
            SetColor(material, "_BaseColor", baseColor);
            SetColor(material, "_1stShadeColor", firstShadeColor);
            SetColor(material, "_2ndShadeColor", secondShadeColor);
            SetFloat(material, "_AutoShadeColor", autoShadeColor);
            SetColor(material, "_1stShadeTint", firstShadeTint);
            SetColor(material, "_2ndShadeTint", secondShadeTint);
            SetFloat(material, "_ShadeSaturation", shadeSaturation);
            SetFloat(material, "_LitColorBoost", litColorBoost);
            SetFloat(material, "_BaseStep", baseStep);
            SetFloat(material, "_BaseFeather", baseFeather);
            SetFloat(material, "_ShadeStep", shadeStep);
            SetFloat(material, "_ShadeFeather", shadeFeather);
            SetFloat(material, "_UseVirtualLight", useVirtualLight);
            SetVector(material, "_VirtualLightDirection", virtualLightDirection);
            SetFloat(material, "_LightColorInfluence", lightColorInfluence);
            SetFloat(material, "_AmbientStrength", ambientStrength);
            SetFloat(material, "_AdditionalLightInfluence", additionalLightInfluence);
            SetColor(material, "_SpecularColor", specularColor);
            SetFloat(material, "_SpecularSize", specularSize);
            SetFloat(material, "_SpecularFeather", specularFeather);
            SetFloat(material, "_SpecularIntensity", specularIntensity);
            SetColor(material, "_RimColor", rimColor);
            SetFloat(material, "_RimPower", rimPower);
            SetFloat(material, "_RimThreshold", rimThreshold);
            SetFloat(material, "_RimFeather", rimFeather);
            SetFloat(material, "_RimLightAlign", rimLightAlign);
            SetFloat(material, "_RimIntensity", rimIntensity);
            SetFloat(material, "_OutlineWidth", outlineWidth);
            SetColor(material, "_OutlineColor", outlineColor);
            SetFloat(material, "_OutlineMode", outlineMode);
            SetFloat(material, "_OutlineMinWidth", outlineMinWidth);
            SetFloat(material, "_OutlineMaxWidth", outlineMaxWidth);
            SetFloat(material, "_OutlineMiterStrength", outlineMiterStrength);
            SetFloat(material, "_OutlineMiterMinDot", outlineMiterMinDot);
            SetFloat(material, "_OutlineMiterMaxScale", outlineMiterMaxScale);
            SetFloat(material, "_OutlineUseVertexColorNormals", outlineUseVertexColorNormals);
            SetFloat(material, "_OutlineVertexColorNormalFormat", outlineVertexColorNormalFormat);
            SetFloat(material, "_OutlineVertexNormalBlend", outlineVertexNormalBlend);
            SetFloat(material, "_OutlineRenderReversePass", outlineRenderReversePass);
            SetFloat(material, "_OutlineReverseWidthScale", outlineReverseWidthScale);
            SetFloat(material, "_OutlineReverseZOffset", outlineReverseZOffset);
            SetFloat(material, "_OutlineUseWorldShell", outlineUseWorldShell);
            SetFloat(material, "_OutlineDirectionalBias", outlineDirectionalBias);
            SetVector(material, "_OutlineBiasDirection", outlineBiasDirection);
            SetFloat(material, "_OutlineZOffset", outlineZOffset);
        }
    }

    [Serializable]
    public class PostProcessSettings
    {
        public bool enabled = true;
        [Range(0f, 1f)] public float posterizeStrength = 0.75f;
        [Range(0.5f, 2.5f)] public float contrast = 1.25f;
        [Range(0f, 2f)] public float saturation = 1.25f;
        [Range(0f, 1f)] public float shadowCrush = 0.18f;
        [Range(0f, 1f)] public float halftoneStrength = 0.15f;
        [Range(8f, 160f)] public float halftoneScale = 72f;
        public Color accentColor = new Color(1f, 0.2f, 0.05f, 1f);

        public static PostProcessSettings CreateBoldInkComic()
        {
            return new PostProcessSettings
            {
                enabled = true,
                posterizeStrength = 0.82f,
                contrast = 1.34f,
                saturation = 1.38f,
                shadowCrush = 0.22f,
                halftoneStrength = 0.18f,
                halftoneScale = 68f,
                accentColor = new Color(0.1f, 0.85f, 1f, 1f),
            };
        }

        public static PostProcessSettings CreateGraphicActionAnime()
        {
            return new PostProcessSettings
            {
                enabled = true,
                posterizeStrength = 0.88f,
                contrast = 1.48f,
                saturation = 1.18f,
                shadowCrush = 0.36f,
                halftoneStrength = 0f,
                halftoneScale = 84f,
                accentColor = new Color(1f, 0.24f, 0.02f, 1f),
            };
        }

        public void ApplyTo(NPRStylePostProcessFeature.StylePostProcessSettings target)
        {
            target.enabled = enabled;
            target.posterizeStrength = posterizeStrength;
            target.contrast = contrast;
            target.saturation = saturation;
            target.shadowCrush = shadowCrush;
            target.halftoneStrength = halftoneStrength;
            target.halftoneScale = halftoneScale;
            target.accentColor = accentColor;
        }
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

