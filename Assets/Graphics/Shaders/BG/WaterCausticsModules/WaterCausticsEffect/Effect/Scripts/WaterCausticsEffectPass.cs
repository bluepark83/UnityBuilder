using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;

namespace WaterCausticsModules
{
    public class WaterCausticsEffectPass : ScriptableRenderPass
    {
        private Material _mat;

        private readonly ShaderTagId[] _shaderTagIds = new ShaderTagId[]
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward"),
        };

        private bool _useOpaqueTex;
        private LayerMask _layerMask;

        internal WaterCausticsEffectPass()
        {
            base.profilingSampler = new ProfilingSampler(nameof(WaterCausticsEffectPass));
        }

        internal void Setup(RenderPassEvent evt, Material mat, LayerMask layerMask, bool useOpaqueTex)
        {
            this.renderPassEvent = evt;
            _layerMask = layerMask;
            _mat = mat;
            _useOpaqueTex = useOpaqueTex;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData rendData)
        {
            if (_useOpaqueTex)
                ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData rendData)
        {
            if (!_mat) return;
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, base.profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                var desc = new RendererListDesc(_shaderTagIds, rendData.cullResults, rendData.cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.None,
                    renderQueueRange = RenderQueueRange.opaque,
                    layerMask = _layerMask,
                    rendererConfiguration = (PerObjectData)~0,
                    excludeObjectMotionVectors = true,
                    overrideMaterial = _mat,
                    overrideShaderPassIndex = 0,
                };
                var rendererList = context.CreateRendererList(desc);
                cmd.DrawRendererList(rendererList);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}