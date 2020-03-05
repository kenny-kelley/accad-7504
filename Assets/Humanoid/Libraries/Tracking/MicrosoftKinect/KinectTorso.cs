#if hKINECT2
namespace Passer.Humanoid.Tracking {

    public class KinectTorso : TorsoSensor {
        private readonly KinectDeviceView kinectDevice;

        public KinectTorso(KinectDeviceView device) : base(device) {
            kinectDevice = device;
        }

        public override Status Update() {
            if (!KinectDevice.JointIsTracked(KinectDevice.JointID.HipCenter)) {
                status = Status.Present;
                return status;
            }

            status = Status.Tracking;

            hips.position = kinectDevice.GetTargetPosition(KinectDevice.JointID.HipCenter);
            hips.confidence.position = KinectDevice.TrackingConfidence(KinectDevice.JointID.HipCenter);

            chest.position = kinectDevice.GetTargetPosition(KinectDevice.JointID.SpineShoulder);
            chest.confidence.position = KinectDevice.TrackingConfidence(KinectDevice.JointID.SpineShoulder);

            Vector lHipPosition = kinectDevice.GetTargetPosition(KinectDevice.JointID.HipLeft);
            Vector rHipPosition = kinectDevice.GetTargetPosition(KinectDevice.JointID.HipRight);
            Vector lrDirection = rHipPosition - lHipPosition;
            Vector lrDirection2 = Rotation.AngleAxis(90, Vector.forward) * lrDirection;

            Vector direction = chest.position - hips.position;
            hips.rotation = kinectDevice.ToWorldOrientation(Rotation_.FromToRotation(direction, lrDirection2) * Rotation.AngleAxis(180, Vector.up));
            hips.confidence.rotation = 0.8F;

            hips.length = Vector.Magnitude(hips.position - chest.position);
            hips.confidence.length = 0.6F;

            Vector neckPosition = kinectDevice.GetTargetPosition(KinectDevice.JointID.Neck);
            chest.rotation = CalculateBoneRotation(neckPosition, chest.position, hips.rotation * Vector.forward) * Rotation.AngleAxis(90, Vector.right) * Rotation.AngleAxis(180, Vector.up);
            chest.confidence.rotation = 0.8F;

            chest.length = Vector.Magnitude(neckPosition - chest.position);
            chest.confidence.length = 0.6F;

            return status;
        }
    }
}
#endif