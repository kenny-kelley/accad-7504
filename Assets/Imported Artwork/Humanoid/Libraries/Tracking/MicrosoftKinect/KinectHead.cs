
#if hKINECT2
namespace Passer.Humanoid.Tracking {

    public class KinectHead : HeadSensor {
        private readonly KinectDeviceView kinectDevice;
        private readonly bool headTracking;

        public KinectHead(KinectDeviceView device, bool _headTracking) : base(device) {
            kinectDevice = (KinectDeviceView)device;

            headTracking = _headTracking;
        }

        public override Status Update() {
            if (!KinectDevice.JointIsTracked(KinectDevice.JointID.Neck)) {
                status = Status.Present;
                return status;
            }

            status = Status.Tracking;

            if (headTracking)
                UpdateBones();

            return status;
        }

        #region Head
        private void UpdateBones() {
            head.position = kinectDevice.GetTargetPosition(KinectDevice.JointID.Head);
            head.confidence.position = KinectDevice.TrackingConfidence(KinectDevice.JointID.Head);

            Rotation localRotation = KinectDevice.GetHeadOrientation();
            head.rotation = device.ToWorldOrientation(localRotation);
            head.confidence.rotation = 0.6F;

            neck.position = kinectDevice.GetTargetPosition(KinectDevice.JointID.Neck);
            neck.confidence.position = KinectDevice.TrackingConfidence(KinectDevice.JointID.Neck);

            neck.rotation = head.rotation;
            neck.confidence.rotation = 0.6F;
        }
        #endregion

    }
}
#endif
