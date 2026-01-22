using System.Collections.Generic;
using System.Linq;

using Perception.GroundTruth;

using Unity.MLAgents.Sensors;

using UnityEngine;

namespace Perception
{
    public class BoundingBoxPerceptionSensor : ISensor
    {
        int _numberObjectType;

        int _width;
        int _height;
        BoundingBoxPerceptionCamera _perceptionCamera;
        float _random;

        // Use BufferSensor as the base sensor, wrapped with PerceptionCamera
        BufferSensor _bufferSensor;

        /// <summary>
        /// BoundingBox sensor based on BufferSensor
        /// </summary>
        /// <param name="maxNumberObject">Maximum number of objects that can be detected</param>
        /// <param name="numberObjectType">Maximum number of object types</param>
        /// <param name="width">Camera width</param>
        /// <param name="height">Camera height</param>
        /// <param name="perceptionCamera">Perception camera</param>
        /// <param name="random">Perturbation for detected object bbox</param>
        /// <param name="name">Sensor name</param>
        public BoundingBoxPerceptionSensor(
            int maxNumberObject,
            int numberObjectType,

            int width, int height,
            BoundingBoxPerceptionCamera perceptionCamera,
            float random,
            string name)
        {
            _numberObjectType = numberObjectType;

            _width = width;
            _height = height;
            _perceptionCamera = perceptionCamera;
            _random = random;

            _perceptionCamera.GetComponent<Camera>().targetTexture = RenderTexture.GetTemporary(width, height, 24); // Force rendering at [width height]
            _perceptionCamera.EnableManuallyCapture = true;
            _perceptionCamera.RenderedObjectInfosCalculated += RenderedObjectInfosCalculated;
            LabelManager.singleton.RegisterPendingLabels();

            if (_perceptionCamera.IdLabelConfig.labelEntries.Count() != numberObjectType)
            {
                Debug.LogWarning($"Number Object Type {numberObjectType} does not match Labels Count {_perceptionCamera.IdLabelConfig.labelEntries.Count()}");
            }
            if (!_perceptionCamera.IdLabelConfig.autoAssignIds)
            {
                Debug.LogWarning("Please enable autoAssignIds option in IdLabelConfig, otherwise it may cause category id confusion");
            }
            if (_perceptionCamera.IdLabelConfig.startingLabelId != 0)
            {
                Debug.LogWarning("Please set startingLabelId to 0 in IdLabelConfig, otherwise it may cause category id confusion");
            }

            _bufferSensor = new BufferSensor(maxNumberObject, numberObjectType + 4, name);
        }

        private void RenderedObjectInfosCalculated(List<BoundingBoxInfo> boundingBoxInfos)
        {
            foreach (var boundingBoxInfo in boundingBoxInfos)
            {
                if (Random.value < _random) // Randomly remove some bboxes
                    continue;

                var boundingBox = boundingBoxInfo.boundingBox;
                var x = boundingBox.x / _width + Random.Range(0, _random);
                var y = boundingBox.y / _height + Random.Range(0, _random);
                var xMax = boundingBox.xMax / _width + Random.Range(0, _random);
                var yMax = boundingBox.yMax / _height + Random.Range(0, _random);

                x = Mathf.Clamp(x, 0, 1);
                y = Mathf.Clamp(y, 0, 1);
                xMax = Mathf.Clamp(xMax, 0, 1);
                yMax = Mathf.Clamp(yMax, 0, 1);

                var obs = new float[_numberObjectType + 4];
                obs[boundingBoxInfo.labelId] = 1;

                // Origin (0,0) at TOP-LEFT corner of image, x-axis horizontal, y-axis vertical, record bbox CENTER POINT and width/height
                // Center point range [0, 1], width/height range [0, 1]
                obs[_numberObjectType + 0] = (xMax - x) / 2 + x;
                obs[_numberObjectType + 1] = (yMax - y) / 2 + y;
                obs[_numberObjectType + 2] = xMax - x;
                obs[_numberObjectType + 3] = yMax - y;

                _bufferSensor.AppendObservation(obs);
            }
        }

        public void SetRandom(float random)
        {
            _random = random;
        }

        /// <inheritdoc/>
        public string GetName()
        {
            return _bufferSensor.GetName();
        }

        /// <inheritdoc/>
        public ObservationSpec GetObservationSpec()
        {
            return _bufferSensor.GetObservationSpec();
        }

        /// <inheritdoc/>
        public byte[] GetCompressedObservation()
        {
            return _bufferSensor.GetCompressedObservation();
        }

        /// <inheritdoc/>
        public int Write(ObservationWriter writer)
        {
            return _bufferSensor.Write(writer);
        }

        /// <inheritdoc/>
        public void Update()
        {
            _bufferSensor.Update();

            LabelManager.singleton.RegisterPendingLabels();

            _perceptionCamera.ShouldManuallyCapture = true;
            _perceptionCamera.GetComponent<Camera>().Render();
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _bufferSensor.Reset();
        }

        /// <inheritdoc/>
        public CompressionSpec GetCompressionSpec()
        {
            return _bufferSensor.GetCompressionSpec();
        }

        public void Dispose()
        {
            _perceptionCamera.RenderedObjectInfosCalculated -= RenderedObjectInfosCalculated;
        }
    }

    [System.Serializable]
    public struct LabelColor
    {
        public string Label;
        public Color Color;
    }

    public class BoundingBoxComponent : SensorComponent
    {
        [Tooltip("Camera with BoundingBoxPerceptionCamera attached")]
        public BoundingBoxPerceptionCamera BoundingBoxPerceptionCamera;

        [Tooltip("Maximum number of objects that can be detected")]
        public int MaxNumberObject;

        [Tooltip("Maximum number of object types")]
        public int NumberObjectType;

        [SerializeField, Range(0f, 1f)]
        private float _random = 0;
        public float Random
        {
            get => _random;
            set { _random = value; _sensor?.SetRandom(_random); }
        }

        public string SensorName = "BoundingBoxSensor";

        public Vector2Int ImageSize = new Vector2Int(200, 200);

        BoundingBoxPerceptionSensor _sensor;

        public override ISensor[] CreateSensors()
        {
            if (_sensor != null)
                return new ISensor[] { _sensor };

            Dispose();

            _sensor = new BoundingBoxPerceptionSensor(
                MaxNumberObject,
                NumberObjectType,

                ImageSize.x, ImageSize.y,
                BoundingBoxPerceptionCamera,
                Random,
                SensorName);

            return new ISensor[] { _sensor };
        }

        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null;
        }
    }
}
