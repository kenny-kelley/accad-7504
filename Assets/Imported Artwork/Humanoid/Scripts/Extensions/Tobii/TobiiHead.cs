#if hTOBII
using UnityEngine;
using Tobii.Gaming;

namespace Passer {
    [System.Serializable]
    public class TobiiHead : UnityHeadSensor {
        public override string name {
            get { return "Tobii"; }
        }

        public enum RotationTrackingAxis {
            XYZ,
            XY
        };

        public bool headTracking = true;
        public RotationTrackingAxis rotationTrackingAxis = RotationTrackingAxis.XY;
        public bool eyeTracking = true;

        private Camera fpCamera;

        public Vector2 gazePointScreen;

        public bool virtual3dTracking = false;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = headTarget.humanoid.tobiiTracker;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            fpCamera = Camera.main;//UnityVRHead.GetCamera(headTarget);
            if (fpCamera == null)
                Debug.LogError("Tobii Eye tracking requires a first person camera");
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (TobiiAPI.GetUserPresence() != UserPresence.Present) {
                status = Humanoid.Tracking.Status.Present;
                Debug.LogWarning("No user detected. Is Tobii Eye Tracking enabled in the system tray?");
                return;
            }

            status = Humanoid.Tracking.Status.Tracking;

            if (headTracking)
                UpdateBones();
            if (eyeTracking)
                UpdateEyes();

            if (virtual3dTracking) {
                Vector3 focusPoint = tracker.trackerTransform.position + Vector3.up * 0.2F; // 20 cm above the tobii tracker
                Vector3 lookDirection = focusPoint - fpCamera.transform.position;
                headTarget.neck.target.transform.rotation = tracker.trackerTransform.rotation * Quaternion.LookRotation(lookDirection);

                headTarget.neck.target.transform.position = new Vector3(-headTarget.neck.target.transform.position.x, headTarget.neck.target.transform.position.y, headTarget.humanoid.transform.position.z);

                Debug.DrawLine(fpCamera.transform.position, focusPoint, Color.cyan);
                Debug.DrawRay(fpCamera.transform.position, lookDirection, Color.magenta);
            }

        }

        #region Head
        Vector3 lastHeadPosition;
        Quaternion lastHeadRotation;

        public void UpdateBones() {
            HeadPose headPose = TobiiAPI.GetHeadPose();
            if (headPose.IsRecent()) {
                Vector3 headPosition = new Vector3(-headPose.Position.x, headPose.Position.y, headPose.Position.z);
                headPosition = SmoothPosition(lastHeadPosition, headPosition);
                lastHeadPosition = headPosition;

                headTarget.head.target.transform.position = tracker.ToWorldPosition(headPosition / 1000);
                headTarget.head.target.confidence.position = 0.8F;

                Vector3 headAngles = headPose.Rotation.eulerAngles;
                Quaternion headRotation = tracker.ToWorldOrientation(Quaternion.Euler(headAngles.x, headAngles.y + 180, headAngles.z));
                headRotation = SmoothRotation(lastHeadRotation, headRotation);
                lastHeadRotation = headRotation;

                switch (rotationTrackingAxis) {
                    case RotationTrackingAxis.XYZ:
                        headTarget.head.target.transform.rotation = headRotation;
                        headTarget.head.target.confidence.rotation = 0.8F;
                        break;
                    case RotationTrackingAxis.XY:
                        headRotation = Quaternion.LookRotation(headRotation * Vector3.forward);
                        headTarget.head.target.transform.rotation = headRotation;
                        headTarget.head.target.confidence.rotation = 0.8F;
                        break;
                    default:
                        break;
                }
            }
        }

        #region Smoothing
        public static Vector3 SmoothPosition(Vector3 lastTargetPosition, Vector3 sensorPosition) {
            // complementary filter
            return Vector3.Lerp(lastTargetPosition, sensorPosition, Time.deltaTime * 10);
        }

        public static Quaternion SmoothRotation(Quaternion lastTargetRotation, Quaternion sensorRotation) {
            // complementary filter
            return Quaternion.Slerp(lastTargetRotation, sensorRotation, Time.deltaTime * 5);
        }
        #endregion
        #endregion

        #region Face
        public void UpdateEyes() {
            if (fpCamera == null)
                return;
#if hFACE
            GazePoint gazePoint = TobiiAPI.GetGazePoint();
            if (gazePoint.IsRecent()) {
                gazePointScreen = gazePoint.Viewport;
                Vector3 gazePoint3 = new Vector3(gazePoint.Viewport.x, gazePoint.Viewport.y, 1);
                Vector3 lookDirection = fpCamera.ViewportPointToRay(gazePoint3).direction;
                lookDirection = Quaternion.Inverse(headTarget.transform.rotation) * headTarget.humanoid.transform.rotation * lookDirection;
                Debug.DrawRay(headTarget.transform.position, lookDirection.normalized, Color.magenta);
                headTarget.face.SetGazeDirection(lookDirection, 0.9F);
            }
#endif
        }
        #endregion
        #endregion
    }
}
#endif