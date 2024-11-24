#define FOG_ROTATION
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomVolumetricFog
{
    public enum VolumetricFogShape
    {
        Box,
        Sphere
    }

    [ExecuteInEditMode]
    public partial class VolumetricFog : MonoBehaviour
    {
        public VolumetricFogProfile profile;

        [Tooltip("Fog volume blending starts when reference controller is within this fade distance to any volume border.")]
        public float fadeDistance = 1;
        [Tooltip("메인 카메라의 위치")]
        public Transform fadeController;
        [Tooltip("볼륨의 범위를 나타냅니다.")]
        public bool showBoundary;

        // Volume의 범위 내에 들어왔는가?
        public bool IsInsideVolume = false;
        // Volume 내에 들어 왔을 때, Fog Duration
        private float fogFadeInDuration = 4f;
        private float fogFadeOutDuration = 1f;
        // Time
        private float fadeInTime = 0f;
        private float fadeOutTime = 0f;
        // Fade In/Out Percentage
        private float fadeInOutPercentage = 0f;

        [NonSerialized]
        public MeshRenderer meshRenderer;
        Material fogMat, noiseMat, turbulenceMat;
        Shader fogShader;
        RenderTexture rtNoise, rtTurbulence;
        float noiseRotationSpeed;
        Vector3 windAcum;

        List<string> shaderKeywords;
        Texture3D detailTex, refDetailTex;
        Mesh debugMesh;
        Material fogDebugMat;
        VolumetricFogProfile activeProfile, lerpProfile;
        Vector3 lastControllerPosition;
        float alphaMultiplier = 1f;

        bool profileIsInstanced;
        bool requireUpdateMaterial;
        Color ambientMultiplied;
        
        private void Awake()
        {
            // 초기 실행에 null로 초기화
            fadeController = null;
        }

        void OnEnable()
        {
            UpdateMaterialPropertiesNow();
        }

        private void OnDisable()
        {
            if (profile != null)
            {
                profile.onSettingsChanged -= UpdateMaterialProperties;
            }
        }

        private void OnValidate()
        {
            UpdateMaterialProperties();
        }

        private void OnDestroy()
        {
            if (rtNoise != null)
            {
                rtNoise.Release();
            }
            if (rtTurbulence != null)
            {
                rtTurbulence.Release();
            }
            if (fogMat != null)
            {
                DestroyImmediate(fogMat);
                fogMat = null;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 0, 0.75F);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
        
        // Time 계산
        private void Update()
        {
            if (activeProfile == null) return;
            if (IsInsideVolume)
            {
                if (fadeInTime <= activeProfile.fadeInOutTime)
                {
                    fadeInTime += Time.deltaTime;
                }

                if (fadeOutTime >= 0f)
                {
                    fadeOutTime -= Time.deltaTime;
                }
            }
            else
            {
                if (fadeOutTime <= activeProfile.fadeInOutTime)
                {
                    fadeOutTime += Time.deltaTime;
                }

                if (fadeInTime >= 0f)
                {
                    fadeInTime -= Time.deltaTime;
                }
            }
        }

        void LateUpdate()
        {
            
            if (fogMat == null || meshRenderer == null || profile == null) return;

            if (requireUpdateMaterial) {
                requireUpdateMaterial = false;
                UpdateMaterialPropertiesNow();
            }
            
            // Fog의 위치와 회전을 설정
#if FOG_ROTATION
            Matrix4x4 rot = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
            fogMat.SetMatrix(ShaderParams.RotationMatrix, rot);
            fogMat.SetMatrix(ShaderParams.RotationInvMatrix, rot.inverse);
#else
            transform.rotation = Quaternion.identity;
#endif

            Vector3 center = transform.position;
            // 볼륨의 Scale을 가져온다. (부모가 있을 경우, 부모까지 계산)
            Vector3 extents = transform.lossyScale * 0.5f;

            ComputeActiveProfile();
            ApplyProfileSettings();
            
            if (activeProfile.shape == VolumetricFogShape.Sphere)
            {
                Vector3 scale = transform.localScale;
                if (scale.z != scale.x)
                {
                    scale.z = scale.x;
                    transform.localScale = scale;
                    extents = transform.lossyScale * 0.5f;
                }
                extents.x *= extents.x;
            }

            Vector4 border = new Vector4(extents.x * activeProfile.border + 0.0001f, extents.x * (1f - activeProfile.border), extents.z * activeProfile.border + 0.0001f, extents.z * (1f - activeProfile.border));
            fogMat.SetVector(ShaderParams.BoundsCenter, center);
            fogMat.SetVector(ShaderParams.BoundsExtents, extents);
            fogMat.SetVector(ShaderParams.BoundsBorder, border);
            Vector4 boundsData = new Vector4(activeProfile.verticalOffset, center.y - extents.y, extents.y * 2f, 0);
            fogMat.SetVector(ShaderParams.BoundsData, boundsData);

            Color ambientColor = RenderSettings.ambientLight;
            float ambientIntensity = RenderSettings.ambientIntensity;
            ambientMultiplied = ambientColor * ambientIntensity;

            windAcum += activeProfile.windDirection * Time.deltaTime;
            windAcum.x %= 10000;
            windAcum.y %= 10000;
            windAcum.z %= 10000;
            fogMat.SetVector(ShaderParams.WindDirection, windAcum);

            UpdateNoise();
            
            // 외부 윤곽선 표시 (디버그용)
            if (showBoundary)
            {
                if (fogDebugMat == null)
                {
                    fogDebugMat = new Material(Shader.Find("Playwith/VolumetricFog/VolumeDebug"));
                }
                if (debugMesh == null)
                {
                    MeshFilter mf = GetComponent<MeshFilter>();
                    if (mf != null)
                    {
                        debugMesh = mf.sharedMesh;
                    }
                }
                Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Graphics.DrawMesh(debugMesh, m, fogDebugMat, 0);
            }
        }
        // Noise
        void UpdateNoise()
        {
            if (activeProfile == null) return;
            fogMat.SetFloat(ShaderParams.InsideBorder, activeProfile.insideBorder);
            fogMat.SetFloat(ShaderParams.InsideStrength, activeProfile.insideStrength);
            fogMat.SetFloat(ShaderParams.NoiseRotationSpeed, activeProfile.noiseRotationSpeed);
            fogMat.SetFloat(ShaderParams.NoiseStrength, activeProfile.noiseStrength);
            fogMat.SetFloat(ShaderParams.NoiseFinalMultiplier, activeProfile.noiseFinalMultiplier);

            Color textureBaseColor = activeProfile.albedo;
            fogMat.SetColor(ShaderParams.Color, textureBaseColor);
            fogMat.SetFloat(ShaderParams.Brightness, activeProfile.brightness);
        }


        public void UpdateMaterialProperties()
        {
            requireUpdateMaterial = true;
        }

        public void UpdateMaterialPropertiesNow()
        {
            if (gameObject == null || !gameObject.activeInHierarchy)
            {
                return;
            }

            fadeDistance = Mathf.Max(0.1f, fadeDistance);

            meshRenderer = GetComponent<MeshRenderer>();

            if (profile == null)
            {
                if (fogMat == null && meshRenderer != null)
                {
                    fogMat = new Material(Shader.Find("VolumetricFog2/Empty"));
                    fogMat.hideFlags = HideFlags.DontSave;
                    meshRenderer.sharedMaterial = fogMat;
                }
                return;
            }
            // Subscribe to profile changes
            profile.onSettingsChanged -= UpdateMaterialProperties;
            profile.onSettingsChanged += UpdateMaterialProperties;
            
            if (meshRenderer != null)
            {
                if (fogShader == null)
                {
                    fogShader = Shader.Find("Playwith/VolumetricFog/VolumetricFog2DURP");
                    if (fogShader == null) return;
                }
                if (fogMat == null || fogMat.shader != fogShader)
                {
                    fogMat = new Material(fogShader);
                    meshRenderer.sharedMaterial = fogMat;
                }
            }

            if (fogMat == null) return;

            profile.ValidateSettings();

            lastControllerPosition.x = float.MaxValue;
            activeProfile = profile;

            ComputeActiveProfile();
            ApplyProfileSettings();
        }

        void ComputeActiveProfile()
        {
            if (Application.isPlaying)
            {
                if (fadeController == null)
                {
                    Camera cam = Camera.main;
                    if (cam != null)
                    {
                        fadeController = Camera.main.transform;
                    }
                }
                if (fadeController != null && lastControllerPosition != fadeController.position)
                {
                    lastControllerPosition = fadeController.position;
                    activeProfile = profile;
                    alphaMultiplier = 1f;
                    
                    float t = ComputeVolumeFade(transform, fadeDistance);
                    alphaMultiplier *= t;
                            
                    if(alphaMultiplier >= 0.9)
                    {
                        IsInsideVolume = true;
                    }
                    else
                    {
                        IsInsideVolume = false;
                    }
                }
            }
            else
            {
                alphaMultiplier = 1f;
            }
            
            // Alpha 값 설정
            fogMat.SetFloat(ShaderParams.AlphaMultiplier, activeProfile.alphaMultiplier);

            if (activeProfile == null)
            {
                activeProfile = profile;
            }
        }
        
        // 볼륨 Fade 계산
        float ComputeVolumeFade(Transform transform, float fadeDistance)
        {
            Vector3 diff = transform.position - fadeController.position;
            diff.x = diff.x < 0 ? -diff.x : diff.x;
            diff.y = diff.y < 0 ? -diff.y : diff.y;
            diff.z = diff.z < 0 ? -diff.z : diff.z;
            Vector3 extents = transform.lossyScale * 0.5f;
            Vector3 gap = diff - extents;
            float maxDiff = gap.x > gap.y ? gap.x : gap.y;
            maxDiff = maxDiff > gap.z ? maxDiff : gap.z;
            fadeDistance += 0.0001f;
            float t = 1f - Mathf.Clamp01(maxDiff / fadeDistance);
            return t;
        }
        // 영역 내에 들어왔을 경우, 처리
        void ApplyProfileSettings()
        {
            meshRenderer.sortingOrder = activeProfile.sortingOrder;
            fogMat.renderQueue = activeProfile.renderQueue;
            float noiseScale = 0.1f / activeProfile.noiseScale;
            fogMat.SetFloat(ShaderParams.NoiseScale, noiseScale);
            fogMat.SetFloat(ShaderParams.NoiseHeight, activeProfile.noiseHeight);
            fogMat.SetVector(ShaderParams.RaymarchSettings, new Vector4(activeProfile.raymarchQuality, activeProfile.dithering * 0.01f, activeProfile.jittering, activeProfile.raymarchMinStep));
            if (!activeProfile.alwaysOn)
            {
                // Density의 값을 0 ~ 1 값으로 부드럽게 전환
                if (Application.isPlaying)
                {
                    if (IsInsideVolume)
                    {
                        // Mathf.SmoothStep을 사용하여 부드럽게 전환
                        float t = Mathf.Clamp01(fadeInTime / activeProfile.fadeInOutTime);
                        fadeInOutPercentage = Mathf.SmoothStep(0, 1, t);
                        fogMat.SetFloat(ShaderParams.Density, fadeInOutPercentage);
                    }
                    else
                    {
                        // 부드럽게 감소시키기 위해 다시 t를 계산하고
                        float t = Mathf.Clamp01(fadeOutTime / activeProfile.fadeInOutTime);
                        // 1에서 0으로 감소하는 SmoothStep 함수 사용
                        fadeInOutPercentage = Mathf.SmoothStep(1, 0, t);
                        fogMat.SetFloat(ShaderParams.Density, fadeInOutPercentage);
                    }
                }
                else
                {
                    fogMat.SetFloat(ShaderParams.Density, 1);
                }
            }
            else
            {
                fogMat.SetFloat(ShaderParams.Density, 1);
            }

            if (shaderKeywords == null)
            {
                shaderKeywords = new List<string>();
            } else
            {
                shaderKeywords.Clear();
            }
            if (activeProfile.shape == VolumetricFogShape.Box)
            {
                shaderKeywords.Add(ShaderParams.SKW_SHAPE_BOX);
            }
            else
            {
                shaderKeywords.Add(ShaderParams.SKW_SHAPE_SPHERE);
            }
            fogMat.shaderKeywords = shaderKeywords.ToArray();
        }
    }
}
