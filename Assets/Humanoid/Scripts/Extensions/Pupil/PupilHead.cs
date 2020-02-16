#if hPUPIL
using UnityEngine;
using Humanoid.Tracking;

namespace Passer.Tracking.Pupil {

    [System.Serializable]
    public class Head : UnityHeadSensor {
        /// <summary>
        /// Name of the head tracker
        /// </summary>
        public override string name { get { return "Pupil Labs"; } }

        #region Start
        /// <summary>
        /// Start head sensor
        /// For pupil labs, this is limited to eye tracking
        /// </summary>
        /// <param name="_humanoid">The humanoid controlled by this head sensor</param>
        /// <param name="targetTransform">The target transform of the head sensor</param>
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);

            // Get the tracker for this sensor
            tracker = headTarget.humanoid.pupil;

            // Only start this sensor when it is enabled
            if (tracker == null || !tracker.enabled || !enabled)
                return;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update head sensor
        /// </summary>
        public override void Update() {
            // Only update this sensor when it is enabled
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            status = Status.Unavailable;
            if (!Device.present)
                return;

            status = Status.Present;

            UpdateEyes();
        }

        public void UpdateEyes() {
            // Eye tracking requires Humanoid Face Control
#if hFACE
            if (Device.blink) {
                headTarget.face.leftEye.closed = 1;
                headTarget.face.rightEye.closed = 1;
            }

            Vector3 gazePosition;
            if (Device.GetGazePoint(out gazePosition)) {
                status = Status.Tracking;

                Vector3 lookDirection;
                if (headTarget.humanoid.pupil.trackingMode == Device.TrackingMode._2D)
                    lookDirection = Get2DLookDirection(gazePosition);
                else
                    lookDirection = gazePosition;

                Debug.DrawRay(headTarget.unityVRHead.camera.transform.position, lookDirection, Color.blue);
                headTarget.face.SetGazeDirection(lookDirection, 0.9F);
            }
#endif
        }
        
        private Vector3 Get2DLookDirection(Vector3 gazePosition) {
            Vector3 position;
            position.x = gazePosition.x;
            position.y = gazePosition.y;
            position.z = PupilTools.CalibrationType.vectorDepthRadius[0].x;
            Vector3 worldPosition = headTarget.unityVRHead.camera.ViewportToWorldPoint(position);

            Vector3 lookDirection = worldPosition - headTarget.unityVRHead.camera.transform.position;
            return lookDirection;
        }

        private Vector3 Get3DLookDirection(Vector3 gazePosition) {
            Vector3 worldPosition = headTarget.unityVRHead.camera.transform.TransformPoint(gazePosition);

            Vector3 lookDirection = worldPosition - headTarget.unityVRHead.camera.transform.position;
            return lookDirection;
        }

        #endregion
    }
}
#endif