using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;

namespace CustomVolumetricFog {
    
    // Volumetric Fog Profile 관리
    [CustomEditor(typeof(VolumetricFog))]
    public partial class VolumetricFogEditor : Editor {

        VolumetricFogProfile cachedProfile;
        Editor cachedProfileEditor;
        SerializedProperty profile;
        SerializedProperty showBoundary;

        static GUIStyle boxStyle;

        void OnEnable() {
            profile = serializedObject.FindProperty("profile");
            showBoundary = serializedObject.FindProperty("showBoundary");
        }


        public override void OnInspectorGUI() {

            var pipe = GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (pipe == null) {
                EditorGUILayout.HelpBox("Universal Rendering Pipeline asset is not set in Project Settings / Graphics !", MessageType.Error);
                return;
            }

            if (!pipe.supportsCameraDepthTexture) {
                EditorGUILayout.HelpBox("Depth Texture option is required in Universal Rendering Pipeline asset!", MessageType.Error);
                if (GUILayout.Button("Go to Universal Rendering Pipeline Asset")) {
                    Selection.activeObject = pipe;
                }
                EditorGUILayout.Separator();
                GUI.enabled = false;
            }

            if (boxStyle == null) {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.padding = new RectOffset(15, 10, 5, 5);
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(profile);

            if (profile.objectReferenceValue != null) {
                if (cachedProfile != profile.objectReferenceValue) {
                    cachedProfile = null;
                }
                if (cachedProfile == null) {
                    cachedProfile = (VolumetricFogProfile)profile.objectReferenceValue;
                    cachedProfileEditor = CreateEditor(profile.objectReferenceValue);
                }

                // Drawing the profile editor
                EditorGUILayout.BeginVertical(boxStyle);
                cachedProfileEditor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            } else {
                EditorGUILayout.HelpBox("Create or assign a fog profile.", MessageType.Info);
                if (GUILayout.Button("New Fog Profile")) {
                    CreateFogProfile();
                }
            }

            EditorGUILayout.PropertyField(showBoundary);
            EditorGUILayout.Separator();

            serializedObject.ApplyModifiedProperties();
        }

        void CreateFogProfile()
        {
            float brightness = 1f;
            Light[] lights = FindObjectsOfType<Light>();
            if (lights != null) {
                foreach (Light light in lights) {
                    if (light.type == LightType.Directional) {
                        brightness /= light.intensity;
                        break;
                    }
                }
            }

            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets)) {
                path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path)) {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }
            VolumetricFogProfile fp = CreateInstance<VolumetricFogProfile>();
            fp.name = "New Volumetric Fog Profile";
            fp.brightness = brightness;
            AssetDatabase.CreateAsset(fp, path + "/" + fp.name + ".asset");
            AssetDatabase.SaveAssets();
            profile.objectReferenceValue = fp;
            EditorGUIUtility.PingObject(fp);
        }
    }
}