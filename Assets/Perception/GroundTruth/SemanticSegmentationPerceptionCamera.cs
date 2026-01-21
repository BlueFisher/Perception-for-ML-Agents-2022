using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Perception.GroundTruth
{
    
    public class SemanticSegmentationPerceptionCamera : PerceptionCamera
    {
        public SemanticSegmentationLabelConfig SemanticSegmentationLabelConfig;

        RenderTexture m_SemanticSegmentationTexture;

        Camera m_Camera;
        int m_LastFrameEndRendering = -1;
        public event Action RenderedObjectInfosCalculated;
        public RenderTexture SemanticSegmentationTexture => m_SemanticSegmentationTexture;
        public Camera Camera { get { return m_Camera; } }

        SemanticSegmentationUrpPass m_SemanticSegmentationPass;

        public void Start()
        {
            Camera camera = GetComponent<Camera>();
            m_Camera = camera;

            var width = m_Camera.pixelWidth;
            var height = m_Camera.pixelHeight;

            m_SemanticSegmentationTexture = new RenderTexture(width, height, 8, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_SemanticSegmentationTexture.Create();
            m_SemanticSegmentationTexture.name = "SemanticSegmentation";

            if (TestRawImage != null)
            {
                TestRawImage.texture = m_SemanticSegmentationTexture;
                m_TestRawImage = TestRawImage;
            }

            if (TestVisualElement != null)
            {
                TestVisualElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(m_SemanticSegmentationTexture));
            }

            m_SemanticSegmentationPass = new SemanticSegmentationUrpPass(m_Camera, m_SemanticSegmentationTexture, SemanticSegmentationLabelConfig);
            passes.Add(m_SemanticSegmentationPass);

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
                    TestRawImage.texture = m_SemanticSegmentationTexture;
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