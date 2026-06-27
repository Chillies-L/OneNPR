Shader "Hidden/NPR/DebugVisualization"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

        float _DepthScale;
        float _LinearizeDepth;

        struct Attributes
        {
            uint vertexID : SV_VertexID;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        // Fullscreen triangle vertex shader
        // 全屏三角形顶点着色器
        Varyings VertexFullscreen(Attributes input)
        {
            Varyings output;

            // Generate fullscreen triangle
            // 生成全屏三角形
            float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
            output.positionHCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
            output.uv = uv;

            // Flip Y for correct UV coordinates
            // 翻转Y以获得正确的UV坐标
            #if UNITY_UV_STARTS_AT_TOP
            output.uv.y = 1.0 - output.uv.y;
            #endif

            return output;
        }

        ENDHLSL

        // ==================== Pass 0: Depth Buffer Visualization ====================
        Pass
        {
            Name "Debug Depth"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex VertexFullscreen
            #pragma fragment FragDepth

            float4 FragDepth(Varyings input) : SV_Target
            {
                // Sample scene depth
                // 采样场景深度
                float depth = SampleSceneDepth(input.uv);

                // Linearize depth if enabled
                // 如果启用，则线性化深度
                if (_LinearizeDepth > 0.5)
                {
                    depth = Linear01Depth(depth, _ZBufferParams);
                }

                // Apply depth scale for better visualization
                // 应用深度缩放以获得更好的可视化效果
                depth = saturate(depth * _DepthScale);

                // Visualize as grayscale (near=white, far=black)
                // 以灰度显示（近=白色，远=黑色）
                float3 color = float3(1.0 - depth, 1.0 - depth, 1.0 - depth);

                // Add color gradient for better readability
                // 添加颜色渐变以提高可读性
                float3 gradientColor = lerp(float3(0.2, 0.5, 1.0), float3(1.0, 0.2, 0.2), depth);

                return float4(gradientColor, 1.0);
            }
            ENDHLSL
        }

        // ==================== Pass 1: Normals Buffer Visualization ====================
        Pass
        {
            Name "Debug Normals"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex VertexFullscreen
            #pragma fragment FragNormals

            float4 FragNormals(Varyings input) : SV_Target
            {
                // Sample scene normals (world space)
                // 采样场景法线（世界空间）
                float3 normal = SampleSceneNormals(input.uv);

                // Normalize to ensure unit length
                // 归一化以确保单位长度
                normal = normalize(normal);

                // Convert from [-1, 1] to [0, 1] for visualization
                // 从[-1, 1]转换到[0, 1]用于可视化
                // Red = X axis, Green = Y axis, Blue = Z axis
                // 红色=X轴，绿色=Y轴，蓝色=Z轴
                float3 color = normal * 0.5 + 0.5;

                return float4(color, 1.0);
            }
            ENDHLSL
        }

        // ==================== Pass 2: Sobel Edge Detection Visualization ====================
        Pass
        {
            Name "Debug Sobel Edges"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex VertexFullscreen
            #pragma fragment FragSobel

            // Simplified Sobel edge detection for visualization
            // 简化的Sobel边缘检测用于可视化
            float SobelDepthEdge(float2 uv, float2 texelSize)
            {
                // Sample 3x3 neighborhood
                // 采样3x3邻域
                float d00 = SampleSceneDepth(uv + float2(-texelSize.x, -texelSize.y));
                float d10 = SampleSceneDepth(uv + float2(0, -texelSize.y));
                float d20 = SampleSceneDepth(uv + float2(texelSize.x, -texelSize.y));

                float d01 = SampleSceneDepth(uv + float2(-texelSize.x, 0));
                float d21 = SampleSceneDepth(uv + float2(texelSize.x, 0));

                float d02 = SampleSceneDepth(uv + float2(-texelSize.x, texelSize.y));
                float d12 = SampleSceneDepth(uv + float2(0, texelSize.y));
                float d22 = SampleSceneDepth(uv + float2(texelSize.x, texelSize.y));

                // Convert to linear depth
                // 转换为线性深度
                d00 = Linear01Depth(d00, _ZBufferParams);
                d10 = Linear01Depth(d10, _ZBufferParams);
                d20 = Linear01Depth(d20, _ZBufferParams);
                d01 = Linear01Depth(d01, _ZBufferParams);
                d21 = Linear01Depth(d21, _ZBufferParams);
                d02 = Linear01Depth(d02, _ZBufferParams);
                d12 = Linear01Depth(d12, _ZBufferParams);
                d22 = Linear01Depth(d22, _ZBufferParams);

                // Apply Sobel operator
                // 应用Sobel算子
                float Gx = -d00 + d20 - 2.0 * d01 + 2.0 * d21 - d02 + d22;
                float Gy = -d00 - 2.0 * d10 - d20 + d02 + 2.0 * d12 + d22;

                // Calculate gradient magnitude
                // 计算梯度幅值
                return sqrt(Gx * Gx + Gy * Gy);
            }

            float SobelNormalEdge(float2 uv, float2 texelSize)
            {
                // Sample 3x3 neighborhood normals
                // 采样3x3邻域法线
                float3 n00 = SampleSceneNormals(uv + float2(-texelSize.x, -texelSize.y));
                float3 n10 = SampleSceneNormals(uv + float2(0, -texelSize.y));
                float3 n20 = SampleSceneNormals(uv + float2(texelSize.x, -texelSize.y));

                float3 n01 = SampleSceneNormals(uv + float2(-texelSize.x, 0));
                float3 n21 = SampleSceneNormals(uv + float2(texelSize.x, 0));

                float3 n02 = SampleSceneNormals(uv + float2(-texelSize.x, texelSize.y));
                float3 n12 = SampleSceneNormals(uv + float2(0, texelSize.y));
                float3 n22 = SampleSceneNormals(uv + float2(texelSize.x, texelSize.y));

                // Apply Sobel operator
                // 应用Sobel算子
                float3 Gx = -n00 + n20 - 2.0 * n01 + 2.0 * n21 - n02 + n22;
                float3 Gy = -n00 - 2.0 * n10 - n20 + n02 + 2.0 * n12 + n22;

                // Calculate gradient magnitude
                // 计算梯度幅值
                return sqrt(dot(Gx, Gx) + dot(Gy, Gy));
            }

            float4 FragSobel(Varyings input) : SV_Target
            {
                float2 texelSize = _ScreenParams.zw - 1.0; // 1/width, 1/height

                // Calculate edges from both depth and normals
                // 从深度和法线计算边缘
                float depthEdge = SobelDepthEdge(input.uv, texelSize * 2.0);
                float normalEdge = SobelNormalEdge(input.uv, texelSize * 2.0);

                // Combine edges
                // 组合边缘
                float edge = max(depthEdge, normalEdge);

                // Scale for better visibility
                // 缩放以获得更好的可见性
                edge = saturate(edge * 10.0);

                // Color code: Red = depth edges, Green = normal edges, Yellow = both
                // 颜色编码：红色=深度边缘，绿色=法线边缘，黄色=两者
                float3 color = float3(0, 0, 0);
                color.r = depthEdge * 10.0;
                color.g = normalEdge * 5.0;
                color.b = edge * 0.2;

                // Make edges more visible on dark background
                // 使边缘在暗背景上更可见
                color = saturate(color);

                return float4(color, 1.0);
            }
            ENDHLSL
        }

        // ==================== Pass 3: Base Color Visualization ====================
        Pass
        {
            Name "Debug Base Color"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex VertexFullscreen
            #pragma fragment FragBaseColor

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            float4 FragBaseColor(Varyings input) : SV_Target
            {
                // Sample camera color buffer
                // 采样相机颜色缓冲区
                float4 color = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, input.uv);

                return color;
            }
            ENDHLSL
        }

        // ==================== Pass 4: Split View (Depth + Normals) ====================
        Pass
        {
            Name "Debug Split View"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex VertexFullscreen
            #pragma fragment FragSplitView

            float4 FragSplitView(Varyings input) : SV_Target
            {
                float3 color;

                // Split screen vertically
                // 垂直分屏
                if (input.uv.x < 0.5)
                {
                    // Left: Depth
                    // 左侧：深度
                    float depth = SampleSceneDepth(input.uv);
                    depth = Linear01Depth(depth, _ZBufferParams);
                    depth = saturate(depth * _DepthScale);
                    color = lerp(float3(0.2, 0.5, 1.0), float3(1.0, 0.2, 0.2), depth);
                }
                else
                {
                    // Right: Normals
                    // 右侧：法线
                    float3 normal = SampleSceneNormals(input.uv);
                    normal = normalize(normal);
                    color = normal * 0.5 + 0.5;
                }

                // Draw center divider line
                // 绘制中心分割线
                if (abs(input.uv.x - 0.5) < 0.002)
                {
                    color = float3(1, 1, 0); // Yellow line
                }

                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
