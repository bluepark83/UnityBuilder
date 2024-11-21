#if WCE_URP
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace WaterCausticsModules
{
    [DisallowMultipleRendererFeature ("WaterCausticsEffect (Renderer Feature)")]
    public class WaterCausticsEffectFeature : ScriptableRendererFeature
    {
        static private WaterCausticsEffectFeature s_ins;
        static public event Action<ScriptableRenderer, Camera> onEnqueue;
        static private int s_lastFrame;
        static internal bool effective => (s_ins != null && s_ins.isActive && s_lastFrame >= Time.renderedFrameCount - 1);
        static internal void OnAddedByScript () => s_lastFrame = Time.renderedFrameCount;
        public override void Create () { }
        public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData rendData) {
            s_ins = this;
            s_lastFrame = Time.renderedFrameCount;
            var cam = rendData.cameraData.camera;
            onEnqueue?.Invoke (renderer, cam);
        }
    }
}
#endif
