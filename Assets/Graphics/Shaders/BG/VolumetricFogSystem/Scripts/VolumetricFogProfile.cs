using UnityEngine;
using UnityEngine.Serialization;

namespace CustomVolumetricFog{

    public delegate void OnSettingsChanged();

    [CreateAssetMenu(menuName = "Volumetric Fog/Fog Profile", fileName = "VolumetricFogProfile", order = 1001)]
    public class VolumetricFogProfile : ScriptableObject
    {

        [Header("Rendering")] 
        [Range(1, 16)] public int raymarchQuality = 4;
        public float raymarchMinStep = 0.1f;
        public float jittering = 0.5f;
        [Range(0, 2)] public float dithering = 1f;
        public int renderQueue = 3100;
        public int sortingOrder;

        [Header("Density")]
        [Range(0, 3)] public float noiseStrength = 1f;
        public float noiseScale = 15f;
        public float noiseHeight = 1f;
        public float noiseFinalMultiplier = 1f;

        [Header("Duration")]
        public float fadeInOutTime = 5f;
        
        [Header("Always Visible")]
        public bool alwaysOn = false;

        [Header("Geometry")]
        public VolumetricFogShape shape = VolumetricFogShape.Box;
        [Range(0, 1f)] public float border = 0.05f;
        public float verticalOffset;
        public float insideBorder = 1;
        public float insideStrength = 1;

        [Header("Colors")]
        public Color albedo = new Color32(227, 227, 227, 255);
        [Range(0, 1)] public float alphaMultiplier = 1f;
        public float brightness = 1f;
        
        [Header("Animation")]
        public float noiseRotationSpeed = 0.73f;
        public Vector3 windDirection = new Vector3(0.02f, 0, 0);

        [Header("Directional Light")]
        [Tooltip("Ambient light influence")]
        public float ambientLightMultiplier;

        public event OnSettingsChanged onSettingsChanged;

        private void OnEnable() {
            // if (noiseTexture == null) {
            //     noiseTexture = Resources.Load<Texture2D>("Textures/NoiseTex256");
            // }
            ValidateSettings();
        }

        private void OnValidate() {
            ValidateSettings();
            if (onSettingsChanged != null) {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () => onSettingsChanged();
#endif
            }
        }

        public void ValidateSettings() {
            noiseScale = Mathf.Max(0.1f, noiseScale);
            noiseFinalMultiplier = Mathf.Max(0, noiseFinalMultiplier);
            ambientLightMultiplier = Mathf.Max(0, ambientLightMultiplier);
            
            fadeInOutTime = Mathf.Max(0, fadeInOutTime);
        }
    }
}

