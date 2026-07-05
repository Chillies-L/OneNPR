using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NPRStylePostProcessFeature : ScriptableRendererFeature
{
    public enum PatternShadingType
    {
        Off = 0,
        Dots = 1,
        Hatching = 2,
    }

    [System.Serializable]
    public class StylePostProcessSettings
    {
        [Header("Master Switches")]
        public bool enabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Header("Screen Space Outline")]
        public bool outlineEnabled = true;
        public Color outlineColor = Color.black;
        [Range(0f, 1f)] public float outlineIntensity = 0.85f;
        [Range(0.5f, 4f)] public float outlineThickness = 1.15f;
        [Range(0.0005f, 0.08f)] public float depthThreshold = 0.015f;
        [Range(0.02f, 1f)] public float normalThreshold = 0.22f;
        [Range(0.01f, 0.5f)] public float colorThreshold = 0.12f;
        public bool useDepth = true;
        public bool useNormals = true;
        public bool useColor = false;

        [Header("Color Grading")]
        public bool colorGradingEnabled = true;
        [Range(0f, 1f)] public float posterizeStrength = 0.82f;
        [Range(0.5f, 2.5f)] public float contrast = 1.34f;
        [Range(0f, 2f)] public float saturation = 1.38f;
        [Range(0f, 1f)] public float shadowCrush = 0.22f;
        public Color accentColor = new Color(0.1f, 0.85f, 1f, 1f);

        [Header("Pattern Shading / Halftone")]
        public bool halftoneEnabled = true;
        public PatternShadingType patternType = PatternShadingType.Dots;
        [Range(0f, 1f)] public float halftoneStrength = 0.65f;
        [Range(1f, 50f)] public float halftoneScale = 10f;
        [Range(0f, 3.14159f)] public float patternAngle = 0.785398f;
        [Range(0f, 2f)] public float patternIntensity = 0.8f;
        public Color patternColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        [Range(0f, 1f)] public float patternLumaThreshold = 0.5f;

        public bool HasAnyActiveEffect()
        {
            return outlineEnabled || colorGradingEnabled || HasActivePattern();
        }

        public bool HasActivePattern()
        {
            return halftoneEnabled && patternType != PatternShadingType.Off && halftoneStrength > 0f;
        }
    }

    public NPRStylePreset activePreset;
    public StylePostProcessSettings settings = new StylePostProcessSettings();

    private NPRStylePostProcessPass stylePass;

    public override void Create()
    {
        if (activePreset != null)
        {
            bool wasEnabled = settings.enabled;
            activePreset.ApplyTo(settings);
            settings.enabled = wasEnabled;
        }

        stylePass = new NPRStylePostProcessPass(settings)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (stylePass == null || !settings.enabled || !settings.HasAnyActiveEffect() || renderingData.cameraData.cameraType == CameraType.Preview)
        {
            return;
        }

        stylePass.renderPassEvent = settings.renderPassEvent;
        stylePass.ConfigureRequiredInputs();
        renderer.EnqueuePass(stylePass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (stylePass == null || !settings.enabled || !settings.HasAnyActiveEffect() || renderingData.cameraData.cameraType == CameraType.Preview)
        {
            return;
        }

        stylePass.Setup(renderer.cameraColorTargetHandle);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && stylePass != null)
        {
            stylePass.Dispose();
        }
    }

    private sealed class NPRStylePostProcessPass : ScriptableRenderPass
    {
        private readonly StylePostProcessSettings settings;
        private readonly Material material;
        private RTHandle cameraColorTarget;
        private RTHandle tempTexture;

        private const string TempTextureName = "_NPRStylePostProcessTexture";

        private static readonly int PosterizeStrengthId = Shader.PropertyToID("_PosterizeStrength");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        private static readonly int ShadowCrushId = Shader.PropertyToID("_ShadowCrush");
        private static readonly int ColorGradingEnabledId = Shader.PropertyToID("_ColorGradingEnabled");
        private static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineIntensityId = Shader.PropertyToID("_OutlineIntensity");
        private static readonly int OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");
        private static readonly int DepthThresholdId = Shader.PropertyToID("_DepthThreshold");
        private static readonly int NormalThresholdId = Shader.PropertyToID("_NormalThreshold");
        private static readonly int ColorThresholdId = Shader.PropertyToID("_ColorThreshold");
        private static readonly int UseDepthId = Shader.PropertyToID("_UseDepth");
        private static readonly int UseNormalsId = Shader.PropertyToID("_UseNormals");
        private static readonly int UseColorId = Shader.PropertyToID("_UseColor");
        private static readonly int PatternTypeId = Shader.PropertyToID("_PatternType");
        private static readonly int PatternScaleId = Shader.PropertyToID("_PatternScale");
        private static readonly int PatternAngleId = Shader.PropertyToID("_PatternAngle");
        private static readonly int PatternIntensityId = Shader.PropertyToID("_PatternIntensity");
        private static readonly int PatternBlendId = Shader.PropertyToID("_PatternBlend");
        private static readonly int PatternColorId = Shader.PropertyToID("_PatternColor");
        private static readonly int PatternLumaThresholdId = Shader.PropertyToID("_PatternLumaThreshold");
        private static readonly int AccentColorId = Shader.PropertyToID("_AccentColor");

        public NPRStylePostProcessPass(StylePostProcessSettings settings)
        {
            this.settings = settings;

            Shader shader = Shader.Find("Hidden/NPR/StylePostProcess");
            if (shader != null)
            {
                material = CoreUtils.CreateEngineMaterial(shader);
            }
            else
            {
                Debug.LogError("NPRStylePostProcessFeature: Cannot find shader 'Hidden/NPR/StylePostProcess'.");
            }
        }

        public void ConfigureRequiredInputs()
        {
            ScriptableRenderPassInput input = ScriptableRenderPassInput.None;

            if (settings.outlineEnabled)
            {
                if (settings.useDepth)
                {
                    input |= ScriptableRenderPassInput.Depth;
                }

                if (settings.useNormals)
                {
                    input |= ScriptableRenderPassInput.Normal;
                }
            }

            ConfigureInput(input);
        }

        public void Setup(RTHandle colorTarget)
        {
            cameraColorTarget = colorTarget;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor descriptor = cameraTextureDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: TempTextureName);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null || cameraColorTarget == null || tempTexture == null)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("NPR Style Post Process");
            material.SetFloat(ColorGradingEnabledId, settings.colorGradingEnabled ? 1f : 0f);
            material.SetFloat(OutlineEnabledId, settings.outlineEnabled ? 1f : 0f);
            material.SetColor(OutlineColorId, settings.outlineColor);
            material.SetFloat(OutlineIntensityId, settings.outlineIntensity);
            material.SetFloat(OutlineThicknessId, settings.outlineThickness);
            material.SetFloat(DepthThresholdId, settings.depthThreshold);
            material.SetFloat(NormalThresholdId, settings.normalThreshold);
            material.SetFloat(ColorThresholdId, settings.colorThreshold);
            material.SetFloat(UseDepthId, settings.useDepth ? 1f : 0f);
            material.SetFloat(UseNormalsId, settings.useNormals ? 1f : 0f);
            material.SetFloat(UseColorId, settings.useColor ? 1f : 0f);
            material.SetFloat(PosterizeStrengthId, settings.posterizeStrength);
            material.SetFloat(ContrastId, settings.contrast);
            material.SetFloat(SaturationId, settings.saturation);
            material.SetFloat(ShadowCrushId, settings.shadowCrush);
            material.SetInteger(PatternTypeId, settings.HasActivePattern() ? (int)settings.patternType : 0);
            material.SetFloat(PatternScaleId, settings.halftoneScale);
            material.SetFloat(PatternAngleId, settings.patternAngle);
            material.SetFloat(PatternIntensityId, settings.patternIntensity);
            material.SetFloat(PatternBlendId, settings.halftoneStrength);
            material.SetColor(PatternColorId, settings.patternColor);
            material.SetFloat(PatternLumaThresholdId, settings.patternLumaThreshold);
            material.SetColor(AccentColorId, settings.accentColor);

            Blitter.BlitCameraTexture(cmd, cameraColorTarget, tempTexture, material, 0);
            Blitter.BlitCameraTexture(cmd, tempTexture, cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            if (material != null)
            {
                CoreUtils.Destroy(material);
            }

            tempTexture?.Release();
        }
    }
}
