using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP Render Feature for screen-space Sobel edge detection outline
/// 基于屏幕空间Sobel边缘检测的描边Render Feature
/// </summary>
public class NPR_OutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        [Header("Outline Settings")]
        public Color outlineColor = Color.black;
        [Range(0f, 5f)] public float outlineThickness = 1.0f;

        [Header("Edge Detection")]
        [Range(0f, 1f)] public float depthThreshold = 0.1f;
        [Range(0f, 1f)] public float normalThreshold = 0.4f;

        [Header("Advanced")]
        public bool useDepth = true;
        public bool useNormals = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public OutlineSettings settings = new OutlineSettings();
    private NPR_OutlinePass outlinePass;

    public override void Create()
    {
        outlinePass = new NPR_OutlinePass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (outlinePass != null)
        {
            renderer.EnqueuePass(outlinePass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && outlinePass != null)
        {
            outlinePass.Dispose();
        }
    }

    /// <summary>
    /// Custom render pass for Sobel edge detection
    /// Sobel边缘检测的自定义渲染通道
    /// </summary>
    class NPR_OutlinePass : ScriptableRenderPass
    {
        private OutlineSettings settings;
        private Material outlineMaterial;
        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetHandle tempTexture;

        // Shader property IDs for performance
        // 着色器属性ID用于性能优化
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");
        private static readonly int DepthThresholdId = Shader.PropertyToID("_DepthThreshold");
        private static readonly int NormalThresholdId = Shader.PropertyToID("_NormalThreshold");
        private static readonly int UseDepthId = Shader.PropertyToID("_UseDepth");
        private static readonly int UseNormalsId = Shader.PropertyToID("_UseNormals");

        public NPR_OutlinePass(OutlineSettings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;
            tempTexture.Init("_TempOutlineTexture");

            // Create material from shader
            // 从着色器创建材质
            Shader outlineShader = Shader.Find("Hidden/NPR/SobelOutline");
            if (outlineShader != null)
            {
                outlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);
            }
            else
            {
                Debug.LogError("NPR_OutlineFeature: Cannot find shader 'Hidden/NPR/SobelOutline'");
            }
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;

            // Configure depth-normals texture requirement
            // 配置深度法线纹理需求
            if (settings.useNormals)
            {
                ConfigureInput(ScriptableRenderPassInput.Normal);
            }
            if (settings.useDepth)
            {
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (outlineMaterial == null)
            {
                Debug.LogWarning("NPR_OutlineFeature: Material is null, skipping pass");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("NPR Screen Space Outline");

            // Get camera render texture descriptor
            // 获取相机渲染纹理描述符
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0; // No depth buffer needed for post-process

            // Get temporary render texture
            // 获取临时渲染纹理
            cmd.GetTemporaryRT(tempTexture.id, descriptor, FilterMode.Bilinear);

            // Set material properties
            // 设置材质属性
            outlineMaterial.SetColor(OutlineColorId, settings.outlineColor);
            outlineMaterial.SetFloat(OutlineThicknessId, settings.outlineThickness);
            outlineMaterial.SetFloat(DepthThresholdId, settings.depthThreshold);
            outlineMaterial.SetFloat(NormalThresholdId, settings.normalThreshold);
            outlineMaterial.SetFloat(UseDepthId, settings.useDepth ? 1.0f : 0.0f);
            outlineMaterial.SetFloat(UseNormalsId, settings.useNormals ? 1.0f : 0.0f);

            // Blit with outline shader
            // 使用描边着色器Blit
            cmd.Blit(cameraColorTarget, tempTexture.Identifier(), outlineMaterial, 0);
            cmd.Blit(tempTexture.Identifier(), cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }

        public void Dispose()
        {
            if (outlineMaterial != null)
            {
                CoreUtils.Destroy(outlineMaterial);
            }
        }
    }
}
