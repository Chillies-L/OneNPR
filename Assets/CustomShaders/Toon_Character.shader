Shader "Custom/NPR/Toon_Character"
{
    Properties
    {
        [Header(Base Colors)]
        [MainTexture] _BaseMap("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _1stShadeColor("1st Shade Color", Color) = (0.7,0.7,0.7,1)
        _2ndShadeColor("2nd Shade Color", Color) = (0.4,0.4,0.4,1)

        [Header(Shading Control)]
        _BaseStep("Base Shadow Step", Range(0,1)) = 0.5
        _BaseFeather("Base Shadow Feather", Range(0,0.5)) = 0.05
        _ShadeStep("2nd Shadow Step", Range(0,1)) = 0.3
        _ShadeFeather("2nd Shadow Feather", Range(0,0.5)) = 0.05

        [Header(Specular Highlight)]
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularSize("Specular Size", Range(0,1)) = 0.1
        _SpecularIntensity("Specular Intensity", Range(0,2)) = 1.0

        [Header(Rim Light)]
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.1,8)) = 3.0
        _RimIntensity("Rim Intensity", Range(0,2)) = 0.5

        [Header(Outline)]
        _OutlineWidth("Outline Width", Range(0,0.1)) = 0.005
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidthMap("Outline Width Map (Optional)", 2D) = "white" {}

        [Header(Rendering Options)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        // ==================== Pass 1: Forward Lit (Main Toon Shading) ====================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ToonVertex
            #pragma fragment ToonFragment

            // URP keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ==================== Properties ====================
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _1stShadeColor;
                float4 _2ndShadeColor;
                float _BaseStep;
                float _BaseFeather;
                float _ShadeStep;
                float _ShadeFeather;
                float4 _SpecularColor;
                float _SpecularSize;
                float _SpecularIntensity;
                float4 _RimColor;
                float _RimPower;
                float _RimIntensity;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // ==================== Vertex Input/Output ====================
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            // ==================== Vertex Shader ====================
            Varyings ToonVertex(Attributes input)
            {
                Varyings output;

                // Transform to world space
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            // ==================== Toon Lighting Functions ====================

            // Three-color banding: applies stepped lighting to create cartoon shading
            // 三段色阶：应用阶梯式光照创建卡通阴影效果
            float3 ApplyThreeColorToon(
                float3 baseColor,
                float3 shade1Color,
                float3 shade2Color,
                float NdotL)
            {
                // Convert NdotL to 0-1 range (Half-Lambert)
                // 将NdotL转换到0-1范围（半朗伯）
                float halfLambert = NdotL * 0.5 + 0.5;

                // Smoothstep creates soft or hard boundaries between color bands
                // smoothstep创建色阶之间的软边界或硬边界
                float toShade1 = 1.0 - smoothstep(_BaseStep - _BaseFeather, _BaseStep + _BaseFeather, halfLambert);
                float toShade2 = 1.0 - smoothstep(_ShadeStep - _ShadeFeather, _ShadeStep + _ShadeFeather, halfLambert);

                // Blend between shade colors based on lighting
                // 根据光照在阴影颜色之间混合
                float3 shadeColor = lerp(shade1Color, shade2Color, toShade2);
                return lerp(baseColor, shadeColor, toShade1);
            }

            // Hard specular highlight for toon style
            // 卡通风格的硬高光
            float ComputeToonSpecular(
                float3 normalWS,
                float3 viewDirWS,
                float3 lightDirWS)
            {
                // Blinn-Phong half vector
                // Blinn-Phong半角向量
                float3 halfDir = normalize(viewDirWS + lightDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));

                // Hard edge specular using step function
                // 使用阶跃函数创建硬边高光
                float specularMask = step(1.0 - _SpecularSize, NdotH);
                return specularMask * _SpecularIntensity;
            }

            // Rim light for edge highlighting
            // 边缘光用于轮廓高亮
            float3 ComputeRimLight(float3 normalWS, float3 viewDirWS)
            {
                // Fresnel-like rim effect
                // 类菲涅尔边缘效果
                float rimDot = 1.0 - saturate(dot(normalWS, viewDirWS));
                float rimIntensity = pow(rimDot, _RimPower) * _RimIntensity;
                return _RimColor.rgb * rimIntensity;
            }

            // ==================== Fragment Shader ====================
            float4 ToonFragment(Varyings input) : SV_Target
            {
                // Sample base texture
                // 采样基础纹理
                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                float3 albedo = baseMap.rgb * _BaseColor.rgb;

                // Normalize vectors
                // 归一化向量
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));

                // Get main light
                // 获取主光源
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float3 lightDirWS = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;
                float lightAttenuation = mainLight.shadowAttenuation * mainLight.distanceAttenuation;

                // Calculate N dot L for lighting
                // 计算N·L用于光照
                float NdotL = dot(normalWS, lightDirWS);

                // Apply three-color toon shading
                // 应用三段色阶卡通光照
                float3 diffuse = ApplyThreeColorToon(
                    albedo,
                    albedo * _1stShadeColor.rgb,
                    albedo * _2ndShadeColor.rgb,
                    NdotL
                );

                // Apply main light color and shadows
                // 应用主光源颜色和阴影
                diffuse *= lightColor * lightAttenuation;

                // Add specular highlight
                // 添加高光
                float specular = ComputeToonSpecular(normalWS, viewDirWS, lightDirWS);
                float3 specularContribution = _SpecularColor.rgb * specular * lightColor;

                // Add rim light
                // 添加边缘光
                float3 rimContribution = ComputeRimLight(normalWS, viewDirWS);

                // Additional lights support
                // 附加光源支持
                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    float3 additionalLightDir = normalize(light.direction);
                    float additionalNdotL = dot(normalWS, additionalLightDir);

                    // Simple toon shading for additional lights
                    // 附加光源的简单卡通光照
                    float additionalDiffuse = smoothstep(_BaseStep - _BaseFeather, _BaseStep + _BaseFeather, additionalNdotL * 0.5 + 0.5);
                    diffuse += albedo * light.color * additionalDiffuse * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif

                // Add ambient lighting
                // 添加环境光
                float3 ambient = SampleSH(normalWS) * albedo * 0.3;

                // Final composition
                // 最终合成
                float3 finalColor = diffuse + ambient + specularContribution + rimContribution;

                return float4(finalColor, _BaseColor.a);
            }

            ENDHLSL
        }

        // ==================== Pass 2: Outline (Normal Expansion) ====================
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front  // Render back faces only for outline shell

            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _OutlineColor;
                float4 _OutlineWidthMap_ST;
            CBUFFER_END

            TEXTURE2D(_OutlineWidthMap);
            SAMPLER(sampler_OutlineWidthMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;  // 顶点颜色存储平滑法线
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            // Camera-scaled outline width calculation
            // 相机缩放的描边宽度计算
            float GetCameraScaledWidth(float3 positionWS)
            {
                // Scale outline based on distance to camera
                // 根据到相机的距离缩放描边
                float3 cameraPos = GetCameraPositionWS();
                float distance = length(positionWS - cameraPos);

                // Prevent outline from becoming too thin or thick
                // 防止描边变得太细或太粗
                float distanceScale = saturate(distance * 0.2);
                return _OutlineWidth * (0.5 + distanceScale * 0.5);
            }

            Varyings OutlineVertex(Attributes input)
            {
                Varyings output;

                // Use smoothed normals from vertex color if available
                // 如果可用，从顶点颜色使用平滑法线
                float3 normalOS = input.normalOS;

                // Check if vertex color contains smoothed normal (not white)
                // 检查顶点颜色是否包含平滑法线（不是白色）
                float colorIntensity = dot(input.color.rgb, float3(1, 1, 1));
                if (colorIntensity < 2.9) // 不是纯白色(3.0)
                {
                    // Decode normal from [0,1] color to [-1,1] vector
                    // 从[0,1]颜色解码到[-1,1]向量
                    float3 smoothedNormal = input.color.rgb * 2.0 - 1.0;
                    normalOS = normalize(smoothedNormal);
                }

                // Transform to world space
                // 转换到世界空间
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);

                // Sample outline width map if available
                // 如果可用，采样描边宽度贴图
                float2 uv = TRANSFORM_TEX(input.uv, _OutlineWidthMap);
                float widthMapValue = SAMPLE_TEXTURE2D_LOD(_OutlineWidthMap, sampler_OutlineWidthMap, uv, 0).r;

                // Calculate final outline width
                // 计算最终描边宽度
                float finalWidth = GetCameraScaledWidth(positionWS) * widthMapValue;

                // Expand vertex along smoothed normal direction
                // 沿平滑法线方向扩展顶点
                positionWS += normalize(normalWS) * finalWidth;

                // Transform to clip space
                // 转换到裁剪空间
                output.positionHCS = TransformWorldToHClip(positionWS);

                return output;
            }

            float4 OutlineFragment(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }

            ENDHLSL
        }

        // ==================== Pass 3: Shadow Caster ====================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            // Simple shadow bias to prevent shadow acne
            // 简单的阴影偏移以防止阴影失真
            float3 ApplyShadowBiasSimple(float3 positionWS, float3 normalWS)
            {
                // Apply a small offset along the normal
                // 沿法线方向应用小偏移
                float bias = 0.005;
                return positionWS + normalWS * bias;
            }

            Varyings ShadowVertex(Attributes input)
            {
                Varyings output;

                // Transform to world space
                // 转换到世界空间
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                // Apply simple shadow bias
                // 应用简单的阴影偏移
                positionWS = ApplyShadowBiasSimple(positionWS, normalWS);

                // Transform to clip space
                // 转换到裁剪空间
                output.positionHCS = TransformWorldToHClip(positionWS);

                return output;
            }

            float4 ShadowFragment(Varyings input) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }

        // ==================== Pass 4: Depth Normals ====================
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            float4 DepthNormalsFragment(Varyings input) : SV_Target
            {
                // Output normals in world space for edge detection
                // 输出世界空间法线用于边缘检测
                return float4(normalize(input.normalWS), 0);
            }

            ENDHLSL
        }
    }

    CustomEditor "ToonShaderGUI"
    FallBack "Universal Render Pipeline/Lit"
}
