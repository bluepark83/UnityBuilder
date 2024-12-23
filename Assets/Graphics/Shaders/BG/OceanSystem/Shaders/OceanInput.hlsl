﻿#ifndef WATER_INPUT_INCLUDED
#define WATER_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
half _BumpScale;
half4 _DitherPattern_TexelSize;
CBUFFER_END
half _MaxDepth;
half _MaxWaveHeight;
int _DebugPass;
// 카메라 DepthMap 사용하지 않음
// half4 _VeraslWater_DepthCamParams;

SAMPLER(sampler_ScreenTextures_linear_clamp);
#if defined(_REFLECTION_PLANARREFLECTION)
TEXTURE2D(_PlanarReflectionTexture);
#endif
TEXTURE2D(_CameraDepthTexture);
TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture_linear_clamp);

TEXTURE2D(_WaterDepthMap); SAMPLER(sampler_WaterDepthMap_linear_clamp);

TEXTURE2D(_AbsorptionScatteringRamp); SAMPLER(sampler_AbsorptionScatteringRamp);
TEXTURE2D(_SurfaceMap); SAMPLER(sampler_SurfaceMap);
TEXTURE2D(_FoamMap); SAMPLER(sampler_FoamMap);
TEXTURE2D(_DitherPattern); SAMPLER(sampler_DitherPattern);

float _Frequency;
float _WaveSpeed;

// 물거품 사이즈
float _FoamSize;

// Water Depth Factor
float _WaterDepthFactor;

struct WaterSurfaceData
{
    half3 absorption;
	half3 scattering;
    half3 normal;
    half  foam;
};
#endif