Shader "RM2/Ocean"
{
    Properties
    {
        //_BumpScale("Detail Wave Amount", Range(0, 2)) = 0.2
        _DitherPattern ("Dithering Pattern", 2D) = "bump" {}
        _SurfaceMap ("SurfaceMap", 2D) = "white" {}
        _FoamMap ("FoamMap", 2D) = "white" {}

        // 파도
        //_WaveCount ("WaveCount", Range(2,6)) = 4
        //_WaveSpeed("WaveSpeed", Range(0.1, 3.0)) = 0.5
        //_AvgSwellHeight("AvgSwehllHeight", Range(0.1, 3.0)) = 0.4
        //_AvgWavelength("AvgWavelength", Range(1, 120)) = 8
        //_WindDirection("WindDirection", Range(-180, 180)) = -176

        _Frequency ("Frequency", float) = 2

        [HideInInspector] _MaxWaveHeight ("MaxWaveHeight", Range(0.1, 15.0))= 15
        [KeywordEnum(Off, SSS, Refraction, Reflection, Normal, Fresnel, Foam, WaterDepth)] _Debug ("Debug mode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-100" "RenderPipeline" = "UniversalPipeline" }
        ZWrite On

        Pass
        {
            Name "WaterShading"
            Tags{"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            /////////////////SHADER FEATURES//////////////////
            #pragma shader_feature _REFLECTION_PROBES _REFLECTION_PLANARREFLECTION
            #pragma shader_feature _DEBUG_OFF _DEBUG_SSS _DEBUG_REFRACTION _DEBUG_REFLECTION _DEBUG_NORMAL _DEBUG_FRESNEL _DEBUG_FOAM _DEBUG_WATERDEPTH

            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "OceanCommon.hlsl"

            #pragma vertex OceanVertex
            #pragma fragment OceanFragment

            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
