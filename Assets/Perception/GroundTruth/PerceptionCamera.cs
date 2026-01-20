using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UnityEngine.Perception.GroundTruth
{
    public class PerceptionCamera : MonoBehaviour
    {
        public bool ManuallyCapture = false;
        public bool ShouldCapture = true;
        protected RawImage m_TestRawImage;
        public RawImage TestRawImage;
        [HideInInspector]
        public VisualElement TestVisualElement;
        internal List<ScriptableRenderPass> passes = new List<ScriptableRenderPass>();
    }
}