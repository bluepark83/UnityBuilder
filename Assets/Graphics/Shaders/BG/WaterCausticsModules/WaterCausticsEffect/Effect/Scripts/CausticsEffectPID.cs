using UnityEngine;
/// <summary>
/// 커스틱 효과의 Property ID를 저장하는 클래스입니다.
/// </summary>
namespace WaterCausticsModules
{
    public class CausticsEffectPID
    {
        readonly internal static int _WCE_CausticsTex = Shader.PropertyToID("_WCE_CausticsTex");
        readonly internal static int _WCE_TexChannels = Shader.PropertyToID("_WCE_TexChannels");
        readonly internal static int _WCE_TexRotateSinCos = Shader.PropertyToID("_WCE_TexRotateSinCos");
        readonly internal static int _WCE_IntensityMainLit = Shader.PropertyToID("_WCE_IntensityMainLit");
        readonly internal static int _WCE_IntensityAddLit = Shader.PropertyToID("_WCE_IntensityAddLit");
        readonly internal static int _WCE_Density = Shader.PropertyToID("_WCE_Density");
        readonly internal static int _WCE_ColorShift = Shader.PropertyToID("_WCE_ColorShift");
        readonly internal static int _WCE_SurfaceY = Shader.PropertyToID("_WCE_SurfaceY");
        readonly internal static int _WCE_SurfFadeStart = Shader.PropertyToID("_WCE_SurfFadeStart");
        readonly internal static int _WCE_SurfFadeCoef = Shader.PropertyToID("_WCE_SurfFadeCoef");
        readonly internal static int _WCE_DepthFadeStart = Shader.PropertyToID("_WCE_DepthFadeStart");
        readonly internal static int _WCE_DepthFadeCoef = Shader.PropertyToID("_WCE_DepthFadeCoef");
        readonly internal static int _WCE_DistanceFadeStart = Shader.PropertyToID("_WCE_DistanceFadeStart");
        readonly internal static int _WCE_DistanceFadeCoef = Shader.PropertyToID("_WCE_DistanceFadeCoef");
        readonly internal static int _WCE_LitSaturation = Shader.PropertyToID("_WCE_LitSaturation");
        readonly internal static int _WCE_MultiplyByTex = Shader.PropertyToID("_WCE_MultiplyByTex");
        readonly internal static int _WCE_NormalAtten = Shader.PropertyToID("_WCE_NormalAtten");
        readonly internal static int _WCE_NormalAttenRate = Shader.PropertyToID("_WCE_NormalAttenRate");
        readonly internal static int _WCE_TransparentBack = Shader.PropertyToID("_WCE_TransparentBack");
        readonly internal static int _WCE_BacksideShadow = Shader.PropertyToID("_WCE_BacksideShadow");
        readonly internal static int _WCE_ShadowIntensity = Shader.PropertyToID("_WCE_ShadowIntensity");
        readonly internal static int _StencilComp = Shader.PropertyToID("_StencilComp");
        readonly internal static int _StencilPass = Shader.PropertyToID("_StencilPass");
        readonly internal static int _StencilFail = Shader.PropertyToID("_StencilFail");
        readonly internal static int _StencilZFail = Shader.PropertyToID("_StencilZFail");
        readonly internal static int _CullMode = Shader.PropertyToID("_CullMode");
        readonly internal static int _ZWrite = Shader.PropertyToID("_ZWrite");
        readonly internal static int _ZTest = Shader.PropertyToID("_ZTest");
        readonly internal static int _BlendSrcFactor = Shader.PropertyToID("_BlendSrcFactor");
        readonly internal static int _BlendDstFactor = Shader.PropertyToID("_BlendDstFactor");
    }
}
