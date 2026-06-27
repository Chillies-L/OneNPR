Shader "Hidden/NPR/SobelOutline"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Sobel Edge Detection"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _DepthThreshold;
            float _NormalThreshold;
            float _UseDepth;
            float _UseNormals;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            // Sobel operator for edge detection
            // Sobel算子用于边缘检测
            //
            // Theory (原理):
            // Sobel operator uses convolution kernels to detect edges
            // Sobel算子使用卷积核检测边缘
            //
            // Horizontal kernel (水平核):     Vertical kernel (垂直核):
            // [-1  0  1]                      [-1 -2 -1]
            // [-2  0  2]                      [ 0  0  0]
            // [-1  0  1]                      [ 1  2  1]
            //
            // The gradient magnitude is: sqrt(Gx^2 + Gy^2)
            // 梯度幅值为: sqrt(Gx^2 + Gy^2)

            float SobelDepth(float2 uv)
            {
                // Sample depth at 8 neighboring pixels (3x3 kernel)
                // 在8个相邻像素处采样深度（3x3核）
                float2 texelSize = _MainTex_TexelSize.xy * _OutlineThickness;

                // Center and 8 neighbors
                // 中心和8个邻居
                float d00 = SampleSceneDepth(uv + float2(-texelSize.x, -texelSize.y));
                float d10 = SampleSceneDepth(uv + float2(0, -texelSize.y));
                float d20 = SampleSceneDepth(uv + float2(texelSize.x, -texelSize.y));

                float d01 = SampleSceneDepth(uv + float2(-texelSize.x, 0));
                float d21 = SampleSceneDepth(uv + float2(texelSize.x, 0));

                float d02 = SampleSceneDepth(uv + float2(-texelSize.x, texelSize.y));
                float d12 = SampleSceneDepth(uv + float2(0, texelSize.y));
                float d22 = SampleSceneDepth(uv + float2(texelSize.x, texelSize.y));

                // Convert depth to linear eye depth
                // 将深度转换为线性视空间深度
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
                // Gx = [-1  0  1]   Gy = [-1 -2 -1]
                //      [-2  0  2]        [ 0  0  0]
                //      [-1  0  1]        [ 1  2  1]
                float Gx = -d00 + d20 - 2.0 * d01 + 2.0 * d21 - d02 + d22;
                float Gy = -d00 - 2.0 * d10 - d20 + d02 + 2.0 * d12 + d22;

                // Calculate gradient magnitude
                // 计算梯度幅值
                float depthEdge = sqrt(Gx * Gx + Gy * Gy);

                return depthEdge > _DepthThreshold ? 1.0 : 0.0;
            }

            float SobelNormal(float2 uv)
            {
                // Sample normals at 8 neighboring pixels
                // 在8个相邻像素处采样法线
                float2 texelSize = _MainTex_TexelSize.xy * _OutlineThickness;

                float3 n00 = SampleSceneNormals(uv + float2(-texelSize.x, -texelSize.y));
                float3 n10 = SampleSceneNormals(uv + float2(0, -texelSize.y));
                float3 n20 = SampleSceneNormals(uv + float2(texelSize.x, -texelSize.y));

                float3 n01 = SampleSceneNormals(uv + float2(-texelSize.x, 0));
                float3 n21 = SampleSceneNormals(uv + float2(texelSize.x, 0));

                float3 n02 = SampleSceneNormals(uv + float2(-texelSize.x, texelSize.y));
                float3 n12 = SampleSceneNormals(uv + float2(0, texelSize.y));
                float3 n22 = SampleSceneNormals(uv + float2(texelSize.x, texelSize.y));

                // Apply Sobel operator to each normal component
                // 对法线的每个分量应用Sobel算子
                float3 Gx = -n00 + n20 - 2.0 * n01 + 2.0 * n21 - n02 + n22;
                float3 Gy = -n00 - 2.0 * n10 - n20 + n02 + 2.0 * n12 + n22;

                // Calculate gradient magnitude for normals
                // 计算法线的梯度幅值
                float normalEdge = sqrt(dot(Gx, Gx) + dot(Gy, Gy));

                return normalEdge > _NormalThreshold ? 1.0 : 0.0;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Sample original color
                // 采样原始颜色
                float4 originalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Detect edges using depth and normals
                // 使用深度和法线检测边缘
                float depthEdge = 0.0;
                float normalEdge = 0.0;

                if (_UseDepth > 0.5)
                {
                    depthEdge = SobelDepth(input.uv);
                }

                if (_UseNormals > 0.5)
                {
                    normalEdge = SobelNormal(input.uv);
                }

                // Combine depth and normal edges (OR operation)
                // 结合深度和法线边缘（OR操作）
                float edge = max(depthEdge, normalEdge);

                // Blend outline color with original color
                // 将描边颜色与原始颜色混合
                float4 finalColor = lerp(originalColor, _OutlineColor, edge);

                return finalColor;
            }

            ENDHLSL
        }
    }
}
