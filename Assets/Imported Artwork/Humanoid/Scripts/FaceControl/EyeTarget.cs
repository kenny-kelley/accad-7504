#if hFACE
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class EyeTarget : HumanoidTarget.TargetedBone {
        public HeadTarget headTarget;
        [System.NonSerialized]
        public FaceTarget face;
        private bool isLeft;

        public EyeTarget(FaceTarget face, bool isLeft) {
            this.face = face;
            this.headTarget = face.headTarget;
            this.isLeft = isLeft;

            boneId = isLeft ? Bone.LeftEye : Bone.RightEye;
            upperLid.boneId = isLeft ? Bone.LeftUpperLid : Bone.RightUpperLid;
            lowerLid.boneId = isLeft ? Bone.LeftLowerLid : Bone.RightLowerLid;
        }

        public HumanoidTarget.TargetedBone upperLid = new HumanoidTarget.TargetedBone();
        public HumanoidTarget.TargetedBone lowerLid = new HumanoidTarget.TargetedBone();

        public int blink;

        public float closed;

        #region Limitations
        public static readonly Vector3 minEyeAngles = new Vector3(-15, -30, -10);
        public static readonly Vector3 maxEyeAngles = new Vector3(10, 30, 10);
        #endregion

        public void Init(HeadTarget _headTArget, bool _isLeft) {
            headTarget = _headTArget;
            face = headTarget.face;

            isLeft = _isLeft;

            string eyeTargetName = isLeft ? "LeftEye" : "RightEye";
            if (headTarget.head.target.transform != null)
                target.transform = headTarget.head.target.transform.Find(eyeTargetName);
            if (target.transform == null) {
                target.transform = NewTargetTransform(eyeTargetName);
                target.transform.parent = headTarget.head.target.transform;
            }
            RetrieveBone(headTarget.humanoid, isLeft ? HumanBodyBones.LeftEye : HumanBodyBones.RightEye);
            if (bone.transform != null) {
                target.transform.position = bone.transform.position;
                target.transform.rotation = headTarget.head.bone.targetRotation; // target.transform.rotation;
            }

            DoMeasurements();

            bone.minAngles = minEyeAngles;
            bone.maxAngles = maxEyeAngles;

            InitEyeLids();

            SkinnedMeshRenderer[] avatarMeshes = HeadTarget.FindAvatarMeshes(headTarget.humanoid);
            if (avatarMeshes != null && avatarMeshes.Length > 0) {
                int meshWithBlendshapes = HeadTarget.FindMeshWithBlendshapes(avatarMeshes);
                string[] blendshapes = HeadTarget.GetBlendshapes(avatarMeshes[meshWithBlendshapes]);

                if (isLeft) {
                    HeadTarget.FindBlendshapeWith(blendshapes, "AU45", "Left", ref headTarget.face.leftEye.blink);
                } else {
                    HeadTarget.FindBlendshapeWith(blendshapes, "AU45", "Right", ref headTarget.face.rightEye.blink);
                }
            }
        }

        private void InitEyeLids() {
            if (upperLid != null) {
                string lidTargetName = isLeft ? "LeftUpperEyeLid" : "RightUpperEyeLid";
                upperLid.target.transform = headTarget.head.target.transform.Find(lidTargetName);
                if (upperLid.target.transform == null) {
                    upperLid.target.transform = NewTargetTransform(lidTargetName);
                    upperLid.target.transform.parent = headTarget.head.target.transform;
                }
                if (upperLid.bone.transform != null) {
                    upperLid.target.transform.position = upperLid.bone.transform.position;
                    upperLid.target.transform.rotation = headTarget.head.target.transform.rotation;
                }
                upperLid.DoMeasurements();
                upperLid.MatchTargetToAvatar();
            }
            if (lowerLid != null) {
                string lidTargetName = isLeft ? "LeftLowerEyeLid" : "RightLowerEyeLid";
                lowerLid.target.transform = headTarget.head.target.transform.Find(lidTargetName);
                if (lowerLid.target.transform == null) {
                    lowerLid.target.transform = NewTargetTransform(lidTargetName);
                    lowerLid.target.transform.parent = headTarget.head.target.transform;
                }
                if (lowerLid.bone.transform != null) {
                    lowerLid.target.transform.position = lowerLid.bone.transform.position;
                    lowerLid.target.transform.rotation = headTarget.head.target.transform.rotation;
                }
                lowerLid.DoMeasurements();
                lowerLid.MatchTargetToAvatar();
            }
        }

        public static Transform FindUpperLidBone(Transform[] lidBones, Transform eyeBone) {
            if (lidBones != null) {
                int lidBoneIndex = GetUpperLidBoneIndex(eyeBone, lidBones);
                if (lidBoneIndex == 0) {
                    return lidBones[0];
                } else if (lidBoneIndex == 1) {
                    return lidBones[1];
                }
            }
            return null;
        }

        public static Transform FindLowerLidBone(Transform[] lidBones, Transform eyeBone) {
            if (lidBones != null) {
                int lidBoneIndex = GetUpperLidBoneIndex(eyeBone, lidBones);
                if (lidBoneIndex == 0) {
                    return lidBones[1];
                } else if (lidBoneIndex == 1) {
                    return lidBones[0];
                }
            }
            return null;
        }

        private static int GetUpperLidBoneIndex(Transform eyeBone, Transform[] lidBones) {
            for (int i = 0; i < lidBones.Length; i++) {
                if (lidBones[i].name.Contains("Up"))
                    return i;
            }
            return 2;
            //Quaternion fromNormEye = Quaternion.identity;
            //Vector3 toForward = Quaternion.Inverse(fromNormEye) * Vector3.forward;
            //Vector3 lid0Forward = lidBones[0].forward;//rotation * toForward;
            //if (lid0Forward.y > eyeBone.forward.y)
            //    return 0;
            //else
            //    return 1;
        }

        public static Transform[] FindLidBones(Transform rootBone, Transform eyeBone) {
            if (eyeBone == null)
                return null;

            Transform[] allBones = rootBone.GetComponentsInChildren<Transform>();
            Transform[] lidBones = new Transform[2];
            int lidIndex = 0;
            for (int i = 0; i < allBones.Length; i++) {
                if (allBones[i].position == eyeBone.position && allBones[i].rotation != eyeBone.rotation) {
                    if (lidIndex >= 2) {
                        return null;
                        // We should not find more than 2 additional bones
                    } else {
                        lidBones[lidIndex] = allBones[i];
                        lidIndex++;
                    }
                }
            }

            return lidIndex == 2 ? lidBones : null;
            // We should find exactly 2 bones
        }

    }
}
#endif