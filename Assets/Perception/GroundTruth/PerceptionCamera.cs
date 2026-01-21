using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Perception.GroundTruth
{   
    [RequireComponent(typeof(Camera))]
    public class PerceptionCamera : MonoBehaviour
    {
        public bool EnableManuallyCapture = false;
        [HideInInspector]
        public bool ShouldManuallyCapture = true;
        protected RawImage m_TestRawImage;
        public RawImage TestRawImage;
        [HideInInspector]
        public VisualElement TestVisualElement;
        internal List<ScriptableRenderPass> passes = new List<ScriptableRenderPass>();
    }
}