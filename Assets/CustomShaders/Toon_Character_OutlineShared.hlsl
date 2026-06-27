#ifndef TOON_CHARACTER_OUTLINE_SHARED_INCLUDED
#define TOON_CHARACTER_OUTLINE_SHARED_INCLUDED

#ifndef OUTLINE_NORMAL_SIGN
#define OUTLINE_NORMAL_SIGN 1.0
#endif

#ifndef OUTLINE_REVERSE_PASS
#define OUTLINE_REVERSE_PASS 0
#endif

#include "Toon_Character_MaterialInput.hlsl"

TEXTURE2D(_OutlineWidthMap);
SAMPLER(sampler_OutlineWidthMap);

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float4 color : COLOR;
};

float3 NormalizeOrFallback(float3 value, float3 fallback)
{
    float lenSq = dot(value, value);
    if (lenSq > 1e-6)
    {
        return value * rsqrt(lenSq);
    }

    return normalize(fallback);
}

bool IsUsableOutlineNormal(float3 candidate)
{
    float len = length(candidate);
    return len > 0.45 && len < 1.55;
}

float3 SelectAutoVertexColorNormal(float4 vertexColor, float3 normalOS)
{
    float3 baseNormal = normalize(normalOS);
    float3 ysaDirect = vertexColor.rgb;
    float3 encodedZeroToOne = vertexColor.rgb * 2.0 - 1.0;
    bool ysaValid = IsUsableOutlineNormal(ysaDirect);
    bool encodedValid = IsUsableOutlineNormal(encodedZeroToOne);

    if (ysaValid && any(vertexColor.rgb < 0.0))
    {
        return normalize(ysaDirect);
    }

    if (ysaValid && encodedValid)
    {
        float3 ysaNormal = normalize(ysaDirect);
        float3 encodedNormal = normalize(encodedZeroToOne);
        float ysaScore = abs(length(ysaDirect) - 1.0) + (1.0 - saturate(dot(ysaNormal, baseNormal)));
        float encodedScore = abs(length(encodedZeroToOne) - 1.0) + (1.0 - saturate(dot(encodedNormal, baseNormal)));
        return encodedScore < ysaScore ? encodedNormal : ysaNormal;
    }

    // YSA Toon writes the averaged normal directly into RGB, not as 0..1 remapped color.
    if (ysaValid)
    {
        return normalize(ysaDirect);
    }

    if (encodedValid)
    {
        return normalize(encodedZeroToOne);
    }

    return baseNormal;
}

float3 DecodeYSAVertexColorNormal(float3 normalOS, float4 vertexColor)
{
    float3 baseNormal = normalize(normalOS);

    if (_OutlineUseVertexColorNormals < 0.5)
    {
        return baseNormal;
    }

    float3 vertexNormal = baseNormal;

    if (_OutlineVertexColorNormalFormat < 0.5)
    {
        vertexNormal = SelectAutoVertexColorNormal(vertexColor, normalOS);
    }
    else if (_OutlineVertexColorNormalFormat < 1.5)
    {
        float3 ysaDirect = vertexColor.rgb;
        if (IsUsableOutlineNormal(ysaDirect))
        {
            vertexNormal = normalize(ysaDirect);
        }
    }
    else
    {
        float3 encodedZeroToOne = vertexColor.rgb * 2.0 - 1.0;
        if (IsUsableOutlineNormal(encodedZeroToOne))
        {
            vertexNormal = normalize(encodedZeroToOne);
        }
    }

    return NormalizeOrFallback(lerp(baseNormal, vertexNormal, saturate(_OutlineVertexNormalBlend)), baseNormal);
}

float3 GetPositionDirection(float3 positionOS, float3 normalOS)
{
    float3 normal = normalize(normalOS);
    float3 posDir = NormalizeOrFallback(positionOS, normal);
    float alignment = dot(posDir, normal);
    return NormalizeOrFallback(lerp(normal, posDir, saturate(alignment)), normal);
}

float3 GetHybridDirection(float3 positionOS, float3 normalOS)
{
    float3 normal = normalize(normalOS);
    float3 posDir = NormalizeOrFallback(positionOS, normal);
    float weight = abs(dot(normal, posDir));
    return NormalizeOrFallback(lerp(normal, posDir, weight * 0.5), normal);
}

float GetMiterScale(float3 faceNormalWS, float3 outlineNormalWS)
{
    float miterDot = saturate(abs(dot(normalize(faceNormalWS), normalize(outlineNormalWS))));
    float miterScale = 1.0 / max(_OutlineMiterMinDot, miterDot);
    miterScale = min(miterScale, _OutlineMiterMaxScale);
    return lerp(1.0, miterScale, saturate(_OutlineMiterStrength));
}

float GetAdaptiveWidth(float3 positionWS)
{
    float distance = length(positionWS - GetCameraPositionWS());
    float distanceScale = saturate(distance * 0.1);
    return lerp(1.0, 1.5, distanceScale);
}

float2 SafeNormalize2(float2 value, float2 fallback)
{
    float lenSq = dot(value, value);
    return lenSq > 1e-5 ? value * rsqrt(lenSq) : fallback;
}

float GetDirectionalWidthScale(float3 normalWS)
{
    float2 biasDir = SafeNormalize2(_OutlineBiasDirection.xy, float2(1.0, 1.0));
    float2 normalDir = SafeNormalize2(normalize(mul((float3x3)UNITY_MATRIX_V, normalWS)).xy, biasDir);
    float directional = dot(normalDir, biasDir);
    return max(0.05, 1.0 + directional * _OutlineDirectionalBias);
}

float3 GetOutlineDirectionOS(Attributes input)
{
    #if defined(_OUTLINEMODE_NORMAL)
        return DecodeYSAVertexColorNormal(input.normalOS, input.color);
    #elif defined(_OUTLINEMODE_POSITION)
        return GetPositionDirection(input.positionOS.xyz, input.normalOS);
    #else
        float3 smoothNormal = DecodeYSAVertexColorNormal(input.normalOS, input.color);
        float3 hybridDirection = GetHybridDirection(input.positionOS.xyz, smoothNormal);
        return NormalizeOrFallback(hybridDirection, smoothNormal);
    #endif
}

Varyings OutlineVertex(Attributes input)
{
    Varyings output;

    float2 uv = TRANSFORM_TEX(input.uv, _OutlineWidthMap);
    float widthMapValue = SAMPLE_TEXTURE2D_LOD(_OutlineWidthMap, sampler_OutlineWidthMap, uv, 0).r;

    float3 outlineDirection = GetOutlineDirectionOS(input);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 faceNormalWS = normalize(TransformObjectToWorldNormal(input.normalOS));
    float3 normalWS = normalize(TransformObjectToWorldNormal(outlineDirection)) * OUTLINE_NORMAL_SIGN;

    float finalWidth = _OutlineWidth * widthMapValue * GetAdaptiveWidth(positionWS);
    finalWidth = clamp(finalWidth, _OutlineMinWidth, _OutlineMaxWidth);

    #if OUTLINE_REVERSE_PASS
        finalWidth *= max(_OutlineReverseWidthScale, 0.0);
    #endif

    float miterScale = 1.0;
    #if defined(_OUTLINEMODE_NORMAL)
        miterScale = GetMiterScale(faceNormalWS, normalWS);
    #endif

    finalWidth *= miterScale;
    finalWidth *= GetDirectionalWidthScale(normalWS);

    #if OUTLINE_REVERSE_PASS
        positionWS += normalWS * finalWidth;
    #else
        if (_OutlineUseWorldShell > 0.5)
        {
            positionWS += normalWS * finalWidth;
        }
    #endif

    output.positionHCS = TransformWorldToHClip(positionWS);

    #if !OUTLINE_REVERSE_PASS
        if (_OutlineUseWorldShell <= 0.5)
        {
            float4 positionVS = mul(UNITY_MATRIX_MV, input.positionOS);
            float4 originVS = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0));
            float3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, normalWS));
            float2 normalDir = SafeNormalize2(normalVS.xy, float2(0.0, 1.0));
            float2 radialDir = SafeNormalize2(positionVS.xy - originVS.xy, normalDir);

            #if defined(_OUTLINEMODE_POSITION)
                float2 screenDir = radialDir;
            #elif defined(_OUTLINEMODE_NORMAL)
                float2 screenDir = normalDir;
            #else
                float2 screenDir = SafeNormalize2(normalDir + radialDir, normalDir);
            #endif

            output.positionHCS.xy += screenDir * finalWidth * output.positionHCS.w;
        }
    #endif

    #if OUTLINE_REVERSE_PASS
        output.positionHCS.z += _OutlineReverseZOffset * 0.0001;
    #else
        output.positionHCS.z += _OutlineZOffset * 0.0001;
    #endif

    output.color = input.color;
    return output;
}

float4 OutlineFragment(Varyings input) : SV_Target
{
    #if OUTLINE_REVERSE_PASS
        clip(_OutlineRenderReversePass - 0.5);
        clip(_OutlineReverseWidthScale - 0.0001);
    #endif

    return _OutlineColor;
}

#endif
