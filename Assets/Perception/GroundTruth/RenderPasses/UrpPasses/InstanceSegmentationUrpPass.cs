using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Perception.GroundTruth
{
    public class InstanceSegmentationUrpPass : ScriptableRenderPass
    {
        InstanceSegmentationCrossPipelinePass m_InstanceSegmentationPass;

        public InstanceSegmentationUrpPass(Camera camera, RenderTexture targetTexture)
        {
            m_InstanceSegmentationPass = new InstanceSegmentationCrossPipelinePass(camera);
            ConfigureTarget(RTHandles.Alloc(targetTexture));
            m_InstanceSegmentationPass.Setup();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var commandBuffer = CommandBufferPool.Get(nameof(InstanceSegmentationUrpPass));
            m_InstanceSegmentationPass.Execute(context, commandBuffer, renderingData.cameraData.camera, renderingData.cullResults);
            CommandBufferPool.Release(commandBuffer);
        }

        public void Cleanup()
        {
            m_InstanceSegmentationPass.Cleanup();
        }
    }
}
