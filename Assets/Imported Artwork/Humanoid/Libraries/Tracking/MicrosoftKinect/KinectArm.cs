#if hKINECT2
using System;

namespace Passer.Humanoid.Tracking {

    public class KinectArm : ArmSensor {
        private readonly KinectDeviceView kinectDevice;

        private readonly bool handTracking;

        private readonly KinectDevice.JointID shoulderJoint;
        private readonly KinectDevice.JointID elbowJoint;
        private readonly KinectDevice.JointID wristJoint;
        private readonly KinectDevice.JointID handJoint;
        private readonly KinectDevice.JointID handTipJoint;
        private readonly KinectDevice.JointID thumbJoint;

        public KinectArm(bool isLeft, KinectDeviceView device, bool _handTracking) : base(isLeft, device) {
            kinectDevice = device;
            handTracking = _handTracking;

            if (isLeft) {
                shoulderJoint = KinectDevice.JointID.ShoulderLeft;
                elbowJoint = KinectDevice.JointID.ElbowLeft;
                wristJoint = KinectDevice.JointID.WristLeft;
                handJoint = KinectDevice.JointID.HandLeft;
                handTipJoint = KinectDevice.JointID.HandTipLeft;
                thumbJoint = KinectDevice.JointID.ThumbLeft;
            }
            else {
                shoulderJoint = KinectDevice.JointID.ShoulderRight;
                elbowJoint = KinectDevice.JointID.ElbowRight;
                wristJoint = KinectDevice.JointID.WristRight;
                handJoint = KinectDevice.JointID.HandRight;
                handTipJoint = KinectDevice.JointID.HandTipRight;
                thumbJoint = KinectDevice.JointID.ThumbRight;
            }
        }

        public override Status Update() {
            if (!KinectDevice.JointIsTracked(handJoint)) {
                status = Status.Present;
                return status;
            }

            status = Status.Tracking;

            UpdateArm();
            if (handTracking)
                UpdateFingers();

            return status;
        }

        private void UpdateArm() {
            Vector shoulderPosition = kinectDevice.GetTargetPosition(shoulderJoint);
            Vector elbowPosition = kinectDevice.GetTargetPosition(elbowJoint);
            Vector wristPosition = kinectDevice.GetTargetPosition(wristJoint);

            Vector upperArmDirection = elbowPosition - shoulderPosition;
            Vector forearmDirection = wristPosition - elbowPosition;

            float upperArmLength = Vector.Magnitude(upperArmDirection);
            float forearmLength = Vector.Magnitude(forearmDirection);

            Vector elbowUp = Vector.Cross(upperArmDirection, forearmDirection);
            if (!isLeft)
                elbowUp = -elbowUp;

            upperArm.confidence.position = KinectDevice.TrackingConfidence(upperArm.position, shoulderPosition);
            upperArm.position = shoulderPosition;
            upperArm.rotation = CalculateUpperArmOrientation(shoulderPosition, upperArmLength, elbowUp, forearmLength, wristPosition, isLeft);

            forearm.confidence.position = KinectDevice.TrackingConfidence(forearm.position, elbowPosition);
            forearm.position = elbowPosition;

            upperArm.confidence.rotation = Math.Min(upperArm.confidence.position, forearm.confidence.position);

            //float handPositionConfidence = KinectDevice.TrackingConfidence(hand.position, wristPosition);
            //if (handPositionConfidence >= hand.confidence.position) { // Is this tracking info better than we already have?
                hand.confidence.position = 0.9F;

                Vector thumbPosition = kinectDevice.GetTargetPosition(thumbJoint);
                Vector handPosition = kinectDevice.GetTargetPosition(handTipJoint);

                Vector handDirection = handPosition - wristPosition;
                Vector thumbDirection = wristPosition - thumbPosition;
                Vector handUp = Vector.Cross(handDirection, thumbDirection);
                if (isLeft)
                    handUp = -handUp;

                forearm.rotation = CalculateArmOrientation(elbowPosition, elbowUp, wristPosition, isLeft);
                forearm.confidence.rotation = Math.Min(forearm.confidence.position, hand.confidence.position);

                hand.position = wristPosition;
                // hand rotational tracking is quite unreliable

                hand.rotation = CalculateArmOrientation(wristPosition, handUp, handPosition, isLeft);
                hand.confidence.rotation = 0.6F;

            //}
            //else {
            //    float weight = 0.01F;

            //    hand.confidence.position = handPositionConfidence * weight + hand.confidence.position * (1 - weight);
            //    hand.position = wristPosition * weight + hand.position * (1 - weight);
            //    // do not use wristRotation here, quality is low, we expect any other tracker is better

            //    forearm.rotation = CalculateArmOrientation(elbowPosition, elbowUp, hand.position, isLeft);
            //    forearm.confidence.rotation = Math.Min(forearm.confidence.position, hand.confidence.position);
            //}
        }

        private void UpdateFingers() {
            bool handLasso, handClosed;
            if (isLeft) {
                handLasso = KinectDevice.GetLeftHandState() == Windows.Kinect.HandState.Lasso;
                handClosed = KinectDevice.GetLeftHandState() == Windows.Kinect.HandState.Closed;
            }
            else {
                handLasso = KinectDevice.GetRightHandState() == Windows.Kinect.HandState.Lasso;
                handClosed = KinectDevice.GetRightHandState() == Windows.Kinect.HandState.Closed;
            }

            thumb.curl = MaxChange(thumb.curl, (handLasso || handClosed) ? 1 : 0, 0.1F);
            indexFinger.curl = MaxChange(indexFinger.curl, handClosed ? 1 : 0, 0.1F);
            middleFinger.curl = MaxChange(middleFinger.curl, handClosed ? 1 : 0, 0.1F);
            ringFinger.curl = MaxChange(ringFinger.curl, (handLasso || handClosed) ? 1 : 0, 0.1F);
            littleFinger.curl = MaxChange(littleFinger.curl, (handLasso || handClosed) ? 1 : 0, 0.1F);
        }

        private float MaxChange(float currentValue, float newValue, float maxChange) {
            float difference = newValue - currentValue;
            if (difference > maxChange) {
                return currentValue + maxChange;
            }
            else if (difference < -maxChange) {
                return currentValue - maxChange;
            }
            else
                return newValue;
        }

    }
}
#endif
