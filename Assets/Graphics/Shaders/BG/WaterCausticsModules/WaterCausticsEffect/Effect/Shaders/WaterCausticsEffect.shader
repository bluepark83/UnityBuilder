Shader "Hidden/WaterCausticsModules/Effect"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Int) = 2
        // --- Texture
        [NoScaleOffset]_WCE_CausticsTex ("Caustics Texture", 2D) = "black" { }
        _WCE_TexChannels ("Channel", Vector) = (0, 1, 2, 0)
        _WCE_TexRotateSinCos ("Rotation Sin and Cos", Vector) = (0, 1, 0, 0)
        
        // --- Dimension
        _WCE_Density ("Density", Float) = 0.2
        _WCE_SurfaceY ("Water Surface Y", Float) = 2
        _WCE_SurfFadeCoef ("Surface Fade Coef", Float) = 2
        _WCE_SurfFadeStart ("Surface Fade Start", Float) = 0
        _WCE_DepthFadeStart ("Depth Fade Start", Float) = 0
        _WCE_DepthFadeCoef ("Depth Fade Coef", Float) = 0.01
        _WCE_DistanceFadeStart ("Distance Fade Start", Float) = 0
        _WCE_DistanceFadeCoef ("Distance Fade Coef", Float) = 0.01
        // --- Effect
        _WCE_IntensityMainLit ("Main Light Intensity", Range(0, 50)) = 1
        _WCE_IntensityAddLit ("Additional Lights Intensity", Range(0, 50)) = 1
        [ToggleOff(_RECEIVE_SHADOWS_OFF)] _RECEIVE_SHADOWS_OFF ("Receive Shadow", Float) = 1
        _WCE_ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 1
        _WCE_ColorShift ("ColorShift", Vector) = (0.004, -0.001, 0, 0)
        _WCE_LitSaturation ("Light Saturation", Range(0, 2)) = 0.2
        _WCE_MultiplyByTex ("Multiply Color", Range(0, 1)) = 1
        // --- Normal Atten
        _WCE_NormalAtten ("Normal Atten Intensity", Range(0, 1)) = 1
        _WCE_NormalAttenRate ("Normal Atten Rate", Range(1, 8)) = 2
        _WCE_TransparentBack ("Transparent Backside", Range(0, 1)) = 0
        _WCE_BacksideShadow ("Backside Shadow", Range(0, 1)) = 0
        // --- Depth Buffer
        _ZWrite ("ZWrite", Int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4

        // --- Stencil Buffer
        _StencilRef ("Ref [0-255]", Range(0, 255)) = 0
        _StencilReadMask ("Read Mask [0-255]", Range(0, 255)) = 255
        _StencilWriteMask ("Write Mask [0-255]", Range(0, 255)) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Comp", Int) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Pass", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("ZFail", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("ZFail", Int) = 0
        
        // --- Blend
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrcFactor ("SrcFactor", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDstFactor ("DstFactor", Int) = 1
    }

    SubShader
    {
        LOD 0
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" "DisableBatching" = "True" "IgnoreProjector" = "True" }

        Pass {
            Name "WCE_EffectPass"
            Tags { "LightMode" = "WCE_EffectPass" }

            Blend [_BlendSrcFactor] [_BlendDstFactor]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_CullMode]
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Comp [_StencilComp]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.5

            #define WCE_EFFECT_SHADER
            #define REQUIRE_OPAQUE_TEXTURE
            
            #pragma multi_compile_local_fragment _ WCE_DEBUG_NORMAL WCE_DEBUG_DEPTH WCE_DEBUG_FACING WCE_DEBUG_CAUSTICS WCE_DEBUG_AREA

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
            #if VERSION_GREATER_EQUAL(11, 0)
                #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #else
                #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
                #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #endif
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS

            // ---- ※ URP14
            #if UNITY_VERSION >= 202220 // Unity2022.2.0 URP14.0
                #if !defined(_RECEIVE_SHADOWS_OFF)
                    #define _ADDITIONAL_LIGHT_SHADOWS
                #endif
            #else
                #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #endif
            // ----
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #if UNITY_VERSION >= 202120 // Unity2021.2.0 URP12.0
                #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
                #pragma multi_compile_fragment _ _LIGHT_LAYERS
                #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #endif

            #pragma multi_compile_fog
            #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                #define _USE_FOG
            #endif

            #include "WaterCausticsEffectCommon.hlsl"
            #pragma multi_compile_local_fragment _ _RECEIVE_SHADOWS_OFF

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 posClip : SV_POSITION;
                float4 posScrn : TEXCOORD0;
                float3 posWld : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                #if defined(_USE_FOG)
                    float viewDepth : TEXCOORD3;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float _WCE_Density;
                int3 _WCE_TexChannels;
                float2 _WCE_TexRotateSinCos;
                float _WCE_SurfaceY;
                float _WCE_SurfFadeStart;
                float _WCE_SurfFadeCoef;
                float _WCE_DepthFadeStart;
                float _WCE_DepthFadeCoef;
                float _WCE_DistanceFadeStart;
                float _WCE_DistanceFadeCoef;
                half _WCE_IntensityMainLit;
                half _WCE_IntensityAddLit;
                float2 _WCE_ColorShift;
                half _WCE_LitSaturation;
                half _WCE_MultiplyByTex;
                half _WCE_NormalAtten;
                half _WCE_NormalAttenRate;
                half _WCE_TransparentBack;
                half _WCE_BacksideShadow;
                half _WCE_ShadowIntensity;
            CBUFFER_END
            // ------------------------------------------------------------------------ Fill Clipped Hole
            float3 WCE_camDirWS()
            {
                return -UNITY_MATRIX_V[2].xyz;
            }

            float3 WCE_viewDirWS(float3 posWS)
            {
                return (unity_OrthoParams.w == 0) ? normalize(posWS - _WorldSpaceCameraPos) : WCE_camDirWS();
            }
            float3 WCE_viewDirRawWS(float3 posWS)
            {
                return (unity_OrthoParams.w == 0) ? posWS - _WorldSpaceCameraPos : WCE_camDirWS();
            }
            
            // ------------------------------------------------------------------------ Vertex Shader
            v2f vert(appdata v)
            {
                v2f o = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float4 posClip = TransformObjectToHClip(v.vertex.xyz);
                float3 posWld = TransformObjectToWorld(v.vertex.xyz);
                float4 posScrn = ComputeScreenPos(posClip);
                o.posClip = posClip;
                o.posWld = posWld;
                o.posScrn = posScrn;
                o.normalWS = TransformObjectToWorldNormal(v.normal, true);
                #if defined(_USE_FOG)
                    o.viewDepth = -TransformWorldToView(posWld).z;
                #endif
                return o;
            }
            
            // ------------------------------------------------------------------------ Reconstruct World Pos and Normal
            float WCE_fixReversedZ(float Depth)
            {
                #if UNITY_REVERSED_Z
                    return 1 - Depth;
                #else
                    return Depth;
                #endif
            }

            half3 WCE_getNormal(v2f IN, float2 screenUV, float3 posWS, float rawDepth)
            {
                float3 normal = IN.normalWS;
                normal *= -sign(dot(normal, WCE_viewDirRawWS(posWS)));
                return normal;
            }

            // ------------------------------------------------------------------------ Fog (Oblique Projection Supported)
            #if defined(_USE_FOG)
                float WCE_computeFogFactorZ0ToFar(float z)
                {
                    #if defined(FOG_LINEAR)
                        return saturate(z * unity_FogParams.z + unity_FogParams.w);
                    #elif defined(FOG_EXP) || defined(FOG_EXP2)
                        return unity_FogParams.x * z;
                    #else
                        return 0;
                    #endif
                }

                float WCE_calcFog(float viewDepth)
                {
                    float nearToFarZ = max(viewDepth - _ProjectionParams.y, 0);
                    return ComputeFogIntensity(WCE_computeFogFactorZ0ToFar(nearToFarZ));
                }
            #endif
            
            // ------------------------------------------------------------------------ Fragment
            #define CLR_COL half4(0, 0, 0, 0)
            TEXTURE2D(_WCE_CausticsTex);
            SAMPLER(sampler_WCE_CausticsTex);
            
            half4 frag(v2f IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                // ---------- WorldPos
                float2 screenUV = IN.posScrn.xy / IN.posScrn.w;
                float3 posWS = IN.posWld;
                float rawDepth = 0.5;

                // ---------- Debug Info
                #if defined(WCE_DEBUG_NORMAL) || defined(WCE_DEBUG_DEPTH) || defined(WCE_DEBUG_FACING) || defined(WCE_DEBUG_CAUSTICS) || defined(WCE_DEBUG_AREA)
                    if (rawDepth == UNITY_RAW_FAR_CLIP_VALUE) discard;
                #endif
                #if defined(WCE_DEBUG_NORMAL)
                    return half4(pow(saturate(WCE_getNormal(IN, screenUV, posWS, rawDepth) * 0.5 + 0.5), 4) * 0.9, 1);
                #elif defined(WCE_DEBUG_DEPTH)
                    float debugDepth = -TransformWorldToView(posWS).z * _ProjectionParams.w;
                    return half4(pow(abs(debugDepth), 0.7).xxx, 1);
                #elif defined(WCE_DEBUG_FACING)
                    float3 n = WCE_getNormal(IN, screenUV, posWS, rawDepth);
                    float b = dot(-n, WCE_viewDirWS(posWS));
                    return half4((saturate(pow(abs(b), 2) * 0.5)).xxx, 1);
                #elif defined(WCE_DEBUG_CAUSTICS)
                    _WCE_MultiplyByTex = 0;
                #elif defined(WCE_DEBUG_AREA)
                    _WCE_MultiplyByTex = 0;
                #endif

                // ---------- Clip Outside
                [branch] if (rawDepth == UNITY_RAW_FAR_CLIP_VALUE) return CLR_COL;

                // ---------- Atten Start
                float atten = 1;
                const float ATTEN_TH = 0.001;

                // ---------- Fog
                #if defined(_USE_FOG)
                float viewDepth = IN.viewDepth;
                atten *= WCE_calcFog(viewDepth);
                #endif

                // ---------- Distance Fade
                float3 viewDir = posWS - _WorldSpaceCameraPos;
                [branch] if (atten > ATTEN_TH && _WCE_DistanceFadeCoef > 0.0001f) {
                    atten *= smoothstep(0, 1, 1 - (length(viewDir) - _WCE_DistanceFadeStart) * _WCE_DistanceFadeCoef);
                }

                // ---------- Atten End
                [branch] if (atten <= ATTEN_TH) return CLR_COL;
                _WCE_IntensityMainLit *= atten;
                _WCE_IntensityAddLit *= atten;

                // ---------- Normal
                half3 normalWS = WCE_getNormal(IN, screenUV, posWS, rawDepth);

                // ---------- Caustics
                half3 c = WCE_EffectCore(posWS, normalWS, screenUV, _WCE_CausticsTex, sampler_WCE_CausticsTex, _WCE_TexRotateSinCos, _WCE_TexChannels, _WCE_Density, _WCE_SurfaceY, _WCE_SurfFadeStart, _WCE_SurfFadeCoef, _WCE_DepthFadeStart, _WCE_DepthFadeCoef, _WCE_IntensityMainLit, _WCE_IntensityAddLit, _WCE_ShadowIntensity, _WCE_ColorShift, _WCE_LitSaturation, _WCE_NormalAtten, _WCE_NormalAttenRate, _WCE_TransparentBack, _WCE_BacksideShadow);
                
                // ---------- Multiply Opaque Tex
                [branch] if (_WCE_MultiplyByTex > 0)
                {
                    c *= 1 - (1 - SHADERGRAPH_SAMPLE_SCENE_COLOR(screenUV)) * _WCE_MultiplyByTex;
                }
                return half4(c, 1);
            }

            ENDHLSL
        }
    }

    Fallback "Hidden/InternalErrorShader"
}
