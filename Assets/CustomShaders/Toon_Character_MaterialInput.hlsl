#ifndef TOON_CHARACTER_MATERIAL_INPUT_INCLUDED
#define TOON_CHARACTER_MATERIAL_INPUT_INCLUDED

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _BaseColor;
    float4 _1stShadeColor;
    float4 _2ndShadeColor;
    float _AutoShadeColor;
    float4 _1stShadeTint;
    float4 _2ndShadeTint;
    float _ShadeSaturation;
    float _LitColorBoost;
    float _BaseStep;
    float _BaseFeather;
    float _ShadeStep;
    float _ShadeFeather;
    float _UseVirtualLight;
    float4 _VirtualLightDirection;
    float _LightColorInfluence;
    float _AmbientStrength;
    float _AdditionalLightInfluence;
    float4 _SpecularColor;
    float _SpecularSize;
    float _SpecularFeather;
    float _SpecularIntensity;
    float4 _RimColor;
    float _RimPower;
    float _RimThreshold;
    float _RimFeather;
    float _RimLightAlign;
    float _RimIntensity;
    float _OutlineWidth;
    float4 _OutlineColor;
    float4 _OutlineWidthMap_ST;
    float _OutlineZOffset;
    float _OutlineMinWidth;
    float _OutlineMaxWidth;
    float _OutlineMiterStrength;
    float _OutlineMiterMinDot;
    float _OutlineMiterMaxScale;
    float _OutlineUseVertexColorNormals;
    float _OutlineVertexColorNormalFormat;
    float _OutlineVertexNormalBlend;
    float _OutlineRenderReversePass;
    float _OutlineReverseWidthScale;
    float _OutlineReverseZOffset;
    float _OutlineUseWorldShell;
    float _OutlineDirectionalBias;
    float4 _OutlineBiasDirection;
CBUFFER_END

#endif
