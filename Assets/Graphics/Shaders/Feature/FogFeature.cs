//#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
/*
1. ScriptableRendererFeature를 상속
2. Create 메서드 구현
3. Renderer Data에서 Add Renderer Feature가 생성될 때 호출
4. 렌더링 Pass가 생성
5. AddRenderPasses 호출되며 렌더링 대기열에 Pass를 추가
6. 생성된 Pass는 ScriptableRenderPass를 상속받아야 하며 Pass가 실행될 단계인 RenderPassEvent를 생성하지만, 여기서는 Create 메서드에서 구현함
7. 사용할 렌더텍스쳐를 초기화
8. Pass 스크립트에서 Pass가 실행되면, Execute 함수가 호출되는데 여기서 렌더텍스쳐를 생성한 후에 현재 화면에 표시되는 내용을 미리 만들어놓은 셰이더를 수정하여
생성된 렌더텍스쳐로 출력합니다.
9. 
*/ 
[Obsolete("Obsolete")]
public class BlitTexturePass : ScriptableRenderPass
    {
        /*
        ScriptableRenderPass
        내부에서 화면 데이터를 가져와 효과를 입히기 위해서 필요, 사용자가 정의한 Render Pass를 렌더링 파이프라인에서 원하는 타이밍에 패스를 추가할 수 있음
        실제 렌더링 작업을 수행하며, 렌더링 처리를 구현, 렌더링 처리의 실행을 위한 타이밍을 정의
        pass의 생성과 ScriptableRenderer에 전달
        Render Feature에 의해 추가되며, 구체적으로 어떤 렌더링이 수행되는 정의함
        */
        private readonly ProfilingSampler _profilingSampler;
        //public static readonly string CopyEffectShaderName = "Hidden/CopyTexture";
        private readonly Material _effectMaterial;
        //private readonly Material _copyMaterial;
        private readonly ScriptableRenderPassInput _passInput;

        private RenderTargetHandle _temporaryColorTexture;
        
        /*
        Blit 렌더 텍스쳐를 복사, 인자값에 Material를 넣으면 효과를 입히며 복사됨
         */
        public BlitTexturePass(Material effectMaterial, bool useDepth, bool useNormals, bool useColor) {
        _effectMaterial = effectMaterial;
        // 프레임디버거, 프로프로파일러 이름
        var name = effectMaterial.name.Substring(effectMaterial.name.LastIndexOf('/') + 1);
        _profilingSampler = new ProfilingSampler($"Blit {name}");
        _passInput = (useColor ? ScriptableRenderPassInput.Color : ScriptableRenderPassInput.None) |
                     (useDepth ? ScriptableRenderPassInput.Depth : ScriptableRenderPassInput.None) |
                     (useNormals ? ScriptableRenderPassInput.Normal : ScriptableRenderPassInput.None);
        //_copyMaterial = CoreUtils.CreateEngineMaterial(CopyEffectShaderName);
    }
        
        //렌더링 처리전에 호출되고 렌더링 타겟을 변경
        //렌더링 설정
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
            ConfigureInput(_passInput);
            base.Configure(cmd, cameraTextureDescriptor);
        }
        
        //셰이더의 속성을 설정(카메라를 렌더링하기 전에 렌더러에 의해 불러짐)
        //렌더 타겟을 설정하기 위해 카메라를 렌더링하기 전에 렌더러에 의해 호출됩니다.
        //렌더 대상 및 해당 지우기 상태를 구성하고 임시 렌더 대상 텍스처를 만들어야 하는 경우 이 메서드를 재정의합니다.
        //렌더 패스가 이 메서드를 재정의하지 않는 경우 이 렌더 패스는 활성 카메라의 렌더 대상으로 렌더링됩니다.
        //CommandBuffer.SetRenderTarget을 호출하면 안 됩니다.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            //렌더링 대상을 렌더 텍스쳐로 설정함
            /// <summary>
            ///   <para>Creates a render target identifier.</para>
            /// </summary>
            /// <param name="type">Built-in temporary render texture type.</param>
            /// <param name="name">Temporary render texture name.</param>
            /// <param name="nameID">Temporary render texture name (as integer, see Shader.PropertyToID).</param>
            /// <param name="tex">RenderTexture or Texture object to use.</param>
            /// <param name="mipLevel">MipLevel of the RenderTexture to use.</param>
            /// <param name="cubemapFace">Cubemap face of the Cubemap RenderTexture to use.</param>
            /// <param name="depthSlice">Depth slice of the Array RenderTexture to use. The symbolic constant RenderTargetIdentifier.AllDepthSlices indicates that all slices should be bound for rendering. The default value is 0.</param>
            /// <param name="renderTargetIdentifier">An existing render target identifier.</param>
            /// <param name="cubeFace"></param>
            ConfigureTarget(new RenderTargetIdentifier(renderingData.cameraData.renderer.cameraColorTarget, 0,
                CubemapFace.Unknown, -1));
        }
        
        //렌더링 처리를 작성
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (_effectMaterial == null) return;
            
            _temporaryColorTexture = new RenderTargetHandle();
            //커맨드버퍼를 얻기
            //커맨드 버퍼는 렌더 타겟 설정, 드로우 메시 등 렌더링 커맨드의 리스트를 포함하며 카메라 렌더링 중 여러 지점에서 실행하도록 설정
            CommandBuffer cmd = CommandBufferPool.Get();
            
            //나중에 이 렌더링 pass를 확인할 때 알기 쉽게하기 위해 $"Blit {name}"으로 분리
            using (new ProfilingScope(cmd, _profilingSampler)) {
                var descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                SetSourceSize(cmd, descriptor);

    #if UNITY_2022_1_OR_NEWER
                //이 렌더러의 카메라 색상 대상을 반환
                var cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
    #else
                var cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTarget;
    #endif
                //렌더텍스쳐를 생성
                cmd.GetTemporaryRT(_temporaryColorTexture.id, descriptor);

                // Also seen as `renderingData.cameraData.xr.enabled` and `#if ENABLE_VR && ENABLE_XR_MODULE`.
                // if (renderingData.cameraData.xrRendering) {
                //     _effectMaterial.EnableKeyword("_USE_DRAW_PROCEDURAL"); // `UniversalRenderPipelineCore.cs`.
                //     cmd.SetRenderTarget(_temporaryColorTexture.Identifier());
                //     cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _effectMaterial, 0, 0);
                //     cmd.SetGlobalTexture("_EffectTexture", _temporaryColorTexture.Identifier());
                //     cmd.SetRenderTarget(new RenderTargetIdentifier(cameraTargetHandle, 0, CubemapFace.Unknown, -1));
                //     cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _copyMaterial, 0, 0);
                // } else {
                    //변경됨 "_USE_DRAW_PROCEDURAL" 프래그먼트 셰이더에는 필요하지 않기 때문에 사후 처리 관련 셰이더의 버텍스 셰이더에서만 사용됩니다.
                    //결과적으로 이제 셰이더 변형이 더 적게 생성
                    //_effectMaterial.DisableKeyword("_USE_DRAW_PROCEDURAL");
                    // Note: `FinalBlitPass` has `cmd.SetRenderTarget` at this point, but it's unclear what that does.
                    //실행을 위해 컨텍스트에 blit 명령을 추가합니다. 이렇게 하면 ScriptableRenderer의 활성 렌더링 대상이 대상으로 변경됩니다.
                    //현재 카메라 이미지를 렌더텍스쳐에 복사
                    cmd.Blit(cameraTargetHandle, _temporaryColorTexture.Identifier(), _effectMaterial, 0);
                    //복사된 렌더텍스쳐를 현재 카메라 렌더타킷에 복사
                    cmd.Blit(_temporaryColorTexture.Identifier(), cameraTargetHandle);
                //}
            }
            // 커맨드 버퍼를 실행(렌더 호출) 이후 메모리 해제
            // context의 ExecuteCommandBuffer 함수는 Command Buffer를 인자로 갖고 Command Buffer에 예약된 명령들을 Execute하는 함수
            context.ExecuteCommandBuffer(cmd);
            //함수가 실행됐다고 Command Buffer의 명령들이 Clear되지 않습니다. 따로 Clear를 통해 Buffer를 비워주어야 다음에 문제없이 사용
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        //`PostProcessUtils.cs'
        private static void SetSourceSize(CommandBuffer cmd, RenderTextureDescriptor desc) {
            float width = desc.width;
            float height = desc.height;
            if (desc.useDynamicScale) {
                width *= ScalableBufferManager.widthScaleFactor;
                height *= ScalableBufferManager.heightScaleFactor;
            }
        
            cmd.SetGlobalVector("_SourceSize", new Vector4(width, height, 1.0f / width, 1.0f / height));
            }
        }
/*
public static class AlwaysIncludedShaders {
    public static void Add(string shaderName) {
        var shader = Shader.Find(shaderName);
        if (shader == null) return;

        var graphicsSettingsObj =
            AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        if (graphicsSettingsObj == null) return;
        var serializedObject = new SerializedObject(graphicsSettingsObj);
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
        bool hasShader = false;
        for (int i = 0; i < arrayProp.arraySize; ++i) {
            var arrayElem = arrayProp.GetArrayElementAtIndex(i);
            if (shader == arrayElem.objectReferenceValue) {
                hasShader = true;
                break;
            }
        }

        if (!hasShader) {
            int arrayIndex = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(arrayIndex);
            var arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
            arrayElem.objectReferenceValue = shader;

            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
        }
    }
}
*/
[Obsolete("Obsolete")]
public class FogFeature : ScriptableRendererFeature
{   
    /*
     ScriptableRendererFeature
     스크립팅 가능한 파이프라인의 나머지 부분과 상호 작용하는 방법
     Scriptable Renderer Feature에서 사용자가 자유롭게 정의한 Render Pass를 렌더링 파이프라인에 추가
     */
    [Header("Create > RM2 > Fog Setting")]
    public FogSetting settings;
    
    [SerializeField, HideInInspector]
    //렌더링 파이프라인에 추가할 렌더링 경로
    private BlitTexturePass _blitTexturePass;
    //Pass 생성
    //private ScriptableRenderer _renderer;
    private Material _effectMaterial = null;
    //private RenderTargetHandle _fogTexture;
    
    private Texture2D _lutDepth;
        
    private static readonly string ShaderName = "Hidden/RM2_CustomFog";
    private static readonly int DistanceLut = Shader.PropertyToID("_DistanceLUT");
    private static readonly int Near = Shader.PropertyToID("_Near");
    private static readonly int Far = Shader.PropertyToID("_Far");
    private static readonly int UseDistanceFog = Shader.PropertyToID("_UseDistanceFog");
    private static readonly int UseOnSky = Shader.PropertyToID("_UseDistanceFogOnSky");
    private static readonly int DistanceFogIntensity = Shader.PropertyToID("_DistanceFogIntensity");
    //기능이 처음 로드될 때 호출, ScriptableRenderPass이를 사용하여 모든 인스턴스 를 만들고 구성
    public override void Create()
    {
// #if UNITY_EDITOR
//         if (_effectMaterial == null) {
//             //copyTexture
//             //AlwaysIncludedShaders.Add(BlitTexturePass.CopyEffectShaderName);
//             //"Hidden/RM2_CustomFog";
//             AlwaysIncludedShaders.Add(ShaderName);
//         }
// #endif
        if (settings == null)
        {
            return;
        }
        
        if (!CreateMaterials()) {
            return;
        }
            SetMaterialProperties();
            //렌더링 Pass 생성
            _blitTexturePass = new BlitTexturePass(_effectMaterial, useDepth: true, useNormals: false, useColor: false) {
            //렌더링 타이밍
            renderPassEvent = settings.renderEvent
        };
    }
    
    //Feature에서 Pass를 생성하고 생성된 Pass를 ScriptableRenderer에 전달
    //Feature -> Pass -> ScriptableRenderer
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        if (renderingData.cameraData.isPreviewCamera) return;
        if (!settings.applyInSceneView && renderingData.cameraData.cameraType == CameraType.SceneView) return;
#endif
        //메서드를 정의하고 렌더링하기 전에 Feature에서 매개 변수를 전달
        SetMaterialProperties();
        
        //Pass를 렌더링 파이프라인에 추가
        renderer.EnqueuePass(_blitTexturePass);
    }
    
    private bool CreateMaterials() {
        if (_effectMaterial == null) {
            var effectShader = Shader.Find(ShaderName);
            //var blitShader = Shader.Find(BlitTexturePass.CopyEffectShaderName);
            //if (effectShader == null || blitShader == null) return false;
            if (effectShader == null) return false;
            _effectMaterial = CoreUtils.CreateEngineMaterial(effectShader);
        }

        return _effectMaterial != null;
    }
    
    private void SetMaterialProperties()
    {
        if (_effectMaterial == null)
        {
            return;
        }
        UpdateDistanceLut();
        _effectMaterial.SetTexture(DistanceLut, _lutDepth);
        _effectMaterial.SetFloat(Near, settings.fogNear);
        _effectMaterial.SetFloat(Far, settings.fogFar);
        _effectMaterial.SetFloat(UseDistanceFog, settings.enableDistance ? 1f : 0f);
        _effectMaterial.SetFloat(UseOnSky, settings.enableOnSky ? 1f : 0f);
        _effectMaterial.SetFloat(DistanceFogIntensity, settings.fogIntensity);
    }
    
    private void UpdateDistanceLut()
    {
        if (settings.distanceGradient == null) return;

        if (_lutDepth != null)
        {
            DestroyImmediate(_lutDepth);
        }

        const int width = 256;
        const int height = 1;
        _lutDepth = new Texture2D(width, height, TextureFormat.RGBA32, /*mipChain=*/false)
        {
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear
        };
        
        for (float x = 0; x < width; x++)
        {
            Color color = settings.distanceGradient.Evaluate(x / (width - 1));
            for (float y = 0; y < height; y++)
            {
                _lutDepth.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), color);
            }
        }

        _lutDepth.Apply();
    }

        
}

//#endif
