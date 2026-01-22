using System.Collections.Generic;
using System.Linq;

using Perception.GroundTruth;

using Unity.MLAgents.Sensors;

using UnityEngine;

namespace Perception
{
    public class SemanticSegmentationPerceptionRenderTextureSensor : ISensor
    {
        RenderTexture _renderTexture;
        SemanticSegmentationPerceptionCamera _perceptionCamera;
        float _random;

        // Use RenderTextureSensor as the base sensor, wrapped with PerceptionCamera
        RenderTextureSensor _renderTextureSensor;

        /// <summary>
        /// Semantic Segmentation sensor based on RenderTextureSensor
        /// </summary>
        /// <param name="width">Camera width</param>
        /// <param name="height">Camera height</param>
        /// <param name="perceptionCamera">Perception camera</param>
        /// <param name="random">Perturbation for detected object bbox</param>
        /// <param name="name">Sensor name</param>
        /// <param name="compressionType">Compression method for the render texture</param>
        public SemanticSegmentationPerceptionRenderTextureSensor(
            int width, int height,
            SemanticSegmentationPerceptionCamera perceptionCamera,
            float random,
            string name,
            SensorCompressionType compressionType)
        {
            _renderTexture = new(width, height, 8, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _perceptionCamera = perceptionCamera;
            _random = random;

            _perceptionCamera.GetComponent<Camera>().targetTexture = RenderTexture.GetTemporary(width, height, 8, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _perceptionCamera.EnableManuallyCapture = true;
            _perceptionCamera.RenderedObjectInfosCalculated += RenderedObjectInfosCalculated;
            LabelManager.singleton.RegisterPendingLabels();

            _renderTextureSensor = new RenderTextureSensor(_renderTexture, false, name, compressionType);
        }

        private void RenderedObjectInfosCalculated()
        {
            var oriRenderTexture = RenderTexture.active;
            RenderTexture.active = _renderTexture;
            Graphics.Blit(_perceptionCamera.SemanticSegmentationTexture, _renderTexture);
            RenderTexture.active = oriRenderTexture;
        }

        public RenderTexture RenderTexture
        {
            get { return _renderTexture; }
        }

        public void SetRandom(float random)
        {
            _random = random;
        }

        /// <inheritdoc/>
        public string GetName()
        {
            return _renderTextureSensor.GetName();
        }

        /// <inheritdoc/>
        public ObservationSpec GetObservationSpec()
        {
            return _renderTextureSensor.GetObservationSpec();
        }

        /// <inheritdoc/>
        public byte[] GetCompressedObservation()
        {
            return _renderTextureSensor.GetCompressedObservation();
        }

        /// <inheritdoc/>
        public int Write(ObservationWriter writer)
        {
            return _renderTextureSensor.Write(writer);
        }

        /// <inheritdoc/>
        public void Update()
        {
            LabelManager.singleton.RegisterPendingLabels();

            var registeredLabels = LabelManager.singleton.registeredLabels.ToList();
            List<Vector3> tmpPosition = new(registeredLabels.Count);

            if (_random != 0)
            {
                registeredLabels.ForEach(label =>
                            {
                                tmpPosition.Add(label.transform.position);
                                Vector3 randomDelta = new(Random.Range(-_random, _random),
                                                            Random.Range(-_random, _random),
                                                            Random.Range(-_random, _random));
                                label.transform.position += randomDelta;
                            });
            }

            _perceptionCamera.ShouldManuallyCapture = true;
            _perceptionCamera.GetComponent<Camera>().Render();

            if (_random != 0)
            {
                for (int i = 0; i < registeredLabels.Count; i++)
                {
                    var label = registeredLabels[i];
                    label.transform.position = tmpPosition[i];
                }
            }
        }

        /// <inheritdoc/>
        public void Reset()
        {
        }

        /// <inheritdoc/>
        public CompressionSpec GetCompressionSpec()
        {
            return _renderTextureSensor.GetCompressionSpec();
        }

        public void Dispose()
        {
            _perceptionCamera.RenderedObjectInfosCalculated -= RenderedObjectInfosCalculated;
        }
    }

    public class SemanticSegmentationComponent : SensorComponent
    {
        [Tooltip("Camera with SemanticSegmentationPerceptionCamera component attached")]
        public SemanticSegmentationPerceptionCamera SemanticSegmentationPerceptionCamera;

        [SerializeField]
        private float _random = 0;
        public float Random
        {
            get => _random;
            set { _random = value; _sensor?.SetRandom(_random); }
        }

        public string SensorName = "SegmentationSensor";
        public SensorCompressionType Compression = SensorCompressionType.PNG;

        public Vector2Int ImageSize = new Vector2Int(200, 200);

        SemanticSegmentationPerceptionRenderTextureSensor _sensor;

        public override ISensor[] CreateSensors()
        {
            if (_sensor != null)
                return new ISensor[] { _sensor };

            Dispose();

            _sensor = new(
                ImageSize.x, ImageSize.y,
                SemanticSegmentationPerceptionCamera,
                Random,
                SensorName,
                Compression);

            return new ISensor[] { _sensor };
        }

        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null;
        }
    }
}
