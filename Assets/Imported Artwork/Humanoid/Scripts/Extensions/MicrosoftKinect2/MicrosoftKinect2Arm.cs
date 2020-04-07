#if hKINECT2
using UnityEngine;

namespace Passer {
    using Humanoid;
    using Humanoid.Tracking;

    [System.Serializable]
    public class Kinect2Arm : UnityArmSensor {

        public override string name {
            get { return NativeKinectDevice.name; }
        }

        private Kinect2Tracker kinectTracker;
        private KinectArm kinectArm;

        private SensorBone upperArmSensor;
        private SensorBone forearmSensor;
        private SensorBone handSensor;

        public bool handTracking = true;

        public override void Start(HumanoidControl humanoid, Transform targetTransform) {
            base.Start(humanoid, targetTransform);
            kinectTracker = handTarget.humanoid.kinectTracker;

            kinectArm = new KinectArm(handTarget.isLeft, kinectTracker.kinectDevice, handTracking);
            sensor = kinectArm;

            //if (kinectTracker.device == null)
            //    return;

            //upperArmSensor = kinectTracker.device.GetBone(0, handTarget.side, SideBone.UpperArm);
            //forearmSensor = kinectTracker.device.GetBone(0, handTarget.side, SideBone.Forearm);
            //handSensor = kinectTracker.device.GetBone(0, handTarget.side, SideBone.Hand);
        }

        public override void Update() {
            if (!kinectTracker.enabled || !enabled) // || kinectTracker.device == null)
                return;

            status = kinectArm.Update();
            if (status != Status.Tracking)
                return;

            UpdateArm();
            if (handTracking)
                UpdateFingers(kinectArm);
        }

        private void UpdateArm() {
            UpdateUpperArm(kinectArm);
            UpdateForearm(kinectArm);
            UpdateHand(kinectArm);
        }

        protected Vector3 lastUpperArmPosition;
        protected Quaternion lastUpperArmRotation;
        protected override void UpdateUpperArm(ArmSensor armSensor) {
            armSensor.upperArm.position = Target.ToVector(Kinect2Tracker.SmoothPosition(lastUpperArmPosition, armSensor.upperArm.position));
            armSensor.upperArm.rotation = Target.ToRotation(Kinect2Tracker.SmoothRotation(lastUpperArmRotation, armSensor.upperArm.rotation));
            base.UpdateUpperArm(armSensor);

            lastUpperArmPosition = handTarget.upperArm.target.transform.position;
            lastUpperArmRotation = handTarget.upperArm.target.transform.rotation;
        }

        protected Vector3 lastForearmPosition;
        protected Quaternion lastForearmRotation;
        protected override void UpdateForearm(ArmSensor armSensor) {
            armSensor.forearm.position = Target.ToVector(Kinect2Tracker.SmoothPosition(lastForearmPosition, armSensor.forearm.position));
            armSensor.forearm.rotation = Target.ToRotation(Kinect2Tracker.SmoothRotation(lastForearmRotation, armSensor.forearm.rotation));
            base.UpdateForearm(armSensor);

            lastForearmPosition = handTarget.forearm.target.transform.position;
            lastForearmRotation = handTarget.forearm.target.transform.rotation;
        }

        protected Vector3 lastHandPosition;
        protected Quaternion lastHandRotation;
        protected override void UpdateHand(ArmSensor armSensor) {
            armSensor.hand.position = Target.ToVector(Kinect2Tracker.SmoothPosition(lastHandPosition, armSensor.hand.position));
            armSensor.hand.rotation = Target.ToRotation(Kinect2Tracker.SmoothRotation(lastHandRotation, armSensor.hand.rotation));
            base.UpdateHand(armSensor);

            lastHandPosition = handTarget.hand.target.transform.position;
            lastHandRotation = handTarget.hand.target.transform.rotation;
        }

        protected override void UpdateFingers(ArmSensor armSensor) {
            for (int i = 0; i < (int)Finger.Count; i++)
                handTarget.SetFingerCurl((Finger)i, armSensor.fingers[i].curl);
        }

        /*
        private void UpdateArm() {
            UpdateUpperArm(handTarget.upperArm.target);
            UpdateForearm(handTarget.forearm.target);
            UpdateHand(handTarget.hand.target);
        }

        protected void UpdateUpperArm(HumanoidTarget.TargetTransform upperArmTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, handTarget.side, SideBone.UpperArm);
            //if (confidence > 0) {
            //    handTarget.upperArm.target.transform.position = kinectTracker.device.GetBonePosition(0, handTarget.side, SideBone.UpperArm);
            //    handTarget.upperArm.target.confidence.position = confidence;
            //}
            float confidence = upperArmSensor.positionConfidence;
            if (confidence > 0) {
                upperArmTarget.transform.position = upperArmSensor.position;
                upperArmTarget.confidence.position = confidence;
            }
        }

        protected void UpdateForearm(HumanoidTarget.TargetTransform forearmTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, handTarget.side, SideBone.Forearm);
            //if (confidence > 0) {
            //    handTarget.forearm.target.transform.position = kinectTracker.device.GetBonePosition(0, handTarget.side, SideBone.Forearm);
            //    handTarget.forearm.target.confidence.position = confidence;
            //}
            float confidence = forearmSensor.positionConfidence;
            if (confidence > 0) {
                forearmTarget.transform.position = forearmSensor.position;
                forearmTarget.confidence.position = confidence;
            }
        }

        protected void UpdateHand(HumanoidTarget.TargetTransform handTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, handTarget.side, SideBone.Hand);
            //if (confidence > 0) {
            //    handTarget.hand.target.transform.position = kinectTracker.device.GetBonePosition(0, handTarget.side, SideBone.Hand);
            //    handTarget.hand.target.confidence.position = confidence;
            //}
            float confidence = handSensor.positionConfidence;
            if (confidence > 0) {
                handTarget.transform.position = handSensor.position;
                handTarget.confidence.position = confidence;
            }
        }
        
        protected void UpdateFingers() {
            KinectInterface.HandPose handPose = kinectTracker.device.GetHandPose(0, handTarget.isLeft);
            bool handLasso = (handPose == KinectInterface.HandPose.Lasso);
            bool handClosed = (handPose == KinectInterface.HandPose.Closed);

            handTarget.SetFingerCurl(Finger.Thumb, (handLasso || handClosed) ? 1 : 0);
            handTarget.SetFingerCurl(Finger.Index, handClosed ? 1 : 0);
            handTarget.SetFingerCurl(Finger.Middle, handClosed ? 1 : 0);
            handTarget.SetFingerCurl(Finger.Ring, (handLasso || handClosed) ? 1 : 0);
            handTarget.SetFingerCurl(Finger.Little, (handLasso || handClosed) ? 1 : 0);
        }
        */
    }
}
#endif