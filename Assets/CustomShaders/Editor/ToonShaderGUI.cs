using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToonShaderGUI : ShaderGUI
{
    private const string HighFidelityRendererPath = "Assets/Settings/URP-HighFidelity-Renderer.asset";
    private const string BalancedRendererPath = "Assets/Settings/URP-Balanced-Renderer.asset";
    private const string PerformantRendererPath = "Assets/Settings/URP-Performant-Renderer.asset";

    private MaterialEditor materialEditor;
    private MaterialProperty[] properties;
    private int selectedStylePresetIndex;
    private int selectedLegacyPresetIndex;

    private bool showBaseColors = true;
    private bool showAnimePalette = true;
    private bool showShading = true;
    private bool showAnimeLight = true;
    private bool showSpecular = true;
    private bool showRim = true;
    private bool showOutline = true;
    private bool showAdvanced;

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] propertiesIn)
    {
        materialEditor = materialEditorIn;
        properties = propertiesIn;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("NPR Toon Character Shader", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "三渲二（3D-to-2D / toon rendering）角色着色器：支持自动动漫配色、虚拟主光、硬阴影、硬高光、边缘光和法线外扩描边。",
            MessageType.Info);
        EditorGUILayout.Space();

        DrawBaseColorSection();
        DrawAnimePaletteSection();
        DrawShadingSection();
        DrawAnimeLightSection();
        DrawSpecularSection();
        DrawRimSection();
        DrawOutlineSection();
        DrawAdvancedSection();
        DrawPresetSection();
    }

    private void DrawBaseColorSection()
    {
        showBaseColors = EditorGUILayout.BeginFoldoutHeaderGroup(showBaseColors, "基础颜色（Base Colors）");
        if (showBaseColors)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_BaseMap", "基础贴图（Base Texture）");
            DrawProperty("_BaseColor", "基础颜色（Base Color）");
            DrawProperty("_1stShadeColor", "一级阴影颜色（1st Shade）");
            DrawProperty("_2ndShadeColor", "二级阴影颜色（2nd Shade）");
            EditorGUILayout.HelpBox("Base = 亮部；1st Shade = 主阴影；2nd Shade = 深阴影。", MessageType.None);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void DrawAnimePaletteSection()
    {
        showAnimePalette = EditorGUILayout.BeginFoldoutHeaderGroup(showAnimePalette, "动漫配色（Anime Palette）");
        if (showAnimePalette)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_AutoShadeColor", "自动阴影配色（Auto Shade Palette）");
            DrawProperty("_1stShadeTint", "一级阴影色相（1st Shade Tint）");
            DrawProperty("_2ndShadeTint", "二级阴影色相（2nd Shade Tint）");
            DrawProperty("_ShadeSaturation", "阴影饱和度增强（Shade Saturation）");
            DrawProperty("_LitColorBoost", "亮部颜色增强（Lit Color Boost）");
            EditorGUILayout.HelpBox("自动配色会基于基础色生成带色相偏移的动漫阴影，减少白模灰阶感。", MessageType.None);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void DrawShadingSection()
    {
        showShading = EditorGUILayout.BeginFoldoutHeaderGroup(showShading, "光照控制（Shading Control）");
        if (showShading)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("基础阴影（Base Shadow）", EditorStyles.miniBoldLabel);
            DrawProperty("_BaseStep", "阴影阈值（Shadow Step）");
            DrawProperty("_BaseFeather", "边界羽化（Feather）");
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("二级阴影（2nd Shadow）", EditorStyles.miniBoldLabel);
            DrawProperty("_ShadeStep", "阴影阈值（Shadow Step）");
            DrawProperty("_ShadeFeather", "边界羽化（Feather）");
            EditorGUILayout.HelpBox("Step 控制阴影面积，Feather 控制边界硬度；参考图风格通常需要很小的 Feather。", MessageType.None);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void DrawAnimeLightSection()
    {
        showAnimeLight = EditorGUILayout.BeginFoldoutHeaderGroup(showAnimeLight, "动漫光照（Anime Light）");
        if (showAnimeLight)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_UseVirtualLight", "使用虚拟主光（Use Virtual Key Light）");
            DrawProperty("_VirtualLightDirection", "虚拟主光方向（Virtual Light Direction）");
            DrawProperty("_LightColorInfluence", "场景光颜色影响（Light Color Influence）");
            DrawProperty("_AmbientStrength", "环境光强度（Ambient Strength）");
            DrawProperty("_AdditionalLightInfluence", "附加光影响（Additional Lights）");
            EditorGUILayout.HelpBox("虚拟主光用于稳定阴影方向，减少真实场景光破坏二次元阴影块。", MessageType.None);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void DrawSpecularSection()
    {
        showSpecular = EditorGUILayout.BeginFoldoutHeaderGroup(showSpecular, "高光（Specular Highlight）");
        if (showSpecular)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_SpecularColor", "高光颜色（Specular Color）");
            DrawProperty("_SpecularSize", "高光大小（Size）");
            DrawProperty("_SpecularFeather", "高光边界羽化（Edge Feather）");
            DrawProperty("_SpecularIntensity", "高光强度（Intensity）");
            EditorGUILayout.HelpBox("硬边块状高光用于漫画/动画感，不追求物理真实。", MessageType.None);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void DrawRimSection()
    {
        showRim = EditorGUILayout.BeginFoldoutHeaderGroup(showRim, "边缘光（Rim Light）");
        if (showRim)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_RimColor", "边缘光颜色（Rim Color）");
            DrawProperty("_RimPower", "边缘光宽度（Power）");
            DrawProperty("_RimThreshold", "边缘光阈值（Threshold）");
            DrawProperty("_RimFeather", "边缘光羽化（Feather）");
            DrawProperty("_RimLightAlign", "按光照方向收束（Light Align）");
            DrawProperty("_RimIntensity", "边缘光强度（Intensity）");
            EditorGUILayout.HelpBox("强边缘光用于热血动画风格，也能提高轮廓可读性。", MessageType.None);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void DrawOutlineSection()
    {
        showOutline = EditorGUILayout.BeginFoldoutHeaderGroup(showOutline, "描边（Outline）");
        if (showOutline)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_OutlineWidth", "描边宽度（Width）");
            DrawProperty("_OutlineColor", "描边颜色（Color）");
            DrawProperty("_OutlineWidthMap", "描边宽度贴图（Width Map）");
            DrawProperty("_OutlineMode", "Outline Mode");
            DrawProperty("_OutlineMinWidth", "Outline Min Width");
            DrawProperty("_OutlineMaxWidth", "Outline Max Width");
            DrawProperty("_OutlineMiterStrength", "Miter Strength");
            DrawProperty("_OutlineMiterMinDot", "Miter Min Dot");
            DrawProperty("_OutlineMiterMaxScale", "Miter Max Scale");
            DrawProperty("_OutlineUseVertexColorNormals", "Use Vertex Color Normals");
            DrawProperty("_OutlineVertexColorNormalFormat", "Vertex Color Normal Format");
            DrawProperty("_OutlineVertexNormalBlend", "Vertex Color Normal Blend");
            DrawProperty("_OutlineRenderReversePass", "Render Reverse Pass");
            DrawProperty("_OutlineReverseWidthScale", "Reverse Pass Width Scale");
            DrawProperty("_OutlineReverseZOffset", "Reverse Pass Z Offset");
            DrawProperty("_OutlineUseWorldShell", "Use World Normal Shell");
            DrawProperty("_OutlineDirectionalBias", "Directional Width Bias");
            DrawProperty("_OutlineBiasDirection", "Bias Direction XY");
            DrawProperty("_OutlineZOffset", "Outline Z Offset");
            EditorGUILayout.HelpBox("法线外扩描边用于角色外轮廓；屏幕空间线条由 Renderer Feature 负责。", MessageType.None);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void DrawAdvancedSection()
    {
        showAdvanced = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvanced, "高级选项（Advanced）");
        if (showAdvanced)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_Cull", "剔除模式（Cull Mode）");
            EditorGUILayout.Space(5);
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
    }

    private void DrawPresetSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("资源化风格预设（Style Preset Assets）", EditorStyles.boldLabel);

        NPRStylePreset[] stylePresets = LoadStylePresets();
        if (stylePresets.Length > 0)
        {
            string[] stylePresetNames = new string[stylePresets.Length];
            for (int i = 0; i < stylePresets.Length; i++)
            {
                stylePresetNames[i] = stylePresets[i].DisplayName;
            }

            selectedStylePresetIndex = Mathf.Clamp(selectedStylePresetIndex, 0, stylePresetNames.Length - 1);
            selectedStylePresetIndex = EditorGUILayout.Popup("资源化预设（Style Preset Asset）", selectedStylePresetIndex, stylePresetNames);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("应用资源化预设（Apply Style Preset）"))
            {
                ApplyStylePreset(stylePresets[selectedStylePresetIndex]);
            }

            if (GUILayout.Button("粗线彩色漫画"))
            {
                ApplyNamedStylePreset(stylePresets, NPRStylePresetKind.BoldInkComic);
            }

            if (GUILayout.Button("热血图形动画"))
            {
                ApplyNamedStylePreset(stylePresets, NPRStylePresetKind.GraphicActionAnime);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("资源化预设会同时写入材质参数，并同步更新已安装的 NPRStylePostProcessFeature 后处理参数。", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("未找到 NPRStylePreset 资源；仍可使用下方兼容材质预设。", MessageType.None);
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("兼容材质预设（Legacy Material Presets）", EditorStyles.boldLabel);

        string[] presetNames = ToonMaterialPreset.GetPresetNames();
        selectedLegacyPresetIndex = Mathf.Clamp(selectedLegacyPresetIndex, 0, presetNames.Length - 1);
        selectedLegacyPresetIndex = EditorGUILayout.Popup("兼容预设（Legacy Preset）", selectedLegacyPresetIndex, presetNames);

        if (GUILayout.Button("应用兼容预设（Apply Legacy Preset）"))
        {
            ApplyLegacyPreset((ToonMaterialPresetKind)selectedLegacyPresetIndex);
        }

        EditorGUILayout.HelpBox("兼容材质预设只写入材质参数，不会同步或启用屏幕空间后处理。", MessageType.None);
    }

    private void DrawProperty(string propertyName, string displayName)
    {
        MaterialProperty prop = FindProperty(propertyName, properties, false);
        if (prop != null)
        {
            materialEditor.ShaderProperty(prop, displayName);
        }
    }

    private static NPRStylePreset[] LoadStylePresets()
    {
        string[] guids = AssetDatabase.FindAssets("t:NPRStylePreset", new[] { "Assets/CustomShaders/Presets" });
        List<NPRStylePreset> presets = new List<NPRStylePreset>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            NPRStylePreset preset = AssetDatabase.LoadAssetAtPath<NPRStylePreset>(path);
            if (preset != null)
            {
                presets.Add(preset);
            }
        }

        presets.Sort((left, right) => left.StyleKind.CompareTo(right.StyleKind));
        return presets.ToArray();
    }

    private void ApplyNamedStylePreset(NPRStylePreset[] presets, NPRStylePresetKind kind)
    {
        for (int i = 0; i < presets.Length; i++)
        {
            if (presets[i].StyleKind == kind)
            {
                selectedStylePresetIndex = i;
                ApplyStylePreset(presets[i]);
                return;
            }
        }

        Debug.LogWarning($"Cannot find NPR style preset: {kind}");
    }

    private void ApplyStylePreset(NPRStylePreset preset)
    {
        foreach (Object target in materialEditor.targets)
        {
            Material material = target as Material;
            if (material == null)
            {
                continue;
            }

            Undo.RecordObject(material, "Apply NPR Style Preset");
            preset.ApplyTo(material);
            EditorUtility.SetDirty(material);
        }

        int updatedFeatureCount = ApplyPresetToInstalledStyleFeatures(preset);
        AssetDatabase.SaveAssets();
        Debug.Log($"Applied NPR style preset: {preset.DisplayName}. Updated {updatedFeatureCount} style post-process feature(s).");
    }

    private void ApplyLegacyPreset(ToonMaterialPresetKind presetKind)
    {
        foreach (Object target in materialEditor.targets)
        {
            Material material = target as Material;
            if (material == null)
            {
                continue;
            }

            Undo.RecordObject(material, "Apply Toon Material Preset");
            ToonMaterialPreset.Apply(presetKind, material);
            EditorUtility.SetDirty(material);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Applied legacy toon preset: {ToonMaterialPreset.GetPresetName(presetKind)}");
    }

    private static int ApplyPresetToInstalledStyleFeatures(NPRStylePreset preset)
    {
        int updated = 0;
        string[] rendererPaths =
        {
            HighFidelityRendererPath,
            BalancedRendererPath,
            PerformantRendererPath,
        };

        foreach (string rendererPath in rendererPaths)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(rendererPath))
            {
                NPRStylePostProcessFeature feature = asset as NPRStylePostProcessFeature;
                if (feature == null)
                {
                    continue;
                }

                Undo.RecordObject(feature, "Apply NPR Style Post Process Preset");
                feature.activePreset = preset;
                bool wasEnabled = feature.settings.enabled;
                preset.ApplyTo(feature.settings);
                feature.settings.enabled = wasEnabled;
                EditorUtility.SetDirty(feature);
                updated++;
            }
        }

        if (updated == 0)
        {
            Debug.LogWarning("No NPRStylePostProcessFeature is installed in the known URP renderer assets.");
        }

        return updated;
    }
}
