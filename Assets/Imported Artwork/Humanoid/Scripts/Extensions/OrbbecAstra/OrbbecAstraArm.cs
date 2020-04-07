#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_WSA_10_0)
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class AstraArm : UnityArmSensor {
        public override string name {
            get { return AstraDevice.name; }
        }

        private AstraTracker astraTracker;
        private SensorBone upperArmSensor;
        private SensorBone forearmSensor;
        public SensorBone handSensor;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            astraTracker = handTarget.humanoid.astra;
            tracker = astraTracker;

            if (astraTracker.device == null)
                return;

            Side side = handTarget.isLeft ? Side.Left : Side.Right;

            upperArmSensor = astraTracker.device.GetBone(0, side, SideBone.UpperArm);
            forearmSensor = astraTracker.device.GetBone(0, side, SideBone.Forearm);
            handSensor = astraTracker.device.GetBone(0, side, SideBone.Hand);
        }
        #endregion

        #region Update
        public override void Update() {
            status = Status.Unavailable;
            if (tracker == null ||
                    !tracker.enabled ||
                    !enabled ||
                    astraTracker.device == null ||
                    tracker.status == Status.Unavailable)
                return;

            status = Status.Present;
            if (handSensor.positionConfidence == 0)
                return;

            // We only check for right, because then the left has already been updated)
            if (!handTarget.isLeft) {
                if ((handTarget.hand.target.confidence.position >= handSensor.positionConfidence) &&
                        (handTarget.otherHand.hand.target.confidence.position >= handSensor.positionConfidence)) {
                    astraTracker.CalibrateWithHands(handSensor, handTarget.otherHand.astra.handSensor);
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

            Vector3 upperArmForward = forearmSensor.position -  upperArmSensor.position;//upperArmSensor.rotation * Vector3.forward; is not good enough
            Vector3 forearmForward = handSensor.position - forearmSensor.position; //forearmSensor.rotation * Vector3.forward; is not good enough
            Vector3 forearmUp = Vector3.Cross(upperArmForward, forearmForward);
            if (!handTarget.isLeft)
                forearmUp = -forearmUp;

            forearmTarget.transform.rotation = Quaternion.LookRotation(forearmForward, forearmUp);
            // forearmTarget.transform.rotation =  forearmSensor.rotation; is not good enough
            //forearmTarget.confidence.rotation = forearmSensor.rotationConfidence;
            // forearmSensor.rotationConfidence is not yet used
            forearmTarget.confidence.rotation = (upperArmSensor.positionConfidence + forearmSensor.positionConfidence) / 2 * 0.8F;
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