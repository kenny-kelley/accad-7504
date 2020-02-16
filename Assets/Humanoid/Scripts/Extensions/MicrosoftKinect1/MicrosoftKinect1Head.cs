#if hKINECT1
using System;
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [Serializable]
    public class Kinect1Head : UnityHeadSensor {
        public override string name {
            get { return Kinect1Device.name; }
        }

        private Kinect1Tracker kinectTracker;
        private SensorBone neckSensor;
        private SensorBone headSensor;

        public bool headTracking = true;
        public enum RotationTrackingAxis {
            XYZ,
            XY
        }
        public RotationTrackingAxis rotationTrackingAxis = RotationTrackingAxis.XY;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);

            kinectTracker = headTarget.humanoid.kinect1;
            tracker = kinectTracker;

            if (kinectTracker.device == null)
                return;

            neckSensor = kinectTracker.device.GetBone(0, Bone.Neck);
            headSensor = kinectTracker.device.GetBone(0, Bone.Head);
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
            if (headSensor.positionConfidence == 0)
                return;

            if (headTracking) 
                UpdateBones();

            status = Status.Tracking;
        }

        protected void UpdateBones() {
            if (headTarget.head.target.confidence.position > headSensor.positionConfidence) {
                if (headTarget.humanoid.leftHandTarget.hand.target.confidence.position > kinectTracker.device.GetBoneConfidence(0, Side.Left, SideBone.Hand) &&
                    headTarget.humanoid.rightHandTarget.hand.target.confidence.position > kinectTracker.device.GetBoneConfidence(0, Side.Right, SideBone.Hand))
                    kinectTracker.CalibrateWithHeadAndHands(headSensor, headTarget.humanoid.leftHandTarget.kinect1.handSensor, headTarget.humanoid.rightHandTarget.kinect1.handSensor);
                else
                    kinectTracker.CalibrateWithHead(headSensor);
                return;
            }

            UpdateNeck(headTarget.neck.target);
            UpdateHead(headTarget.head.target);
        }

        private void UpdateNeck(HumanoidTarget.TargetTransform neckTarget) {
            neckTarget.confidence.position = neckSensor.positionConfidence;
            if (neckTarget.confidence.position > 0)
                neckTarget.transform.position = neckSensor.position;
        }

        private void UpdateHead(HumanoidTarget.TargetTransform headTarget) {
            headTarget.confidence.position = headSensor.positionConfidence;
            if (headTarget.confidence.position > 0)
                headTarget.transform.position = headSensor.position;

            if (rotationTrackingAxis == RotationTrackingAxis.XY)
                headTarget.transform.rotation = Quaternion.LookRotation(headTarget.transform.rotation * Vector3.forward);
        }

        #endregion

    }
}
#endif