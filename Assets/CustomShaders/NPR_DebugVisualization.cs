using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Debug visualization system for NPR rendering pipeline
/// 用于NPR渲染管线的调试可视化系统
///
/// Features:
/// - View depth buffer in screen
/// - View normals buffer in screen
/// - View Sobel edge detection result
/// - View individual RGB channels
/// - Toggle each window independently
/// </summary>
public class NPR_DebugVisualization : ScriptableRendererFeature
{
    [System.Serializable]
    public class DebugSettings
    {
        [Header("Debug Windows")]
        public bool showDepthBuffer = false;
        public bool showNormalsBuffer = false;
        public bool showSobelEdges = false;
        public bool showBaseColor = false;

        [Header("Window Layout")]
        [Range(0.1f, 0.5f)] public float windowSize = 0.25f;
        public WindowPosition position = WindowPosition.TopRight;

        [Header("Depth Visualization")]
        [Range(0.1f, 100f)] public float depthScale = 1.0f;
        public bool linearizeDepth = true;

        [Header("Normal Visualization")]
        public bool worldSpaceNormals = true;
    }

    public enum WindowPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public DebugSettings settings = new DebugSettings();
    private NPR_DebugPass debugPass;

    public override void Create()
    {
        debugPass = new NPR_DebugPass(settings);
        debugPass.renderPassEvent = RenderPassEvent.AfterRendering;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Only add debug pass if at least one visualization is enabled
        // 仅当至少启用一个可视化窗口时才添加调试Pass
        if (settings.showDepthBuffer || settings.showNormalsBuffer ||
            settings.showSobelEdges || settings.showBaseColor)
        {
            debugPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(debugPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && debugPass != null)
        {
            debugPass.Dispose();
        }
    }

    class NPR_DebugPass : ScriptableRenderPass
    {
        private DebugSettings settings;
        private Material debugMaterial;
        private RenderTargetIdentifier cameraColorTarget;

        private static readonly int DepthScaleId = Shader.PropertyToID("_DepthScale");
        private static readonly int LinearizeDepthId = Shader.PropertyToID("_LinearizeDepth");

        public NPR_DebugPass(DebugSettings settings)
        {
            this.settings = settings;

            // Create material from shader
            // 从着色器创建材质
            Shader debugShader = Shader.Find("Hidden/NPR/DebugVisualization");
            if (debugShader != null)
            {
                debugMaterial = CoreUtils.CreateEngineMaterial(debugShader);
            }
            else
            {
                Debug.LogError("NPR_DebugVisualization: Cannot find shader 'Hidden/NPR/DebugVisualization'");
            }
        }

        public void Setup(RenderTargetIdentifier colorTarget)
        {
            this.cameraColorTarget = colorTarget;

            // Configure required inputs
            // 配置所需的输入
            ScriptableRenderPassInput passInput = ScriptableRenderPassInput.None;

            if (settings.showDepthBuffer)
                passInput |= ScriptableRenderPassInput.Depth;

            if (settings.showNormalsBuffer || settings.showSobelEdges)
                passInput |= ScriptableRenderPassInput.Normal;

            ConfigureInput(passInput);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (debugMaterial == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("NPR Debug Visualization");

            // Get camera descriptor
            // 获取相机描述符
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            // Calculate window positions and size
            // 计算窗口位置和大小
            int windowCount = 0;
            if (settings.showDepthBuffer) windowCount++;
            if (settings.showNormalsBuffer) windowCount++;
            if (settings.showSobelEdges) windowCount++;
            if (settings.showBaseColor) windowCount++;

            if (windowCount == 0)
            {
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                return;
            }

            float windowSize = settings.windowSize;
            int currentWindow = 0;

            // Draw each enabled debug window
            // 绘制每个启用的调试窗口
            if (settings.showDepthBuffer)
            {
                DrawDebugWindow(cmd, descriptor, currentWindow, windowCount, "Depth Buffer", 0);
                currentWindow++;
            }

            if (settings.showNormalsBuffer)
            {
                DrawDebugWindow(cmd, descriptor, currentWindow, windowCount, "Normals Buffer", 1);
                currentWindow++;
            }

            if (settings.showSobelEdges)
            {
                DrawDebugWindow(cmd, descriptor, currentWindow, windowCount, "Sobel Edges", 2);
                currentWindow++;
            }

            if (settings.showBaseColor)
            {
                DrawDebugWindow(cmd, descriptor, currentWindow, windowCount, "Base Color", 3);
                currentWindow++;
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void DrawDebugWindow(CommandBuffer cmd, RenderTextureDescriptor descriptor,
            int windowIndex, int totalWindows, string label, int passIndex)
        {
            // Calculate window dimensions
            // 计算窗口尺寸
            int windowWidth = Mathf.RoundToInt(descriptor.width * settings.windowSize);
            int windowHeight = Mathf.RoundToInt(descriptor.height * settings.windowSize);

            // Calculate window position based on settings
            // 根据设置计算窗口位置
            Vector2Int position = GetWindowPosition(descriptor.width, descriptor.height,
                windowWidth, windowHeight, windowIndex, totalWindows);

            // Set viewport for this window
            // 为此窗口设置视口
            cmd.SetViewport(new Rect(position.x, position.y, windowWidth, windowHeight));

            // Set material properties
            // 设置材质属性
            debugMaterial.SetFloat(DepthScaleId, settings.depthScale);
            debugMaterial.SetFloat(LinearizeDepthId, settings.linearizeDepth ? 1.0f : 0.0f);

            // Draw fullscreen quad with debug visualization
            // 使用调试可视化绘制全屏四边形
            cmd.DrawProcedural(Matrix4x4.identity, debugMaterial, passIndex, MeshTopology.Triangles, 3);

            // Reset viewport
            // 重置视口
            cmd.SetViewport(new Rect(0, 0, descriptor.width, descriptor.height));
        }

        private Vector2Int GetWindowPosition(int screenWidth, int screenHeight,
            int windowWidth, int windowHeight, int windowIndex, int totalWindows)
        {
            int x = 0, y = 0;
            int padding = 10;

            switch (settings.position)
            {
                case WindowPosition.TopRight:
                    x = screenWidth - windowWidth - padding;
                    y = screenHeight - windowHeight - padding - (windowIndex * (windowHeight + padding));
                    break;

                case WindowPosition.TopLeft:
                    x = padding;
                    y = screenHeight - windowHeight - padding - (windowIndex * (windowHeight + padding));
                    break;

                case WindowPosition.BottomRight:
                    x = screenWidth - windowWidth - padding;
                    y = padding + (windowIndex * (windowHeight + padding));
                    break;

                case WindowPosition.BottomLeft:
                    x = padding;
                    y = padding + (windowIndex * (windowHeight + padding));
                    break;
            }

            return new Vector2Int(x, y);
        }

        public void Dispose()
        {
            if (debugMaterial != null)
            {
                CoreUtils.Destroy(debugMaterial);
            }
        }
    }
}
