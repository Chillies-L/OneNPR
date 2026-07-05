Shader "Hidden/NPR/StylePostProcess"
{
    Properties
    {
        _BlitTexture ("Blit Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "NPR Style Post Process"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            float _OutlineEnabled;
            float4 _OutlineColor;
            float _OutlineIntensity;
            float _OutlineThickness;
            float _DepthThreshold;
            float _NormalThreshold;
            float _ColorThreshold;
            float _UseDepth;
            float _UseNormals;
            float _UseColor;
            float _ColorGradingEnabled;
            float _HalftoneEnabled;
            float _PosterizeStrength;
            float _Contrast;
            float _Saturation;
            float _ShadowCrush;
            int _PatternType;
            float _PatternScale;
            float _PatternAngle;
            float _PatternIntensity;
            float _PatternBlend;
            float4 _PatternColor;
            float _PatternLumaThreshold;
            float4 _AccentColor;

            float3 AdjustSaturation(float3 color, float saturation)
            {
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(float3(luminance, luminance, luminance), color, saturation);
            }

            float NPRStyleLuminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            float2x2 GetRotationMatrix(float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float2x2(c, -s, s, c);
            }

            float3 ApplyPatternShading(float2 uv, float3 color)
            {
                if (_PatternType <= 0 || _PatternBlend <= 0.0)
                {
                    return color;
                }

                float luminance = NPRStyleLuminance(color);
                float threshold = max(_PatternLumaThreshold, 0.001);
                float patternRegion = 1.0 - smoothstep(threshold - 0.05, threshold + 0.05, luminance);

                float2 aspect = float2(_ScreenParams.x / max(_ScreenParams.y, 1.0), 1.0);
                float2 patternUV = (uv - 0.5) * aspect * max(_PatternScale, 1.0) * 50.0;
                patternUV = mul(GetRotationMatrix(_PatternAngle), patternUV);

                float dotRadius = saturate(luminance / threshold) * 0.7 * saturate(_PatternIntensity);
                float patternMask = 0.0;

                if (_PatternType == 1)
                {
                    float2 grid = frac(patternUV) - 0.5;
                    patternMask = 1.0 - smoothstep(dotRadius - 0.05, dotRadius + 0.05, length(grid));
                }
                else if (_PatternType == 2)
                {
                    float lineGradient = abs(frac(patternUV.x) - 0.5) * 2.0;
                    patternMask = 1.0 - smoothstep(dotRadius - 0.05, dotRadius + 0.05, lineGradient);
                }

                float3 patternedColor = lerp(_PatternColor.rgb, color, patternMask);
                return lerp(color, patternedColor, saturate(_PatternBlend) * patternRegion);
            }

            float RelativeDepthEdge(float2 uv, float2 texelSize)
            {
                float d00 = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float d11 = LinearEyeDepth(SampleSceneDepth(uv + texelSize), _ZBufferParams);
                float d10 = LinearEyeDepth(SampleSceneDepth(uv + float2(texelSize.x, 0.0)), _ZBufferParams);
                float d01 = LinearEyeDepth(SampleSceneDepth(uv + float2(0.0, texelSize.y)), _ZBufferParams);

                float baseDepth = max(d00, 0.001);
                float diagonalA = abs(d11 - d00) / baseDepth;
                float diagonalB = abs(d10 - d01) / baseDepth;
                return sqrt(diagonalA * diagonalA + diagonalB * diagonalB);
            }

            float NormalEdge(float2 uv, float2 texelSize)
            {
                float3 n00 = SampleSceneNormals(uv);
                float3 n11 = SampleSceneNormals(uv + texelSize);
                float3 n10 = SampleSceneNormals(uv + float2(texelSize.x, 0.0));
                float3 n01 = SampleSceneNormals(uv + float2(0.0, texelSize.y));

                float3 diagonalA = n11 - n00;
                float3 diagonalB = n10 - n01;
                return sqrt(dot(diagonalA, diagonalA) + dot(diagonalB, diagonalB));
            }

            float ColorEdge(float2 uv, float2 texelSize)
            {
                float l00 = NPRStyleLuminance(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel).rgb);
                float l11 = NPRStyleLuminance(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + texelSize, _BlitMipLevel).rgb);
                float l10 = NPRStyleLuminance(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x, 0.0), _BlitMipLevel).rgb);
                float l01 = NPRStyleLuminance(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(0.0, texelSize.y), _BlitMipLevel).rgb);

                float diagonalA = l11 - l00;
                float diagonalB = l10 - l01;
                return sqrt(diagonalA * diagonalA + diagonalB * diagonalB);
            }

            float DetectOutline(float2 uv)
            {
                if (_OutlineEnabled <= 0.5 || _OutlineIntensity <= 0.0)
                {
                    return 0.0;
                }

                float2 texelSize = max(_OutlineThickness, 0.1) / _ScreenParams.xy;
                float edge = 0.0;

                if (_UseDepth > 0.5)
                {
                    edge = max(edge, step(max(_DepthThreshold, 0.0005), RelativeDepthEdge(uv, texelSize)));
                }

                if (_UseNormals > 0.5)
                {
                    edge = max(edge, step(max(_NormalThreshold, 0.001), NormalEdge(uv, texelSize)));
                }

                if (_UseColor > 0.5)
                {
                    edge = max(edge, step(max(_ColorThreshold, 0.001), ColorEdge(uv, texelSize)));
                }

                return saturate(edge * _OutlineIntensity);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                float4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                float3 color = source.rgb;

                if (_ColorGradingEnabled > 0.5)
                {
                    color = saturate((color - 0.5) * _Contrast + 0.5);
                    color = AdjustSaturation(color, _Saturation);

                    float gradedLuminance = NPRStyleLuminance(color);
                    float shadowMask = 1.0 - smoothstep(0.24, 0.72, gradedLuminance);
                    color = lerp(color, color * (1.0 - _ShadowCrush), shadowMask);
                    color = lerp(color, color * _AccentColor.rgb, shadowMask * saturate(_ShadowCrush * 0.75));

                    float levels = lerp(255.0, 5.0, saturate(_PosterizeStrength));
                    float3 posterized = floor(saturate(color) * levels + 0.5) / levels;
                    color = lerp(color, posterized, saturate(_PosterizeStrength));
                }

                color = ApplyPatternShading(uv, color);

                float outline = DetectOutline(uv);
                color = lerp(color, _OutlineColor.rgb, outline);

                return float4(saturate(color), source.a);
            }
            ENDHLSL
        }
    }
}
