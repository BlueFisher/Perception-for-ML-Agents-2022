using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DepthRender
{
    public class DepthRenderPassFeature : ScriptableRendererFeature
    {
        public Material DepthMat;

        DepthRenderPass _scriptablePass;

        public override void Create()
        {
            _scriptablePass = new DepthRenderPass("DepthRenderPass", RenderPassEvent.AfterRendering, DepthMat);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            var src = renderer.cameraColorTargetHandle;
            _scriptablePass.Setup(src);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (DepthMat == null)
            {
                Debug.LogWarningFormat("丢失blit材质");
                return;
            }
            renderer.EnqueuePass(_scriptablePass);
        }
    }
}