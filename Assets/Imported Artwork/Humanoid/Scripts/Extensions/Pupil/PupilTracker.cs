#if hPUPIL
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Humanoid.Tracking;
//using Pupil;

namespace Passer.Tracking.Pupil {

    [System.Serializable]
    public class Tracker : Passer.Tracker {
        /// <summary>
        /// Tracker name
        /// </summary>
        public override string name { get { return "Pupil Tracking"; } }

        /// <summary>
        /// Pupil Labs tracking mode or Detection and Mapping mode
        /// This will also determine which kine of calibration will be executed
        /// </summary>
        public Device.TrackingMode trackingMode;

        /// <summary>
        /// This will start calibration as soon as device is active
        /// </summary>
        public bool autoCalibration;

        #region Start
        /// <summary>
        /// Start the tracker
        /// </summary>
        /// <param name="_humanoid">The humanoid controlled by this tracker</param>
        public override void StartTracker(HumanoidControl _humanoid) {
            base.StartTracker(_humanoid);

            if (enabled)
                AddTracker(humanoid, name);


            PupilSettings.Instance.currentCamera = UnityVRHead.GetCamera(humanoid.headTarget);

            Device.OnStarted += OnStarted;
            Device.Start(trackingMode);
        }

        /// <summary>
        /// Will start calibration automatically when the Pupil connection has been completed
        /// </summary>
        private void OnStarted() {
            if (autoCalibration)
                Calibrate();
        }

        // Add the tracker and set the tracker transform at the default location
        private readonly Vector3 defaultTrackerPosition = new Vector3(0, 1.2F, 1);
        private readonly Quaternion defaultTrackerRotation = Quaternion.Euler(0, 180, 0);
        public override bool AddTracker(HumanoidControl humanoid, string resourceName) {
            bool trackerAdded = base.AddTracker(humanoid, resourceName);
            if (trackerAdded) {
                trackerTransform.transform.localPosition = defaultTrackerPosition;
                trackerTransform.transform.localRotation = defaultTrackerRotation;
            }
            return trackerAdded;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the tracker
        /// </summary>
        public override void UpdateTracker() {
            base.UpdateTracker();

            // Do nothing when the tracking is not enabled
            if (!enabled)
                return;

            // We assume the device is always tracking for now, will be improved later
            status = humanoid.headTarget.pupil.status;

            // Tell the device where it is located
            deviceView.position = Target.ToVector(trackerTransform.position);
            deviceView.orientation = Target.ToRotation(trackerTransform.rotation);
        }
        #endregion

        #region Stop
        public override void StopTracker() {
            base.StopTracker();

            Device.Stop();
        }
        #endregion

        #region Calibration
        private static FramePublishing trackingCameraImages;
        private bool calibrationPrepared = false;

        /// <summary>
        /// Eye Tracking Calibration.
        /// The first call Prepares the calibration, shows the calibration screen and camera images in the headset.
        /// Calling this function again will start the calibration process
        /// </summary>
        public override void Calibrate() {
            if (!Device.present)
                return;

            if (calibrationPrepared)
                StartCalibration();
            else
                PrepareCalibration();
        }

        /// <summary>
        /// Starts the calibration process
        /// Disables the tracking camera images
        /// </summary>
        private void StartCalibration() {
            Debug.Log("Start Calibration");

            // Disable the tracking camera images
            trackingCameraImages.enabled = false;

            // Start the calibration process
            Device.StartCalibration();

            // Reset the preparation so that the preparation will be started next time
            calibrationPrepared = false;
        }

        /// <summary>
        /// Prepare the calibration process.
        /// Show the calibration screen and the tracking camera images
        /// </summary>
        private void PrepareCalibration() {
            Debug.Log("Prepare Calibration");
            
            // Do we have the tracking camera image present already?
            if (trackingCameraImages != null)
                // The enable them
                trackingCameraImages.enabled = true;

            else {
                // We do not have the tracking camera images present yet.
                // Get the headset camera
                Camera camera = PupilSettings.Instance.currentCamera;

                if (camera != null && camera.gameObject != null) {
                    // Add the tracking camera images
                    trackingCameraImages = camera.gameObject.AddComponent<FramePublishing>();
                }
            }
            
            // Show the calibration screen
            Device.PrepareCalibration();

            calibrationPrepared = true;
        }
        #endregion
    }
}
#endif