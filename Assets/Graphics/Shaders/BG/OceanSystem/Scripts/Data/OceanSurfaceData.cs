using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace OceanSystem.Data
{
    [System.Serializable][CreateAssetMenu(fileName = "OceanSurfaceData", menuName = "OceanSystem/Surface Data", order = 0)]
    public class OceanSurfaceData : ScriptableObject
    {
        public float _waterMaxVisibility = 40.0f;
        public float _BumpScale = 0.2f;
        public Gradient _absorptionRamp;
        public Gradient _scatterRamp;
        
        public int _WaveCount = 3;
        public float _WaveSpeed = 2.0f;
        public float _AvgSwellHeight = 0.4f;
        public int _AvgWavelength = 20;
        public int _WindDirection = -176;
        public float _Steepness = 0.5f;
        // 물의 깊이 조절 factor
        public float _WaterDepthFactor = 0.1f;
        
        // 파도 랜덤 시드
        public int randomSeed = 3234;
        
        // 물거품 크기
        public float _FoamSize = 1f;
        
        public FoamSettings _foamSettings = new FoamSettings();
        public ReflectionType refType = ReflectionType.ReflectionProbe;
        public MeshType meshType = MeshType.StaticMesh;
        [SerializeField]
        public bool _init = false;
    }
    
    [System.Serializable]
    public struct Wave
    {
        public float amplitude; // 파도의 높이
        public float direction; // 파도의 방향
        public float wavelength; // 파도의 길이
        public float steepness; // 파도의 기울기
        public float2 origin;
        public float onmiDir;

        public Wave(float amp, float dir, float length, float stp, float2 org, bool omni)
        {
            amplitude = amp;
            direction = dir;
            wavelength = length;
            steepness = stp;
            origin = org;
            onmiDir = omni ? 1 : 0;
        }
    }

    [System.Serializable]
    public class FoamSettings
    {
        public AnimationCurve basicFoam;
        
        public FoamSettings()
        {
            basicFoam = new AnimationCurve(new Keyframe[2]{new Keyframe(0.25f, 0f),
                                                                    new Keyframe(1f, 1f)});
        }
    }
    
    [System.Serializable]
    public enum ReflectionType
    {
        ReflectionProbe,
        PlanarReflection
    }

    [System.Serializable]
    public enum MeshType
    {
        DynamicMesh,
        StaticMesh
    }
}