using Perception.GroundTruth;

using Unity.MLAgents.Sensors;

using UnityEngine;

namespace Perception
{
    public class DepthRenderTextureSensor : ISensor
    {
        readonly RenderTexture _renderTexture;
        readonly DepthRenderPerceptionCamera _perceptionCamera;

        // Use RenderTextureSensor as the base sensor, wrapped with PerceptionCamera
        readonly RenderTextureSensor _renderTextureSensor;

        /// <summary>
        /// Depth render sensor based on RenderTextureSensor
        /// </summary>
        /// <param name="width">Camera width</param>
        /// <param name="height">Camera height</param>
        /// <param name="perceptionCamera">Perception camera</param>
        /// <param name="name">Sensor name</param>
        /// <param name="compressionType">Compression method for the render texture</param>
        public DepthRenderTextureSensor(
            int width, int height,
            DepthRenderPerceptionCamera perceptionCamera,
            string name,
            SensorCompressionType compressionType)
        {
            _renderTexture = new RenderTexture(width, height, 8, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _perceptionCamera = perceptionCamera;

            _perceptionCamera.GetComponent<Camera>().targetTexture = RenderTexture.GetTemporary(width, height, 8, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _perceptionCamera.EnableManuallyCapture = true;
            _perceptionCamera.RenderedObjectInfosCalculated += RenderedObjectInfosCalculated;
            LabelManager.singleton.RegisterPendingLabels();

            _renderTextureSensor = new RenderTextureSensor(_renderTexture, true, name, compressionType);
        }

        private void RenderedObjectInfosCalculated()
        {
            var oriRenderTexture = RenderTexture.active;
            RenderTexture.active = _renderTexture;
            Graphics.Blit(_perceptionCamera.DepthRenderTexture, _renderTexture);
            RenderTexture.active = oriRenderTexture;
        }

        public RenderTexture RenderTexture
        {
            get { return _renderTexture; }
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

    public class DepthRenderComponent : SensorComponent
    {
        [Tooltip("Camera with DepthRenderPerceptionCamera component attached")]
        public DepthRenderPerceptionCamera DepthRenderPerceptionCamera;

        public string SensorName = "DepthRenderSensor";
        public SensorCompressionType Compression = SensorCompressionType.PNG;

        public Vector2Int ImageSize = new(200, 200);

        DepthRenderTextureSensor _sensor;

        public override ISensor[] CreateSensors()
        {
            if (_sensor != null)
                return new ISensor[] { _sensor };

            Dispose();

            _sensor = new DepthRenderTextureSensor(
                ImageSize.x, ImageSize.y,
                DepthRenderPerceptionCamera,
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
