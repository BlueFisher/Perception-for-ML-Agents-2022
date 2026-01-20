using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DepthRender
{
    public class DepthRenderPass : ScriptableRenderPass
    {
        public Material DepthMat;
        // 使用第几个pass
        public int blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }
        private RenderTargetIdentifier source { get; set; }

        RTHandle m_temporaryColorTexture;

        string _profilerTag; // 专门给profiler看的名字
        public DepthRenderPass(string passname, RenderPassEvent _event, Material _mat)
        {
            _profilerTag = passname;
            renderPassEvent = _event;
            DepthMat = _mat;
        }

        public void Setup(RenderTargetIdentifier src)
        {
            source = src;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0; // Color and depth cannot be combined in RTHandles
            RenderingUtils.ReAllocateIfNeeded(ref m_temporaryColorTexture, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "temporaryColorTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            //不能读写同一个颜色target，创建一个临时的render Target去blit

            cmd.GetTemporaryRT(0, opaqueDesc, filterMode);
            cmd.Blit(source, m_temporaryColorTexture.nameID, DepthMat, blitShaderPassIndex);
            //因为destination和source是同一个target了，传给source就行？
            cmd.Blit(m_temporaryColorTexture.nameID, source);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(0);
        }
    }
}
