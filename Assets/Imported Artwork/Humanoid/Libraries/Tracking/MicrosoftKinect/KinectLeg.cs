#if hKINECT2
using System;

namespace Passer.Humanoid.Tracking {

    public class KinectLeg : LegSensor {
        private readonly KinectDeviceView kinectDevice;

        private readonly KinectDevice.JointID hipJoint;
        private readonly KinectDevice.JointID kneeJoint;
        private readonly KinectDevice.JointID ankleJoint;
        private readonly KinectDevice.JointID footJoint;

        public KinectLeg(bool isLeft, KinectDeviceView device) : base(isLeft, device) {
            kinectDevice = (KinectDeviceView)device;

            if (isLeft) {
                hipJoint = KinectDevice.JointID.HipLeft;
                kneeJoint = KinectDevice.JointID.KneeLeft;
                ankleJoint = KinectDevice.JointID.AnkleLeft;
                footJoint = KinectDevice.JointID.FootLeft;
            } else {
                hipJoint = KinectDevice.JointID.HipRight;
                kneeJoint = KinectDevice.JointID.KneeRight;
                ankleJoint = KinectDevice.JointID.AnkleRight;
                footJoint = KinectDevice.JointID.FootRight;
            }
        }

        public Status Update(Rotation hipsOrientation) {
            if (!KinectDevice.JointIsTracked(footJoint)) {
                status = Status.Present;
                return status;
            }
            status = Status.Tracking;

            Vector hipsPosition = kinectDevice.GetTargetPosition(hipJoint);
            Vector kneePosition = kinectDevice.GetTargetPosition(kneeJoint);
            Vector anklePosition = kinectDevice.GetTargetPosition(ankleJoint);
            //Vector footPosition = kinectDevice.GetTargetPosition(footJoint);

            // Position
            upperLeg.confidence.position = KinectDevice.TrackingConfidence(upperLeg.position, hipsPosition);
            upperLeg.position = hipsPosition;

            lowerLeg.confidence.position = KinectDevice.TrackingConfidence(lowerLeg.position, kneePosition);
            lowerLeg.position = kneePosition;

            foot.confidence.position = KinectDevice.TrackingConfidence(foot.position, anklePosition);
            foot.position = anklePosition;

            // Rotation
            upperLeg.rotation = device.ToWorldOrientation(CalculateLegOrientation(hipsPosition, kneePosition, hipsOrientation));
            upperLeg.confidence.rotation = Math.Min(upperLeg.confidence.position, lowerLeg.confidence.position);

            lowerLeg.rotation = device.ToWorldOrientation(CalculateLegOrientation(kneePosition, anklePosition, hipsOrientation));
            lowerLeg.confidence.rotation = Math.Min(lowerLeg.confidence.position, foot.confidence.position);

            foot.rotation = device.ToWorldOrientation(Rotation.AngleAxis(180, Vector.up));
            foot.confidence.rotation = foot.confidence.position;

            return status;
        }
    }
}
#endif