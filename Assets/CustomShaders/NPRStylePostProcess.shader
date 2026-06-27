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

            float _PosterizeStrength;
            float _Contrast;
            float _Saturation;
            float _ShadowCrush;
            float _HalftoneStrength;
            float _HalftoneScale;
            float4 _AccentColor;

            float3 AdjustSaturation(float3 color, float saturation)
            {
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(float3(luminance, luminance, luminance), color, saturation);
            }

            float HalftoneMask(float2 uv, float luminance)
            {
                float scale = max(_HalftoneScale, 1.0);
                float2 cell = frac(uv * _ScreenParams.xy / scale);
                float dotDistance = distance(cell, float2(0.5, 0.5));
                float radius = lerp(0.42, 0.15, saturate(luminance));
                return step(dotDistance, radius);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                float4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                float3 color = source.rgb;

                color = saturate((color - 0.5) * _Contrast + 0.5);
                color = AdjustSaturation(color, _Saturation);

                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                float shadowMask = 1.0 - smoothstep(0.24, 0.72, luminance);
                color = lerp(color, color * (1.0 - _ShadowCrush), shadowMask);
                color = lerp(color, color * _AccentColor.rgb, shadowMask * saturate(_ShadowCrush * 0.75));

                float levels = lerp(255.0, 5.0, saturate(_PosterizeStrength));
                float3 posterized = floor(saturate(color) * levels + 0.5) / levels;
                color = lerp(color, posterized, saturate(_PosterizeStrength));

                float halftone = HalftoneMask(uv, luminance);
                float3 inkedColor = color * lerp(0.58, 1.0, halftone);
                color = lerp(color, inkedColor, saturate(_HalftoneStrength) * shadowMask);

                return float4(saturate(color), source.a);
            }
            ENDHLSL
        }
    }
}
