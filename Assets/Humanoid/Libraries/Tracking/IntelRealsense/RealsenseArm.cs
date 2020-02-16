/*
namespace Humanoid.Tracking.Realsense {
    public class RealsenseArm : ArmSensor {
        private readonly bool handTracking;

        public RealsenseArm(bool _isLeft, DeviceView device, bool _handTracking) : base(_isLeft, device) {
            handTracking = _handTracking;
        }

        public override Status Update() {
            if (RealsenseDevice.GetHandTargetConfidence(isLeft) == 0) {
                status = RealsenseDevice.present ? Status.Present : Status.Unavailable;
                return status;
            }

            status = Status.Tracking;

            UpdateBones();
            return status;
        }

        private void UpdateBones() {
            if (handTracking)
                UpdateHand();
        }

        protected override void UpdateHand() {
            Vector localHandPosition = RealsenseDevice.GetHandTargetLocalPosition(PXCMHandData.JointType.JOINT_WRIST, isLeft);
            hand.position = device.ToWorldPosition(localHandPosition);
            hand.confidence.position = RealsenseDevice.GetHandTargetConfidence(isLeft);

            Rotation localNeckRotation = RealsenseDevice.GetHandTargetLocalOrientation(PXCMHandData.JointType.JOINT_WRIST, isLeft);
            hand.rotation = device.ToWorldOrientation(localNeckRotation);
            hand.confidence.rotation = RealsenseDevice.GetFaceTargetConfidence();
        }
    }
}
*/