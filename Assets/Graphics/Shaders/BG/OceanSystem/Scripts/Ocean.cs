using System;
using OceanSystem.Data;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace OceanSystem
{
    [ExecuteAlways]
    public class Ocean : MonoBehaviour
    {
        private PlanarReflections _planarReflections;

        public Texture bakedDepthTex;
        private Camera _depthCam;
        private Texture2D _rampTexture;
        
        [SerializeField]
        public OceanSurfaceData surfaceData;
        [SerializeField]
        private OceanResources resources;

        [SerializeField]
        public Wave[] _waves;
        
        private static readonly int WaterDepthMap = Shader.PropertyToID("_WaterDepthMap");
        private static readonly int MaxDepth = Shader.PropertyToID("_MaxDepth");
        private static readonly int AbsorptionScatteringRamp = Shader.PropertyToID("_AbsorptionScatteringRamp");
        private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");
        private static readonly int WaveCount = Shader.PropertyToID("_WaveCount");
        private static readonly int WaveSpeed = Shader.PropertyToID("_WaveSpeed");
        private static readonly int AvgSwellHeight = Shader.PropertyToID("_AvgSwellHeight");
        private static readonly int AvgWavelength = Shader.PropertyToID("_AvgWavelength");
        private static readonly int WindDirection = Shader.PropertyToID("_WindDirection");
        // 물거품 크기
        private static readonly int FoamSize = Shader.PropertyToID("_FoamSize");
        
        // Water Depth Factor 조절
        private static readonly int WaterDepthFactor = Shader.PropertyToID("_WaterDepthFactor");

        private void OnEnable()
        {
            Init();

            if(resources == null)
            {
                resources = Resources.Load("OceanResources") as OceanResources;
            }
        }

        public void Init()
        {
            // 어떤 이유에서든 SurfaceData가 빠져 있는 경우 가장 기본 적인 데이터 Resource/ShaderDatas에서 로드
            if (surfaceData == null)
            {
                surfaceData = Resources.Load("ShaderDatas/OceanSurfaceData") as OceanSurfaceData;
            }
            
            SetWaves();
            GenerateColorRamp();
            if (bakedDepthTex)
            {
                Shader.SetGlobalTexture(WaterDepthMap, bakedDepthTex);
            }
            
            if(resources == null)
            {
                resources = Resources.Load("OceanResources") as OceanResources;
            }
        }

        private void SetWaves()
        {
            SetupWaves();
            
            Shader.SetGlobalFloat(BumpScale, surfaceData._BumpScale);
            Shader.SetGlobalFloat(MaxDepth, surfaceData._waterMaxVisibility);
            
            // 파도
            Shader.SetGlobalInt(WaveCount, surfaceData._WaveCount);
            Shader.SetGlobalFloat(WaveSpeed, surfaceData._WaveSpeed);
            Shader.SetGlobalFloat(AvgSwellHeight, surfaceData._AvgSwellHeight);
            Shader.SetGlobalFloat(AvgWavelength, surfaceData._AvgWavelength);
            Shader.SetGlobalFloat(WindDirection, surfaceData._WindDirection);
            
            Shader.SetGlobalVectorArray("waveData", GetWaveData());
            // 물거품 크기 조절
            Shader.SetGlobalFloat(FoamSize, surfaceData._FoamSize);
            // Water Depth Factor
            Shader.SetGlobalFloat(WaterDepthFactor,  math.saturate(surfaceData._WaterDepthFactor - 0.01f));
            
            switch(surfaceData.refType)
            {
                case ReflectionType.ReflectionProbe:
                    Shader.EnableKeyword("_REFLECTION_PROBES");
                    Shader.DisableKeyword("_REFLECTION_PLANARREFLECTION");
                    break;
                case ReflectionType.PlanarReflection:
                    Shader.DisableKeyword("_REFLECTION_PROBES");
                    Shader.EnableKeyword("_REFLECTION_PLANARREFLECTION");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private Vector4[] GetWaveData()
        {
            var waveData = new Vector4[20];
            for (var i = 0; i < _waves.Length; i++)
            {
                waveData[i] = new Vector4(_waves[i].amplitude, _waves[i].direction, _waves[i].wavelength, _waves[i].onmiDir);
                waveData[i+10] = new Vector4(_waves[i].origin.x, _waves[i].origin.y, _waves[i].steepness, 0);
            }
            return waveData;
        }

        private void SetupWaves()
        {
            //create basic waves based off basic wave settings
            var backupSeed = Random.state;
            Random.InitState(surfaceData.randomSeed);
            var a = surfaceData._AvgSwellHeight;
            var d = surfaceData._WindDirection;
            var l = surfaceData._AvgWavelength;
            var s = surfaceData._Steepness;
            var numWave = surfaceData._WaveCount;
            _waves = new Wave[numWave];

            var r = 1f / numWave;

            for (var i = 0; i < numWave; i++)
            {
                var p = Mathf.Lerp(0.5f, 1.5f, i * r);
                var amp = a * p * Random.Range(-0.8f, 1.2f);
                var dir = d + Random.Range(-90f, 90f);
                var len = l * p * Random.Range(0.6f, 1.4f);
                var stp = s * Random.Range(0.5f, 1.6f);
                _waves[i] = new Wave(amp, dir, len, stp,Vector2.zero, false);
                Random.InitState(surfaceData.randomSeed + i + 1);
            }
            Random.state = backupSeed;
        }
        
        private void GenerateColorRamp()
        {
            if(_rampTexture == null)
                _rampTexture = new Texture2D(128, 4, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
            _rampTexture.wrapMode = TextureWrapMode.Clamp;

            var defaultFoamRamp = resources.defaultFoamRamp;
            var cols = new Color[512];
            for (var i = 0; i < 128; i++)
            {
                cols[i] = surfaceData._absorptionRamp.Evaluate(i / 128f);
            }
            for (var i = 0; i < 128; i++)
            {
                cols[i + 128] = surfaceData._scatterRamp.Evaluate(i / 128f);
            }
            for (var i = 0; i < 128; i++)
            {
                cols[i + 256] = defaultFoamRamp.GetPixelBilinear(surfaceData._foamSettings.basicFoam.Evaluate(i / 128f) , 0.5f);
            }
            _rampTexture.SetPixels(cols);
            _rampTexture.Apply();
            Shader.SetGlobalTexture(AbsorptionScatteringRamp, _rampTexture);
        }
    }
}
