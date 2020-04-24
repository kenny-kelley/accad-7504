#if hKINECT1
using System;
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [Serializable]
    public class Kinect1Arm : UnityArmSensor {
        public override string name {
            get { return Kinect1Device.name; }
        }

        private Kinect1Tracker kinectTracker;
        private SensorBone upperArmSensor;
        private SensorBone forearmSensor;
        public SensorBone handSensor;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);

            tracker = kinectTracker = handTarget.humanoid.kinect1;

            if (kinectTracker.device == null)
                return;

            Side side = handTarget.isLeft ? Side.Left : Side.Right;

            upperArmSensor = kinectTracker.device.GetBone(0, side, SideBone.UpperArm);
            forearmSensor = kinectTracker.device.GetBone(0, side, SideBone.Forearm);
            handSensor = kinectTracker.device.GetBone(0, side, SideBone.Hand);
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
            if (handSensor.positionConfidence == 0)
                return;

            // We only check for right, because then the left has already been updated)
            if (!handTarget.isLeft) {
                if ((handTarget.hand.target.confidence.position >= handSensor.positionConfidence) &&
                        (handTarget.otherHand.hand.target.confidence.position >= handSensor.positionConfidence)) {
                    kinectTracker.CalibrateWithHands(handSensor, handTarget.otherHand.kinect1.handSensor);
                    return;
                }
            }

            UpdateUpperArm(handTarget.upperArm.target);
            UpdateForearm(handTarget.forearm.target);
            UpdateHand(handTarget.hand.target);

            status = Status.Tracking;
        }

        private void UpdateUpperArm(HumanoidTarget.TargetTransform upperArmTarget) {
            if (upperArmSensor.positionConfidence > upperArmTarget.confidence.position) {
                upperArmTarget.transform.position = upperArmSensor.position;
                upperArmTarget.confidence.position = upperArmSensor.positionConfidence;
            }

            if (upperArmSensor.rotationConfidence > upperArmTarget.confidence.rotation) {
                upperArmTarget.transform.rotation = upperArmSensor.rotation;
                upperArmTarget.confidence.rotation = 0.8F; // upperArmSensor.rotationConfidence;
            }
        }

        private void UpdateForearm(HumanoidTarget.TargetTransform forearmTarget) {
            if (forearmSensor.positionConfidence > forearmTarget.confidence.position) {
                forearmTarget.transform.position = forearmSensor.position;
                forearmTarget.confidence.position = forearmSensor.positionConfidence;
            }

            Vector3 upperArmForward = forearmSensor.position - upperArmSensor.position;
            Vector3 forearmForward = handSensor.position - forearmSensor.position;
            Vector3 forearmUp = Vector3.Cross(upperArmForward, forearmForward);
            if (!handTarget.isLeft)
                forearmUp = -forearmUp;

            forearmTarget.transform.rotation = Quaternion.LookRotation(forearmForward, forearmUp);
            //forearmTarget.transform.rotation =  forearmSensor.rotation; is not good enough
            forearmTarget.confidence.rotation = forearmSensor.rotationConfidence;
        }

        private void UpdateHand(HumanoidTarget.TargetTransform handTarget) {
            if (handSensor.positionConfidence > handTarget.confidence.position) {
                handTarget.transform.position = handSensor.position;
                handTarget.confidence.position = handSensor.positionConfidence;
            }
            if (handSensor.rotationConfidence > handTarget.confidence.rotation) {
                handTarget.transform.rotation = handSensor.rotation;
                handTarget.confidence.rotation = 0.8F; // handSensor.rotationConfidence;
            }
        }
        #endregion
    }
}
#endif