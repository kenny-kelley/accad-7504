using System;
using Microsoft.Kinect.Face;

#if hKINECT2 && hFACE
namespace Passer.Humanoid.Tracking {

    public class KinectFace : FaceSensor {
        //private readonly KinectDeviceView kinectDevice;

        private readonly bool faceTracking;
        private readonly bool audioInput;

        public KinectFace(KinectDeviceView device, bool _faceTracking, bool _audioInput) : base(device) {
            //kinectDevice = device;

            faceTracking = _faceTracking;
            audioInput = _audioInput;
        }

        public override Status Update() {
            status = Status.Tracking;

            if (faceTracking)
                UpdateFace();
            if (audioInput)
                UpdateAudio();

            return status;
        }

        #region Face
        private void UpdateFace() {
            UpdateEyeBrows();
            UpdateEyes();
            UpdateCheeks();
            UpdateNose();
            UpdateMouth();
            UpdateJaw();

            smile = KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.LipCornerPullerLeft);
        }

        private void UpdateEyeBrows() {
            float confidence = 0.7F;
            Vector browPosition;

            Vector leftEyePositon = KinectDevice.GetFacePoint((int)HighDetailFacePoints.LefteyeMidbottom);

            Vector browExpressionPosition = Vector.zero;   // (KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.BrowLowererLeft) - 0.75F) * 0.01F * Vector.up;
                                                            // This is very unstable

            browPosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.LeftOuterBrow) - leftEyePositon;
            browPosition -= browExpressionPosition;
            leftBrow.outer.position = Filter(leftBrow.outer.position, leftBrow.outer.startPosition + browPosition);
            leftBrow.outer.confidence.position = confidence;

            browPosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.LeftBrow) - leftEyePositon;
            browPosition -= browExpressionPosition;
            leftBrow.center.position = Filter(leftBrow.center.position, leftBrow.center.startPosition + browPosition);
            leftBrow.center.confidence.position = confidence;

            browPosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.LeftInnerBrow) - leftEyePositon;
            browPosition -= browExpressionPosition;
            leftBrow.inner.position = Filter(leftBrow.inner.position, leftBrow.inner.startPosition + browPosition);
            leftBrow.inner.confidence.position = confidence;

            Vector rightEyePosition = KinectDevice.GetFacePoint((int)HighDetailFacePoints.RighteyeMidbottom);

            browPosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.RightInnerBrow) - rightEyePosition;
            rightBrow.inner.position = Filter(rightBrow.inner.position, rightBrow.inner.startPosition + browPosition);
            rightBrow.inner.confidence.position = confidence;

            browPosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.RightBrow) - rightEyePosition;
            rightBrow.center.position = Filter(rightBrow.center.position, rightBrow.center.startPosition + browPosition);
            rightBrow.center.confidence.position = confidence;

            browPosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.RightOuterBrow) - rightEyePosition;
            rightBrow.outer.position = Filter(rightBrow.outer.position, rightBrow.outer.startPosition + browPosition);
            rightBrow.outer.confidence.position = confidence;
        }

        private void UpdateEyes() {
            leftEye.closed = KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.EyeClosedLeft);
            leftEye.closed = Clamp01((leftEye.closed - 0.5F) * 2);

            rightEye.closed = KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.EyeClosedRight);
            rightEye.closed = Clamp01((rightEye.closed - 0.5F) * 2);
        }

        private void UpdateCheeks() {
            float confidence = 0.7F;

            Vector referencePoint = KinectDevice.GetFacePoint((int)HighDetailFacePoints.NoseBottom);
            Vector cheekPosition;

            cheekPosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.LeftCheek) - referencePoint;
            leftCheek.position = Filter(leftCheek.position, leftCheek.startPosition + cheekPosition);
            leftCheek.confidence.position = confidence;

            cheekPosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.RightCheek) - referencePoint;
            rightCheek.position = Filter(rightCheek.position, rightCheek.startPosition + cheekPosition);
            rightCheek.confidence.position = confidence;
        }

        private void UpdateNose() {
            float confidence = 0.7F;

            // We could also use NoseTip as reference point...
            Vector referencePoint = KinectDevice.GetFacePoint((int)HighDetailFacePoints.NoseBottom);
            Vector nosePosition;

            nosePosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.NoseTop) - referencePoint;
            nose.top.position = Filter(nose.top.position, nose.top.startPosition + nosePosition);
            nose.top.confidence.position = confidence;

            nosePosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.NoseTopLeft) - referencePoint;
            nose.topLeft.position = Filter(nose.topLeft.position, nose.topLeft.startPosition + nosePosition);
            nose.topLeft.confidence.position = confidence;

            nosePosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.NoseTopRight) - referencePoint;
            nose.topRight.position = Filter(nose.topRight.position, nose.topRight.startPosition + nosePosition);
            nose.topRight.confidence.position = confidence;

            nosePosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.NoseTip) - referencePoint;
            nose.tip.position = Filter(nose.tip.position, nose.tip.startPosition + nosePosition);
            nose.tip.confidence.position = confidence;

            nosePosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.NoseBottomLeft) - referencePoint;
            nose.bottomLeft.position = Filter(nose.bottomLeft.position, nose.bottomLeft.startPosition + nosePosition);
            nose.bottomLeft.confidence.position = confidence;

            nosePosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.NoseBottom) - referencePoint;
            nose.bottom.position = Filter(nose.bottom.position, nose.bottom.startPosition + nosePosition);
            nose.bottom.confidence.position = confidence;

            nosePosition = KinectDevice.GetFacePoint(KinectDevice.FaceBone.NoseBottomRight) - referencePoint;
            nose.bottomRight.position = Filter(nose.bottomRight.position, nose.bottomRight.startPosition + nosePosition);
            nose.bottomRight.confidence.position = confidence;
        }

        private void UpdateMouth() {
            float confidence = 0.7F;

            Vector referencePoint = KinectDevice.GetFacePoint((int)HighDetailFacePoints.NoseBottom);
            Vector lipPosition;

            lipPosition = mouth.upperLipLeft.startPosition + KinectDevice.GetFacePoint(KinectDevice.FaceBone.UpperLipLeft) - referencePoint;
            mouth.upperLipLeft.position = Filter(mouth.upperLipLeft.position, lipPosition);
            mouth.upperLipLeft.confidence.position = confidence;

            lipPosition = mouth.upperLip.startPosition + KinectDevice.GetFacePoint(KinectDevice.FaceBone.UpperLip) - referencePoint;
            mouth.upperLip.position = Filter(mouth.upperLip.position, lipPosition);
            mouth.upperLip.confidence.position = confidence;

            lipPosition = mouth.upperLipRight.startPosition + KinectDevice.GetFacePoint(KinectDevice.FaceBone.UpperLipRight) - referencePoint;
            mouth.upperLipRight.position = Filter(mouth.upperLipRight.position, lipPosition);
            mouth.upperLipRight.confidence.position = confidence;


            lipPosition = mouth.lipLeft.startPosition + KinectDevice.GetFacePoint(KinectDevice.FaceBone.LipLeft) - referencePoint;
            lipPosition += (KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.LipPucker)) * 0.01F * Vector.right;
            mouth.lipLeft.position = Filter(mouth.lipLeft.position, lipPosition);
            mouth.lipLeft.confidence.position = confidence;

            lipPosition = mouth.lipRight.startPosition + KinectDevice.GetFacePoint(KinectDevice.FaceBone.LipRight) - referencePoint;
            lipPosition += (KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.LipPucker)) * -0.01F * Vector.right;
            mouth.lipRight.position = Filter(mouth.lipRight.position, lipPosition);
            mouth.lipRight.confidence.position = confidence;

            lipPosition = mouth.lowerLipLeft.startPosition + KinectDevice.GetFacePoint(KinectDevice.FaceBone.LowerLipLeft) - referencePoint;
            mouth.lowerLipLeft.position = Filter(mouth.lowerLipLeft.position, lipPosition);
            mouth.lowerLipLeft.confidence.position = confidence;

            lipPosition = mouth.lowerLip.startPosition + KinectDevice.GetFacePoint(KinectDevice.FaceBone.LowerLip) - referencePoint;
            mouth.lowerLip.position = Filter(mouth.lowerLip.position, lipPosition);
            mouth.lowerLip.confidence.position = confidence;

            lipPosition = mouth.lowerLipRight.startPosition + KinectDevice.GetFacePoint(KinectDevice.FaceBone.LowerLipRight) - referencePoint;
            mouth.lowerLipRight.position = Filter(mouth.lowerLipRight.position, lipPosition);
            mouth.lowerLipRight.confidence.position = confidence;
        }

        private static void UpdateFacePoint(TargetData faceBone, KinectDevice.FaceBone faceBoneId, Vector referencePoint) {
            Vector facePointPosition = faceBone.startPosition + KinectDevice.GetFacePoint(faceBoneId) - referencePoint;
            faceBone.position = Filter(faceBone.position, facePointPosition);
            faceBone.confidence.position = 0.7F;
        }

        //private static float jawBias = 0;
        private const float beta = 0.05F;
        private static Vector minAngles = new Vector(0, -5, 0);
        private static Vector maxAngles = new Vector(20, 5, 0);
        private float lastJawOpen;
        private void UpdateJaw() {

            float jawOpen = KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.JawOpen);
            float jawAngleX = minAngles.x + jawOpen * (maxAngles.x - minAngles.x);

            Rotation newRotation = Rotation.AngleAxis(jawAngleX, Vector.right);

            jaw.rotation = Rotation.Slerp(newRotation, jaw.rotation, beta);

            jaw.confidence.rotation = 0.7F;
        }

        private static Vector Filter(Vector oldPosition, Vector newPosition) {
            return new Vector(
                Filter(oldPosition.x, newPosition.x),
                Filter(oldPosition.y, newPosition.y),
                Filter(oldPosition.z, newPosition.z)
                );
        }

        // This is not framerate compensated!
        public static float Filter(float oldPosition, float newPosition) {
            const float limit = 0.1F;
            const float maxStep = 0.01F;
            const float minStep = 0.002F;
            const float sensitivity = 0.2F;

            float difference = newPosition - oldPosition;
            float distance = Math.Abs(difference);
            float direction = Math.Sign(difference);

            if (distance > limit)
                return newPosition;
            else if (distance < minStep)
                return oldPosition;
            else if (distance > maxStep)
                return oldPosition + sensitivity * direction * maxStep;
            else
                return oldPosition + sensitivity * difference;
        }

        private float Clamp01(float x) {
            if (x < 0)
                return 0;
            else if (x > 1)
                return 1;
            else {
                return x;
            }
        }

        #endregion

        #region Audio
        private void UpdateAudio() {
            //            headTarget.audioEnergy = KinectDevice.GetAudioEnergy();
        }
        #endregion
    }
}
#endif
