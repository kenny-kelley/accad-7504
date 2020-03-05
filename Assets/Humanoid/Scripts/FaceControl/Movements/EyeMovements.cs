/* Eye Movements
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.0
 * date: December 31, 2014
 * 
 * - First version
 */

using UnityEngine;
using System.Collections;

#if hFACE

namespace Passer {

    [System.Serializable]
    public class EyeMovements {
        private EyeTarget eyeTarget;

        public void Start(EyeTarget _eyeTarget) {
            eyeTarget = _eyeTarget;
        }

        public void Update() {
            if (!Application.isPlaying || eyeTarget == null || eyeTarget.bone.transform == null)
                return;

            LookAtTarget(eyeTarget.face.focusPoint);

            //Debug.DrawRay(eyeTarget.bone.transform.position, eyeTarget.target.transform.forward * 10, Color.white);
            Debug.DrawLine(eyeTarget.bone.transform.position, eyeTarget.face.focusPoint, Color.gray);

            eyeTarget.bone.transform.rotation = eyeTarget.target.transform.rotation * eyeTarget.target.toBoneRotation;

            UpdateEyeLid(eyeTarget.upperLid);
            //UpdateEyeLid(eyeTarget.lowerLid);

            UpdateBlinking();
        }

        private void LookAtTarget(Vector3 targetPosition) {
            Vector3 targetDirection = targetPosition - eyeTarget.bone.transform.position;
            Quaternion headRotation = eyeTarget.headTarget.head.bone.transform.rotation * eyeTarget.headTarget.head.bone.toTargetRotation;

            Vector3 headForward = headRotation * Vector3.forward;
            Vector3 headUp = headRotation * Vector3.up;

            Vector3 targetEyeAngles = Quaternion.FromToRotation(headForward, targetDirection).eulerAngles;
            Vector3 eyeAngles = UnityAngles.Clamp(targetEyeAngles, eyeTarget.bone.minAngles, eyeTarget.bone.maxAngles);

            targetDirection = Quaternion.Euler(eyeAngles) * headForward;

            eyeTarget.target.transform.rotation = Quaternion.LookRotation(targetDirection, headUp);
        }

        private void UpdateEyeLid(HumanoidTarget.TargetedBone eyeLid) {
            if (eyeLid.bone.transform == null)
                return;

            //eyeLid.target.transform.LookAt(eyeTarget.face.focusPoint);
            eyeLid.bone.transform.rotation = eyeLid.target.transform.rotation * eyeLid.target.toBoneRotation;

            //Quaternion eyeLidRotationOnParent = faceTarget.headTarget.head.bone.targetRotation * jaw.target.transform.localRotation;
            //jaw.bone.transform.rotation = eyeLidRotationOnParent * jaw.target.toBoneRotation;
        }

        private float eyeClosingSpeed = 0.200f; // duration of a blink in seconds
        private void UpdateBlinking() {
            if (eyeTarget.headTarget == null)
                return;

            float blinkPhase = (Time.realtimeSinceStartup - eyeTarget.face.lastBlink) / eyeClosingSpeed;

            float closedness = eyeTarget.closed;
            if (closedness > 0.9F && blinkPhase > 1) { // expected later -> increase stress
                eyeTarget.headTarget.stress = Mathf.Clamp01(eyeTarget.headTarget.stress + 0.05F);
                eyeTarget.face.lastBlink = Time.realtimeSinceStartup;
                blinkPhase = (Time.realtimeSinceStartup - eyeTarget.face.lastBlink) / eyeClosingSpeed;
            }

            if (blinkPhase < 1) {
                if (blinkPhase < 0.5f) {
                    closedness = Mathf.Max(closedness, blinkPhase * 2);
                } else {
                    closedness = Mathf.Max(closedness, 1 - blinkPhase * 2);
                }
            }

            bool hasEyeBones = eyeTarget.upperLid != null && eyeTarget.lowerLid != null;
            if (eyeTarget.headTarget.smRenderer != null && eyeTarget.blink < eyeTarget.headTarget.smRenderer.sharedMesh.blendShapeCount) {
                // use blendshape
                eyeTarget.headTarget.smRenderer.SetBlendShapeWeight(eyeTarget.blink, closedness * 100);

            } else if (hasEyeBones && eyeTarget.upperLid.bone.transform != null) {
                // use eyelidbones
                float lidDistance = Vector3.Distance(eyeTarget.target.transform.position, eyeTarget.upperLid.target.transform.position);
                if (lidDistance > 0.001F) {
                    Vector3 lidDifference = eyeTarget.headTarget.transform.forward * lidDistance;
                    eyeTarget.upperLid.bone.transform.position = eyeTarget.bone.transform.position + Quaternion.Euler((1 - closedness) * -20, 0, 0) * lidDifference;
                } else {
                    // eye and lids are on the same position, so we use rotation to close the lids
                    eyeTarget.upperLid.bone.transform.localRotation = Quaternion.Euler((closedness) * 20 + 10, 0, 0) * eyeTarget.upperLid.target.toBoneRotation;

                }
            }

            float angle = Vector3.Angle(eyeTarget.target.transform.forward, eyeTarget.headTarget.transform.forward);
            float tSinceLastBlink = Time.realtimeSinceStartup - eyeTarget.face.lastBlink;
            if (angle > 10 && tSinceLastBlink > 2)
                Blink(0.300f);
            if (tSinceLastBlink > 2 + (1 - eyeTarget.headTarget.stress) + Random.value * 2)
                Blink(0.200f);
        }

        private void Blink(float blinkSpeed) {
            // Did not sense a real blink -> decrease stress
            eyeTarget.headTarget.stress = Mathf.Clamp01(eyeTarget.headTarget.stress - 0.05F);

            eyeTarget.face.lastBlink = Time.realtimeSinceStartup;
            eyeClosingSpeed = blinkSpeed;
        }
    }
}
#endif
