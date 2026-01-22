using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Perception.GroundTruth
{
    public class DepthRenderUrpPass : ScriptableRenderPass
    {
        const string k_ShaderName = "Perception/DepthRender";
        Shader m_DepthShader;
        Material m_OverrideMaterial;

        RTHandle m_TargetHandle;

        public DepthRenderUrpPass(RenderTexture targetTexture)
        {
            m_DepthShader = Shader.Find(k_ShaderName);
            m_OverrideMaterial = new Material(m_DepthShader);

            m_TargetHandle = RTHandles.Alloc(targetTexture);

            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var commandBuffer = CommandBufferPool.Get(nameof(DepthRenderUrpPass));

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            m_OverrideMaterial.SetFloat("_Near", renderingData.cameraData.camera.nearClipPlane);
            m_OverrideMaterial.SetFloat("_Far", renderingData.cameraData.camera.farClipPlane);

            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            commandBuffer.GetTemporaryRT(0, opaqueDesc, FilterMode.Point);
            commandBuffer.Blit(source, m_TargetHandle.nameID, m_OverrideMaterial, 0);

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}