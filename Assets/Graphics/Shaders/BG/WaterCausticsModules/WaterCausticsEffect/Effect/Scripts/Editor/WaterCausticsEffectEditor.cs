#if UNITY_EDITOR && WCE_URP
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WaterCausticsModules.Effect;

namespace WaterCausticsModules
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof (WaterCausticsEffect))]
    public class WaterCausticsEffectEditor : Editor
    {
        private SerializedProperty m_debugInfo;
        private SerializedProperty m_debugMode;
        private SerializedProperty m_layerMask;
        private SerializedProperty m_texture;
        private SerializedProperty m_textureChannel;
        private SerializedProperty m_textureRotation;
        private SerializedProperty m_texRotSinCos;
        private SerializedProperty m_intensity;
        private SerializedProperty m_mainLit;
        private SerializedProperty m_addLit;
        private SerializedProperty m_colorShift;
        private SerializedProperty m_colorShiftDir;
        private SerializedProperty m_scale;
        private SerializedProperty m_surfaceY;
        private SerializedProperty m_surfFadeStart;
        private SerializedProperty m_surfFadeEnd;
        private SerializedProperty m_useDepthFade;
        private SerializedProperty m_depthFadeStart;
        private SerializedProperty m_depthFadeEnd;
        private SerializedProperty m_useDistanceFade;
        private SerializedProperty m_distanceFadeStart;
        private SerializedProperty m_distanceFadeEnd;
        private SerializedProperty m_litSaturation;
        private SerializedProperty m_multiply;
        private SerializedProperty m_normalAttenRate;
        private SerializedProperty m_normalAtten;
        private SerializedProperty m_transparentBackside;
        private SerializedProperty m_backsideShadow;
        private SerializedProperty m_shadowIntensity;
        private SerializedProperty m_receiveShadows;
        private SerializedProperty m_useMainLit;
        private SerializedProperty m_useAddLit;
        private SerializedProperty m_stencilComp;
        private SerializedProperty m_stencilPass;
        private SerializedProperty m_stencilFail;
        private SerializedProperty m_stencilZFail;
        private SerializedProperty m_cullMode;
        private SerializedProperty m_zWriteMode;
        private SerializedProperty m_zTestMode;
        private SerializedProperty m_renderEvent;
        private SerializedProperty m_renderEventAdjust;

        private void prepProperties ()
        {
            m_debugInfo = serializedObject.FindProperty ("m_debugInfo");
            m_debugMode = serializedObject.FindProperty ("m_debugMode");
            m_layerMask = serializedObject.FindProperty ("m_layerMask");
            m_texture = serializedObject.FindProperty ("m_texture");
            m_textureChannel = serializedObject.FindProperty ("m_textureChannel");
            m_textureRotation = serializedObject.FindProperty ("m_textureRotation");
            m_texRotSinCos = serializedObject.FindProperty ("m_texRotSinCos");
            m_intensity = serializedObject.FindProperty ("m_intensity");
            m_mainLit = serializedObject.FindProperty ("m_mainLit");
            m_addLit = serializedObject.FindProperty ("m_addLit");
            m_colorShift = serializedObject.FindProperty ("m_colorShift");
            m_colorShiftDir = serializedObject.FindProperty ("m_colorShiftDir");
            m_scale = serializedObject.FindProperty ("m_scale");
            m_surfaceY = serializedObject.FindProperty ("m_surfaceY");
            m_surfFadeStart = serializedObject.FindProperty ("m_surfFadeStart");
            m_surfFadeEnd = serializedObject.FindProperty ("m_surfFadeEnd");
            m_useDepthFade = serializedObject.FindProperty ("m_useDepthFade");
            m_depthFadeStart = serializedObject.FindProperty ("m_depthFadeStart");
            m_depthFadeEnd = serializedObject.FindProperty ("m_depthFadeEnd");
            m_useDistanceFade = serializedObject.FindProperty ("m_useDistanceFade");
            m_distanceFadeStart = serializedObject.FindProperty ("m_distanceFadeStart");
            m_distanceFadeEnd = serializedObject.FindProperty ("m_distanceFadeEnd");
            m_litSaturation = serializedObject.FindProperty ("m_litSaturation");
            m_multiply = serializedObject.FindProperty ("m_multiply");
            m_normalAttenRate = serializedObject.FindProperty ("m_normalAttenRate");
            m_normalAtten = serializedObject.FindProperty ("m_normalAtten");
            m_transparentBackside = serializedObject.FindProperty ("m_transparentBackside");
            m_backsideShadow = serializedObject.FindProperty ("m_backsideShadow");
            m_shadowIntensity = serializedObject.FindProperty ("m_shadowIntensity");
            m_receiveShadows = serializedObject.FindProperty ("m_receiveShadows");
            m_useMainLit = serializedObject.FindProperty ("m_useMainLit");
            m_useAddLit = serializedObject.FindProperty ("m_useAddLit");
            m_stencilComp = serializedObject.FindProperty ("m_stencilComp");
            m_stencilPass = serializedObject.FindProperty ("m_stencilPass");
            m_stencilFail = serializedObject.FindProperty ("m_stencilFail");
            m_stencilZFail = serializedObject.FindProperty ("m_stencilZFail");
            m_cullMode = serializedObject.FindProperty ("m_cullMode");
            m_zWriteMode = serializedObject.FindProperty ("m_zWriteMode");
            m_zTestMode = serializedObject.FindProperty ("m_zTestMode");
            serializedObject.FindProperty ("m_shader");
            serializedObject.FindProperty ("m_noTexture");
            m_renderEvent = serializedObject.FindProperty ("m_renderEvent");
            m_renderEventAdjust = serializedObject.FindProperty ("m_renderEventAdjust");
        }

        private SerializedObject _wceData;
        private SerializedProperty m_autoManageFeature;
        private SerializedObject prepWceData ()
        {
            if (_wceData == null)
            {
                _wceData = new SerializedObject (WaterCausticsEffectData.GetAsset ());
            }
                
            if (m_autoManageFeature == null)
            {
                m_autoManageFeature = _wceData.FindProperty ("m_autoManageFeature");
            }
            _wceData.Update ();
            return _wceData;
        }

        static readonly GUIContent [] _cullingEnumStr =
        {
            new GUIContent ("Both"),
            new GUIContent ("Back"),
            new GUIContent ("Front"),
        };

        static readonly string descGenFromDepth = "[Generate from Depth] \nGenerate from _CameraDepthTexture generated by the system. This method is not good for smooth surfaces, but it does produce the correct normals.";
        static readonly string descCamNormalTex = "[Camera Normals Tex] \nSampling _CameraNormalsTexture generated by the system. This is high quality, but may produce strange results with materials that do not support normal output.";

        private List<ScriptableRendererData> _rendererDataList;
        private bool _hasGetRenderData;

        UniversalRenderPipelineAsset urpAsset => GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        public override void OnInspectorGUI ()
        {
            if (urpAsset) {
                var wceData = prepWceData ();
                prepProperties ();
                serializedObject.Update ();
                using (var check = new EditorGUI.ChangeCheckScope ()) {
                    drawProperties ();
                    serializedObject.ApplyModifiedProperties ();
                    wceData.ApplyModifiedProperties ();
                    if (check.changed) {
                        foreach (var tar in targets.OfType<WaterCausticsEffect> ())
                            tar.OnInspectorChanged ();
                    }
                }
            } else
            {
                OnInspectorGUI_NotSettingYet ();
            }
        }

        // -----------------------------------------------------------
        readonly Color colorPinkBar = new Color (1f, 0.3f, 0.6f, 0.3f);
        readonly Color colorPinkContent = new Color (1f, 0.3f, 0.6f, 1f);
        private float lineH = EditorGUIUtility.singleLineHeight + 2;
        readonly float SPACE_SUB_TOP_5 = 5f;
        readonly float SPACE_SUB_BTM_12 = 12f;
        readonly float SPACE_MAIN_TOP_7 = 7f;
        readonly float SPACE_MAIN_BTM_5 = 5f;
        private Color _defaultGUIColor;

        private void setLabelAreaWidth (float labelWidthMin, float valWidthMin)
        {
            if (EditorGUIUtility.labelWidth < labelWidthMin)
            {
                EditorGUIUtility.labelWidth = labelWidthMin;
            }

            if (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth < valWidthMin)
            {
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - valWidthMin;
            }
        }

        private void drawProperties ()
        {
            _defaultGUIColor = GUI.color;
            storeIndentWidth ();
            EditorGUIUtility.labelWidth += 6;
            EditorGUI.indentLevel++;
            setLabelAreaWidth (labelWidthMin: 140f, valWidthMin: 170f);
            bool isEditingMultiObj = serializedObject.isEditingMultipleObjects;
            // ---------------------------------------------------------------------------------- System

            // ------ RendererFeature
            if (!_hasGetRenderData) {
                _hasGetRenderData = true;
                WaterCausticsEffectFeatureEditor.GetAllRendererData (out _rendererDataList);
                if (WaterCausticsEffectData.GetAsset ().AutoManageFeature)
                    WaterCausticsEffectFeatureEditor.AddFeatureToAllRenderers (_rendererDataList, useUndo: false);
            }
            bool someRenderNotHasFeature = !WaterCausticsEffectFeatureEditor.CheckAllHasActiveFeature (_rendererDataList);
            bool currentRenderNotHasFeature = someRenderNotHasFeature && !WaterCausticsEffectFeature.effective;
            // ----------------------

            {
                EditorGUILayout.Space (SPACE_MAIN_TOP_7);

                // ------ RendererFeature
                if (someRenderNotHasFeature)
                {
                    EditorGUI.indentLevel -= 1;
                    using (new ColorScope (currentRenderNotHasFeature ? colorPinkContent : _defaultGUIColor)) {
                        string str = "To apply this effect, a Renderer Feature needs to be added to a Renderer.";
                        if (currentRenderNotHasFeature)
                            EditorGUILayout.HelpBox ($"The Renderer Feature has not been added or activated in the current Renderer.\n{str}", MessageType.Warning);
                        else
                            EditorGUILayout.HelpBox ($"The Renderer Feature has not been added or activated in some Renderers.\n{str}", MessageType.Warning);
                        EditorGUILayout.Space (2);
                        Rect rect = GUILayoutUtility.GetRect (0, 0);
                        rect.height = lineH;
                        rect.width *= 0.5f;
                        if (GUI.Button (rect, new GUIContent ("Select Renderer", "Search and select RendererData assets."), EditorStyles.miniButton)) {
                            if (WaterCausticsEffectFeatureEditor.GetAllRendererData (out var list))
                            {
                                WaterCausticsEffectFeatureEditor.SelectAndPing (list);
                                if (list.Count >= 2) {
                                    var pathStr = WaterCausticsEffectFeatureEditor.AssetsToPathStr (list);
                                    EditorApplication.delayCall += () => EditorApplication.delayCall += () =>
                                        EditorUtility.DisplayDialog ("Multiple found.", $"Multiple RendererData assets found.\n\n{pathStr}", "OK");
                                }
                            } else
                            {
                                EditorUtility.DisplayDialog ("Not found.", $"Not found.", "OK");
                            }
                        }
                        rect.x += rect.width;
                        if (GUI.Button (rect, new GUIContent ("Fix It", "Add a Renderer Feature to Renderers."), EditorStyles.miniButton))
                        {
                            if (WaterCausticsEffectFeatureEditor.AddFeatureToAllRenderers (useUndo: true))
                            {
                            }
                        }
                        EditorGUILayout.Space (lineH);
                        using (new ColorScope (_defaultGUIColor))
                        {
                            EditorGUILayout.Space (4);
                        }
                        EditorGUI.indentLevel += 1;
                        EditorGUILayout.Space (15);
                    }
                }
                // ----------------------
                drawBoolAndValue (m_debugInfo, m_debugMode, true, new GUIContent ("Debug Info", "Display data for debugging. This is only valid on the editor.\n\n" +
                    $"[{DebugMode.Normal}]\nDisplays world-space normal data.\n\n" +
                    $"[{DebugMode.Depth}]\nDisplays the depth data.\n\n" +
                    $"[{DebugMode.Facing}]\nDisplays the plane facing the camera as bright.\n\n" +
                    $"[{DebugMode.Caustics}]\nDisplays only caustics effects.\n\n" +
                    $"[{DebugMode.LightArea}]\nDisplays the affected area by each light.\n\n" +
                    $"If some objects are not rendering correctly on At Once method with Camera Normals Tex, the object's material is outputting the wrong normals. Check the normals on this screen and modify the material (shader) of the object."));

                EditorGUILayout.Space (SPACE_SUB_BTM_12);
                // ---------------------------------------------------------------------------------- Influence Scope
                
                EditorGUILayout.Space (SPACE_SUB_TOP_5);
                SelectGameObjectLayer (new GUIContent ("Layer", "레이어 설정"));
                EditorGUILayout.PropertyField (m_layerMask, new GUIContent ("Layer Mask", "레이어 마스크 설정"));
                popup (m_cullMode, _cullingEnumStr, "Render Face", "렌더링할 면을 선택합니다.");

                bool isExpanded = isExpand (m_renderEvent, true, new GUIContent (""));
                EditorGUILayout.Space (-EditorGUIUtility.singleLineHeight - 2f);
                string baseTimingName = splitCamelCase (((RenderPassEvent) m_renderEvent.intValue).ToString ());
                int sysOpqTiming = (int) WaterCausticsEffect.SYS_OPAQUE_TEX_EVENT;
                string sysOpqName = WaterCausticsEffect.SYS_OPAQUE_TEX_EVENT.ToString ();
                string sysOpqDesc = $"{sysOpqName}({sysOpqTiming})";
                string sysOpqDescPlusOne = $"{sysOpqName}+1 ({sysOpqTiming + 1})";
                string defaultTiming = WaterCausticsEffect.RENDER_EVENT.ToString ();
                int defaultTimingAdj = WaterCausticsEffect.RENDER_EVENT_ADJ;
                int baseTiming = m_renderEvent.intValue;
                int adjTiming = m_renderEventAdjust.intValue;
                int adjusted = baseTiming + adjTiming;
                bool isHasDifVal = m_renderEvent.hasMultipleDifferentValues || m_renderEventAdjust.hasMultipleDifferentValues;
                EditorGUILayout.LabelField (new GUIContent ("Draw Timing", $"Specifies the timing of drawing.\n\nTo display this effect on _CameraOpaqueTexture, it must be drawn before {sysOpqDesc}."), new GUIContent (isHasDifVal ? "-" : $"{baseTimingName} {(adjTiming < 0 ? "-" : "+")}{Mathf.Abs (adjTiming)} ({Mathf.Clamp (adjusted, 0, 1000)})"));
                if (isExpanded) {
                    using (new IndentScope (-2f, 4f)) {
                        EditorGUILayout.PropertyField (m_renderEvent, new GUIContent ($"Render Event", $"Controls when the render executes. \n[Default: {defaultTiming}]"));
                        EditorGUILayout.PropertyField (m_renderEventAdjust, new GUIContent ("Adjustment", $"Controls when the render executes. This number is added to the Draw Timing above. \n[Default: {defaultTimingAdj}]"));
                        bool isEarly = (adjusted <= sysOpqTiming);
                        string warning = isEarly ? $"To use a value between 0 and 1 in the Multiply Color setting, set it after {sysOpqDescPlusOne}." :
                            $"To display this effect on _CameraOpaqueTexture, it must be drawn before {sysOpqDesc}.";
                        EditorGUILayout.HelpBox (new GUIContent (warning, ""), true);
                    }
                }
                if (isExpand (m_zTestMode, false, new GUIContent ("Depth Buffer", "Depth Buffer 설정")))
                {
                    using (new IndentScope (0, 2))
                    {
                        EditorGUILayout.PropertyField (m_zWriteMode, new GUIContent ("ZWrite", "ZWrite 설정"));
                        EditorGUILayout.PropertyField (m_zTestMode, new GUIContent ("ZTest", "ZTest 설정"));
                    }
                }
                EditorGUILayout.Space (2);
                EditorGUILayout.PropertyField (m_stencilComp, new GUIContent ("Comp", "스텐실 비교 연산 설정"));
                EditorGUILayout.PropertyField (m_stencilPass, new GUIContent ("Pass", "스텐실 Pass 설정"));
                EditorGUILayout.PropertyField (m_stencilFail, new GUIContent ("Fail", "스텐실 Fail 설정"));
                EditorGUILayout.PropertyField (m_stencilZFail, new GUIContent ("ZFail", "스탠실 ZFail 설정"));
                EditorGUILayout.Space (SPACE_SUB_BTM_12);
            }

            EditorGUILayout.Space (SPACE_MAIN_BTM_5);
            // ---------------------------------------------------------------------------------- Effect Group
            bool useTextureWarning = (m_texture.objectReferenceValue == null && isGameObjectOnScene () && !isEditingMultiObj);
            if (expandMainGroup (m_texture, true, "Caustics Effect", isPink: useTextureWarning && m_texture.isExpanded))
            {
                EditorGUILayout.Space (SPACE_MAIN_TOP_7);

                if (expandSubGroup (m_textureRotation, true, "Texture", isPink: useTextureWarning)) {
                    EditorGUILayout.Space (SPACE_SUB_TOP_5);

                    using (new ColorScope (useTextureWarning ? colorPinkContent : GUI.color)) {
                        EditorGUILayout.PropertyField (m_texture, new GUIContent ("Caustics Texture", $"Set the RenderTexture specified as the output destination in the {typeof (WaterCausticsTexGenerator).Name}."));
                        if (!m_texture.hasMultipleDifferentValues) {
                            if (useTextureWarning) {
                                EditorGUILayout.Space (1);
                                EditorGUILayout.BeginHorizontal ();
                                GUILayout.FlexibleSpace ();
                                if (GUILayout.Button ("Search from this Scene", EditorStyles.miniButton, GUILayout.Width (150))) {
                                    var gen = Object.FindObjectsByType<WaterCausticsTexGenerator> (FindObjectsSortMode.None).FirstOrDefault (g => g.renderTexture != null);

                                    if (gen != null)
                                        m_texture.objectReferenceValue = gen.renderTexture;
                                    else
                                        EditorUtility.DisplayDialog ("Not Found", $"There is no {typeof (WaterCausticsTexGenerator).Name} with active and having RenderTexture in this scene.", "OK");
                                }
                                EditorGUILayout.EndHorizontal ();
                                EditorGUILayout.Space (3);
                            } else {
                                var tex = m_texture.objectReferenceValue as Texture;
                                if (tex != null) {
                                    EditorGUILayout.HelpBox ($"{tex.width}x{tex.height} / {tex.graphicsFormat}", MessageType.None);
                                    EditorGUILayout.Space (3);
                                }
                            }
                        }
                    }
                    EditorGUILayout.PropertyField (m_textureChannel, new GUIContent ("Channel", "Channels to be used. Set to R if using R-channel only textures."));
                    using (var check = new EditorGUI.ChangeCheckScope ())
                    {
                        using (new IndentScope (0f, 0f, 1))
                        {
                            EditorGUILayout.PropertyField (m_textureRotation, new GUIContent ("Rotation", "텍스쳐 회전합니다."));
                            drawDirMark (m_textureRotation, true);
                        }
                        if (check.changed) {
                            float rad = m_textureRotation.floatValue * Mathf.Deg2Rad;
                            m_texRotSinCos.vector2Value = new Vector2 (Mathf.Sin (rad), Mathf.Cos (rad));
                        }
                    }
                }

                if (expandSubGroup (m_scale, true, "Dimensions"))
                {
                    EditorGUILayout.Space (SPACE_SUB_TOP_5);

                    EditorGUILayout.PropertyField (m_scale, new GUIContent ("Scale", "커스틱으 스케일을 조절합니다."));
                    EditorGUILayout.PropertyField (m_surfaceY, new GUIContent ("Water Surface Y", "물의 표면의 높이를 계산합니다."));

                    EditorGUILayout.Space (4);
                    drawStartEndProp (m_surfFadeStart, m_surfFadeEnd, new GUIContent ("Surface Fade", "표현의 시작과 끝을 조절합니다."));
                    EditorGUILayout.PropertyField (m_useDepthFade, new GUIContent ("Depth Fade", "Depth에 따른 커스틱 효과를 조절합니다."));
                    if (m_useDepthFade.boolValue)
                    {
                        using (new IndentScope(-2f, 0f))
                            drawStartEndProp(m_depthFadeStart, m_depthFadeEnd,
                                new GUIContent("Range",
                                    "Depth Fade Start와 Depth Fade End 사이의 Depth에 따라 커스틱 효과를 조절합니다."));
                    }
                    EditorGUILayout.PropertyField (m_useDistanceFade, new GUIContent ("Distance Fade", "거리에 따른 커스틱 효과를 조절합니다."));
                    if (m_useDistanceFade.boolValue)
                    {
                        using (new IndentScope (-2f, 0f))
                            drawStartEndProp (m_distanceFadeStart, m_distanceFadeEnd, new GUIContent ("Range", "Fade Start와 Fade End 사이의 거리에 따라 커스틱 효과를 조절합니다."));
                    }

                    EditorGUILayout.Space (SPACE_SUB_BTM_12);
                }
                if (expandSubGroup (m_intensity, true, "Effect")) {
                    EditorGUILayout.Space (SPACE_SUB_TOP_5);

                    EditorGUILayout.PropertyField (m_intensity, new GUIContent ("Intensity", "커스틱의 강도"));
                    using (new IndentScope (-1f, 1f)) {
                        drawBoolAndValue (m_useMainLit, m_mainLit, hide: true, new GUIContent ("Main Light", "Main Light의 강도를 조절합니다. 체크박스가 Off인 경우, Main Light 계산이 생략됩니다."));
                        drawBoolAndValue (m_useAddLit, m_addLit, hide: true, new GUIContent ("Additional Lights", "Additional Lights의 강도를 조절합니다. 체크박스가 Off인 경우, Additional Lights 계산이 생략됩니다."));
                    }
                    EditorGUILayout.Space (2);

                    drawBoolAndValue (m_receiveShadows, m_shadowIntensity, hide: true, new GUIContent ("Shadow", "1의 경우 그림자를 그려주고, 0일 경우 그림자가 그려지더라도 커스틱이 표현됩니다."));
                    EditorGUILayout.Space (2);
                    EditorGUILayout.PropertyField (m_colorShift, new GUIContent ("Color Shift", "빛의 밀어서 프리즘 효과를 만듭니다."));
                    if (m_colorShift.floatValue > 0f) {
                        using (new IndentScope (-2f, 0f, 2)) {
                            EditorGUILayout.PropertyField (m_colorShiftDir, new GUIContent ("Direction", "빛의 방향을 조절합니다."));
                            drawDirMark (m_colorShiftDir, true);
                        }
                    }
                    EditorGUILayout.Space (2);

                    EditorGUILayout.PropertyField (m_litSaturation, new GUIContent ("Light Color", "빛의 Saturation을 조절합니다."));
                    EditorGUILayout.Space (2);
                    EditorGUILayout.PropertyField (m_multiply, new GUIContent ("Multiply Color", "화면에 색상에 곱한다음 더하는 연산 (1또는 0 설정)"));
                    if (!isEditingMultiObj && (m_multiply.floatValue > 0f && m_multiply.floatValue < 1f) &&
                        ((m_renderEvent.intValue + m_renderEventAdjust.intValue) <= (int) WaterCausticsEffect.SYS_OPAQUE_TEX_EVENT)
                    )
                    {
                        using (new IndentScope (0f, 0f)) {
                            EditorGUILayout.HelpBox (new GUIContent ("1값 또는 0으로만 설정할 수 있습니다.", ""), true);
                        }
                    }

                    EditorGUILayout.Space (3);
                    EditorGUILayout.PropertyField (m_normalAtten, new GUIContent ("Normal Attenuation", "법선과 빛 선 간의 각도에 따른 감쇠. \n[[기본값: 1]"));
                    {
                        using (new IndentScope (-2f, 2f)) {
                            using (new DisableScope (m_normalAtten.floatValue > 0f)) {
                                EditorGUILayout.PropertyField (m_normalAttenRate, new GUIContent ("Rate", "법선 감쇠 비율. 빛이 얼마나 빨리 사라지는지에 대한 속도입니다. \n[[기본값: 1.5]]"));
                                EditorGUILayout.PropertyField (m_transparentBackside, new GUIContent ("Transparent", "Transparent Backside \n[기본값: 0]"));
                            }
                            using (new DisableScope (m_receiveShadows.boolValue && (m_normalAtten.floatValue < 1f || m_transparentBackside.floatValue > 0f))) {
                                EditorGUILayout.PropertyField (m_backsideShadow, new GUIContent ("Backside Shadow", "The intensity of shadow on the backside. \n[기본값: 0]"));
                            }
                        }
                    }
                    EditorGUILayout.Space (SPACE_SUB_BTM_12);
                }
            }

            EditorGUILayout.Space (SPACE_MAIN_BTM_5);
            // ---------------------------------------------------------------------------------- Advanced Settings
            if (expandMainGroup (m_backsideShadow, false, "Advanced Settings")) {
                EditorGUILayout.Space (SPACE_MAIN_TOP_7);

                if (expandSubGroup (m_normalAttenRate, false, "Renderer Feature")) {
                    EditorGUILayout.Space (SPACE_SUB_TOP_5);
                    using (var check = new EditorGUI.ChangeCheckScope ()) {
                        EditorGUILayout.PropertyField (m_autoManageFeature, new GUIContent ("Auto-Management", "Automatically manages Renderer Feature. It automatically adds the Renderer Feature for this effect to all Renderer Data in the project.\n\nIf this is turned off, it is required to manually add the Renderer Feature to the Renderer Data."));
                        if (check.changed && m_autoManageFeature.boolValue == true) {
                            WaterCausticsEffectFeatureEditor.AddFeatureToAllRenderers (useUndo: false);
                        }
                        if (!m_autoManageFeature.boolValue) {
                            EditorGUILayout.Space (1);
                            EditorGUI.indentLevel++;
                            EditorGUILayout.HelpBox ("To apply this effect, a Renderer Feature needs to be added to a Renderer.", MessageType.None);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUILayout.Space (SPACE_SUB_BTM_12);
                }
            }
            EditorGUILayout.Space (SPACE_SUB_BTM_12);
            EditorGUILayout.Space (SPACE_MAIN_BTM_5);
            EditorGUILayout.Space (SPACE_MAIN_BTM_5);
            EditorGUI.indentLevel--;
        }

        // ----------------------------------------------------------- Parts
        private string splitCamelCase (string str)
        {
            return Regex.Replace (
                Regex.Replace (str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        private float _indentWidth;
        private void storeIndentWidth ()
        {
            if (_indentWidth != 0f) return;
            var x0 = EditorGUI.IndentedRect (Rect.zero).x;
            EditorGUI.indentLevel++;
            _indentWidth = EditorGUI.IndentedRect (Rect.zero).x - x0;
            EditorGUI.indentLevel--;
        }

        private void drawBoolAndValue (SerializedProperty propBool, SerializedProperty propValue, bool hide, GUIContent label) {
            EditorGUILayout.PropertyField (propBool, label);
            var rect = GUILayoutUtility.GetLastRect ();
            var labelW = EditorGUIUtility.labelWidth;
            if (!hide || propBool.boolValue) {
                using (new DisableScope (propBool.boolValue)) {
                    EditorGUIUtility.labelWidth += 25;
                    EditorGUI.PropertyField (rect, propValue, new GUIContent (" "));
                }
            }
            EditorGUIUtility.labelWidth = labelW;
        }

        private void SelectGameObjectLayer (GUIContent label)
        {
            var gameObjects = targets.Select (t => (t as WaterCausticsEffect).gameObject).ToArray ();
            var layers = gameObjects.Select (go => go.layer).Distinct ().ToArray ();
            using (var check = new EditorGUI.ChangeCheckScope ()) {
                EditorGUI.showMixedValue = (layers.Length != 1);
                int newVal = EditorGUILayout.LayerField (label, layers [0]);
                EditorGUI.showMixedValue = false;
                if (check.changed) {
                    Undo.RecordObjects (gameObjects, "Changed Layer");
                    foreach (var go in gameObjects) {
                        go.layer = newVal;
                        EditorUtility.SetDirty (go);
                    }
                    if (layers.Length >= 2 && gameObjects.Length == Selection.objects.Length)
                    {
                        bool isSelected = true;
                        foreach (var o in gameObjects) {
                            if (!Selection.objects.Contains (o)) {
                                isSelected = false;
                                break;
                            }
                        }
                        if (isSelected) {
                            Selection.activeGameObject = null;
                            EditorApplication.delayCall += () => { Selection.objects = gameObjects; };
                        }
                    }
                }
            }
        }

        private void drawStartEndProp (SerializedProperty propStt, SerializedProperty propEnd, GUIContent label)
        {
            EditorGUILayout.LabelField (label);
            var rect = GUILayoutUtility.GetLastRect ();
            rect.x += EditorGUIUtility.labelWidth;
            rect.width -= EditorGUIUtility.labelWidth;
            drawStartEndPropInRect (rect, propStt, propEnd);
        }


        private void drawStartEndPropInRect (Rect rect, SerializedProperty propStt, SerializedProperty propEnd)
        {
            bool isWide = (rect.width > 140);
            float span = isWide ? 5f : 3f;
            var rect2 = rect;
            var rect3 = rect;
            rect2.width = rect3.width = (rect.width - span) * 0.5f;
            rect3.x += rect2.width + span;
            var storeIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var labelW = EditorGUIUtility.labelWidth;
            using (var check = new EditorGUI.ChangeCheckScope ()) {
                EditorGUI.showMixedValue = propStt.hasMultipleDifferentValues;
                EditorGUIUtility.labelWidth = isWide ? 32 : 10;
                float newStt = EditorGUI.FloatField (rect2, isWide ? "Start" : "S", propStt.floatValue);
                if (check.changed)
                    propStt.floatValue = Mathf.Clamp (newStt, 0, propEnd.floatValue);
            }
            using (var check = new EditorGUI.ChangeCheckScope ()) {
                EditorGUI.showMixedValue = propEnd.hasMultipleDifferentValues;
                EditorGUIUtility.labelWidth = isWide ? 26 : 10;
                float newEnd = EditorGUI.FloatField (rect3, isWide ? "End" : "E", propEnd.floatValue);
                if (check.changed)
                    propEnd.floatValue = Mathf.Max (newEnd, propStt.floatValue);
            }
            EditorGUI.showMixedValue = false;
            EditorGUIUtility.labelWidth = labelW;
            EditorGUI.indentLevel = storeIndent;
        }

        static private Color colorMulAlpha (Color c, float mulAlpha) => new Color (c.r, c.g, c.b, c.a * mulAlpha);

        private void drawDirMark (SerializedProperty prop, bool isActive = true)
        {
            EditorGUI.indentLevel--;
            var rect = EditorGUI.IndentedRect (GUILayoutUtility.GetLastRect ());
            drawDirMark (rect, prop, isActive);
            EditorGUI.indentLevel++;
        }
        private void drawDirMark (Rect rect, SerializedProperty prop, bool isActive = true)
        {
            rect.width = EditorGUIUtility.labelWidth;
            Vector2 origin = new Vector2 (CIRCLE_R + 1, rect.height * 0.5f);
            float dir = prop.floatValue;
            Handles.color = colorMulAlpha (EditorStyles.label.normal.textColor, isActive ? 0.8f : 0.4f);
            var tmpMatrix = Handles.matrix;
            GUI.BeginClip (rect, origin, Vector2.zero, false);
            Handles.matrix = tmpMatrix * Matrix4x4.Scale (Vector3.one * CIRCLE_R);
            Handles.DrawAAPolyLine (Texture2D.whiteTexture, 1, circlePts);
            if (!prop.hasMultipleDifferentValues) {
                Handles.matrix = tmpMatrix * Matrix4x4.Rotate (Quaternion.Euler (0f, 0f, dir)) * Matrix4x4.Scale (Vector3.one * (CIRCLE_R - 0.5f));
                Handles.DrawAAConvexPolygon (arrowAry);
            }
            Handles.matrix = tmpMatrix;
            GUI.EndClip ();
        }

        static private Vector2 dirToVec (float dir) => new Vector2 (Mathf.Sin (dir * Mathf.Deg2Rad), -Mathf.Cos (dir * Mathf.Deg2Rad));
        const float CIRCLE_R = 5f;
        static readonly private Vector3 [] arrowAry =
        {
            dirToVec (0),
            dirToVec (150f),
            dirToVec (170f),
            dirToVec (-170f),
            dirToVec (-150f),
        };
        static readonly private Vector3 [] circlePts =
        {
            new Vector3 (-1f, 0f),
            new Vector3 (-0.87f, -0.5f),
            new Vector3 (-0.5f, -0.87f),
            new Vector3 (0f, -1f),
            new Vector3 (0.5f, -0.87f),
            new Vector3 (0.87f, -0.5f),
            new Vector3 (1f, 0f),
            new Vector3 (0.87f, 0.5f),
            new Vector3 (0.5f, 0.87f),
            new Vector3 (0f, 1f),
            new Vector3 (-0.5f, 0.87f),
            new Vector3 (-0.87f, 0.5f),
            new Vector3 (-1f, 0f),
        };

        bool isGameObjectOnScene ()
        {
            return (target as Component).gameObject.scene.IsValid ();
        }

        private void drawRectMain (bool isPink = false)
        {
            Color color = isPink ? colorPinkBar : new Color (0f, 0f, 0f, 0.2f);
            Rect rect = GUILayoutUtility.GetRect (0, 0);
            rect.height = lineH;
            rect.x -= _indentWidth + 4;
            rect.width += _indentWidth + 8;
            EditorGUI.DrawRect (rect, color);
        }

        private bool expandMainGroup (SerializedProperty prop, bool defOpen, string label, bool isPink = false)
        {
            EditorGUI.indentLevel--;
            drawRectMain (isPink);
            bool expand = isExpand (prop, true, new GUIContent (label));
            EditorGUI.indentLevel++;
            return expand;
        }

        private bool expandSubGroup (SerializedProperty prop, bool defOpen, string label, bool isPink = false)
        {
            Color color = isPink ? colorPinkBar : colorMulAlpha (EditorStyles.label.normal.textColor, 0.1f);
            GUILayout.Label (" ");
            Rect rect = GUILayoutUtility.GetLastRect ();
            rect.y += 1f;
            rect.x += 3f;
            rect.width -= 3f;
            Rect rect2 = rect;
            rect2.x -= 14f;
            rect2.width += 15f;
            EditorGUI.DrawRect (rect2, color);
            GUI.Label (rect, label);
            if (prop == null)
            {
                return true;
            } else
            {
                rect.x -= 14;
                prop.isExpanded = EditorGUI.Foldout (rect, prop.isExpanded != defOpen, " ") != defOpen;
                return prop.isExpanded != defOpen;
            }
        }

        private void popup (SerializedProperty prop, GUIContent [] enumStr, string text, string tooltip)
        {
            using (var check = new EditorGUI.ChangeCheckScope ())
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                var newVal = EditorGUILayout.Popup (new GUIContent (text, tooltip), prop.enumValueIndex, enumStr);
                EditorGUI.showMixedValue = false;
                if (check.changed)
                    prop.enumValueIndex = newVal;
            }
        }

        private bool isExpand (SerializedProperty prop, bool isDefaultOpen, GUIContent label)
        {
            prop.isExpanded = EditorGUILayout.Foldout (prop.isExpanded != isDefaultOpen, label) != isDefaultOpen;
            return prop.isExpanded != isDefaultOpen;
        }

        private class IndentScope : GUI.Scope
        {
            private float _spaceBtm;
            private int _indent;
            internal IndentScope (float spaceTop = 3f, float spaceBtm = 10f, int indent = 1)
            {
                _spaceBtm = spaceBtm;
                _indent = indent;
                EditorGUILayout.Space (spaceTop);
                EditorGUI.indentLevel += indent;
            }
            protected override void CloseScope ()
            {
                EditorGUI.indentLevel -= _indent;
                EditorGUILayout.Space (_spaceBtm);
            }
        }


        private class DisableScope : GUI.Scope
        {
            private readonly bool _tmp;
            internal DisableScope (bool isActive = false)
            {
                _tmp = isActive;
                if (!_tmp) EditorGUI.BeginDisabledGroup (true);
            }
            protected override void CloseScope ()
            {
                if (!_tmp) EditorGUI.EndDisabledGroup ();
            }
        }

        private class ColorScope : GUI.Scope
        {
            private readonly Color _tmp;
            internal ColorScope (Color color)
            {
                _tmp = GUI.color;
                GUI.color = color;
            }
            protected override void CloseScope ()
            {
                GUI.color = _tmp;
            }
        }

        // ----------------------------------------------------------- URP
        private bool isDark => EditorGUIUtility.isProSkin;
        private GUIStyle _textStyle, _linkUrlStyle, _warningStyle;

        private void prepStyle ()
        {
            if (_textStyle == null) {
                _textStyle = new GUIStyle (EditorStyles.label);
                _textStyle.wordWrap = true;
                _textStyle.fontSize = 14;

                _linkUrlStyle = new GUIStyle (_textStyle);
                _linkUrlStyle.wordWrap = false;
                _linkUrlStyle.normal.textColor = isDark ? new Color (0f, 0.8f, 1f, 1f) : new Color (0f, 0.4f, 0.8f, 1f);
                _linkUrlStyle.hover.textColor = _linkUrlStyle.normal.textColor + Color.white * (isDark ? 0.3f : 0.2f);
                _linkUrlStyle.stretchWidth = false;

                _warningStyle = new GUIStyle (_textStyle);
                _warningStyle.normal.textColor = isDark ? Color.white : new Color (0.8f, 0.1f, 0f, 1f);
                _warningStyle.normal.background = Texture2D.whiteTexture;
            }
        }

        private void OnInspectorGUI_NotSettingYet ()
        {
            prepStyle ();
            storeIndentWidth ();
            EditorGUILayout.Space (50);
            labelWarning ($"UniversalRP setup is not completed.");
            EditorGUILayout.Space (10);
            GUILayout.Label ($"Please refer to the Unity manual page to setup, or create a new project with the URP 3D template.", _textStyle);
            EditorGUILayout.Space (50);
        }

        void labelWarning (string text, int fontSize = 18)
        {
            prepStyle ();
            _warningStyle.fontSize = fontSize;
            Color tmp = GUI.backgroundColor;
            GUI.backgroundColor = isDark ? new Color (1f, 0.3f, 0.6f, 0.4f) : new Color (1f, 0.3f, 0.6f, 0.5f);
            GUILayout.Label (text, _warningStyle);
            GUI.backgroundColor = tmp;
        }
        // -----------------------------------------------------------
    }
}

#endif // End of WCE_URP
