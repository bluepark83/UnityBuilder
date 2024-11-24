#if UNITY_EDITOR && WCE_URP
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace WaterCausticsModules
{
    public class OptimizeShader : IPreprocessShaders {
        public int callbackOrder => 1;

        private readonly ShaderKeyword [] delKeys = {
            new ShaderKeyword ("WCE_DEBUG_NORMAL"),
            new ShaderKeyword ("WCE_DEBUG_DEPTH"),
            new ShaderKeyword ("WCE_DEBUG_FACING"),
            new ShaderKeyword ("WCE_DEBUG_CAUSTICS"),
            new ShaderKeyword ("WCE_DEBUG_AREA"),
#if !WCE_URP_12_0 // URP12
            new ShaderKeyword ("_GBUFFER_NORMALS_OCT"),
            new ShaderKeyword ("_LIGHT_LAYERS"),
            new ShaderKeyword ("_LIGHT_COOKIES"),
#endif
#if !WCE_URP_14_0 // URP14
            new ShaderKeyword ("_FORWARD_PLUS"),
#endif
        };

        private bool hasDelKey (ShaderKeywordSet set) {
            foreach (var key in delKeys)
                if (set.IsEnabled (key))
                    return true;
            return false;
        }

        public void OnProcessShader (Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data) {
            if (!shader.name.StartsWith (Constant.SHADER_NAME_HEADER)) return;
            for (var i = data.Count - 1; i >= 0; --i) {
                if (hasDelKey (data [i].shaderKeywordSet))
                    data.RemoveAt (i);
            }
        }

    }
}

#endif
