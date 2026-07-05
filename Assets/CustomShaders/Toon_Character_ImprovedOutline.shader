Shader "Custom/NPR/Toon_Character_ImprovedOutline"
{
    Properties
    {
        [Header(Base Colors)]
        [MainTexture] _BaseMap("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _1stShadeColor("1st Shade Color", Color) = (0.7,0.7,0.7,1)
        _2ndShadeColor("2nd Shade Color", Color) = (0.4,0.4,0.4,1)

        [Header(Anime Palette)]
        _AutoShadeColor("Auto Anime Shade Palette", Range(0,1)) = 1
        _1stShadeTint("Auto 1st Shade Tint", Color) = (0.86,0.78,0.95,1)
        _2ndShadeTint("Auto 2nd Shade Tint", Color) = (0.55,0.48,0.72,1)
        _ShadeSaturation("Shade Saturation Boost", Range(0,1)) = 0.25
        _LitColorBoost("Lit Color Boost", Range(0,0.5)) = 0.08

        [Header(AI Asset Maps)]
        _ShadeMap("Shade Map (Shadow Color Texture)", 2D) = "white" {}
        _ShadeMapStrength("Shade Map Strength", Range(0,1)) = 0
        _ControlMap("Control Map (R Shadow G Feather B Spec A Rim)", 2D) = "gray" {}
        _ControlMapStrength("Control Map Strength", Range(0,1)) = 0
        _ControlShadowStepRange("Control Shadow Step Range", Range(0,0.5)) = 0.15
        _ControlFeatherRange("Control Feather Range", Range(0,0.25)) = 0.06
        _RampTexture("Ramp Texture", 2D) = "gray" {}
        _RampTextureStrength("Ramp Texture Strength", Range(0,1)) = 0
        _RampTextureOffset("Ramp Texture Offset", Range(-1,1)) = 0
        _ShadowAntiFlicker("Shadow Anti-Flicker", Range(0,1)) = 0.35

        [Header(Shading Control)]
        _BaseStep("Base Shadow Step", Range(0,1)) = 0.5
        _BaseFeather("Base Shadow Feather", Range(0,0.5)) = 0.05
        _ShadeStep("2nd Shadow Step", Range(0,1)) = 0.3
        _ShadeFeather("2nd Shadow Feather", Range(0,0.5)) = 0.05

        [Header(Anime Light Control)]
        _UseVirtualLight("Use Virtual Key Light", Range(0,1)) = 1
        _VirtualLightDirection("Virtual Key Light Direction", Vector) = (0.35,0.75,0.55,0)
        _LightColorInfluence("Scene Light Color Influence", Range(0,1)) = 0.25
        _AmbientStrength("Ambient Strength", Range(0,1)) = 0.18
        _AdditionalLightInfluence("Additional Light Influence", Range(0,1)) = 0.15

        [Header(Specular Highlight)]
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularSize("Specular Size", Range(0,1)) = 0.1
        _SpecularFeather("Specular Edge Feather", Range(0,0.25)) = 0.02
        _SpecularIntensity("Specular Intensity", Range(0,2)) = 1.0

        [Header(Rim Light)]
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.1,8)) = 3.0
        _RimThreshold("Rim Threshold", Range(0,1)) = 0.55
        _RimFeather("Rim Feather", Range(0,0.5)) = 0.08
        _RimLightAlign("Rim Light Direction Align", Range(0,1)) = 0.45
        _RimIntensity("Rim Intensity", Range(0,2)) = 0.5

        [Header(Improved Outline)]
        _OutlineWidth("Outline Width", Range(0,0.1)) = 0.005
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidthMap("Outline Width Map (Optional)", 2D) = "white" {}

        [Header(Outline Fix Options)]
        [KeywordEnum(Normal, Position, Hybrid)] _OutlineMode("Outline Mode", Float) = 0
        _OutlineZOffset("Z Offset (Fix Clipping)", Range(-1, 1)) = 0
        _OutlineMinWidth("Min Width (Prevent Too Thin)", Range(0, 0.01)) = 0.0001
        _OutlineMaxWidth("Max Width (Prevent Too Thick)", Range(0, 0.1)) = 0.08
        _OutlineMiterStrength("Miter Strength", Range(0, 1)) = 1
        _OutlineMiterMinDot("Miter Min Dot", Range(0.05, 1)) = 0.35
        _OutlineMiterMaxScale("Miter Max Scale", Range(1, 3)) = 1.8
        [Toggle] _OutlineUseVertexColorNormals("Use Vertex Color Normals", Float) = 1
        [Enum(Auto, 0, YSADirect, 1, EncodedZeroToOne, 2)] _OutlineVertexColorNormalFormat("Vertex Color Normal Format", Float) = 0
        _OutlineVertexNormalBlend("Vertex Color Normal Blend", Range(0, 1)) = 1
        [Toggle] _OutlineRenderReversePass("Render Reverse Normal Pass", Float) = 1
        _OutlineReverseWidthScale("Reverse Pass Width Scale", Range(0, 2)) = 0.5
        _OutlineReverseZOffset("Reverse Pass Z Offset", Range(-1, 1)) = 0
        [Toggle] _OutlineUseWorldShell("Use World Normal Shell", Float) = 1
        _OutlineDirectionalBias("Directional Width Bias", Range(-1, 1)) = 0
        _OutlineBiasDirection("Bias Direction XY", Vector) = (1, 1, 0, 0)

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

        // ==================== Pass 1: Forward Lit ====================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ToonVertex
            #pragma fragment ToonFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Toon_Character_MaterialInput.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_ShadeMap);
            SAMPLER(sampler_ShadeMap);
            TEXTURE2D(_ControlMap);
            SAMPLER(sampler_ControlMap);
            TEXTURE2D(_RampTexture);
            SAMPLER(sampler_RampTexture);

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

            Varyings ToonVertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);
                output.positionHCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.uv = input.uv;
                return output;
            }

            float3 ApplySaturation(float3 color, float saturation)
            {
                float luma = dot(color, float3(0.299, 0.587, 0.114));
                return max(lerp(float3(luma, luma, luma), color, saturation), 0.0);
            }

            float3 BuildAnimeShade(float3 albedo, float3 manualShade, float3 tint, float valueScale)
            {
                float3 autoShade = albedo * tint * valueScale;
                autoShade = ApplySaturation(autoShade, 1.0 + _ShadeSaturation);
                return lerp(manualShade, autoShade, _AutoShadeColor);
            }

            float3 ApplyLightColorInfluence(float3 color, float3 lightColor)
            {
                return color * lerp(float3(1.0, 1.0, 1.0), lightColor, _LightColorInfluence);
            }

            float3 SafeNormalize(float3 value, float3 fallback)
            {
                float lenSq = dot(value, value);
                return lenSq > 1e-5 ? value * rsqrt(lenSq) : fallback;
            }

            float3 GetAnimeLightDirection(float3 sceneLightDirWS)
            {
                float3 virtualLight = _VirtualLightDirection.xyz;
                float invLen = rsqrt(max(dot(virtualLight, virtualLight), 0.0001));
                virtualLight *= invLen;
                return SafeNormalize(lerp(sceneLightDirWS, virtualLight, _UseVirtualLight), virtualLight);
            }

            float4 SampleControlMap(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_ControlMap, sampler_ControlMap, TRANSFORM_TEX(uv, _ControlMap));
            }

            float3 ApplyRampTexture(float3 litColor, float3 shade1Color, float3 shade2Color, float halfLambert)
            {
                float rampU = saturate(halfLambert + _RampTextureOffset);
                float3 rampSample = SAMPLE_TEXTURE2D(_RampTexture, sampler_RampTexture, float2(rampU, 0.5)).rgb;
                float rampValue = saturate(dot(rampSample, float3(0.299, 0.587, 0.114)));
                float3 darkToMid = lerp(shade2Color, shade1Color, saturate(rampValue * 2.0));
                return lerp(darkToMid, litColor, saturate((rampValue - 0.5) * 2.0));
            }

            float3 ApplyThreeColorToon(float3 baseColor, float NdotL, float2 uv, float4 controlMap)
            {
                float halfLambert = NdotL * 0.5 + 0.5;
                float controlStrength = saturate(_ControlMapStrength);
                float shadowStepOffset = (controlMap.r - 0.5) * _ControlShadowStepRange * controlStrength;
                float featherBoost = controlMap.g * _ControlFeatherRange * controlStrength;
                float baseFeather = _BaseFeather + featherBoost;
                float shadeFeather = _ShadeFeather + featherBoost;
                float baseStep = saturate(_BaseStep + shadowStepOffset);
                float shadeStep = saturate(_ShadeStep + shadowStepOffset);
                float toShade1 = 1.0 - smoothstep(baseStep - baseFeather, baseStep + baseFeather, halfLambert);
                float toShade2 = 1.0 - smoothstep(shadeStep - shadeFeather, shadeStep + shadeFeather, halfLambert);
                float3 litColor = saturate(baseColor * (1.0 + _LitColorBoost));
                float3 shade1Color = BuildAnimeShade(baseColor, baseColor * _1stShadeColor.rgb, _1stShadeTint.rgb, 0.72);
                float3 shade2Color = BuildAnimeShade(baseColor, baseColor * _2ndShadeColor.rgb, _2ndShadeTint.rgb, 0.46);
                float3 shadeMap = SAMPLE_TEXTURE2D(_ShadeMap, sampler_ShadeMap, TRANSFORM_TEX(uv, _ShadeMap)).rgb;
                float3 shadeMapTint = lerp(float3(1.0, 1.0, 1.0), shadeMap, saturate(_ShadeMapStrength));
                shade1Color *= shadeMapTint;
                shade2Color *= shadeMapTint;
                float3 shadeColor = lerp(shade1Color, shade2Color, toShade2);
                float3 steppedColor = lerp(litColor, shadeColor, toShade1);
                float3 rampColor = ApplyRampTexture(litColor, shade1Color, shade2Color, halfLambert);
                return lerp(steppedColor, rampColor, saturate(_RampTextureStrength));
            }

            float ComputeToonSpecular(float3 normalWS, float3 viewDirWS, float3 lightDirWS)
            {
                float3 halfDir = normalize(viewDirWS + lightDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                float threshold = 1.0 - _SpecularSize;
                float feather = max(_SpecularFeather, 0.0001);
                float hardMask = step(threshold, NdotH);
                float softMask = smoothstep(threshold - feather, threshold + feather, NdotH);
                float specularMask = lerp(hardMask, softMask, saturate(_SpecularFeather * 20.0));
                return specularMask * _SpecularIntensity;
            }

            float3 ComputeRimLight(float3 normalWS, float3 viewDirWS, float NdotL)
            {
                float rimDot = 1.0 - saturate(dot(normalWS, viewDirWS));
                float rimShape = pow(rimDot, _RimPower);
                float lit01 = saturate(NdotL * 0.5 + 0.5);
                float litAlignedThreshold = lerp(1.0, _RimThreshold, lit01);
                float rimThreshold = lerp(_RimThreshold, litAlignedThreshold, _RimLightAlign);
                float rimMask = smoothstep(rimThreshold, rimThreshold + max(_RimFeather, 0.0001), rimShape);
                return _RimColor.rgb * rimMask * _RimIntensity;
            }

            float4 ToonFragment(Varyings input) : SV_Target
            {
                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap));
                float3 albedo = baseMap.rgb * _BaseColor.rgb;
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                Light mainLight = GetMainLight();
                float3 sceneLightDirWS = normalize(mainLight.direction);
                float3 lightDirWS = GetAnimeLightDirection(sceneLightDirWS);
                float3 lightColor = mainLight.color;
                float lightAttenuation = mainLight.distanceAttenuation;
                float NdotL = dot(normalWS, lightDirWS);
                float4 controlMap = SampleControlMap(input.uv);
                float controlStrength = saturate(_ControlMapStrength);
                float specularControl = lerp(1.0, saturate(controlMap.b * 2.0), controlStrength);
                float rimControl = lerp(1.0, controlMap.a, controlStrength);
                float3 diffuse = ApplyThreeColorToon(albedo, NdotL, input.uv, controlMap);
                diffuse = ApplyLightColorInfluence(diffuse, lightColor) * lightAttenuation;
                float specular = ComputeToonSpecular(normalWS, viewDirWS, lightDirWS) * specularControl;
                float3 specularContribution = ApplyLightColorInfluence(_SpecularColor.rgb * specular, lightColor);
                float3 rimContribution = ComputeRimLight(normalWS, viewDirWS, NdotL) * rimControl;

                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    float3 additionalLightDir = normalize(light.direction);
                    float additionalNdotL = dot(normalWS, additionalLightDir);
                    float additionalDiffuse = smoothstep(_BaseStep - _BaseFeather, _BaseStep + _BaseFeather, additionalNdotL * 0.5 + 0.5);
                    float3 additionalColor = ApplyLightColorInfluence(albedo * additionalDiffuse, light.color);
                    diffuse += additionalColor * light.distanceAttenuation * light.shadowAttenuation * _AdditionalLightInfluence;
                }
                #endif

                float3 ambient = SampleSH(normalWS) * albedo * _AmbientStrength;
                float3 finalColor = diffuse + ambient + specularContribution + rimContribution;
                return float4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }

        // ==================== Pass 2: Improved Outline ====================
        Pass
        {
            Name "OutlineVertexColorNormalsForward"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma multi_compile _OUTLINEMODE_NORMAL _OUTLINEMODE_POSITION _OUTLINEMODE_HYBRID

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Toon_Character_OutlineShared.hlsl"
            ENDHLSL
        }

        // ==================== Pass 3: Reversed Vertex Color Normal Outline ====================
        Pass
        {
            Name "OutlineVertexColorNormalsInverted"
            Tags { "LightMode" = "UniversalForwardOnly" }
            Cull Front
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma multi_compile _OUTLINEMODE_NORMAL _OUTLINEMODE_POSITION _OUTLINEMODE_HYBRID
            #define OUTLINE_NORMAL_SIGN -1.0
            #define OUTLINE_REVERSE_PASS 1

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Toon_Character_OutlineShared.hlsl"
            ENDHLSL
        }

        // ==================== Pass 4: Shadow Caster ====================
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
            #include "Toon_Character_MaterialInput.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float3 ApplyShadowBiasSimple(float3 positionWS, float3 normalWS)
            {
                float bias = 0.005;
                return positionWS + normalWS * bias;
            }

            Varyings ShadowVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                positionWS = ApplyShadowBiasSimple(positionWS, normalWS);
                output.positionHCS = TransformWorldToHClip(positionWS);
                return output;
            }

            float4 ShadowFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // ==================== Pass 5: Depth Normals ====================
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
            #include "Toon_Character_MaterialInput.hlsl"

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
                return float4(normalize(input.normalWS), 0);
            }
            ENDHLSL
        }
    }

    CustomEditor "ToonShaderGUI"
    FallBack "Universal Render Pipeline/Lit"
}
