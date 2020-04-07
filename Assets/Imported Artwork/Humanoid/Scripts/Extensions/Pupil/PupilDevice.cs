#if hPUPIL
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Pupil;

namespace Passer.Tracking.Pupil {

    public class Device {
        /// <summary>
        /// Device name
        /// </summary>
        public const string name = "Pupil Labs";

        public enum TrackingMode {
            _2D,
            _3D
        }
        private static TrackingMode trackingMode;

        /// <summary>
        /// Is the device present?
        /// </summary>
        public static bool present = false;

        public static bool blink = false;

        public delegate void OnStartedFunc();
        public static OnStartedFunc OnStarted;

        #region Start
        /// <summary>
        /// Start the device
        /// </summary>
        public static void Start(TrackingMode _trackingMode) {
            trackingMode = _trackingMode;
            PupilTools.OnConnected += OnConnected;
            PupilTools.OnDisconnecting += OnDisconnecting;

            //PupilTools.OnReceiveData += CustomReceiveData;
        }

        private static void OnConnected() {
            present = true;

            PupilGazeTracker.Instance.StartVisualizingGaze();
            //StartBlinkSubscription();

            OnStarted();
        }

        private static void StartBlinkSubscription() {

            PupilTools.Send(new Dictionary<string, object> {
                { "subject", "start_plugin" },
                { "name", "Blink_Detection" },
                { "args", new Dictionary<string,object> {
                        { "history_length", 0.2F },
                        { "onset_confidence_threshold", 0.5F },
                        { "offset_confidence_threshold", 0.5F }
                    }
                }
            });

            PupilTools.SubscribeTo("blinks");
        }

        #region Calibration
        public static void PrepareCalibration() {
            PupilTools.CalibrationMode = (Calibration.Mode)trackingMode;
            InitializeCalibrationPointPreview();
        }
        public static void StartCalibration() {
            PupilTools.OnCalibrationStarted += OnCalibrationStarted;
            PupilTools.OnCalibrationEnded += OnCalibrationEnded;
            PupilTools.OnCalibrationFailed += OnCalibrationFailed;

            PupilTools.StartCalibration();
        }

        private static void InitializeCalibrationPointPreview() {
            var type = PupilTools.CalibrationType;
            var camera = PupilSettings.Instance.currentCamera;
            camera.cullingMask = (1 << LayerMask.NameToLayer("Calibration")) | (1 << LayerMask.NameToLayer("UI"));
            camera.gameObject.layer = LayerMask.NameToLayer("Calibration");

            Vector3 centerPoint = PupilTools.CalibrationType.centerPoint;
            foreach (var vector in type.vectorDepthRadius) {
                Transform previewCircle = GameObject.Instantiate<Transform>(Resources.Load<Transform>("CalibrationPointExtendPreview"));
                previewCircle.parent = camera.transform;
                float scaleFactor = (centerPoint.x + vector.y) * 0.2f;
                if (PupilTools.CalibrationMode == Calibration.Mode._2D) {
                    centerPoint.z = type.vectorDepthRadius[0].x;
                    scaleFactor = camera.worldToCameraMatrix.MultiplyPoint3x4(camera.ViewportToWorldPoint(centerPoint + Vector3.right * vector.y)).x * 0.2f;
                    centerPoint = camera.worldToCameraMatrix.MultiplyPoint3x4(camera.ViewportToWorldPoint(centerPoint));
                }
                previewCircle.localScale = new Vector3(scaleFactor, scaleFactor / PupilSettings.Instance.currentCamera.aspect, 1);
                previewCircle.localPosition = new Vector3(centerPoint.x, centerPoint.y, vector.x);
                previewCircle.localEulerAngles = Vector3.zero;
            }
        }

        private static void OnCalibrationStarted() {
            Debug.Log("Calibration started");
        }

        private static void OnCalibrationEnded() {
            Debug.Log("Calibration ended");

            PupilSettings.Instance.currentCamera.gameObject.layer = LayerMask.NameToLayer("Default");
            PupilSettings.Instance.currentCamera.cullingMask = -1; // Everything

            PupilTools.OnCalibrationStarted -= OnCalibrationStarted;
            PupilTools.OnCalibrationEnded -= OnCalibrationEnded;
            PupilTools.OnCalibrationFailed -= OnCalibrationFailed;

            PupilGazeTracker.Instance.StartVisualizingGaze();
        }

        private static void OnCalibrationFailed() {
            Debug.Log("Calibration failed");

            PupilSettings.Instance.currentCamera.cullingMask = -1; // Everything

            PupilTools.OnCalibrationStarted -= OnCalibrationStarted;
            PupilTools.OnCalibrationEnded -= OnCalibrationEnded;
            PupilTools.OnCalibrationFailed -= OnCalibrationFailed;
        }
        #endregion
        #endregion

        #region Update
        /// <summary>
        /// Update the device
        /// </summary>
        public static void Update() {
        }

        private static void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null) {
            if (topic == "blinks") {
                if (dictionary.ContainsKey("timestamp")) {
                    Debug.Log("Blink detected: " + dictionary["timestamp"].ToString());
                }
            }
        }
        #endregion

        #region Stop
        /// <summary>
        /// Stop the device
        /// </summary>
        public static void Stop() {
            PupilTools.OnConnected -= OnConnected;
            PupilTools.OnDisconnecting -= OnDisconnecting;

            PupilTools.OnCalibrationStarted -= OnCalibrationStarted;
            PupilTools.OnCalibrationEnded -= OnCalibrationEnded;
            PupilTools.OnCalibrationFailed -= OnCalibrationFailed;

            //PupilTools.OnReceiveData -= CustomReceiveData;
        }

        private static void OnDisconnecting() {
            //StopBlinkSubscription();
        }

        private static void StopBlinkSubscription() {
            UnityEngine.Debug.Log("Disconnected");

            PupilTools.Send(new Dictionary<string, object> {
                { "subject","stop_plugin" },
                { "name", "Blink_Detection" }
            });

            PupilTools.UnSubscribeFrom("blinks");
        }
        #endregion

        public static bool GetGazePoint(out Vector3 gazePosition) {
            if (PupilTools.IsConnected && PupilTools.IsGazing) {
                if (trackingMode == TrackingMode._2D)
                    gazePosition = PupilData._2D.GazePosition;
                else
                    gazePosition = PupilData._3D.GazePosition;
                return (gazePosition.sqrMagnitude > 0);
            } else {
                gazePosition = new Vector3(0.5F, 0.5F, 2);
                return false;
            }
        }
    }
}
#endif