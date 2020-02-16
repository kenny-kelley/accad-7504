namespace Passer.Humanoid.Tracking.Realsense {
    public class RealsenseHead : HeadSensor {
        private readonly bool headTracking;

        public RealsenseHead(RealsenseDeviceView device, bool _headTracking) : base(device) {
            headTracking = _headTracking;
        }

        public override Status Update() {
            if (RealsenseDevice.GetFaceTargetConfidence() <= 0) {
                status = RealsenseDevice.present ? Status.Present : Status.Unavailable;
                return status;
            }

            status = Status.Tracking;

            if (headTracking)
                UpdateBones();

            return status;
        }

        private void UpdateBones() {
            Vector localNeckPosition = RealsenseDevice.GetFaceLocalPosition();
            neck.position = device.ToWorldPosition(localNeckPosition);
            neck.confidence.position = RealsenseDevice.GetFaceTargetConfidence();

            Rotation localNeckRotation = RealsenseDevice.GetFaceLocalOrientation();
            neck.rotation = localNeckRotation;
            neck.confidence.rotation = RealsenseDevice.GetFaceTargetConfidence();
        }

    }
}