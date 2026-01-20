using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Perception.GroundTruth
{
    public class PerceptionRendererFeature : ScriptableRendererFeature
    {
        public override void Create() { }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var cameraObject = renderingData.cameraData.camera.gameObject;
            var perceptionCameras = cameraObject.GetComponents<PerceptionCamera>();

            if (perceptionCameras.Length == 0)
                return;

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                return;
#endif
            foreach (var camera in perceptionCameras)
                foreach (var pass in camera.passes)
                    renderer.EnqueuePass(pass);
        }
    }
}