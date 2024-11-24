Shader "Hidden/WaterCausticsModules/TexGenShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" { }
    }

    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        // -------------------------------------------- Pass0 DrawCaustics
        Pass {
            AlphaToMask Off Cull Off ZWrite Off ZTest Always
            Blend One One
            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 color : TEXCOORD0;
            };

            struct CausticsBufStruct {
                float2 offset;
                float3 color;
            };

            StructuredBuffer<CausticsBufStruct> _BufRefract;

            v2f vert(appdata v) {
                CausticsBufStruct buf = _BufRefract[(uint)v.vertex.z];
                v2f o;
                float2 pos = v.vertex.xy + buf.offset * 2;
                o.pos = float4(pos.xy, 0, 1);
                #if UNITY_UV_STARTS_AT_TOP
                    o.pos.y *= -1;
                #endif
                o.color = buf.color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return max(0, i.color.r).rrrr;
            }

            ENDCG
        }

        // -------------------------------------------- Pass1 Post Process Effect
        Pass {
            Cull Off ZTest Always ZWrite Off Blend Off
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_local _ _SAMPLE4

            struct vIn {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                #if defined(_SAMPLE4)
                    half2 uvG[4] : TEXCOORD0;
                #else // _SAMPLE1
                    half2 uvG[1] : TEXCOORD0;
                #endif
            };

            half4 _MainTex_TexelSize;

            v2f vert(vIn v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                #if defined(_SAMPLE4)
                    half4 ofs = _MainTex_TexelSize.xyxy;
                    half4 uv0 = v.uv.xyxy + ofs.xyzw;
                    half4 uv1 = v.uv.xyxy - ofs.xyzw;
                    o.uvG[0] = uv0.xy;
                    o.uvG[1] = uv0.zw;
                    o.uvG[2] = uv1.xy;
                    o.uvG[3] = uv1.zw;

                #else // _SAMPLE1
                    o.uvG[0] = v.uv.xy;
                #endif
                return o;
            }

            Texture2D_half _MainTex;
            SamplerState sampler_MainTex;
            half _Gamma;

            #if defined(_SAMPLE4)
                half sample(const half2 uv[4], const int channel) {
                    half c0 = _MainTex.Sample(sampler_MainTex, uv[0])[channel];
                    half c1 = _MainTex.Sample(sampler_MainTex, uv[1])[channel];
                    half c2 = _MainTex.Sample(sampler_MainTex, uv[2])[channel];
                    half c3 = _MainTex.Sample(sampler_MainTex, uv[3])[channel];
                    return (c0 + c1 + c2 + c3) * 0.25;
                }
            #else // _SAMPLE1
                half sample(const half2 uv[1], const int channel) {
                    return _MainTex.Sample(sampler_MainTex, uv[0])[channel];
                }
            #endif

            half4 frag(v2f i) : COLOR {
                half4 c;
                #if defined(_USE_RGB)
                    c.r = sample(i.uvR, 0);
                    c.g = sample(i.uvG, 1);
                    c.b = sample(i.uvB, 2);
                    c.a = c.g;
                #else
                    half s = sample(i.uvG, 0);
                    c.rgba = s.rrrr;
                #endif
                return c;
            }

            ENDCG
        }
    }
}

