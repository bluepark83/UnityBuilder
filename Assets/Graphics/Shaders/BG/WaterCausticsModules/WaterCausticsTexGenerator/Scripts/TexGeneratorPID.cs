using UnityEngine;

namespace WaterCausticsModules
{
    public class TexGeneratorPID
    {
        readonly internal static int _WaveCnt = Shader.PropertyToID ("_WaveCnt");
        readonly internal static int _WaveData = Shader.PropertyToID ("_WaveData");
        readonly internal static int _WaveUVShift = Shader.PropertyToID ("_WaveUVShift");
        readonly internal static int _WaveNoiseDir = Shader.PropertyToID ("_WaveNoiseDir");
        readonly internal static int _CalcResUI = Shader.PropertyToID ("_CalcResUI");
        readonly internal static int _CalcTexel = Shader.PropertyToID ("_CalcTexel");
        readonly internal static int _CalcTexelInv = Shader.PropertyToID ("_CalcTexelInv");
        readonly internal static int _LightDir = Shader.PropertyToID ("_LightDir");
        readonly internal static int _Eta = Shader.PropertyToID ("_Eta");
        readonly internal static int _Brightness = Shader.PropertyToID ("_Brightness");
        readonly internal static int _Gamma = Shader.PropertyToID ("_Gamma");
        readonly internal static int _Clamp = Shader.PropertyToID ("_Clamp");
        readonly internal static int _IdxStride = Shader.PropertyToID ("_IdxStride");
        readonly internal static int _DrawOffset = Shader.PropertyToID ("_DrawOffset");
        readonly internal static int _BufNoiseRW = Shader.PropertyToID ("_BufNoiseRW");
        readonly internal static int _BufNoise = Shader.PropertyToID ("_BufNoise");
        readonly internal static int _BufRefractRW = Shader.PropertyToID ("_BufRefractRW");
        readonly internal static int _BufRefract = Shader.PropertyToID ("_BufRefract");
        readonly internal static int _LightDirection = Shader.PropertyToID ("_LightDirection");
    }
}