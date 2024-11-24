#if WCE_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using WaterCausticsModules.Effect;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace WaterCausticsModules
{

    namespace Effect
    {
        // ----------------------------------------------------------- Enum
        public enum TexChannel { RGB, R, G, B, A }
        public enum DebugMode { Normal = 1, Depth, Facing, Caustics, LightArea }
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu ("WaterCausticsModules/WaterCausticsEffect")]
    public class WaterCausticsEffect : MonoBehaviour
    {
        // ----------------------------------------------------------- Constant
        private readonly Vector3 [] _texChannelVec = { new Vector3 (0, 1, 2), Vector3.zero, Vector3.one, Vector3.one * 2, Vector3.one * 3 };

        static internal readonly RenderPassEvent SYS_OPAQUE_TEX_EVENT = RenderPassEvent.AfterRenderingSkybox; // URP OpaqueTex
        static internal readonly RenderPassEvent RENDER_EVENT = SYS_OPAQUE_TEX_EVENT; // EachMesh
        static internal readonly int RENDER_EVENT_ADJ = -1; // EachMesh

        #region SerializeField
        [SerializeField] private bool m_debugInfo = false;
        [SerializeField] private DebugMode m_debugMode = DebugMode.Normal;
        [SerializeField] private LayerMask m_layerMask = ~0;

        [SerializeField] private Texture m_texture;
        [SerializeField] private TexChannel m_textureChannel;
        [SerializeField, Range (-180f, 180f)] private float m_textureRotation = 15f;
        [SerializeField] private Vector2 m_texRotSinCos = new Vector2 (0, 1);

        [SerializeField, Min (0.0001f)] private float m_scale = 1f;
        [FormerlySerializedAs ("m_waterSurfaceY")][SerializeField] private float m_surfaceY = 2f;
        [FormerlySerializedAs ("m_waterSurfaceAttenOffset")][SerializeField, Min (0f)] private float m_surfFadeStart;
        [FormerlySerializedAs ("m_waterSurfaceAttenWide")][SerializeField, Min (0f)] private float m_surfFadeEnd = 0.5f;
        [FormerlySerializedAs ("m_useDepthAtten")][SerializeField] private bool m_useDepthFade = false;
        [SerializeField, Min (0f)] private float m_depthFadeStart = 0f;
        [FormerlySerializedAs ("m_depthAttenDepth")][SerializeField, Min (0f)] private float m_depthFadeEnd = 50f;
        [SerializeField] private bool m_useDistanceFade = false;
        [SerializeField, Min (0f)] private float m_distanceFadeStart = 30f;
        [SerializeField, Min (0f)] private float m_distanceFadeEnd = 100f;
        [SerializeField, Min (0f)] private float m_intensity = 5f;
        [FormerlySerializedAs ("m_adjustMainLit")][SerializeField, Range (0f, 10f)] private float m_mainLit = 1f;
        [FormerlySerializedAs ("m_adjustAddLit")][SerializeField, Range (0f, 10f)] private float m_addLit = 1f;
        [SerializeField, Range (0f, 5f)] float m_colorShift = 0.6f;
        [SerializeField, Range (-180f, 180f)] float m_colorShiftDir = 120f;
        [SerializeField, Range (0f, 2f)] private float m_litSaturation = 0.2f;
        
        [FormerlySerializedAs ("m_multiplyOpaqueIntensity")][SerializeField, Range (0f, 1f)] private float m_multiply = 1f;
        [FormerlySerializedAs ("m_normalAttenIntensity")][SerializeField, Range (0f, 1f)] private float m_normalAtten = 1f;
        [FormerlySerializedAs ("m_normalAttenPower")][SerializeField, Range (1f, 8f)] private float m_normalAttenRate = 1.5f;
        [SerializeField, Range (0f, 1f)] private float m_transparentBackside = 0f;
        [SerializeField, Range (0f, 1f)] private float m_backsideShadow = 0f;
        [SerializeField] private bool m_receiveShadows = true;
        [SerializeField, Range (0f, 1f)] private float m_shadowIntensity = 1f;
        [SerializeField] private bool m_useMainLit = true;
        [FormerlySerializedAs ("m_useAdditionalLights")][SerializeField] private bool m_useAddLit = true;
        
        [SerializeField] private RenderPassEvent m_renderEvent = RENDER_EVENT;
        [SerializeField] private int m_renderEventAdjust = RENDER_EVENT_ADJ;

        [SerializeField] private CompareFunction m_stencilComp = CompareFunction.Always;
        [SerializeField] private StencilOp m_stencilPass = StencilOp.Keep;
        [SerializeField] private StencilOp m_stencilFail = StencilOp.Keep;
        [SerializeField] private StencilOp m_stencilZFail = StencilOp.Keep;
        [SerializeField] private CullMode m_cullMode = CullMode.Off;
        [SerializeField] private bool m_zWriteMode = false;
        [SerializeField] private CompareFunction m_zTestMode = CompareFunction.Equal;

        [SerializeField] private Shader m_shader;
        [SerializeField] private Texture m_noTexture;
        #endregion

        #region Property
        // ----------------------------------------------------------- private property
        private RenderPassEvent eventAdjusted => (RenderPassEvent) Mathf.Clamp ((int) m_renderEvent + m_renderEventAdjust, 0, 1000);
        private bool existOpaqueTex => (eventAdjusted > SYS_OPAQUE_TEX_EVENT);
        private bool useBlendMultiply => (m_multiply == 1f || (m_multiply > 0f && !existOpaqueTex));
        private float multiplyByTex => (m_multiply < 1f && existOpaqueTex) ? m_multiply : 0f;
        private float multiplyRaw => m_multiply;
        private Vector2 calcColorShiftVec () => 0.01f * m_colorShift * CausticsEffectTools.dirToVec (m_colorShiftDir);
        private float finalMainLit => m_intensity * m_mainLit * (m_useMainLit ? 1f : 0f);
        private float finalAddLit => m_intensity * m_addLit * (m_useAddLit ? 1f : 0f);
        private bool isIntensityZero => (finalMainLit + finalAddLit == 0f);
        


        // ----------------------------------------------------------- Public property
        private void SetValAndNeedSetMat<T> (ref T prop, T val)
        {
            prop = val;
            _needUpdateMat = true;
        }

        public Texture texture
        {
            get => m_texture;
            set => SetValAndNeedSetMat (ref m_texture, value);
        }

        public float textureRotation
        {
            get => m_textureRotation;
            set {
                float rad = value * Mathf.Deg2Rad;
                m_texRotSinCos = new Vector2 (Mathf.Sin (rad), Mathf.Cos (rad));
                SetValAndNeedSetMat (ref m_textureRotation, value);
            }
        }
        #endregion

        // ----------------------------------------------------------- Editor
#if UNITY_EDITOR
        internal void OnInspectorChanged ()
        {
            if (isActiveAndEnabled)
                UpdateMaterialValues ();
        }

        private void onUndoCallback ()
        {
            if (isActiveAndEnabled)
                UpdateMaterialValues ();
        }
#endif

        // ----------------------------------------------------------- Init
        private void Reset ()
        {
#if UNITY_EDITOR
            AddFeatureIfNeedManage ();
#endif
            m_scale = Mathf.Clamp (Mathf.Max (transform.lossyScale.x, transform.lossyScale.z), 0.3f, 3f);
        }

        // -----------------------------------------------------------
        private void OnEnable ()
        {
#if UNITY_EDITOR
            Undo.undoRedoPerformed += onUndoCallback;
#endif
            OnEnableForPass ();
            _needUpdateMat = true;
        }

        private void OnDisable ()
        {
#if UNITY_EDITOR
            Undo.undoRedoPerformed -= onUndoCallback;
#endif
            OnDisableForPass ();
        }

        private void OnDestroy ()
        {
            ObjectDestroy (ref __mat);
        }

        private void ObjectDestroy<T> (ref T o) where T : Object
        {
            if (o == null) return;
            ObjectDestroy (o);
            o = null;
        }

        private void ObjectDestroy (Object o)
        {
            if (o == null) return;
            if (Application.isPlaying)
                Destroy (o);
            else
                DestroyImmediate (o);
        }


        // ----------------------------------------------------------- Update
        private bool _needUpdateMat;

        private void LateUpdate ()
        {
            if (_needUpdateMat || !Application.isPlaying)
            {
                _needUpdateMat = false;
                UpdateMaterialValues ();
            }
        }

        // ----------------------------------------------------------- Material
        private Material __mat;
        private Material GetMat ()
        {
            return __mat ? __mat : __mat = CreateMat (m_shader, "WCausticsEffect");
        }

        private Material CreateMat (Shader shader, string name)
        {
            _needUpdateMat = true;
            if (shader == null)
            {
                Debug.LogError ("Shader is null. " + this);
                return null;
            }
            else
            {
                Material mat = new Material (shader)
                {
                    name = name,
                    hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector
                };
                return mat;
            }
        }
        
        /// <summary>
        /// Shader Property를 업데이트합니다.
        /// </summary>
        private void UpdateMaterialValues ()
        {
            Material mat = GetMat ();
            if (mat == null) return;
            mat.SetVector (CausticsEffectPID._WCE_TexChannels, _texChannelVec [(int) m_textureChannel]);
            mat.SetVector (CausticsEffectPID._WCE_TexRotateSinCos, m_texRotSinCos);
            mat.SetFloat (CausticsEffectPID._WCE_IntensityMainLit, finalMainLit);
            mat.SetFloat (CausticsEffectPID._WCE_IntensityAddLit, finalAddLit);
            mat.SetVector (CausticsEffectPID._WCE_ColorShift, calcColorShiftVec ());
            mat.SetFloat (CausticsEffectPID._WCE_SurfaceY, m_surfaceY);
            mat.SetFloat (CausticsEffectPID._WCE_SurfFadeStart, m_surfFadeStart);
            mat.SetFloat (CausticsEffectPID._WCE_SurfFadeCoef, CausticsEffectTools.CalcFadeCoef (m_surfFadeEnd - m_surfFadeStart));
            mat.SetFloat (CausticsEffectPID._WCE_DepthFadeStart, m_depthFadeStart);
            mat.SetFloat (CausticsEffectPID._WCE_DepthFadeCoef, m_useDepthFade ? CausticsEffectTools.CalcFadeCoef (m_depthFadeEnd - m_depthFadeStart) : 0f);
            mat.SetFloat (CausticsEffectPID._WCE_DistanceFadeStart, m_distanceFadeStart);
            mat.SetFloat (CausticsEffectPID._WCE_DistanceFadeCoef, m_useDistanceFade ? CausticsEffectTools.CalcFadeCoef (m_distanceFadeEnd - m_distanceFadeStart) : 0f);
            mat.SetFloat (CausticsEffectPID._WCE_LitSaturation, m_litSaturation);
            mat.SetFloat (CausticsEffectPID._WCE_NormalAttenRate, m_normalAttenRate);
            mat.SetFloat (CausticsEffectPID._WCE_NormalAtten, m_normalAtten);
            mat.SetFloat (CausticsEffectPID._WCE_TransparentBack, m_transparentBackside);
            mat.SetFloat (CausticsEffectPID._WCE_BacksideShadow, m_backsideShadow);

            bool isNoTex = (m_texture == null);
            mat.SetFloat (CausticsEffectPID._WCE_Density, 1f / Mathf.Max (isNoTex ? m_scale * 0.333f : m_scale, 0.0001f));
            mat.SetTexture (CausticsEffectPID._WCE_CausticsTex, isNoTex ? m_noTexture : m_texture);
            bool useShadow = m_receiveShadows && (m_shadowIntensity > 0f || (m_backsideShadow > 0f && (m_normalAtten < 1f || m_transparentBackside > 0f)));
            CausticsEffectTools.SetMatKeyword (mat, !useShadow, "_RECEIVE_SHADOWS_OFF");
            mat.SetFloat (CausticsEffectPID._WCE_ShadowIntensity, m_shadowIntensity);
            mat.SetFloat (CausticsEffectPID._WCE_MultiplyByTex, multiplyByTex);

            mat.SetInt (CausticsEffectPID._BlendSrcFactor, (int) (useBlendMultiply ? BlendMode.DstColor : BlendMode.One));
            mat.SetInt (CausticsEffectPID._BlendDstFactor, (int) BlendMode.One);

            mat.SetInt (CausticsEffectPID._StencilComp, (int) m_stencilComp);
            mat.SetInt (CausticsEffectPID._StencilPass, (int) m_stencilPass);
            mat.SetInt (CausticsEffectPID._StencilFail, (int) m_stencilFail);
            mat.SetInt (CausticsEffectPID._StencilZFail, (int) m_stencilZFail);
            
            // [EachMesh]
            mat.SetInt (CausticsEffectPID._CullMode, (int) m_cullMode);
            mat.SetInt (CausticsEffectPID._ZWrite, System.Convert.ToInt32 (m_zWriteMode));
            mat.SetInt (CausticsEffectPID._ZTest, (int) m_zTestMode);
            mat.renderQueue = -1;

#if UNITY_EDITOR
            // ----- for Debug Info Rendering
            bool isDebugNormal = (m_debugInfo && m_debugMode == DebugMode.Normal);
            bool isDebugDepth = (m_debugInfo && m_debugMode == DebugMode.Depth);
            bool isDebugNormalErr = (m_debugInfo && m_debugMode == DebugMode.Facing);
            bool isDebugCaustics = (m_debugInfo && m_debugMode == DebugMode.Caustics);
            bool isDebugLitArea = (m_debugInfo && m_debugMode == DebugMode.LightArea);
            CausticsEffectTools.SetMatKeyword (mat, isDebugNormal, "WCE_DEBUG_NORMAL");
            CausticsEffectTools.SetMatKeyword (mat, isDebugDepth, "WCE_DEBUG_DEPTH");
            CausticsEffectTools.SetMatKeyword (mat, isDebugNormalErr, "WCE_DEBUG_FACING");
            CausticsEffectTools.SetMatKeyword (mat, isDebugCaustics, "WCE_DEBUG_CAUSTICS");
            CausticsEffectTools.SetMatKeyword (mat, isDebugLitArea, "WCE_DEBUG_AREA");
            if (isDebugNormal || isDebugDepth || isDebugNormalErr || isDebugCaustics || isDebugLitArea)
            {
                mat.SetInt (CausticsEffectPID._BlendSrcFactor, (int) BlendMode.One);
                mat.SetInt (CausticsEffectPID._BlendDstFactor, (int) BlendMode.Zero);
            }
            if (isDebugCaustics)
            {
                float m = Mathf.Lerp (1f, 0.5f, multiplyRaw);
                mat.SetFloat (CausticsEffectPID._WCE_IntensityMainLit, finalMainLit * m);
                mat.SetFloat (CausticsEffectPID._WCE_IntensityAddLit, finalAddLit * m);
            }
#endif
        }


        #region RenderPass
        // ----------------------------------------------------------- Enqueue RenderPass
#if UNITY_EDITOR
        static private readonly string tempFilePath = "Temp/WCEInitialized";
        [InitializeOnLoadMethod]
        static private void RegisterFeature ()
        {
            EditorApplication.delayCall += () => {
                if (!File.Exists (tempFilePath)) {
                    File.Create (tempFilePath);
                    AddFeatureIfNeedManage ();
                }
            };
            
            EditorSceneManager.sceneOpened += (Scene scene, OpenSceneMode mode) => {
                if (mode == OpenSceneMode.Single) AddFeatureIfNeedManage ();
            };
            
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
                if (state == PlayModeStateChange.EnteredPlayMode) AddFeatureIfNeedManage ();
            };
        }

        static private void AddFeatureIfNeedManage ()
        {
            if (WaterCausticsEffectData.GetAsset().AutoManageFeature)
            {
                WaterCausticsEffectFeatureEditor.AddFeatureToAllRenderers (useUndo: false);
            }
        }
#endif

        private void OnEnableForPass ()
        {
            WaterCausticsEffectFeature.onEnqueue -= EnqueuePass;
            WaterCausticsEffectFeature.onEnqueue += EnqueuePass;
        }

        private void OnDisableForPass ()
        {
            WaterCausticsEffectFeature.onEnqueue -= EnqueuePass;
        }
        // 커스틱 패스
        private WaterCausticsEffectPass _eachMeshPass;
        private void EnqueuePass (ScriptableRenderer renderer, Camera cam)
        {
            if (isIntensityZero) return;

            if (m_layerMask == 0) return;
            _eachMeshPass ??= new WaterCausticsEffectPass ();
            bool useOpaqueTex = multiplyByTex > 0f;
            _eachMeshPass.Setup (eventAdjusted, GetMat(), m_layerMask, useOpaqueTex);
            renderer.EnqueuePass (_eachMeshPass);
        }

        // -----------------------------------------------------------
        #endregion

        #region Gizmo
        // ----------------------------------------------------------- Gizmo
#if UNITY_EDITOR
        private void OnDrawGizmosSelected ()
        {
            if (Selection.gameObjects.Length != 1 || Selection.activeGameObject != gameObject)
            {
                DrawGizmoAACube (1f, new Color (0.8f, 0.4f, 0.0f, 0.5f), true);
            }
            else
            {
                DrawGizmoAACube (2f, new Color (0.8f, 0.4f, 0.0f, 1f), true);
            }
        }
        
        // 적용 가능한 범위를 출력하는 코드
        private void DrawGizmoAACube (float width, Color color, bool zTest)
        {
            void drawQuart (float rot)
            {
                Handles.matrix = transform.localToWorldMatrix * Matrix4x4.Rotate (Quaternion.Euler (rot, 0f, 0f));
                Handles.DrawAAPolyLine (Texture2D.whiteTexture, width, new Vector3 (-.5f, -.5f, -.5f), new Vector3 (-.5f, .5f, -.5f), new Vector3 (.5f, .5f, -.5f), new Vector3 (.5f, -.5f, -.5f));
            }
            Handles.color = color;
            Handles.zTest = zTest ? CompareFunction.LessEqual : CompareFunction.Greater;
            var tmp = Handles.matrix;
            for (int i = 0; i < 4; i++) drawQuart (i * 90f);
            Handles.matrix = tmp;
        }
#endif // End of UNITY_EDITOR
        #endregion
    }
}

#endif // End of WCE_URP
