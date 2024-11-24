/*
 *
 * Shadows
 *
 */
void Shadowmask_half (float2 lightmapUV, out half4 Shadowmask){
	#ifdef SHADERGRAPH_PREVIEW
		Shadowmask = half4(1,1,1,1);
	#else
		OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
		Shadowmask = SAMPLE_SHADOWMASK(lightmapUV);
	#endif
}

#ifndef SHADERGRAPH_PREVIEW
	#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
	#if (SHADERPASS != SHADERPASS_FORWARD)
		#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
	#endif
#endif

void MainLightShadows_float (float3 PositionWS, half4 Shadowmask, out float ShadowAtten){
	#ifdef SHADERGRAPH_PREVIEW
		ShadowAtten = 1;
	#else
		float4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
		ShadowAtten = MainLightShadow(shadowCoord, PositionWS, Shadowmask, _MainLightOcclusionProbes);

	#endif
}

void MainLightShadows_float (float3 WorldPos, out float ShadowAtten){
	MainLightShadows_float(WorldPos, half4(1,1,1,1), ShadowAtten);
}

void GetGlobalShadow_half(bool UsingGlobal, float4 PerMaterial_Shadow, out float4 Out)
{
	if (UsingGlobal == false){
		Out = PerMaterial_Shadow;
	}
	else
	{
		#ifdef SHADERGRAPH_PREVIEW
		Out = float4(0,0,0,0);
		#else
		Out = _SubtractiveShadowColor.rgba;
		#endif
	}
	

}

// Shadows End



/*
 *
 * Main Lights
 *
 */
void GetMainLight_half(out half3 Direction, out half4 Color)
{
	Direction = half3(0, 0.5, 0); // 라이트 방향
	Color = half4(1,1,1,1); // 라이트 색상
	#ifndef SHADERGRAPH_PREVIEW
	uint layerMask = _MainLightLayerMask;
	uint meshRenderingLayers = GetMeshRenderingLayer();

	if(IsMatchingLightLayer(layerMask,meshRenderingLayers))
	{
		Direction = _MainLightPosition.xyz;
		Color = _MainLightColor;	
	}
	#endif
}
void GetLightCookie_half(float3 PositionWS, out float3 Out)
{
	#ifdef SHADERGRAPH_PREVIEW
	Out = float3(1,1,1);
	#else
		#ifdef _LIGHT_COOKIES
		Out = SampleMainLightCookie(PositionWS);
		#else
		Out = float3(1,1,1);
		#endif
	#endif
}
// Main Lights End

/*
 *
 * Additional Lights
 *
 */

// CommonShaderUtils는 단 한 번만 호출해야되기 때문에 추후 옮겨질 수 있음
// #include "Assets/Graphics/Shaders/Common/HLSL/CommonShaderUtils.hlsl"

half LinearStep2(half In, half Min, half Max)
{
	half subtractInMin = In - Min;
	half subtractMaxMin = Max - Min;
	return subtractInMin/subtractMaxMin;
}

half Smoother2(half In, half Threshold, half Smoother){
	half Out = 0;
	half addFactor = Threshold + Smoother + Smoother;
	half subtractFactor = Threshold - Smoother;

	half values = LinearStep2(In, addFactor, subtractFactor);
	return Out = 1 - values;
}


void GetAdditionalLight_half(float3 PositionWS, float3 NormalWS, float4 ScreenSpaceUV ,out half3 LightsColor){
	half3 lightsColor = 0;
	#ifdef _ADDITIONAL_LIGHTS
	#if SHADERGRAPH_PREVIEW
	#else
	uint meshRenderingLayers = GetMeshRenderingLayer();
	half4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
	int lightCount = GetAdditionalLightsCount();
	
	#if USE_FORWARD_PLUS
	InputData inputData;
	inputData.positionWS = PositionWS;
	inputData.normalizedScreenSpaceUV = ScreenSpaceUV.xy;

	for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
	{
		FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
		Light light = GetAdditionalLight(lightIndex, PositionWS, shadowCoord);
		if(IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		{
			half attenuatedShadow = saturate(light.distanceAttenuation * light.shadowAttenuation);
			half lambert = dot(NormalWS, light.direction) * 0.5 + 0.5;
			half smooth = saturate(Smoother2(lambert, 0.8,0.3));
			half selfShadow = smooth * attenuatedShadow;
			lightsColor +=  selfShadow * light.color;	
		}
	}

	LIGHT_LOOP_BEGIN(lightCount)
	Light light = GetAdditionalLight(lightIndex, PositionWS, shadowCoord);
	if(IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	{
		half attenuatedShadow = saturate(light.distanceAttenuation * light.shadowAttenuation);
		half lambert = dot(NormalWS, light.direction) * 0.5 + 0.5;
		half smooth = saturate(Smoother2(lambert, 0.8,0.3));
		half selfShadow = smooth * attenuatedShadow;
		lightsColor +=  selfShadow * light.color;
	}
	LIGHT_LOOP_END
	
	#else
	for (int i = 0; i < lightCount; ++i)
	{
		Light light = GetAdditionalLight(i, PositionWS, shadowCoord);
		if(IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		{
			half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
			half lambert = dot(NormalWS, light.direction) *0.5 + 0.5;
			// half smooth = saturate(Smoother2(lambert, 0.8,0.3));
		
			lightsColor += lambert * attenuatedLightColor;
		}
	}
	
	#endif
	
	#endif
	#endif
	LightsColor = lightsColor;
}
// AdditionalLight End
