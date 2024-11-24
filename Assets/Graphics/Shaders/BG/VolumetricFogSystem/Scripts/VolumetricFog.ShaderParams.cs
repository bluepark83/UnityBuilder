using UnityEngine;

namespace CustomVolumetricFog{

    public static class ShaderParams
    {
        public static int Density = Shader.PropertyToID("_Density");
        // 렌더링 세팅 값
        public static int RaymarchSettings = Shader.PropertyToID("_RayMarchSettings");
        // 노이즈 스케일
        public static int NoiseScale = Shader.PropertyToID("_NoiseScale");

        public static int NoiseHeight = Shader.PropertyToID("_NoiseHeight");
        // 노이즈 최종 곱
        public static int NoiseFinalMultiplier = Shader.PropertyToID("_NoiseFinalMultiplier");
        // 노이즈 강도
        public static int NoiseStrength = Shader.PropertyToID("_NoiseStrength");
        public static int NoiseRotationSpeed = Shader.PropertyToID("_NoiseRotationSpeed");
        
        public static int RotationInvMatrix = Shader.PropertyToID("_InvRotMatrix");
        public static int RotationMatrix = Shader.PropertyToID("_RotMatrix");
        
        // 밝기 조절
        public static int Brightness = Shader.PropertyToID("_Brightness");
        // Fog Alpha
        public static int AlphaMultiplier = Shader.PropertyToID("_AlphaMultiplier");
        // Fog 색상
        public static int Color = Shader.PropertyToID("_Color");
        // 노이즈 텍스쳐
        public static int MainTex = Shader.PropertyToID("_MainTex");
        // 바람의 방향
        public static int WindDirection = Shader.PropertyToID("_WindDirection");
        // Board 영역 계산하여, 전달
        public static int BoundsBorder = Shader.PropertyToID("_BoundsBorder");
        // 영역의 확장(부모까지 계산)
        public static int BoundsExtents = Shader.PropertyToID("_BoundsExtents");

        // 오브젝트의 위치
        public static int BoundsCenter = Shader.PropertyToID("_BoundsCenter");
        // 추가적인 데이터 (x : Verticla Offset, y : 바닥 위치, z : 전체 높이)
        public static int BoundsData = Shader.PropertyToID("_BoundsData");
        
        // Box 모양 
        public const string SKW_SHAPE_BOX = "VF2_SHAPE_BOX";
        // 원형 모양
        public const string SKW_SHAPE_SPHERE = "VF2_SHAPE_SPHERE";

        public static int InsideBorder = Shader.PropertyToID("_InsideBorder");
        public static int InsideStrength = Shader.PropertyToID("_InsideStrength");
    }

}

