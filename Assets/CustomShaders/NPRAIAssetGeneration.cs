using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum NPRAIAssetKind
{
    ShadeMap,
    ControlMap,
    RampTexture,
}

public enum NPRAIMaterialCategory
{
    Skin,
    Hair,
    Face,
    Eye,
    Cloth,
    Metal,
    Accessory,
    General,
}

[Serializable]
public class NPRAIAssetPromptOptions
{
    public string subject = "anime character material";
    public string style = "clean cel shaded game asset";
    public string materialNotes = "skin, hair, cloth, and accessory zones";
    public string resolution = "1024x1024";
}

public static class NPRAIAssetPromptBuilder
{
    public static string BuildPrompt(NPRAIAssetKind kind, NPRAIAssetPromptOptions options)
    {
        return BuildPrompt(kind, options, null);
    }

    public static string BuildPrompt(NPRAIAssetKind kind, NPRAIAssetPromptOptions options, NPRAIMaterialContext materialContext)
    {
        options ??= new NPRAIAssetPromptOptions();

        switch (kind)
        {
            case NPRAIAssetKind.ShadeMap:
                return BuildShadeMapPrompt(options, materialContext);
            case NPRAIAssetKind.ControlMap:
                return BuildControlMapPrompt(options, materialContext);
            case NPRAIAssetKind.RampTexture:
                return BuildRampTexturePrompt(options, materialContext);
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown NPR AI asset kind.");
        }
    }

    public static string GetRecommendedFileName(NPRAIAssetKind kind)
    {
        switch (kind)
        {
            case NPRAIAssetKind.ShadeMap:
                return "ai-shade-map.png";
            case NPRAIAssetKind.ControlMap:
                return "ai-control-map.png";
            case NPRAIAssetKind.RampTexture:
                return "ai-ramp-texture.png";
            default:
                return "ai-npr-asset.png";
        }
    }

    private static string BuildShadeMapPrompt(NPRAIAssetPromptOptions options, NPRAIMaterialContext materialContext)
    {
        return string.Join("\n",
            "Generate a Shade Map texture for a Unity URP toon shader.",
            $"Subject: {SafeText(options.subject)}.",
            $"Style: {SafeText(options.style)}.",
            $"Material notes: {SafeText(options.materialNotes)}.",
            BuildMaterialContextLine(materialContext),
            "Output should be a square, seamless-looking texture map, not a beauty render.",
            "This is a utility texture candidate, not a character render, not a pose, and not a perspective scene.",
            "Paint local anime shadow color variation only: cool hue shifts for recessed areas, warmer lit surfaces, no hard lighting direction baked into the image.",
            "Use smooth but readable regions that can multiply first and second shade colors in a cel shader.",
            "no text, no labels, no watermark, no UI, no character pose, no perspective scene.");
    }

    private static string BuildControlMapPrompt(NPRAIAssetPromptOptions options, NPRAIMaterialContext materialContext)
    {
        return string.Join("\n",
            "Generate a Control Map texture for a Unity URP toon shader.",
            $"Subject: {SafeText(options.subject)}.",
            $"Style: {SafeText(options.style)}.",
            $"Material notes: {SafeText(options.materialNotes)}.",
            BuildMaterialContextLine(materialContext),
            "Output must be a square utility texture map with clean channel-separated grayscale information.",
            "R channel controls local shadow threshold: white means larger shadow area, black means smaller shadow area.",
            "G channel controls shadow feather boost: white means softer edge, black means crisp cel edge.",
            "B channel controls specular mask: white allows highlights, black suppresses highlights.",
            "A channel controls rim mask: white allows rim light, black suppresses rim light.",
            "no text, no labels, no watermark, no colored illustration, no scene lighting.");
    }

    private static string BuildRampTexturePrompt(NPRAIAssetPromptOptions options, NPRAIMaterialContext materialContext)
    {
        return string.Join("\n",
            "Generate a Ramp Texture for a Unity URP toon shader.",
            $"Subject: {SafeText(options.subject)}.",
            $"Style: {SafeText(options.style)}.",
            BuildMaterialContextLine(materialContext),
            "Create a horizontal 1D-looking tonal ramp inside a square PNG.",
            "Left side is deep second shade, middle is first shade, right side is lit color.",
            "Keep bands stable and readable for cel shading, with optional narrow feather transitions between bands.",
            "no text, no labels, no watermark, no object, no perspective, no scene.");
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unspecified" : value.Trim();
    }

    private static string BuildMaterialContextLine(NPRAIMaterialContext materialContext)
    {
        if (materialContext == null)
        {
            return "Material context: unspecified.";
        }

        return $"Material name: {SafeText(materialContext.MaterialName)}. Material category: {materialContext.Category}. Base Color: {ColorToHex(materialContext.BaseColor)}. Has Base Map: {materialContext.HasBaseMap}.";
    }

    private static string ColorToHex(Color color)
    {
        int r = Mathf.RoundToInt(Mathf.Clamp01(color.r) * 255f);
        int g = Mathf.RoundToInt(Mathf.Clamp01(color.g) * 255f);
        int b = Mathf.RoundToInt(Mathf.Clamp01(color.b) * 255f);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}

public static class NPRAIMaterialClassifier
{
    public static NPRAIMaterialCategory GuessCategory(string materialName)
    {
        string name = string.IsNullOrWhiteSpace(materialName) ? string.Empty : materialName.ToLowerInvariant();

        if (ContainsAny(name, "eye", "iris", "pupil"))
        {
            return NPRAIMaterialCategory.Eye;
        }

        if (ContainsAny(name, "hair", "bang", "ponytail"))
        {
            return NPRAIMaterialCategory.Hair;
        }

        if (ContainsAny(name, "face", "mouth", "brow", "eyeline"))
        {
            return NPRAIMaterialCategory.Face;
        }

        if (ContainsAny(name, "skin", "body", "hand", "leg", "arm"))
        {
            return NPRAIMaterialCategory.Skin;
        }

        if (ContainsAny(name, "cloth", "clothes", "top", "bottom", "shoe", "skirt", "shirt", "jacket", "dress"))
        {
            return NPRAIMaterialCategory.Cloth;
        }

        if (ContainsAny(name, "metal", "steel", "iron", "gold", "silver"))
        {
            return NPRAIMaterialCategory.Metal;
        }

        if (ContainsAny(name, "accessory", "acc", "ribbon", "belt", "button"))
        {
            return NPRAIMaterialCategory.Accessory;
        }

        return NPRAIMaterialCategory.General;
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            if (value.Contains(tokens[i]))
            {
                return true;
            }
        }

        return false;
    }
}

public sealed class NPRAIMaterialSlotContext
{
    public NPRAIMaterialSlotContext(Renderer renderer, int slotIndex, Material material, Shader targetShader)
    {
        Renderer = renderer;
        SlotIndex = slotIndex;
        Material = material;
        MaterialName = material == null ? string.Empty : material.name;
        BaseColor = NPRAIMaterialContextUtility.GetBaseColor(material);
        BaseMap = NPRAIMaterialContextUtility.GetBaseMap(material);
        HasBaseMap = BaseMap != null;
        IsTargetNPRShader = NPRAIMaterialContextUtility.IsTargetNPRMaterial(material, targetShader);
    }

    public Renderer Renderer { get; }
    public int SlotIndex { get; }
    public Material Material { get; }
    public string MaterialName { get; }
    public Color BaseColor { get; }
    public Texture BaseMap { get; }
    public bool HasBaseMap { get; }
    public bool IsTargetNPRShader { get; }
}

public sealed class NPRAIMaterialContext
{
    private readonly List<NPRAIMaterialSlotContext> slots = new List<NPRAIMaterialSlotContext>();

    private NPRAIMaterialContext(Material material, Shader targetShader)
    {
        Material = material;
        MaterialName = material == null ? string.Empty : material.name;
        BaseColor = NPRAIMaterialContextUtility.GetBaseColor(material);
        BaseMap = NPRAIMaterialContextUtility.GetBaseMap(material);
        HasBaseMap = BaseMap != null;
        IsTargetNPRShader = NPRAIMaterialContextUtility.IsTargetNPRMaterial(material, targetShader);
        Category = NPRAIMaterialClassifier.GuessCategory(MaterialName);
    }

    public Material Material { get; }
    public string MaterialName { get; }
    public Color BaseColor { get; }
    public Texture BaseMap { get; }
    public bool HasBaseMap { get; }
    public bool IsTargetNPRShader { get; }
    public NPRAIMaterialCategory Category { get; set; }
    public int SharedSlotCount => slots.Count;
    public IReadOnlyList<NPRAIMaterialSlotContext> Slots => slots;

    public static NPRAIMaterialContext FromMaterial(Material material, Shader targetShader, int sharedSlotCount)
    {
        NPRAIMaterialContext context = new NPRAIMaterialContext(material, targetShader);
        int count = Mathf.Max(1, sharedSlotCount);
        for (int i = 0; i < count; i++)
        {
            context.slots.Add(new NPRAIMaterialSlotContext(null, i, material, targetShader));
        }

        return context;
    }

    public static NPRAIMaterialContext FromSlot(NPRAIMaterialSlotContext slot, Shader targetShader)
    {
        NPRAIMaterialContext context = new NPRAIMaterialContext(slot.Material, targetShader);
        context.AddSlot(slot);
        return context;
    }

    public void AddSlot(NPRAIMaterialSlotContext slot)
    {
        if (slot != null)
        {
            slots.Add(slot);
        }
    }
}

public static class NPRAIMaterialContextScanner
{
    public static List<NPRAIMaterialSlotContext> ScanSlots(GameObject root, Shader targetShader, bool includeInactive = true)
    {
        List<NPRAIMaterialSlotContext> slots = new List<NPRAIMaterialSlotContext>();
        if (root == null)
        {
            return slots;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive);
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Renderer renderer = renderers[rendererIndex];
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null)
            {
                continue;
            }

            for (int slotIndex = 0; slotIndex < materials.Length; slotIndex++)
            {
                if (materials[slotIndex] != null)
                {
                    slots.Add(new NPRAIMaterialSlotContext(renderer, slotIndex, materials[slotIndex], targetShader));
                }
            }
        }

        return slots;
    }

    public static List<NPRAIMaterialContext> ScanUniqueMaterials(GameObject root, Shader targetShader, bool includeInactive = true)
    {
        List<NPRAIMaterialSlotContext> slots = ScanSlots(root, targetShader, includeInactive);
        Dictionary<Material, NPRAIMaterialContext> byMaterial = new Dictionary<Material, NPRAIMaterialContext>();

        for (int i = 0; i < slots.Count; i++)
        {
            NPRAIMaterialSlotContext slot = slots[i];
            if (!byMaterial.TryGetValue(slot.Material, out NPRAIMaterialContext context))
            {
                context = NPRAIMaterialContext.FromSlot(slot, targetShader);
                byMaterial.Add(slot.Material, context);
            }
            else
            {
                context.AddSlot(slot);
            }
        }

        return new List<NPRAIMaterialContext>(byMaterial.Values);
    }
}

public static class NPRAIMaterialContextUtility
{
    private static readonly string[] BaseColorProperties =
    {
        "_BaseColor",
        "_Color",
        "_MainColor",
    };

    private static readonly string[] BaseMapProperties =
    {
        "_BaseMap",
        "_MainTex",
        "_BaseTexture",
    };

    public static bool IsTargetNPRMaterial(Material material, Shader targetShader)
    {
        return material != null && material.shader != null && targetShader != null && material.shader.name == targetShader.name;
    }

    public static Color GetBaseColor(Material material)
    {
        if (material == null)
        {
            return Color.white;
        }

        for (int i = 0; i < BaseColorProperties.Length; i++)
        {
            string propertyName = BaseColorProperties[i];
            if (material.HasProperty(propertyName))
            {
                return material.GetColor(propertyName);
            }
        }

        return Color.white;
    }

    public static Texture GetBaseMap(Material material)
    {
        if (material == null)
        {
            return null;
        }

        for (int i = 0; i < BaseMapProperties.Length; i++)
        {
            string propertyName = BaseMapProperties[i];
            if (material.HasProperty(propertyName))
            {
                Texture texture = material.GetTexture(propertyName);
                if (texture != null)
                {
                    return texture;
                }
            }
        }

        return null;
    }
}

public static class NPRAIImageGenerationRequest
{
    public const string DefaultModel = "gpt-image-2";
    public const string DefaultBaseUrl = "https://api.openai.com/v1";
    public const string ImageGenerationPath = "/images/generations";

    public static string BuildEndpoint(string baseUrl)
    {
        string normalizedBaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl.Trim();
        return normalizedBaseUrl.TrimEnd('/') + ImageGenerationPath;
    }

    public static string BuildJsonBody(string model, string prompt, string size, string quality)
    {
        StringBuilder body = new StringBuilder();
        body.Append('{');
        AppendJsonProperty(body, "model", string.IsNullOrWhiteSpace(model) ? DefaultModel : model.Trim(), true);
        AppendJsonProperty(body, "prompt", prompt ?? string.Empty, false);
        AppendJsonProperty(body, "size", string.IsNullOrWhiteSpace(size) ? "1024x1024" : size.Trim(), false);
        AppendJsonProperty(body, "quality", string.IsNullOrWhiteSpace(quality) ? "high" : quality.Trim(), false);
        AppendJsonProperty(body, "output_format", "png", false);
        AppendJsonNumberProperty(body, "n", 1, false);
        body.Append('}');
        return body.ToString();
    }

    private static void AppendJsonProperty(StringBuilder body, string name, string value, bool first)
    {
        if (!first)
        {
            body.Append(',');
        }

        body.Append('"');
        body.Append(EscapeJson(name));
        body.Append("\":\"");
        body.Append(EscapeJson(value));
        body.Append('"');
    }

    private static void AppendJsonNumberProperty(StringBuilder body, string name, int value, bool first)
    {
        if (!first)
        {
            body.Append(',');
        }

        body.Append('"');
        body.Append(EscapeJson(name));
        body.Append("\":");
        body.Append(value);
    }

    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}

[Serializable]
public sealed class NPRAIMaterialRecipe
{
    public Color litColor;
    public Color firstShadeColor;
    public Color secondShadeColor;
    public Color firstShadeTint;
    public Color secondShadeTint;
    public float shadeSaturation;
    public float litColorBoost;
    public float baseStep;
    public float shadeStep;
    public float baseFeather;
    public float shadeFeather;
    public Color specularColor;
    public float specularSize;
    public float specularFeather;
    public float specularIntensity;
    public Color rimColor;
    public float rimPower;
    public float rimThreshold;
    public float rimFeather;
    public float rimLightAlign;
    public float rimIntensity;
    public float outlineWidth;
    public Color outlineColor;
    public float outlineMode;
    public float outlineMinWidth;
    public float outlineMaxWidth;
    public float controlShadowStepRange;
    public float controlFeatherRange;
    public float shadowAntiFlicker;

    public void ApplyTo(Material material)
    {
        if (material == null)
        {
            return;
        }

        MaterialIdentity identity = MaterialIdentity.Capture(material);

        SetColor(material, "_1stShadeColor", firstShadeColor);
        SetColor(material, "_2ndShadeColor", secondShadeColor);
        SetFloat(material, "_AutoShadeColor", 1f);
        SetColor(material, "_1stShadeTint", firstShadeTint);
        SetColor(material, "_2ndShadeTint", secondShadeTint);
        SetFloat(material, "_ShadeSaturation", shadeSaturation);
        SetFloat(material, "_LitColorBoost", litColorBoost);
        SetFloat(material, "_BaseStep", baseStep);
        SetFloat(material, "_ShadeStep", shadeStep);
        SetFloat(material, "_BaseFeather", baseFeather);
        SetFloat(material, "_ShadeFeather", shadeFeather);
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
        SetFloat(material, "_ControlShadowStepRange", controlShadowStepRange);
        SetFloat(material, "_ControlFeatherRange", controlFeatherRange);
        SetFloat(material, "_ShadowAntiFlicker", shadowAntiFlicker);

        identity.Restore(material);
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

    private readonly struct MaterialIdentity
    {
        private readonly bool hasBaseColor;
        private readonly Color baseColor;
        private readonly bool hasBaseMap;
        private readonly Texture baseMap;
        private readonly Vector2 baseMapScale;
        private readonly Vector2 baseMapOffset;

        private MaterialIdentity(bool hasBaseColor, Color baseColor, bool hasBaseMap, Texture baseMap, Vector2 baseMapScale, Vector2 baseMapOffset)
        {
            this.hasBaseColor = hasBaseColor;
            this.baseColor = baseColor;
            this.hasBaseMap = hasBaseMap;
            this.baseMap = baseMap;
            this.baseMapScale = baseMapScale;
            this.baseMapOffset = baseMapOffset;
        }

        public static MaterialIdentity Capture(Material material)
        {
            bool hasColor = material.HasProperty("_BaseColor");
            bool hasMap = material.HasProperty("_BaseMap");
            return new MaterialIdentity(
                hasColor,
                hasColor ? material.GetColor("_BaseColor") : Color.white,
                hasMap,
                hasMap ? material.GetTexture("_BaseMap") : null,
                hasMap ? material.GetTextureScale("_BaseMap") : Vector2.one,
                hasMap ? material.GetTextureOffset("_BaseMap") : Vector2.zero);
        }

        public void Restore(Material material)
        {
            if (hasBaseColor && material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }

            if (hasBaseMap && material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", baseMap);
                material.SetTextureScale("_BaseMap", baseMapScale);
                material.SetTextureOffset("_BaseMap", baseMapOffset);
            }
        }
    }
}

public static class NPRAIMaterialRecipeBuilder
{
    public static NPRAIMaterialRecipe Build(Color baseColor, NPRAIMaterialCategory category)
    {
        CategoryTuning tuning = CategoryTuning.For(category);
        Color litColor = BoostColor(baseColor, tuning.litBoost);
        Color firstShade = Mix(Multiply(baseColor, tuning.firstShadeValue), tuning.firstShadeTint, tuning.tintBlend);
        Color secondShade = Mix(Multiply(baseColor, tuning.secondShadeValue), tuning.secondShadeTint, tuning.tintBlend + 0.08f);

        return new NPRAIMaterialRecipe
        {
            litColor = litColor,
            firstShadeColor = WithAlpha(firstShade, baseColor.a),
            secondShadeColor = WithAlpha(secondShade, baseColor.a),
            firstShadeTint = tuning.firstShadeTint,
            secondShadeTint = tuning.secondShadeTint,
            shadeSaturation = tuning.shadeSaturation,
            litColorBoost = tuning.litBoost,
            baseStep = tuning.baseStep,
            shadeStep = tuning.shadeStep,
            baseFeather = tuning.baseFeather,
            shadeFeather = tuning.shadeFeather,
            specularColor = tuning.specularColor,
            specularSize = tuning.specularSize,
            specularFeather = tuning.specularFeather,
            specularIntensity = tuning.specularIntensity,
            rimColor = tuning.rimColor,
            rimPower = tuning.rimPower,
            rimThreshold = tuning.rimThreshold,
            rimFeather = tuning.rimFeather,
            rimLightAlign = tuning.rimLightAlign,
            rimIntensity = tuning.rimIntensity,
            outlineWidth = tuning.outlineWidth,
            outlineColor = tuning.outlineColor,
            outlineMode = 2f,
            outlineMinWidth = 0.0002f,
            outlineMaxWidth = 0.09f,
            controlShadowStepRange = tuning.controlShadowStepRange,
            controlFeatherRange = tuning.controlFeatherRange,
            shadowAntiFlicker = tuning.shadowAntiFlicker,
        };
    }

    private static Color BoostColor(Color color, float boost)
    {
        return new Color(
            Mathf.Clamp01(color.r + boost),
            Mathf.Clamp01(color.g + boost),
            Mathf.Clamp01(color.b + boost),
            color.a);
    }

    private static Color Multiply(Color color, float value)
    {
        return new Color(
            Mathf.Clamp01(color.r * value),
            Mathf.Clamp01(color.g * value),
            Mathf.Clamp01(color.b * value),
            color.a);
    }

    private static Color Mix(Color a, Color b, float amount)
    {
        amount = Mathf.Clamp01(amount);
        return new Color(
            Mathf.Lerp(a.r, b.r, amount),
            Mathf.Lerp(a.g, b.g, amount),
            Mathf.Lerp(a.b, b.b, amount),
            Mathf.Lerp(a.a, b.a, amount));
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    private readonly struct CategoryTuning
    {
        public readonly float firstShadeValue;
        public readonly float secondShadeValue;
        public readonly Color firstShadeTint;
        public readonly Color secondShadeTint;
        public readonly float tintBlend;
        public readonly float shadeSaturation;
        public readonly float litBoost;
        public readonly float baseStep;
        public readonly float shadeStep;
        public readonly float baseFeather;
        public readonly float shadeFeather;
        public readonly Color specularColor;
        public readonly float specularSize;
        public readonly float specularFeather;
        public readonly float specularIntensity;
        public readonly Color rimColor;
        public readonly float rimPower;
        public readonly float rimThreshold;
        public readonly float rimFeather;
        public readonly float rimLightAlign;
        public readonly float rimIntensity;
        public readonly float outlineWidth;
        public readonly Color outlineColor;
        public readonly float controlShadowStepRange;
        public readonly float controlFeatherRange;
        public readonly float shadowAntiFlicker;

        private CategoryTuning(
            float firstShadeValue,
            float secondShadeValue,
            Color firstShadeTint,
            Color secondShadeTint,
            float tintBlend,
            float shadeSaturation,
            float litBoost,
            float baseStep,
            float shadeStep,
            float baseFeather,
            float shadeFeather,
            Color specularColor,
            float specularSize,
            float specularFeather,
            float specularIntensity,
            Color rimColor,
            float rimPower,
            float rimThreshold,
            float rimFeather,
            float rimLightAlign,
            float rimIntensity,
            float outlineWidth,
            Color outlineColor,
            float controlShadowStepRange,
            float controlFeatherRange,
            float shadowAntiFlicker)
        {
            this.firstShadeValue = firstShadeValue;
            this.secondShadeValue = secondShadeValue;
            this.firstShadeTint = firstShadeTint;
            this.secondShadeTint = secondShadeTint;
            this.tintBlend = tintBlend;
            this.shadeSaturation = shadeSaturation;
            this.litBoost = litBoost;
            this.baseStep = baseStep;
            this.shadeStep = shadeStep;
            this.baseFeather = baseFeather;
            this.shadeFeather = shadeFeather;
            this.specularColor = specularColor;
            this.specularSize = specularSize;
            this.specularFeather = specularFeather;
            this.specularIntensity = specularIntensity;
            this.rimColor = rimColor;
            this.rimPower = rimPower;
            this.rimThreshold = rimThreshold;
            this.rimFeather = rimFeather;
            this.rimLightAlign = rimLightAlign;
            this.rimIntensity = rimIntensity;
            this.outlineWidth = outlineWidth;
            this.outlineColor = outlineColor;
            this.controlShadowStepRange = controlShadowStepRange;
            this.controlFeatherRange = controlFeatherRange;
            this.shadowAntiFlicker = shadowAntiFlicker;
        }

        public static CategoryTuning For(NPRAIMaterialCategory category)
        {
            switch (category)
            {
                case NPRAIMaterialCategory.Skin:
                    return new CategoryTuning(0.78f, 0.48f, new Color(1f, 0.68f, 0.62f, 1f), new Color(0.72f, 0.34f, 0.42f, 1f), 0.22f, 0.42f, 0.08f, 0.54f, 0.28f, 0.018f, 0.01f, new Color(1f, 0.92f, 0.82f, 1f), 0.045f, 0.006f, 0.85f, new Color(1f, 0.76f, 0.65f, 1f), 3.1f, 0.52f, 0.035f, 0.35f, 0.38f, 0.0065f, new Color(0.08f, 0.045f, 0.04f, 1f), 0.15f, 0.05f, 0.35f);
                case NPRAIMaterialCategory.Hair:
                    return new CategoryTuning(0.62f, 0.32f, new Color(0.72f, 0.78f, 1f, 1f), new Color(0.32f, 0.25f, 0.62f, 1f), 0.28f, 0.62f, 0.05f, 0.57f, 0.34f, 0.008f, 0.004f, Color.white, 0.08f, 0.003f, 1.35f, new Color(0.65f, 0.88f, 1f, 1f), 2.4f, 0.46f, 0.012f, 0.28f, 0.82f, 0.0085f, Color.black, 0.18f, 0.035f, 0.28f);
                case NPRAIMaterialCategory.Face:
                    return new CategoryTuning(0.84f, 0.58f, new Color(1f, 0.72f, 0.70f, 1f), new Color(0.78f, 0.42f, 0.52f, 1f), 0.18f, 0.3f, 0.1f, 0.58f, 0.3f, 0.025f, 0.014f, new Color(1f, 0.95f, 0.9f, 1f), 0.035f, 0.008f, 0.65f, new Color(1f, 0.82f, 0.72f, 1f), 3.5f, 0.56f, 0.04f, 0.32f, 0.28f, 0.0055f, new Color(0.08f, 0.045f, 0.045f, 1f), 0.12f, 0.06f, 0.4f);
                case NPRAIMaterialCategory.Eye:
                    return new CategoryTuning(0.72f, 0.38f, new Color(0.75f, 0.9f, 1f, 1f), new Color(0.2f, 0.32f, 0.68f, 1f), 0.26f, 0.75f, 0.12f, 0.5f, 0.24f, 0.012f, 0.006f, Color.white, 0.025f, 0.002f, 1.8f, Color.white, 2.0f, 0.42f, 0.01f, 0.25f, 0.5f, 0.004f, Color.black, 0.1f, 0.03f, 0.25f);
                case NPRAIMaterialCategory.Cloth:
                    return new CategoryTuning(0.66f, 0.38f, new Color(0.78f, 0.72f, 0.94f, 1f), new Color(0.34f, 0.28f, 0.62f, 1f), 0.2f, 0.48f, 0.06f, 0.52f, 0.29f, 0.014f, 0.007f, new Color(0.95f, 0.95f, 1f, 1f), 0.04f, 0.006f, 0.75f, new Color(0.82f, 0.9f, 1f, 1f), 3.0f, 0.5f, 0.024f, 0.34f, 0.5f, 0.0075f, Color.black, 0.16f, 0.045f, 0.34f);
                case NPRAIMaterialCategory.Metal:
                    return new CategoryTuning(0.58f, 0.28f, new Color(0.74f, 0.82f, 0.96f, 1f), new Color(0.28f, 0.34f, 0.52f, 1f), 0.32f, 0.3f, 0.04f, 0.61f, 0.37f, 0.006f, 0.003f, Color.white, 0.12f, 0.003f, 1.9f, new Color(0.9f, 0.96f, 1f, 1f), 2.2f, 0.44f, 0.01f, 0.45f, 0.9f, 0.006f, Color.black, 0.14f, 0.025f, 0.25f);
                case NPRAIMaterialCategory.Accessory:
                    return new CategoryTuning(0.7f, 0.42f, new Color(0.86f, 0.78f, 0.98f, 1f), new Color(0.4f, 0.32f, 0.66f, 1f), 0.22f, 0.5f, 0.07f, 0.54f, 0.31f, 0.012f, 0.006f, Color.white, 0.065f, 0.004f, 1.1f, new Color(0.8f, 0.92f, 1f, 1f), 2.7f, 0.48f, 0.018f, 0.35f, 0.62f, 0.0065f, Color.black, 0.15f, 0.04f, 0.3f);
                default:
                    return new CategoryTuning(0.7f, 0.42f, new Color(0.82f, 0.78f, 0.95f, 1f), new Color(0.42f, 0.36f, 0.66f, 1f), 0.2f, 0.42f, 0.07f, 0.54f, 0.31f, 0.016f, 0.008f, Color.white, 0.055f, 0.006f, 0.95f, new Color(0.84f, 0.92f, 1f, 1f), 3.0f, 0.52f, 0.025f, 0.35f, 0.45f, 0.0065f, Color.black, 0.15f, 0.045f, 0.34f);
            }
        }
    }
}

public sealed class NPRAIMaterialRecipeApplyResult
{
    public int AppliedCount { get; internal set; }
    public int SkippedCount { get; internal set; }
}

public static class NPRAIMaterialRecipeApplier
{
    public static NPRAIMaterialRecipeApplyResult ApplyRecipe(IEnumerable<NPRAIMaterialContext> contexts, NPRAIMaterialRecipe recipe, bool useUndo)
    {
        NPRAIMaterialRecipeApplyResult result = new NPRAIMaterialRecipeApplyResult();
        if (contexts == null || recipe == null)
        {
            return result;
        }

        foreach (NPRAIMaterialContext context in contexts)
        {
            if (context == null || context.Material == null || !context.IsTargetNPRShader)
            {
                result.SkippedCount++;
                continue;
            }

            recipe.ApplyTo(context.Material);
            result.AppliedCount++;
        }

        return result;
    }
}

public static class NPRAIRampTextureGenerator
{
    public const int RampWidth = 256;
    public const int RampHeight = 16;

    public static Texture2D CreateRampTexture(NPRAIMaterialRecipe recipe)
    {
        if (recipe == null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        Texture2D texture = new Texture2D(RampWidth, RampHeight, TextureFormat.RGBA32, false)
        {
            name = "AI_NPR_RampTexture",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        for (int y = 0; y < RampHeight; y++)
        {
            for (int x = 0; x < RampWidth; x++)
            {
                float t = x / (float)(RampWidth - 1);
                Color color = t < 0.5f
                    ? Color.Lerp(recipe.secondShadeColor, recipe.firstShadeColor, t / 0.5f)
                    : Color.Lerp(recipe.firstShadeColor, recipe.litColor, (t - 0.5f) / 0.5f);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(false, false);
        return texture;
    }

    public static void ApplyRamp(Material material, Texture texture)
    {
        if (material == null || texture == null)
        {
            return;
        }

        if (material.HasProperty("_RampTexture"))
        {
            material.SetTexture("_RampTexture", texture);
        }

        if (material.HasProperty("_RampTextureStrength"))
        {
            material.SetFloat("_RampTextureStrength", 1f);
        }
    }
}
