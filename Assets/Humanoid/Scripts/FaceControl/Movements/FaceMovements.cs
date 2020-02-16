using UnityEngine;

#if hFACE

namespace Passer {

    public class FaceMovements : Movements {
        private FaceTarget faceTarget;

        private readonly EyeMovements leftEyeMovements = new EyeMovements();
        private readonly EyeMovements rightEyeMovements = new EyeMovements();

        public void StartFaceMovements(HumanoidControl _humanoid, FaceTarget _faceTarget) {
            faceTarget = _faceTarget;
            leftEyeMovements.Start(faceTarget.leftEye);
            rightEyeMovements.Start(faceTarget.rightEye);
        }

        public static void Update(FaceTarget faceTarget) {
            if (faceTarget == null)
                return;

            if (Application.isPlaying) {
                faceTarget.focusPoint = faceTarget.faceMovements.DetemineFocusPoint(faceTarget.localGazeDirection, out faceTarget.focusObject);
            }

            faceTarget.faceMovements.UpdateCheeks(faceTarget);
            faceTarget.faceMovements.UpdateNose(faceTarget);

            faceTarget.faceMovements.UpdateJaw(faceTarget);
            faceTarget.faceMovements.UpdateMouth(faceTarget);

            if (faceTarget.faceMovements.leftEyeMovements != null)
                faceTarget.faceMovements.leftEyeMovements.Update();
            if (faceTarget.faceMovements.rightEyeMovements != null)
                faceTarget.faceMovements.rightEyeMovements.Update();

            faceTarget.faceMovements.UpdateEyeBrow(faceTarget, faceTarget.leftBrow, faceTarget.leftEye);
            faceTarget.faceMovements.UpdateEyeBrow(faceTarget, faceTarget.rightBrow, faceTarget.rightEye);
        }

        #region EyeBrow
        private void UpdateEyeBrow(FaceTarget faceTarget, EyeBrow brow, EyeTarget eye) {
            UpdateFacePoint(faceTarget, brow.outer);
            UpdateFacePoint(faceTarget, brow.center);
            UpdateFacePoint(faceTarget, brow.inner);
        }

        private void MoveBrowFromEye(EyeBrow brow, EyeTarget eye) {

            float yPosition = 0;
            float eyeAngleX = Humanoid.Tracking.Angle.Normalize(eye.target.transform.localEulerAngles.x);
            if (eyeAngleX < 0)
                yPosition = -eyeAngleX * 0.0004F;

            if (brow.outer.isPresent) {
                brow.outer.target.transform.localPosition = brow.outer.startPosition + Vector3.up * yPosition;
                brow.outer.target.confidence.position = 0.1F;
            }
            if (brow.center.isPresent) {
                brow.center.target.transform.localPosition = brow.center.startPosition + Vector3.up * yPosition;
                brow.center.target.confidence.position = 0.1F;
            }
            if (brow.inner.isPresent) {
                brow.inner.target.transform.localPosition = brow.inner.startPosition + Vector3.up * yPosition;
                brow.inner.target.confidence.position = 0.1F;
            }
        }
        #endregion

        #region Ears
        private void UpdateEars() {
            UpdateFacePoint(faceTarget, faceTarget.leftEar);
            UpdateFacePoint(faceTarget, faceTarget.rightEar);
        }
        #endregion

        #region Cheeks
        private void UpdateCheeks(FaceTarget faceTarget) {
            UpdateFacePoint(faceTarget, faceTarget.leftCheek);
            UpdateFacePoint(faceTarget, faceTarget.rightCheek);
        }
        #endregion

        #region Nose
        private void UpdateNose(FaceTarget faceTarget) {
            UpdateFacePoint(faceTarget, faceTarget.nose.top);

            UpdateFacePoint(faceTarget, faceTarget.nose.tip);

            UpdateFacePoint(faceTarget, faceTarget.nose.bottomLeft);
            UpdateFacePoint(faceTarget, faceTarget.nose.bottom);
            UpdateFacePoint(faceTarget, faceTarget.nose.bottomRight);
        }
        #endregion

        #region Mouth
        private void UpdateMouth(FaceTarget faceTarget) {
            Mouth mouth = faceTarget.mouth;

            UpdateFacePoint(faceTarget, mouth.upperLip);

            UpdateFacePoint(faceTarget, mouth.lipLeft);
            UpdateFacePoint(faceTarget, mouth.lipRight);

            UpdateFacePoint(faceTarget, mouth.lowerLip);

            //if (mouth.upperLipLeft.target.confidence.position < 0.1F)
            //    LerpMouthPoint(mouth, mouth.upperLipLeft, mouth.upperLip, mouth.lipLeft, 0.3F);
            //else
            UpdateFacePoint(faceTarget, mouth.upperLipLeft);

            //if (mouth.upperLipRight.target.confidence.position < 0.1F)
            //    LerpMouthPoint(mouth, mouth.upperLipRight, mouth.upperLip, mouth.lipRight, 0.3F);
            //else
            UpdateFacePoint(faceTarget, mouth.upperLipRight);

            //if (mouth.lowerLipLeft.target.confidence.position < 0.1F)
            //    LerpMouthPoint(mouth, mouth.lowerLipLeft, mouth.lowerLip, mouth.lipLeft, 0.3F);
            //else
            UpdateFacePoint(faceTarget, mouth.lowerLipLeft);

            //if (mouth.lowerLipRight.target.confidence.position < 0.1F)
            //    LerpMouthPoint(mouth, mouth.lowerLipRight, mouth.lowerLip, mouth.lipRight, 0.3F);
            //else
            UpdateFacePoint(faceTarget, mouth.lowerLipRight);
        }

        private void LerpMouthPoint(Mouth mouth, FaceBone mouthBone, FaceBone mouthBone1, FaceBone mouthBone2, float f) {
            if (mouthBone.bone.transform == null || mouthBone1.target.confidence.position < 0.1F || mouthBone2.target.confidence.position < 0.1F)
                return;

            Vector3 localPosition1 = mouthBone1.target.transform.localPosition - mouthBone1.startPosition;
            Vector3 localPosition2 = mouthBone2.target.transform.localPosition - mouthBone2.startPosition;
            Vector3 localPosition = Vector3.Lerp(localPosition1, localPosition2, f);
            mouthBone.target.transform.localPosition = mouthBone.startPosition + localPosition;
            UpdateFacePoint(faceTarget, mouthBone);
        }
        #endregion

        #region Jaw
        private void UpdateJaw(FaceTarget faceTarget) {
            if (faceTarget.jaw.bone.transform == null)
                return;

            Quaternion jawRotationOnParent = faceTarget.headTarget.head.bone.targetRotation * faceTarget.jaw.target.transform.localRotation;
            faceTarget.jaw.bone.transform.rotation = jawRotationOnParent * faceTarget.jaw.target.toBoneRotation;
        }
        #endregion


        private void UpdateFacePoint(FaceTarget faceTarget, HumanoidTarget.TargetedBone faceBone) {
            if (faceBone.bone.transform == null)
                return;

            HumanoidTarget.TargetedBone parentBone;
            Quaternion parentRotation;
            if (faceBone.target.transform.parent == faceTarget.jaw.target.transform && faceTarget.jaw.bone.transform != null) {
                parentBone = faceTarget.jaw;
                parentRotation = faceTarget.jaw.bone.targetRotation;
            }
            else {
                parentBone = faceTarget.headTarget.head;
                parentRotation = faceTarget.headTarget.head.bone.targetRotation;
            }

            Vector3 localPosition = parentBone.target.transform.InverseTransformPoint(faceBone.target.transform.position);
            faceBone.bone.transform.position = parentBone.bone.transform.position + parentBone.bone.targetRotation * localPosition;

            Quaternion localRotation = parentBone.bone.targetRotation * faceBone.target.transform.localRotation;
            faceBone.bone.transform.rotation = localRotation * faceBone.target.toBoneRotation;

            //Quaternion jawRotationOnParent = parentRotation * faceBone.target.transform.localRotation;
            //faceBone.bone.transform.rotation = jawRotationOnParent * faceBone.target.toBoneRotation;
        }

        private Vector3 DetemineFocusPoint(Vector3 localLookDirection, out GameObject focusObject) {
            focusObject = null;

            if (faceTarget == null || faceTarget.headTarget == null)
                return Vector3.zero;            

            Vector3 eyePosition = faceTarget.headTarget.GetEyePosition();

            RaycastHit hit;
            Vector3 gazeDirection = faceTarget.headTarget.head.target.transform.TransformDirection(faceTarget.localGazeDirection);
            //Debug.DrawRay(eyePosition + gazeDirection * 0.1F, gazeDirection * 10, Color.magenta);
            if (Physics.Raycast(eyePosition + gazeDirection * 0.1F, gazeDirection, out hit, 10, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {
                focusObject = DetermineFocusObject(hit);
                return hit.point;
            } else
                return eyePosition + gazeDirection * 10;            
        }

        /// <summary>
        /// What at we looking at: a humanoid?
        /// </summary>
        /// <param name="hit">Hit information the raycast from the eyes</param>
        /// <returns>The object we are looking at</returns>
        private GameObject DetermineFocusObject(RaycastHit hit) {
            if (hit.rigidbody != null) {
                // Humanoid has a rigidbody and a body collider
                HeadTarget headTarget = hit.rigidbody.GetComponent<HeadTarget>();
                if (headTarget != null) {
                    // We are looking at the head of a humanoid
                    return headTarget.humanoid.gameObject;
                }
            }
            return hit.transform.gameObject;
        }
    }
}
#endif