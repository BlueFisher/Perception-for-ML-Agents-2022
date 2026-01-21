using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Perception.GroundTruth
{
    public class DepthRenderPerceptionCamera : PerceptionCamera
    {
        RenderTexture m_DepthRenderTexture;

        Camera m_Camera;
        int m_LastFrameEndRendering = -1;
        public event Action RenderedObjectInfosCalculated;
        public RenderTexture DepthRenderTexture => m_DepthRenderTexture;
        public Camera Camera { get { return m_Camera; } }

        DepthRenderUrpPass m_DepthPerceptionPass;

        public void Start()
        {
            Camera camera = GetComponent<Camera>();
            m_Camera = camera;

            var width = m_Camera.pixelWidth;
            var height = m_Camera.pixelHeight;

            m_DepthRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_DepthRenderTexture.Create();
            m_DepthRenderTexture.name = "DepthRenderTexture";

            if (TestRawImage != null)
            {
                TestRawImage.texture = m_DepthRenderTexture;
                m_TestRawImage = TestRawImage;
            }

            if (TestVisualElement != null)
            {
                TestVisualElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(m_DepthRenderTexture));
            }

            m_DepthPerceptionPass = new DepthRenderUrpPass(m_DepthRenderTexture);
            passes.Add(m_DepthPerceptionPass);

            RenderPipelineManager.endContextRendering += OnEndContextRendering;
        }

        void OnEndContextRendering(ScriptableRenderContext scriptableRenderContext, List<Camera> cameras)
        {
            if (Application.isPlaying)
            {
                if (EnableManuallyCapture && !ShouldManuallyCapture
                    || !EnableManuallyCapture && m_LastFrameEndRendering == Time.frameCount)
                    return;

                if (TestRawImage != null && TestRawImage != m_TestRawImage)
                {
                    TestRawImage.texture = m_DepthRenderTexture;
                    m_TestRawImage = TestRawImage;
                }

                ShouldManuallyCapture = false;
                m_LastFrameEndRendering = Time.frameCount;

                if (!cameras.Contains(m_Camera))
                    return;

                RenderedObjectInfosCalculated?.Invoke();
            }
        }
    }
}
