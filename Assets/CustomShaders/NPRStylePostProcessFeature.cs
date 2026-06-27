using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NPRStylePostProcessFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class StylePostProcessSettings
    {
        public bool enabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [Range(0f, 1f)] public float posterizeStrength = 0.82f;
        [Range(0.5f, 2.5f)] public float contrast = 1.34f;
        [Range(0f, 2f)] public float saturation = 1.38f;
        [Range(0f, 1f)] public float shadowCrush = 0.22f;
        [Range(0f, 1f)] public float halftoneStrength = 0.18f;
        [Range(8f, 160f)] public float halftoneScale = 68f;
        public Color accentColor = new Color(0.1f, 0.85f, 1f, 1f);
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
        if (stylePass == null || !settings.enabled)
        {
            return;
        }

        renderer.EnqueuePass(stylePass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (stylePass == null || !settings.enabled)
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
        private static readonly int HalftoneStrengthId = Shader.PropertyToID("_HalftoneStrength");
        private static readonly int HalftoneScaleId = Shader.PropertyToID("_HalftoneScale");
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
            material.SetFloat(PosterizeStrengthId, settings.posterizeStrength);
            material.SetFloat(ContrastId, settings.contrast);
            material.SetFloat(SaturationId, settings.saturation);
            material.SetFloat(ShadowCrushId, settings.shadowCrush);
            material.SetFloat(HalftoneStrengthId, settings.halftoneStrength);
            material.SetFloat(HalftoneScaleId, settings.halftoneScale);
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
