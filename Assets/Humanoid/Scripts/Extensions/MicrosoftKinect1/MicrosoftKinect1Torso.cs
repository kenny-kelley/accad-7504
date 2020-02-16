#if hKINECT1
using System;
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;
    
    [Serializable]
    public class Kinect1Torso : UnityTorsoSensor {
        public override string name {
            get { return Kinect1Device.name; }
        }

        private Kinect1Tracker kinectTracker;
        private SensorBone hipsSensor;
        private SensorBone spineSensor;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);

            kinectTracker = hipsTarget.humanoid.kinect1;
            tracker = kinectTracker;

            if (kinectTracker.device == null)
                return;

            hipsSensor = kinectTracker.device.GetBone(0, Bone.Hips);
            spineSensor = kinectTracker.device.GetBone(0, Bone.Spine);
        }
        #endregion

        #region Update
        public override void Update() {
            status = Status.Unavailable;
            if (tracker == null ||
                !tracker.enabled ||
                !enabled ||
                kinectTracker.device == null ||
                tracker.status == Status.Unavailable)
                return;

            status = Status.Present;
            if (hipsSensor.positionConfidence == 0)
                return;

            UpdateHips(hipsTarget.hips.target);
            UpdateSpine(hipsTarget.spine.target);

            status = Status.Tracking;
        }

        private void UpdateHips(HumanoidTarget.TargetTransform hipsTarget) {
            hipsTarget.confidence.position = hipsSensor.positionConfidence;
            if (hipsTarget.confidence.position > 0)
                hipsTarget.transform.position = hipsSensor.position;
        }

        private void UpdateSpine(HumanoidTarget.TargetTransform spineTarget) {
            spineTarget.confidence.position = spineSensor.positionConfidence;
            if (spineTarget.confidence.position > 0)
                spineTarget.transform.position = spineSensor.position;
        }
        #endregion
    }
}
#endif