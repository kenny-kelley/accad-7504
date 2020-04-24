using System;

#if hREALSENSE && hFACE
namespace Passer.Humanoid.Tracking.Realsense {
    public class RealsenseFace : FaceSensor {

        private readonly bool faceTracking;
        //private readonly bool eyeTracking;

        public RealsenseFace(RealsenseDeviceView device, bool _faceTracking, bool _eyeTracking) : base(device) {
            faceTracking = _faceTracking;
            //eyeTracking = _eyeTracking;
        }

        public override Status Update() {
            if (RealsenseDevice.GetFaceTargetConfidence() <= 0) {
                status = RealsenseDevice.present ? Status.Present : Status.Unavailable;
                return status;
            }

            status = Status.Tracking;

            if (faceTracking)
                UpdateFace();

            return status;
        }

        private void UpdateFace() {
            smile = RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.Smile);
            pucker = RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.Kiss);
            frown = (RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.BrowLowererLeft) +
                RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.BrowLowererLeft)) / 2;

            UpdateEyeBrows();
            UpdateEyeLids();

            UpdateNose();
            UpdateMouth();
            //UpdateJaw();
        }

        private void UpdateEyes() {
            //(int)PXCMFaceData.LandmarkType.LANDMARK_EYE_RIGHT_CENTER

            //if (target.leftEyeTarget.closed + target.rightEyeTarget.closed == 0) { // even then, undetected blinking will give wrong eye direction.
            //    float eyesTurnLeft = RealsenseDevice.GetExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_TURN_LEFT);
            //    float eyesTurnRight = RealsenseDevice.GetExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_TURN_RIGHT);
            //    float eyesLeftRight = eyesTurnRight - eyesTurnLeft;
            //    float eyesUp = RealsenseDevice.GetExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_UP);
            //    float eyesDown = RealsenseDevice.GetExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_DOWN);
            //    float eyesUpdown = eyesDown - eyesUp;
            //    target.focusPoint = Quaternion.Euler(eyesUpdown * 15, eyesLeftRight * 25, 0) * (target.transform.position + new Vector3(0, 0, 2)); // should be realtive to eye position
            //}
            //target.eyeTargetsConfidence.orientation = 0.8F;
        }

        private void UpdateEyeBrows() {
            Vector leftEyePositon;
            if (RealsenseDevice.GetFacePoint(76, out leftEyePositon)) {
                UpdateFacePoint(leftBrow.outer, FaceBone.LeftOuterBrow, leftEyePositon);
                UpdateFacePoint(leftBrow.center, FaceBone.LeftBrow, leftEyePositon);
                UpdateFacePoint(leftBrow.inner, FaceBone.LeftInnerBrow, leftEyePositon);
            }

            Vector rightEyePositon;
            if (RealsenseDevice.GetFacePoint(77, out rightEyePositon)) {
                UpdateFacePoint(rightBrow.inner, FaceBone.RightInnerBrow, rightEyePositon);
                UpdateFacePoint(rightBrow.center, FaceBone.RightBrow, rightEyePositon);
                UpdateFacePoint(rightBrow.outer, FaceBone.RightOuterBrow, rightEyePositon);
            }
        }

        private void UpdateEyeLids() {
            leftEye.closed = RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.EyeClosedLeft);
            rightEye.closed = RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.EyeClosedRight);

        }

        private void UpdateNose() {
            Vector referencePoint;
            UpdateFacePoint(nose.tip, FaceBone.NoseTip, Vector.zero);

            if (nose.tip.confidence.position < 0.7F)
                return;
            referencePoint = nose.tip.position;
            UpdateFacePoint(nose.top, FaceBone.NoseTop, referencePoint);

            UpdateFacePoint(nose.bottomLeft, FaceBone.NoseBottomLeft, referencePoint);
            UpdateFacePoint(nose.bottom, FaceBone.NoseBottom, referencePoint);
            UpdateFacePoint(nose.bottomRight, FaceBone.NoseBottomRight, referencePoint);
        }

        private void UpdateMouth() {
            if (nose.tip.confidence.position < 0.7F)
                return;

            Vector referencePoint = nose.tip.position;

            UpdateFacePoint(mouth.upperLipLeft, FaceBone.UpperLipLeft, referencePoint);
            UpdateFacePoint(mouth.upperLip, FaceBone.UpperLip, referencePoint);
            UpdateFacePoint(mouth.upperLipRight, FaceBone.UpperLipRight, referencePoint);

            UpdateFacePoint(mouth.lipLeft, FaceBone.LipLeft, referencePoint);
            UpdateFacePoint(mouth.lipRight, FaceBone.LipRight, referencePoint);

            UpdateFacePoint(mouth.lowerLipLeft, FaceBone.LowerLipLeft, referencePoint);
            UpdateFacePoint(mouth.lowerLip, FaceBone.LowerLip, referencePoint);
            UpdateFacePoint(mouth.lowerLipRight, FaceBone.LowerLipRight, referencePoint);
        }

        private static void UpdateFacePoint(TargetData faceBone, int faceBoneId, Vector referencePoint, Vector correction) {
            Vector facePointPosition;
            if (RealsenseDevice.GetFacePoint(faceBoneId, out facePointPosition)) {
                facePointPosition = facePointPosition - referencePoint + correction;
                faceBone.position = facePointPosition; // Filter(faceBone.position, faceBone.startPosition + facePointPosition);
                faceBone.confidence.position = 0.7F;
            } else
                faceBone.confidence.position = 0;

        }

        private static void UpdateFacePoint(TargetData faceBone, FaceBone faceBoneId, Vector referencePoint) {
            Vector facePointPosition;
            if (RealsenseDevice.GetFacePoint(faceBoneId, out facePointPosition)) {
                facePointPosition = faceBone.startPosition + facePointPosition - referencePoint;
                faceBone.position = Filter(faceBone.position, facePointPosition);
                faceBone.confidence.position = 0.7F;
            } else
                faceBone.confidence.position = 0;

        }

        //private Vector jawBias = new Vector(20, 0, 0);
        //private static Vector minAngles = new Vector(0, -5, 0);
        //private static Vector maxAngles = new Vector(15, 5, 0);

        private void UpdateJaw() {
            //Rotation jawRotation = RealsenseDevice.GetFacePointOrientation(RealsenseDevice.FacePointName(PXCMFaceData.LandmarkType.LANDMARK_CHIN));
            //Vector jawAngles = Rotation.ToAngles(jawRotation);
            //jawAngles = Angles.Normalize(new Vector(jawAngles.x, jawAngles.y, jawAngles.z));

            //jawAngles += jawBias;
            //if (jawAngles.x > maxAngles.x)
            //    jawBias.x -= 1F;
            //else if (jawAngles.x < minAngles.x)
            //    jawBias.x += 1F;
            //jawAngles = Angles.Clamp(jawAngles, minAngles, maxAngles);

            //jaw.rotation = Rotation_.Euler(jawAngles.x, jawAngles.y, jawAngles.z);
        }

        private static Vector Filter(Vector oldPosition, Vector newPosition) {
            //return newPosition;
            return new Vector(
                Filter(oldPosition.x, newPosition.x),
                Filter(oldPosition.y, newPosition.y),
                Filter(oldPosition.z, newPosition.z)
                //oldPosition.z
                );
        }

        public static float Filter(float oldPosition, float newPosition) {
            const float limit = 0.02F;
            const float maxStep = 0.01F;
            const float minStep = 0.002F;
            const float sensitivity = 0.2F;

            float distance = Math.Abs(newPosition - oldPosition);
            if (distance > limit)
                return newPosition;
            else if (distance < minStep) {
                return oldPosition;
            } else if (distance > maxStep) {
                return oldPosition + sensitivity * Math.Sign(newPosition - oldPosition) * maxStep;
            } else {
                return oldPosition + sensitivity * (newPosition - oldPosition);
            }
        }
    }
}
#endif