﻿#ifndef WATER_COMMON_INCLUDED
#define WATER_COMMON_INCLUDED

#define SHADOWS_SCREEN 0

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "OceanInput.hlsl"
#include "CommonUtilities.hlsl"
#include "GerstnerWaves.hlsl"
#include "OceanLighting.hlsl"

///////////////////////////////////////////////////////////////////////////////
//                  				Structs		                             //
///////////////////////////////////////////////////////////////////////////////
struct OceanVertexInput // vert struct
{
	float4	vertex 					: POSITION;		// vertex positions
	float2	texcoord 				: TEXCOORD0;	// local UVs
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct OceanVertexOutput // fragment struct
{
	// uv의 xy는 모델링의 uv, 월드좌표는 zw
	float4	uv 						: TEXCOORD0;
	float3	posWS					: TEXCOORD1;
	half3 	normal 					: NORMAL;	
	float3 	viewDir 				: TEXCOORD2;
	float3	preWaveSP 				: TEXCOORD3;
	// x: fogFactor, y: noise
	half2 	fogFactorNoise          : TEXCOORD4;
	// x : 표면까지 거리, y : 표면까지 거리, z 정규화된 파도 높이, w = 수평 이동
	float4	additionalData			: TEXCOORD5;
	half4	shadowCoord				: TEXCOORD6;
	half3	posVS					: TEXCOORD7;
	float4	clipPos					: SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

///////////////////////////////////////////////////////////////////////////////
//          	   	      Water shading functions                            //
///////////////////////////////////////////////////////////////////////////////
float3 ReconstructWorldPos(half2 screenPos, float depth)
{
	float4x4 mat = UNITY_MATRIX_I_VP;
	#if UNITY_REVERSED_Z
	mat._12_22_32_42 = -mat._12_22_32_42;              
	#else
	depth = depth * 2 - 1;
	#endif
	float4 raw = mul(mat, float4(screenPos * 2 - 1, depth, 1));
	float3 worldPos = raw.rgb / raw.a;
	return worldPos;
}

half3 Scattering(half depth)
{
	return SAMPLE_TEXTURE2D(_AbsorptionScatteringRamp, sampler_AbsorptionScatteringRamp, half2(depth, 0.375h)).rgb;
}

half3 Absorption(half depth)
{
	return SAMPLE_TEXTURE2D(_AbsorptionScatteringRamp, sampler_AbsorptionScatteringRamp, half2(depth, 0.0h)).rgb;
}

float2 AdjustedDepth(half2 uvs, half4 additionalData)
{
	float rawD = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_ScreenTextures_linear_clamp, uvs);
	float d = LinearEyeDepth(rawD, _ZBufferParams);
	
#if UNITY_REVERSED_Z
	float offset = 0;
#else
	float offset = 1;
#endif
	
 	return float2(d - additionalData.x, (rawD * -_ProjectionParams.x) + offset);
}

// 사용하지 않음
/*
float WaterTextureDepth(float3 posWS)
{
	return (1 - SAMPLE_TEXTURE2D_LOD(_WaterDepthMap, sampler_WaterDepthMap_linear_clamp, posWS.xz * 0.002 + 0.5, 1).r) * (_MaxDepth + _VeraslWater_DepthCamParams.x) - _VeraslWater_DepthCamParams.x;
}
*/

// 새로운 깊이 연산
float WaterTextureDepth2(float2 posWS, float3 postionWS)
{
	float3 viewPos = TransformWorldToView(postionWS);
	float offset = -viewPos.z;
	float rawD = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_WaterDepthMap_linear_clamp, posWS.xy, 1).r;
	float d = LinearEyeDepth(rawD, _ZBufferParams);
	float final = saturate(((d - offset) / 15));
	return max(final, 0.05);
}


float3 WaterDepth(float3 posWS, half4 additionalData, half2 screenUVs)// x = seafloor depth, y = water depth
{
	float3 outDepth = 0;
	outDepth.xz = AdjustedDepth(screenUVs, additionalData);
	float wd = 0;
	outDepth.y = wd + posWS.y;
	return outDepth;
}

half3 Refraction(half2 distortion, half depth, real depthMulti)
{
	half3 output = SAMPLE_TEXTURE2D_LOD(_CameraOpaqueTexture, sampler_CameraOpaqueTexture_linear_clamp, distortion, depth * 0.25).rgb;
	output *= Absorption((depth) * depthMulti);
	return output;
}

half2 DistortionUVs(half depth, float3 normalWS)
{
    half3 viewNormal = mul((float3x3)GetWorldToHClipMatrix(), -normalWS).xyz;

    return viewNormal.xz * saturate((depth) * 0.005);
}

half4 AdditionalData(float3 postionWS, WaveStruct wave)
{
    half4 data = half4(0.0, 0.0, 0.0, 0.0);
    float3 viewPos = TransformWorldToView(postionWS);
	data.x = -viewPos.z;
	// 파도의 높이 표현
    data.y = length(GetCameraPositionWS().xyz - postionWS);
	data.z = wave.position.y / _MaxWaveHeight * 0.5 + 0.5;
	// 파도의 길이 표현
	data.w = wave.position.x + wave.position.z;
	return data;
}

float2 CausticUVs(float2 rawUV, float2 offset)
{
	float2 uv = rawUV * 3;
	return uv + offset * 0.1;
}

OceanVertexOutput WaveVertexOperations(OceanVertexOutput input)
{
	float time = _Time.x * _WaveSpeed;

    input.normal = float3(0, 1, 0);
	input.fogFactorNoise.y = ((noise((input.posWS.xz * 0.5) + time) + noise((input.posWS.xz * 1) + time)) * 0.25 - 0.5) + 1;
	
    input.uv.zw = input.posWS.xz * 0.1h + time * 0.05h + (input.fogFactorNoise.y * 0.1);
    input.uv.xy = input.posWS.xz * 0.4h - time.xx * 0.1h + (input.fogFactorNoise.y * 0.2);

	half4 screenUV = ComputeScreenPos(TransformWorldToHClip(input.posWS));
	screenUV.xyz /= screenUV.w;
	
    half waterDepth = 5;
    input.posWS.y += pow(saturate((-waterDepth + 1.5) * 0.4), 2);

	float waterDepth2 = WaterTextureDepth2(screenUV.xy, input.posWS);

	_WaterDepthFactor = 1 - _WaterDepthFactor;
	// waterDepth2의 값이 1보다 크면 1로 고정
	waterDepth2 = saturate(waterDepth2 + _WaterDepthFactor);
	
	WaveStruct wave;
	SampleWaves(input.posWS, saturate((waterDepth * 0.1 + 0.05)), waterDepth2, wave);
	input.normal = wave.normal;
    input.posWS += wave.position;

#ifdef SHADER_API_PS4
	input.posWS.y -= 0.5;
#endif
	
	// 파도 연산 이후
	input.clipPos = TransformWorldToHClip(input.posWS);
	input.shadowCoord = ComputeScreenPos(input.clipPos);
    input.viewDir = SafeNormalize(_WorldSpaceCameraPos - input.posWS);

    // fog 효과
	input.fogFactorNoise.x = ComputeFogFactor(input.clipPos.z);
	input.preWaveSP = screenUV.xyz;

	// 파도 데이터 표현
	input.additionalData = AdditionalData(input.posWS, wave);

	// 거리에 따른 normal 연산 (거리에 따른 연산 필요 없음)
	// half distanceBlend = saturate(abs(length((_WorldSpaceCameraPos.xz - input.posWS.xz) * 0.005)) - 0.25);
	// input.normal = lerp(input.normal, half3(0, 1, 0), distanceBlend);
	
	return input;
}

// 버텍스 연산
OceanVertexOutput OceanVertex(OceanVertexInput v)
{
    OceanVertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.uv.xy = v.texcoord;
    o.posWS = TransformObjectToWorld(v.vertex.xyz);
	o.posVS = TransformWorldToView(o.posWS);
	o = WaveVertexOperations(o);
    return o;
}
half RM2Specular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
	float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
	float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));

	float NoH = saturate(dot(float3(normalWS), halfDir));
	half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
                
	float d = NoH * NoH * brdfData.roughness2MinusOne + 1.001f;

	half LoH2 = LoH * LoH;
	half specularTerm = brdfData.roughness2/ ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);


	#if REAL_IS_HALF
	specularTerm = specularTerm - HALF_MIN;
	specularTerm = clamp(specularTerm, 0.0, 1000.0);
	#endif

	return specularTerm ;
}

half3 RM2LitPBR(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, half atten)
{
	half NdotL = saturate(dot(normalWS, light.direction));
	half3 radiance = light.color * (atten * NdotL);
                
	half3 brdf = brdfData.diffuse ;             

	#ifndef _SPECULARHIGHLIGHTS_OFF
	brdf += brdfData.specular  * (RM2Specular(brdfData, normalWS, light.direction, viewDirectionWS));
	#endif

	return brdf;
}

// Fragment for water
half4 OceanFragment(OceanVertexOutput IN) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(IN);
	half3 screenUV = IN.shadowCoord.xyz / IN.shadowCoord.w;
	// 깊이감 연산
	float3 depth = WaterDepth(IN.posWS, IN.additionalData, screenUV.xy);
	half depthMulti = 1 / _MaxDepth;
	
	half2 detailBump1 = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_SurfaceMap, IN.uv.zw).xy * 2 - 1;
	half2 detailBump2 = SAMPLE_TEXTURE2D(_SurfaceMap, sampler_SurfaceMap, IN.uv.xy).xy * 2 - 1;
	half2 detailBump = (detailBump1 + detailBump2 * 0.5) * saturate(depth.x * 0.25 + 0.25);

	IN.normal += half3(detailBump.x, 0, detailBump.y) * _BumpScale;
	IN.normal = normalize(IN.normal);

    // Distortion
	half2 distortion = DistortionUVs(depth.x, IN.normal) * 2;
	distortion = screenUV.xy + distortion;
	float d = depth.x;
	depth.xz = AdjustedDepth(distortion, IN.additionalData);
	distortion = depth.x < 0 ? screenUV.xy : distortion;
	depth.x = depth.x < 0 ? d : depth.x;

    // 프레넬 구간
	half fresnelTerm = CalculateFresnelTerm(IN.normal, IN.viewDir.xyz) * 0.3;

	// 라이트 연산
	Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.posWS));
    half shadow = SoftShadows(screenUV, IN.posWS, IN.viewDir.xyz, depth.x);
    half3 GI = SampleSH(IN.normal);

    // SSS(Scattering, Subsurface Scattering) 바다의 산란되는 현상
    half3 directLighting = dot(mainLight.direction, half3(0, 1, 0)) * mainLight.color;
    directLighting += saturate(pow(dot(IN.viewDir, -mainLight.direction) * IN.additionalData.z, 3)) * 5 * mainLight.color;
    half3 sss = directLighting * shadow + GI;

	// 물거품
	half3 foamMap = SAMPLE_TEXTURE2D(_FoamMap, sampler_FoamMap,  IN.uv.zw * _FoamSize).rgb; //r=thick, g=medium, b=light
	half depthEdge = saturate(depth.x * 20);
	half waveFoam = saturate(IN.additionalData.z - 0.75 * 0.5); // wave tips
	half depthAdd = saturate(1 - depth.x * 4) * 0.5;
	half edgeFoam = saturate((1 - min(depth.x, depth.y) * 0.5 - 0.25) + depthAdd) * depthEdge;
	half foamBlendMask = max(waveFoam, edgeFoam);
	half3 foamBlend = SAMPLE_TEXTURE2D(_AbsorptionScatteringRamp, sampler_AbsorptionScatteringRamp, half2(foamBlendMask, 0.66)).rgb;
	half foamMask = saturate(length(foamMap * foamBlend) * 1.5 - 0.1);
	
	// 물거품 + 라이팅
	half3 foam = foamMask.xxx * (mainLight.shadowAttenuation * mainLight.color + GI);

    BRDFData brdfData;
    half alpha = 1;
    InitializeBRDFData(half3(0, 0, 0), 0, half3(1, 1, 1), 0.95, alpha, brdfData);
	half3 spec = RM2LitPBR(brdfData, mainLight, IN.normal, IN.viewDir, 1) * shadow * mainLight.color;
#ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, IN.posWS);
        spec += LightingPhysicallyBased(brdfData, light, IN.normal, IN.viewDir);
        sss += light.distanceAttenuation * light.color;
    }
#endif

    sss *= Scattering(depth.x * depthMulti);
	
	half3 reflection = SampleReflections(IN.normal, IN.viewDir.xyz, screenUV.xy, 0.0);
	half3 refraction = Refraction(distortion, depth.x, depthMulti);
	half3 comp = half3(0,0,0);
	#if _CAUSTICS_SHADER
	// 커스틱
	float time = _Time.y * _CausticsSpeed * 0.1;
	// 커스틱 Blend
	half causticsBlendMask = max(waveFoam, edgeFoam) * _CausticDistance;
	// 커스틱 사이즈 조절
	float2 modifiedUV = (float2)(IN.uv / _CausticsSize);
	half4 causticBlendA = lerp(SAMPLE_TEXTURE2D(_CausticsMap, sampler_CausticsMap, modifiedUV  + time), 0, 1-causticsBlendMask);
	half4 causticBlendB = lerp(SAMPLE_TEXTURE2D(_CausticsMap, sampler_CausticsMap, modifiedUV  - _CausticsSpeed * 0.01), 0, 1-causticsBlendMask);
	
	float CausticsDriver = (causticBlendA.z * causticBlendB.z) * 10 + causticBlendA.z + causticBlendB.z;
	half3 Caustics = CausticsDriver * half3(causticBlendA.w * 0.5, causticBlendB.w * 0.75, causticBlendB.x) * mainLight.color;
	comp = lerp(lerp(refraction, reflection, fresnelTerm) + sss + spec + Caustics, foam, foamMask);
	#else
	comp = lerp(lerp(refraction, reflection, fresnelTerm) + sss + spec, foam, foamMask);
	#endif
	
	// Fog
    float fogFactor = IN.fogFactorNoise.x;
    comp = MixFog(comp, fogFactor);

	// 테스트 수치
	// float depthXX = WaterTextureDepth2(screenUV.xy, IN.posWS);;
	
#if defined(_DEBUG_CAUSTICS)
	#if _CAUSTICS_SHADER
	return half4(Caustics, 1);
	#else
	return half4(0, 0, 0, 0);
	#endif
#elif defined(_DEBUG_FOAM)
    return half4(foamMask.xxx, 1);
#elif defined(_DEBUG_SSS)
    return half4(sss, 1);
#elif defined(_DEBUG_REFRACTION)
    return half4(refraction, 1);
#elif defined(_DEBUG_REFLECTION)
    return half4(reflection, 1);
#elif defined(_DEBUG_NORMAL)
    return half4(IN.normal.x * 0.5 + 0.5, 0, IN.normal.z * 0.5 + 0.5, 1);
#elif defined(_DEBUG_FRESNEL)
    return half4(fresnelTerm.xxx, 1);
#elif defined(_DEBUG_WATERDEPTH)
	// return half4(depthXX.x, depthXX.x, depthXX.x, 1);
    return half4(frac(depth), 1);
#else
    return half4(comp, 1);
#endif
}

#endif