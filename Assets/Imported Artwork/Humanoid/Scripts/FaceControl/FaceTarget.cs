using UnityEngine;


#if hFACE

namespace Passer {
    using Humanoid;
    using Humanoid.Tracking;

    [System.Serializable]
    public class FaceTarget : ITarget {
        public HeadTarget headTarget;

        public FaceTarget(HeadTarget headTarget) {
            this.headTarget = headTarget;

            leftEye = new EyeTarget(this, true);
            rightEye = new EyeTarget(this, false);

            leftBrow = new EyeBrow(true);
            rightBrow = new EyeBrow(false);

            leftEar = new FaceBone() { boneId = Bone.LeftEar };
            rightEar = new FaceBone() { boneId = Bone.RightEar };

            leftCheek = new FaceBone() { boneId = Bone.LeftCheek };
            rightCheek = new FaceBone() { boneId = Bone.RightCheek };

            jaw = new TargetedJawBone(headTarget);
        }

        public FaceMovements faceMovements = new FaceMovements();

        #region Sensors
        public MicrophoneHead microphone = new MicrophoneHead();
#if hKINECT2
        public Kinect2Face kinect2Face = new Kinect2Face();
#endif
#if hREALSENSE
        public IntelRealsenseFace realsenseFace = new IntelRealsenseFace();
#endif
#if hTOBII
        public TobiiHead tobiiHead = new TobiiHead();
#endif
#if hDLIB
        public Tracking.Dlib.Face dlib = new Tracking.Dlib.Face();
#endif

        private UnityFaceSensor[] _sensors;
        public UnityFaceSensor[] sensors {
            get {
                if (_sensors == null)
                    InitSensors();
                return _sensors;
            }
        }

        public void InitSensors() {
#if hKINECT2 && hFACE
            if (headTarget.kinectFace == null)
                headTarget.kinectFace = new Kinect2Face();
#endif
            if (_sensors == null) {
                _sensors = new UnityFaceSensor[] {
#if hKINECT2 && hFACE
                    headTarget.kinectFace,
#endif
#if hREALSENSE
                    realsenseFace,
#endif
#if hDLIB
                    dlib,
#endif
                    null
                };
            }
        }

        public void StartSensors() {
            for (int i = 0; i < sensors.Length - 1; i++)
                sensors[i].Start(headTarget.humanoid, headTarget.transform);
        }

        protected void UpdateSensors() {
            for (int i = 0; i < sensors.Length - 1; i++)
                sensors[i].Update();
        }

        #endregion

        #region SubTargets

        public EyeTarget leftEye = null;
        public EyeTarget rightEye = null;
        public float lastBlink;

        public EyeBrow leftBrow = null;
        public EyeBrow rightBrow = null;

        public FaceBone leftEar = null;
        public FaceBone rightEar = null;

        public FaceBone leftCheek = new FaceBone();
        public FaceBone rightCheek = new FaceBone();

        public Nose nose = new Nose();

        public Mouth mouth = new Mouth();

        public Vector3 gazeDirection = Vector3.forward;
        // hips relative
        public Vector3 localGazeDirection = Vector3.forward;
        public Vector3 focusPoint;
        public GameObject focusObject;

        public void GazeTo(Vector3 position, float confidence) {
            Vector3 eyePosition = headTarget.GetEyePosition();

            Vector3 direction = (position - eyePosition).normalized;
            SetGazeDirection(direction, confidence);
        }

        public void SetGazeDirection(Vector3 direction, float confidence) {
            gazeDirection = direction;
            localGazeDirection = headTarget.humanoid.hipsTarget.hips.target.transform.InverseTransformDirection(direction);

            // need to set eyeTargets here...
            leftEye.target.confidence.rotation = confidence;
            rightEye.target.confidence.rotation = confidence;

            Transform cameraTransform = headTarget.unityVRHead.cameraTransform;
            if (cameraTransform == null)
                return;

            InteractionPointer interaction = headTarget.GetComponentInChildren<InteractionPointer>();
            if (interaction != null)
                interaction.SetRayDirection(gazeDirection);
        }

        #region Jaw

        public TargetedJawBone jaw = null;

        [System.Serializable]
        public class TargetedJawBone : HumanoidTarget.TargetedBone {
            private HeadTarget headTarget;

            public TargetedJawBone(HeadTarget headTarget) {
                this.headTarget = headTarget;

                boneId = Bone.Jaw;
            }

            public override void Init() {
                boneId = Bone.Jaw;
                parent = headTarget.neck;
                nextBone = null;
            }

            public override Quaternion DetermineRotation() {
                Quaternion humanoidRotation = headTarget.humanoid.hipsTarget.hips.bone.targetRotation;
                Vector3 jawForward = humanoidRotation * Quaternion.AngleAxis(45, Vector3.right) * Vector3.forward;

                Quaternion jawRotation = Quaternion.LookRotation(jawForward);
                return jawRotation;
            }
        }

        #endregion

        #endregion

        #region Configuration

        private static readonly string[] boneNames = {

            "Left Upper Lid",
            "Left Lower Lid",
            "Right Upper Lid",
            "Right Lower Lid",

            "Left Outer Brow",
            "Left Brow",
            "Left Inner Brow",
            "Right Inner Brow",
            "Right Brow",
            "Right Outer Brow",

            "Left Ear",
            "Right Ear",

            "Left Cheek",
            "Right Cheek",

            "Nose Top",
            "Nose Tip",
            "Nose Bottom Left",
            "Nose Bottom",
            "Nose Bottom Right",

            "Upper Lip Left",
            "Upper Lip",
            "Upper Lip Right",
            "Lip Left",
            "Lip Right",
            "Lower Lip Left",
            "Lower Lip",
            "Lower Lip Right",

            "Jaw"
        };

        private HumanoidTarget.TargetedBone[] bones = null;

        public Transform GetDefaultTarget() {
            throw new System.NotImplementedException();
        }

        public void RetrieveBones(HeadTarget headTarget) {
            if (headTarget == null || headTarget.humanoid == null || headTarget.humanoid.avatarRig == null)
                return;

            RetrieveEyeBones(headTarget);
            RetrieveEyeBrowBones(headTarget);
            RetrieveCheekBones(headTarget);
            RetrieveNoseBones(headTarget);
            RetreiveJawBone(headTarget, ref jaw);
            RetrieveMouthBones(headTarget);
        }

        private void RetrieveEyeBrowBones(HeadTarget headTarget) {
            GetDefaultBone(headTarget.humanoid.avatarRig, ref leftBrow.outer, "LeftEyebrowUp", null, "BrowOuterL");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref leftBrow.center, "LeftEyebrowMiddle", null, null);
            GetDefaultBone(headTarget.humanoid.avatarRig, ref leftBrow.inner, "LeftEyebrowLow", null, "BrowInnerL");

            GetDefaultBone(headTarget.humanoid.avatarRig, ref rightBrow.outer, "RightEyebrowUp", null, "BrowOuterR");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref rightBrow.center, "RightEyebrowMiddle", null, null);
            GetDefaultBone(headTarget.humanoid.avatarRig, ref rightBrow.inner, "RightEyebrowLow", null, "BrowInnerR");

            FindEyeBrowBones(headTarget);
        }

        private void FindEyeBrowBones(HeadTarget headTarget) {
            Transform[] facialBones = GetFacialBones(headTarget.head.bone.transform);


            foreach (Transform facialBone in facialBones) {
                Vector3 faceLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - headTarget.head.bone.transform.position);

                if (leftEye.bone.transform != null && facialBone.position.y > leftEye.bone.transform.position.y && Vector3.Distance(facialBone.position, leftEye.bone.transform.position) < 0.05F) {
                    Vector3 eyeLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - leftEye.bone.transform.position);
                    if (eyeLocalPosition.y > 0.01F) {
                        if (leftBrow.center.bone.transform == null && Mathf.Abs(eyeLocalPosition.x) < 0.01F)
                            leftBrow.center.bone.transform = facialBone;
                        else if (leftBrow.outer.bone.transform == null && eyeLocalPosition.x < -0.01F)
                            leftBrow.outer.bone.transform = facialBone;
                        else if (leftBrow.inner.bone.transform == null && eyeLocalPosition.x > 0.01F && faceLocalPosition.x < -0.009F)
                            leftBrow.inner.bone.transform = facialBone;
                    }
                }
                if (rightEye.bone.transform != null && facialBone.position.y > rightEye.bone.transform.position.y && Vector3.Distance(facialBone.position, rightEye.bone.transform.position) < 0.05F) {
                    Vector3 localPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - rightEye.bone.transform.position);
                    if (localPosition.y > 0.01F) {
                        if (rightBrow.center.bone.transform == null && Mathf.Abs(localPosition.x) < 0.01F)
                            rightBrow.center.bone.transform = facialBone;
                        else if (rightBrow.inner.bone.transform == null && localPosition.x < -0.01F && faceLocalPosition.x > 0.009F)
                            rightBrow.inner.bone.transform = facialBone;
                        else if (rightBrow.outer.bone.transform == null && localPosition.x > 0.01F)
                            rightBrow.outer.bone.transform = facialBone;
                    }
                }
            }
        }

        private void RetrieveEyeBones(HeadTarget headTarget) {
            HumanoidTarget.GetDefaultBone(headTarget.humanoid.avatarRig, ref leftEye.bone.transform, HumanBodyBones.LeftEye, "LeftEye", "lEye", "LeftEye");
            if (leftEye.bone.transform == leftEye.target.transform)
                leftEye.bone.transform = null;
            Transform[] lidBones = EyeTarget.FindLidBones(headTarget.head.bone.transform, leftEye.bone.transform);
            if (leftEye.upperLid.bone.transform == null)
                leftEye.upperLid.bone.transform = EyeTarget.FindUpperLidBone(lidBones, leftEye.bone.transform);
            HumanoidTarget.GetDefaultBone(headTarget.humanoid.avatarRig, ref leftEye.upperLid.bone.transform, "LeftUpperLid", null, "UpperLidL");
            if (leftEye.lowerLid.bone.transform == null)
                leftEye.lowerLid.bone.transform = EyeTarget.FindLowerLidBone(lidBones, leftEye.bone.transform);
            HumanoidTarget.GetDefaultBone(headTarget.humanoid.avatarRig, ref leftEye.lowerLid.bone.transform, "LeftLowerLid", null, "LowerLidL");

            HumanoidTarget.GetDefaultBone(headTarget.humanoid.avatarRig, ref rightEye.bone.transform, HumanBodyBones.RightEye, "RightEye", "rEye", "RightEye");
            if (rightEye.bone.transform == rightEye.target.transform)
                rightEye.bone.transform = null;
            lidBones = EyeTarget.FindLidBones(headTarget.head.bone.transform, rightEye.bone.transform);
            if (rightEye.upperLid.bone.transform == null)
                rightEye.upperLid.bone.transform = EyeTarget.FindUpperLidBone(lidBones, rightEye.bone.transform);
            HumanoidTarget.GetDefaultBone(headTarget.humanoid.avatarRig, ref rightEye.upperLid.bone.transform, "RightUpperLid", null, "UpperLidR");
            if (rightEye.lowerLid.bone.transform == null)
                rightEye.lowerLid.bone.transform = EyeTarget.FindLowerLidBone(lidBones, rightEye.bone.transform);
            HumanoidTarget.GetDefaultBone(headTarget.humanoid.avatarRig, ref rightEye.lowerLid.bone.transform, "RightLowerLid", null, "LowerLidR");
        }

        private void RetrieveCheekBones(HeadTarget headTarget) {
            GetDefaultBone(headTarget.humanoid.avatarRig, ref leftCheek, "LeftCheek", null, "CheekL", "Cheek_L");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref rightCheek, "RightCheek", null, "CheekR", "Cheek_R");
        }

        private void RetrieveNoseBones(HeadTarget headTarget) {
            GetDefaultBone(headTarget.humanoid.avatarRig, ref nose.top, "NoseTop", null, null);

            GetDefaultBone(headTarget.humanoid.avatarRig, ref nose.tip, "NoseBase", null, null);

            GetDefaultBone(headTarget.humanoid.avatarRig, ref nose.bottomLeft, "LeftNose", null, null);
            GetDefaultBone(headTarget.humanoid.avatarRig, ref nose.bottom, null, null, "Nostrils");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref nose.bottomRight, "RightNose", null, null);

            FindNoseBones(headTarget);
        }

        private void FindNoseBones(HeadTarget headTarget) {
            if (leftEye.bone.transform == null || rightEye.bone.transform == null)
                return;

            Transform[] facialBones = GetFacialBones(headTarget.head.bone.transform);

            foreach (Transform facialBone in facialBones) {
                Vector3 localPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - headTarget.head.bone.transform.position);
                if (Mathf.Abs(localPosition.x) < 0.001F) {
                    // facialBone  is in the middle
                    float headAngle = Vector3.Angle(Vector3.up, headTarget.head.bone.targetRotation * Vector3.up);
                    if (headAngle < 10 && Mathf.Abs(facialBone.position.y - leftEye.bone.transform.position.y) < 0.01F)
                        // around same height as the eyes
                        nose.top.bone.transform = facialBone;

                    else if (facialBone.position.y - leftEye.bone.transform.position.y < -0.02F) {
                        float eyeDistance = Vector3.Distance(leftEye.bone.transform.position, rightEye.bone.transform.position);
                        float nosetipDistance = Vector3.Distance(leftEye.bone.transform.position, facialBone.position);
                        if (Mathf.Abs(eyeDistance - nosetipDistance) < 0.01F) {
                            Vector3 eyeLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - leftEye.bone.transform.position);
                            // distance to the eye is about the same as distance between the eyes?
                            if (eyeLocalPosition.z > 0.01F && nose.tip.bone.transform == null)
                                nose.tip.bone.transform = facialBone;
                            else if (nose.tip.bone.transform != null) {
                                Vector3 oldEyeLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (nose.tip.bone.transform.position - leftEye.bone.transform.position);
                                if (eyeLocalPosition.z > oldEyeLocalPosition.z) { // look for foremost nosetip
                                    if (nose.bottom.bone.transform == null)
                                        // old tip is likely the bottom of the nose
                                        nose.bottom.bone.transform = nose.tip.bone.transform;
                                    nose.tip.bone.transform = facialBone;
                                }
                            }
                        }
                    }
                }
            }
            foreach (Transform facialBone in facialBones) {
                Vector3 localPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - headTarget.head.bone.transform.position);
                if (nose.tip.bone.transform != null && Mathf.Abs(facialBone.position.y - nose.tip.bone.transform.position.y) < 0.02F) {
                    // same height as nos tip?
                    if (localPosition.x < -0.001F && localPosition.x > -0.02F) {
                        // somewhat left from the middle
                        nose.bottomLeft.bone.transform = facialBone;
                    }
                    else if (localPosition.x > 0.001F && localPosition.x < 0.02F) {
                        // somewhat right from the middle
                        nose.bottomRight.bone.transform = facialBone;
                    }
                }
            }
        }

        private void RetrieveMouthBones(HeadTarget headTarget) {
            GetDefaultBone(headTarget.humanoid.avatarRig, ref mouth.upperLip, "LipsSuperior", null, "LipUpper");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref mouth.upperLipLeft, "LeftLipsSuperiorMiddle", null, "LipUpperL");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref mouth.upperLipRight, "RightLipsSuperiorMiddle", null, "LipUpperR");

            GetDefaultBone(headTarget.humanoid.avatarRig, ref mouth.lipLeft, "LeftLips", null, "LipCornerL");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref mouth.lipRight, "RightLips", null, "LipCornerR");

            GetDefaultBone(headTarget.humanoid.avatarRig, ref mouth.lowerLip, "LipsInferior", null, "LipLower");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref mouth.lowerLipLeft, "LeftLipsInferior", null, "LipLowerL");
            GetDefaultBone(headTarget.humanoid.avatarRig, ref mouth.lowerLipRight, "RightLipsInferior", null, "LipLowerR");

            FindMouthBones(headTarget);

            HumanoidTarget.TargetedBone headBone = headTarget.head;
            HumanoidTarget.TargetedBone jawBone = headTarget.face.jaw;

            mouth.upperLip.parent = headBone;
            mouth.upperLipLeft.parent = headBone;
            mouth.upperLipRight.parent = headBone;

            mouth.lowerLip.parent = jawBone;
            mouth.lowerLipLeft.parent = jawBone;
            mouth.lowerLipRight.parent = jawBone;
        }

        private void FindMouthBones(HeadTarget headTarget) {
            Transform[] facialBones = GetFacialBones(headTarget.head.bone.transform);
            foreach (Transform facialBone in facialBones) {
                FindUpperLipBone(headTarget, facialBone);
                FindUpperLipBones(headTarget, facialBone);
            }

            if (jaw.bone.transform != null) {
                Transform[] jawFacialBones = GetFacialBones(jaw.bone.transform);
                foreach (Transform facialBone in jawFacialBones) {
                    FindLowerLipBone(headTarget, facialBone);
                    FindLowerLipBones(headTarget, facialBone);
                }
                foreach (Transform facialBone in jawFacialBones)
                    FindLipCornerBones(headTarget, facialBone);
            }

            foreach (Transform facialBone in facialBones) {
                FindLowerLipBone(headTarget, facialBone);
                FindLowerLipBones(headTarget, facialBone);
            }
            foreach (Transform facialBone in facialBones) {
                FindLipCornerBones(headTarget, facialBone);
            }
        }

        private void FindLowerLipBone(HeadTarget headTarget, Transform facialBone) {
            if (mouth.lowerLip.bone.transform != null)
                return;

            Vector3 localPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - headTarget.head.bone.transform.position);
            if (localPosition.z < 0.08F)
                // Must be in front of the head bone
                return;

            if (Mathf.Abs(localPosition.x) > 0.001F)
                // Must be in the middle
                return;

            if (!facialBone.name.Contains("lip") && !facialBone.name.Contains("Lip") && !facialBone.name.Contains("LIP"))
                // must contain lip in the name
                return;

            if (mouth.upperLip.bone.transform != null) {
                Vector3 upperLipLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.upperLip.bone.transform.position);
                if (upperLipLocalPosition.y > 0.001F || upperLipLocalPosition.y < -0.02F)
                    // Must be lower than the upper lip, but max 2 cm
                    return;
            }


            mouth.lowerLip.bone.transform = facialBone;
        }

        private void FindLowerLipBones(HeadTarget headTarget, Transform facialBone) {
            if (mouth.lowerLip.bone.transform == null)
                // We need an lower lip bone first;
                return;

            Vector3 lowerLipLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.lowerLip.bone.transform.position);
            if (lowerLipLocalPosition.y < 0 || Mathf.Abs(lowerLipLocalPosition.x) > 0.02F)
                // must be lower than and close to the lower lip bone
                return;

            if (mouth.lowerLipLeft.bone.transform == null && lowerLipLocalPosition.x < -0.001F) {
                // left side
                if (mouth.lipLeft.bone.transform != null) {
                    Vector3 lipLeftLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.lipLeft.bone.transform.position);
                    if (lipLeftLocalPosition.x < 0 || lipLeftLocalPosition.y < 0)
                        // must be to the right and higher than the left lip corner
                        return;
                }

                mouth.lowerLipLeft.bone.transform = facialBone;
            }
            else if (mouth.lowerLipRight.bone.transform == null && lowerLipLocalPosition.x > 0.001F) {
                // right side
                if (mouth.lipRight.bone.transform != null) {
                    Vector3 lipRightLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.lipRight.bone.transform.position);
                    if (lipRightLocalPosition.x > 0 || lipRightLocalPosition.y < 0)
                        // must be to the right and higer thaan the left lip corner
                        return;
                }

                mouth.lowerLipRight.bone.transform = facialBone;
            }
        }

        private void FindUpperLipBone(HeadTarget headTarget, Transform facialBone) {
            if (mouth.upperLip.bone.transform != null)
                return;

            Quaternion invHeadRotation = Quaternion.Inverse(headTarget.head.bone.targetRotation);

            Vector3 localBonePosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - headTarget.head.bone.transform.position);
            if (localBonePosition.z > 0.15F || Mathf.Abs(localBonePosition.x) > 0.001F || localBonePosition.z < 0.07F)
                // Must be in front of the head bone, in the middle and lower than the head
                return;

            if ((nose.tip.bone.transform != null && nose.tip.bone.transform == facialBone) ||
                (nose.bottom.bone.transform != null && nose.bottom.bone.transform == facialBone))
                //ust not be the nose tip or bottom bone.
                return;


            if (!facialBone.name.Contains("lip") && !facialBone.name.Contains("Lip") && !facialBone.name.Contains("LIP"))
                // must contain lip in the name
                return;

            if (nose.bottom.bone.transform != null) {
                Vector3 noseLocalPosition = invHeadRotation * (facialBone.position - nose.bottom.bone.transform.position);
                if (noseLocalPosition.y < -0.02F)
                    // must be max 2 cm below the nose bottom
                    return;
            }

            if (nose.tip.bone.transform != null) {
                Vector3 noseLocalPosition1 = invHeadRotation * (facialBone.position - nose.tip.bone.transform.position);
                if (noseLocalPosition1.y < -0.03F)
                    //if (facialBone.position.y - nose.tip.bone.transform.position.y < -0.03F)
                    // must be max 3 cm below the nose tip
                    return;
            }

            if (mouth.lowerLip.bone.transform != null) {
                Vector3 lowerLipLocalPosition = invHeadRotation * (facialBone.position - mouth.lowerLip.bone.transform.position);
                if (lowerLipLocalPosition.y < 0)
                    // must above the lower lip
                    return;
            }

            mouth.upperLip.bone.transform = facialBone;
        }

        private void FindUpperLipBones(HeadTarget headTarget, Transform facialBone) {
            if (mouth.upperLip.bone.transform == null)
                // We need an upper lip bone first;
                return;

            Vector3 upperLipLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.upperLip.bone.transform.position);
            if (upperLipLocalPosition.y > 0 || Mathf.Abs(upperLipLocalPosition.x) > 0.02F)
                // must be lower than and close to the upper lip bone
                return;

            if (mouth.upperLipLeft.bone.transform == null && upperLipLocalPosition.x < -0.001F) {
                // left side
                if (mouth.lipLeft.bone.transform != null) {
                    Vector3 lipLeftLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.lipLeft.bone.transform.position);
                    if (lipLeftLocalPosition.x < 0 || lipLeftLocalPosition.y < 0)
                        // must be to the right and higher than the left lip corner
                        return;
                }

                mouth.upperLipLeft.bone.transform = facialBone;
            }
            else if (mouth.upperLipRight.bone.transform == null && upperLipLocalPosition.x > 0.001F) {
                // right side
                if (mouth.lipRight.bone.transform != null) {
                    Vector3 lipRightLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.lipRight.bone.transform.position);
                    if (lipRightLocalPosition.x > 0 || lipRightLocalPosition.y < 0)
                        // must be to the right and higer thaan the left lip corner
                        return;
                }

                mouth.upperLipRight.bone.transform = facialBone;
            }
        }

        private void FindLipCornerBones(HeadTarget headTarget, Transform facialBone) {
            if (mouth.lipLeft.bone.transform != null && mouth.lipRight.bone.transform != null)
                return;

            if (mouth.upperLip.bone.transform == null || mouth.lowerLip.bone.transform == null)
                // We need an upper and lower lip bone first;
                return;

            Vector3 upperLipLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.upperLip.bone.transform.position);
            Vector3 lowerLipLocalPosition = Quaternion.Inverse(headTarget.head.bone.targetRotation) * (facialBone.position - mouth.lowerLip.bone.transform.position);
            if (upperLipLocalPosition.y < -0.01F || lowerLipLocalPosition.y > 0.01F)
                // must be between upper and lower lip height
                return;
            if (upperLipLocalPosition.z < -0.02F || upperLipLocalPosition.z > 0.02F)
                // Must be a bit behind upper lip
                return;

            if (mouth.lipLeft.bone.transform == null && upperLipLocalPosition.x < -0.015F && lowerLipLocalPosition.x < -0.015)
                mouth.lipLeft.bone.transform = facialBone;
            else if (mouth.lipRight.bone.transform == null && upperLipLocalPosition.x > 0.015F && lowerLipLocalPosition.x > 0.015)
                mouth.lipRight.bone.transform = facialBone;
        }

        private static void RetreiveJawBone(HeadTarget headTarget, ref TargetedJawBone jaw) {
            HumanoidTarget.GetDefaultBone(headTarget.humanoid.avatarRig, ref jaw.bone.transform, Bone.Jaw, "Jaw");
            HumanoidTarget.GetDefaultTargetBone(headTarget.humanoid.targetsRig, ref jaw.target.transform, Bone.Jaw, "Jaw");
            if (jaw.target.transform.parent == null)
                jaw.target.transform.parent = headTarget.head.target.transform;
        }

        private static void GetDefaultBone(Animator rig, ref FaceBone faceBone, params string[] boneNames) {
            HumanoidTarget.GetDefaultBone(rig, ref faceBone.bone.transform, boneNames);
        }

        private Transform[] GetFacialBones(Transform headBone) {
            int nFacialBones = CountFacialBones(headBone);
            Transform[] facialBones = new Transform[nFacialBones];

            int i = 0;
            GetFacialBones(headBone, ref facialBones, ref i);
            return facialBones;
        }

        private void GetFacialBones(Transform headBone, ref Transform[] facialBones, ref int i) {
            if (headBone == null)
                return;

            for (int j = 0; j < headBone.childCount; j++) {
                Transform facialBone = headBone.GetChild(j);
                if (facialBone.childCount > 1)
                    GetFacialBones(facialBone, ref facialBones, ref i);
                else {
                    // First Person Camera may be set on the head
                    // We need to ignore this transform
                    Camera camera = facialBone.GetComponent<Camera>();
                    if (camera == null)
                        facialBones[i++] = facialBone;
                }
            }
        }

        private int CountFacialBones(Transform headBone) {
            if (headBone == null)
                return 0;

            int n = 0;
            for (int i = 0; i < headBone.childCount; i++) {
                Transform facialBone = headBone.GetChild(i);
                if (facialBone.childCount > 0)
                    n += CountFacialBones(facialBone);
                else {
                    // First Person Camera may be set on the head
                    // We need to ignore this transform
                    Camera camera = facialBone.GetComponent<Camera>();
                    if (camera == null)
                        n++;
                }
            }

            return n;
        }

        #region ITarget
        HumanoidTarget.TargetedBone[] ITarget.GetBones() {
            if (bones == null || bones.Length == 0) {
                //bones = new Target.TargetedBone[(int)FaceBoneId.LastBone];
            }

            bones = new HumanoidTarget.TargetedBone[] {

                leftEye.upperLid,
                leftEye.lowerLid,
                rightEye.upperLid,
                rightEye.lowerLid,

                leftBrow.outer,
                leftBrow.center,
                leftBrow.inner,
                rightBrow.inner,
                rightBrow.center,
                rightBrow.outer,

                leftEar,
                rightEar,

                leftCheek,
                rightCheek,

                nose.top,
                nose.tip,
                nose.bottomLeft,
                nose.bottom,
                nose.bottomRight,

                mouth.upperLipLeft,
                mouth.upperLip,
                mouth.upperLipRight,
                mouth.lipLeft,
                mouth.lipRight,
                mouth.lowerLipLeft,
                mouth.lowerLip,
                mouth.lowerLipRight,

                jaw
            };

            for (int i = 0; i < bones.Length; i++)
                bones[i].name = boneNames[i];

            return bones;
        }

        private string[] blendShapes = null;
        public string[] GetBlendshapeNames() {
            if (headTarget.smRenderer == null) {
                SkinnedMeshRenderer[] avatarMeshes = HeadTarget.FindAvatarMeshes(headTarget.humanoid);
                //string[] avatarMeshNames = HeadTarget.DistillAvatarMeshNames(avatarMeshes);
                int meshWithBlendshapes = HeadTarget.FindBlendshapemesh(avatarMeshes, headTarget.smRenderer);
                headTarget.smRenderer = avatarMeshes[meshWithBlendshapes];
            }

            blendShapes = HeadTarget.GetBlendshapes(headTarget.smRenderer);
            return blendShapes;
        }
        public SkinnedMeshRenderer blendshapeRenderer {
            get { return headTarget.smRenderer; }
        }

        public int FindBlendshape(string namepart) {
            for (int i = 0; i < blendShapes.Length; i++) {
                if (blendShapes[i].Equals(namepart)) {
                    return i;
                }
            }
            return -1;
        }

        public void SetBlendshapeWeight(string name, float weight) {
            int index = FindBlendshape(name);
            headTarget.smRenderer.SetBlendShapeWeight(index, weight * 100);
        }
        public float GetBlendshapeWeight(string name) {
            int index = FindBlendshape(name);
            return (headTarget.smRenderer.GetBlendShapeWeight(index) / 100);
        }
        #endregion

        #endregion

        #region Expressions        
        public int pose;
        public PoseMixer poseMixer = new PoseMixer();
        public void SetPose(Pose pose, float weight = 1) {
            poseMixer.SetPoseValue(pose, weight);
        }
        #endregion

        #region Init
        public void InitAvatar(HeadTarget headTarget) {
            this.headTarget = headTarget;

            leftEye.Init(headTarget, true);
            rightEye.Init(headTarget, false);

            InitEyeBrows();
            InitEars();
            InitCheeks();
            InitNose();
            jaw.Init();
            InitMouth();

            jaw.DoMeasurements();
        }

        //public void NewComponent(HeadTarget _headtarget) {
        //    headTarget = _headtarget;
        //}

        public void InitComponent() {
            if (headTarget == null)
                return;

            localGazeDirection = Vector3.forward;
            lastBlink = 0;

            InitSensors();

            InitEyeBrows();
            InitEars();
            InitCheeks();
            InitNose();
            jaw.Init();
            InitMouth();

            faceMovements.StartFaceMovements(headTarget.humanoid, this);

            //expressions.InitPoses(this);
        }

        private void InitEyeBrows() {
            InitFaceBone(headTarget, leftBrow.outer, "LeftOuterBrow");
            InitFaceBone(headTarget, leftBrow.center, "LeftCenterBrow");
            InitFaceBone(headTarget, leftBrow.inner, "LeftInnerBrow");

            InitFaceBone(headTarget, rightBrow.outer, "RightOuterBrow");
            InitFaceBone(headTarget, rightBrow.center, "RightCenterBrow");
            InitFaceBone(headTarget, rightBrow.inner, "RightInnerBrow");
        }

        public void InitEars() {
            InitFaceBone(headTarget, leftEar, "Left_Ear");
            InitFaceBone(headTarget, rightEar, "Right_Ear");
        }

        public void InitCheeks() {
            InitFaceBone(headTarget, leftCheek, "Left_Cheek");
            InitFaceBone(headTarget, rightCheek, "Right_Cheek");
        }

        private void InitNose() {
            InitFaceBone(headTarget, nose.top, "NoseTop");

            InitFaceBone(headTarget, nose.tip, "NoseTip");

            InitFaceBone(headTarget, nose.bottomLeft, "NoseBottomLeft");
            InitFaceBone(headTarget, nose.bottom, "NoseBottom");
            InitFaceBone(headTarget, nose.bottomRight, "NoseBottomRight");
        }

        private void InitMouth() {
            InitFaceBone(headTarget, mouth.upperLipLeft, "UpperLipLeft");
            InitFaceBone(headTarget, mouth.upperLip, "UpperLip");
            InitFaceBone(headTarget, mouth.upperLipRight, "UpperLipRight");

            InitFaceBone(headTarget, mouth.lipLeft, "LipLeft", jaw);
            InitFaceBone(headTarget, mouth.lipRight, "LipRight", jaw);

            InitFaceBone(headTarget, mouth.lowerLipLeft, "LowerLipLeft", jaw);
            InitFaceBone(headTarget, mouth.lowerLip, "LowerLip", jaw);
            InitFaceBone(headTarget, mouth.lowerLipRight, "LowerLipRight", jaw);
        }

        private void InitFaceBone(HeadTarget headTarget, FaceBone faceBone, string name, HumanoidTarget.TargetedBone _parent = null) {
            faceBone.parent = _parent;
            if (faceBone.parent != null && faceBone.parent.bone.transform == null && faceBone.bone.transform != null)
                faceBone.parent.bone.transform = faceBone.bone.transform.parent;

            if (faceBone.parent == null || faceBone.parent.bone.transform == null ||
                (faceBone.bone.transform != null && !IsAncestor(faceBone.parent.bone.transform, faceBone.bone.transform))
                ) {
                faceBone.parent = headTarget.head;
            }

            faceBone.target.transform = faceBone.parent.target.transform.Find(name);
            if (faceBone.target.transform == null) {
                faceBone.target.transform = HumanoidTarget.TargetedBone.NewTargetTransform(name);
            }

            faceBone.target.transform.parent = faceBone.parent.target.transform;

            if (faceBone.bone.transform != null) {
                faceBone.target.transform.rotation = faceBone.parent.target.transform.rotation;
                faceBone.target.toBoneRotation = Quaternion.Inverse(faceBone.target.transform.rotation) * faceBone.bone.transform.rotation;
                //faceBone.target.toBoneRotation = Quaternion.Inverse(faceBone.bone.targetRotation) * faceBone.bone.transform.rotation;

                faceBone.startPosition = Quaternion.Inverse(faceBone.parent.bone.targetRotation) * (faceBone.bone.transform.position - faceBone.parent.bone.transform.position);

                faceBone.target.transform.localPosition = faceBone.startPosition;

                faceBone.target.confidence.position = 0.1F;
                faceBone.target.confidence.rotation = 0.1F;
            }
            else {

                faceBone.target.confidence.position = 0;
                faceBone.target.confidence.rotation = 0;
            }
            faceBone.DoMeasurements();
        }

        private bool IsAncestor(Transform potentialAncestor, Transform transform) {
            if (transform.parent == null)
                return false;
            else if (transform.parent == potentialAncestor)
                return true;
            else
                return IsAncestor(potentialAncestor, transform.parent);
        }

        public void MatchTargetsToAvatar() {
            leftBrow.outer.MatchTargetToAvatar();
            leftBrow.center.MatchTargetToAvatar();
            leftBrow.inner.MatchTargetToAvatar();
            rightBrow.inner.MatchTargetToAvatar();
            rightBrow.center.MatchTargetToAvatar();
            rightBrow.outer.MatchTargetToAvatar();

            leftEar.MatchTargetToAvatar();
            rightEar.MatchTargetToAvatar();

            leftCheek.MatchTargetToAvatar();
            rightCheek.MatchTargetToAvatar();

            nose.top.MatchTargetToAvatar();
            nose.tip.MatchTargetToAvatar();
            nose.bottomLeft.MatchTargetToAvatar();
            nose.bottom.MatchTargetToAvatar();
            nose.bottomRight.MatchTargetToAvatar();

            mouth.upperLipLeft.MatchTargetToAvatar();
            mouth.upperLip.MatchTargetToAvatar();
            mouth.upperLipRight.MatchTargetToAvatar();

            jaw.MatchTargetToAvatar();

            mouth.lipLeft.MatchTargetToAvatar();
            mouth.lipRight.MatchTargetToAvatar();

            //if (jaw.bone.transform != null)
            //    jaw.target.transform.position = jaw.bone.transform.position;
            //if (jaw.target.transform != null && headTarget.head.bone.transform != null)
            //    jaw.target.transform.rotation = headTarget.head.bone.targetRotation * Quaternion.AngleAxis(45, Vector3.right);
            //jaw.bone.baseRotation = Quaternion.AngleAxis(45, Vector3.right);

            mouth.lowerLipLeft.MatchTargetToAvatar();
            mouth.lowerLip.MatchTargetToAvatar();
            mouth.lowerLipRight.MatchTargetToAvatar();

        }
        #endregion

        public Vector3 GetWorldPosition(Transform target) {
            return GetWorldPosition(target.position);
        }

        public Vector3 GetWorldPosition(Vector3 targetPosition) {
            if (headTarget == null)
                return Vector3.zero;
            else
                return headTarget.transform.TransformPoint(targetPosition);
        }

        #region Update
        public void UpdateTarget() {
            //expressions.Show(expressions.rest, expressions.expressions);

            UpdateSensors();

            if (jaw.target.confidence.rotation < 0.1F)
                BasicAudioJawMovement(jaw);
        }

        private void BasicAudioJawMovement(HumanoidTarget.TargetedBone jaw) {
            float energy = Mathf.Clamp01(headTarget.audioEnergy);
            float jawAngle = 45 + energy * 10;
            if (jaw.target.transform != null)
                jaw.target.transform.localRotation = Quaternion.Euler(jawAngle, 0, 0);
        }

        public bool directFaceMovements = true;
        public void UpdateMovements() {
            if (directFaceMovements)
                FaceMovements.Update(this);
        }
        #endregion

        #region DrawRigs
        public void DrawTargetRig() {
            if (jaw.target.transform != null)
                DrawTargetLine(jaw.target.transform.position, jaw.target.transform.position + jaw.target.transform.forward * 0.1F, jaw.target.confidence.rotation);

            DrawBrowsTargets();
            DrawNoseTargets();
            DrawMouthTargets();
        }

        private void DrawBrowsTargets() {
            if (leftBrow.center.bone.transform != null) {
                DrawTargetLine(leftBrow.outer, leftBrow.center);
                DrawTargetLine(leftBrow.center, leftBrow.inner);
            }
            else {
                DrawTargetLine(leftBrow.outer, leftBrow.inner);
            }

            if (rightBrow.center.bone.transform != null) {
                DrawTargetLine(rightBrow.outer, rightBrow.center);
                DrawTargetLine(rightBrow.center, rightBrow.inner);
            }
            else {
                DrawTargetLine(rightBrow.outer, rightBrow.inner);
            }
        }

        private void DrawNoseTargets() {
            if (nose.top.isPresent && nose.tip.isPresent)
                DrawTargetLine(nose.top, nose.tip);

            if (nose.bottomLeft.isPresent && nose.bottomRight.isPresent) {
                if (nose.bottom.isPresent) {
                    DrawTargetLine(nose.bottomLeft, nose.bottom);
                    DrawTargetLine(nose.bottom, nose.bottomRight);
                }
                else {
                    DrawTargetLine(nose.bottomLeft, nose.bottomRight);
                }
            }
        }

        private void DrawMouthTargets() {
            if (mouth.lipLeft.bone.transform == null || mouth.lipRight.bone.transform == null)
                return;

            DrawTargetLine(mouth.lipLeft, mouth.upperLipLeft);
            if (mouth.upperLip.bone.transform != null) {
                DrawTargetLine(mouth.upperLipLeft, mouth.upperLip);
                DrawTargetLine(mouth.upperLip, mouth.upperLipRight);
            }
            else {
                DrawTargetLine(mouth.upperLipLeft, mouth.upperLipRight);
            }
            DrawTargetLine(mouth.upperLipRight, mouth.lipRight);

            DrawTargetLine(mouth.lipLeft, mouth.lowerLipLeft);
            if (mouth.lowerLip.bone.transform != null) {
                DrawTargetLine(mouth.lowerLipLeft, mouth.lowerLip);
                DrawTargetLine(mouth.lowerLip, mouth.lowerLipRight);
            }
            else {
                DrawTargetLine(mouth.lowerLipLeft, mouth.lowerLipRight);
            }
            DrawTargetLine(mouth.lowerLipRight, mouth.lipRight);
        }

        private void DrawTargetLine(FaceBone start, FaceBone end) {
            if (start.target.transform == null || end.target.transform == null)
                return;

            float confidence = Mathf.Min(start.target.confidence.position, end.target.confidence.position);
            DrawTargetLine(start.target.transform.position, end.target.transform.position, confidence);
        }

        private void DrawTargetLine(Vector3 start, Vector3 end, float confidence) {
            if (confidence > 0.8F)
                Debug.DrawLine(start, end, Color.green);
            else if (confidence > 0.6F)
                Debug.DrawLine(start, end, Color.yellow);
            else if (confidence > 0.2F)
                Debug.DrawLine(start, end, Color.red);
            else
                Debug.DrawLine(start, end, Color.black);
        }

        public void DrawAvatarRig() {
            if (jaw.bone.transform != null)
                Debug.DrawRay(jaw.bone.transform.position, jaw.bone.targetRotation * Vector3.forward * 0.1F, Color.cyan);

            DrawAvatarBrow(leftBrow);
            DrawAvatarBrow(rightBrow);

            DrawAvatarEyeLid(leftEye.lowerLid);
            DrawAvatarEyeLid(leftEye.upperLid);
            DrawAvatarEyeLid(rightEye.lowerLid);
            DrawAvatarEyeLid(rightEye.upperLid);

            DrawAvatarNose(nose);

            DrawAvatarMouth(mouth);
        }

        private static void DrawAvatarBrow(EyeBrow brow) {
            if (brow.outer.bone.transform == null || brow.inner.bone.transform == null)
                return;

            if (brow.center.bone.transform == null)
                DrawAvatarLine(brow.outer, brow.inner);
            else {
                DrawAvatarLine(brow.outer, brow.center);
                DrawAvatarLine(brow.center, brow.inner);
            }
        }

        private static void DrawAvatarEyeLid(HumanoidTarget.TargetedBone eyeLid) {
            if (eyeLid.bone.transform == null)
                return;

            Debug.DrawLine(
                eyeLid.bone.transform.position + eyeLid.bone.transform.rotation * new Vector3(-0.01F, 0, 0),
                eyeLid.bone.transform.position + eyeLid.bone.transform.rotation * new Vector3(0.01F, 0, 0),
                Color.cyan);
        }

        private static void DrawAvatarNose(Nose nose) {
            if (nose.top.bone.transform != null && nose.tip.bone.transform != null)
                DrawAvatarLine(nose.top, nose.tip);
            if (nose.bottomLeft.bone.transform != null && nose.bottomRight.bone.transform != null) {
                if (nose.bottom.bone.transform == null)
                    DrawAvatarLine(nose.bottomLeft, nose.bottomRight);
                else {
                    DrawAvatarLine(nose.bottomLeft, nose.bottom);
                    DrawAvatarLine(nose.bottom, nose.bottomRight);
                }
            }
        }

        private static void DrawAvatarMouth(Mouth mouth) {
            if (mouth.lipLeft.bone.transform == null || mouth.lipRight.bone.transform == null)
                return;

            // upper lip
            if (mouth.upperLipLeft.bone.transform != null && mouth.upperLipRight.bone.transform != null) {
                DrawAvatarLine(mouth.lipLeft, mouth.upperLipLeft);
                if (mouth.upperLip.bone.transform == null) {
                    DrawAvatarLine(mouth.upperLipLeft, mouth.upperLipRight);
                }
                else {
                    DrawAvatarLine(mouth.upperLipLeft, mouth.upperLip);
                    DrawAvatarLine(mouth.upperLip, mouth.upperLipRight);
                }
                DrawAvatarLine(mouth.upperLipRight, mouth.lipRight);
            }
            else if (mouth.upperLip.bone.transform != null) {
                DrawAvatarLine(mouth.lipLeft, mouth.upperLip);
                DrawAvatarLine(mouth.upperLip, mouth.lipRight);
            }
            else {
                DrawAvatarLine(mouth.lipLeft, mouth.lipRight);
            }

            // lower lip
            if (mouth.lowerLipLeft.bone.transform != null && mouth.lowerLipRight.bone.transform != null) {
                DrawAvatarLine(mouth.lipLeft, mouth.lowerLipLeft);
                if (mouth.lowerLip.bone.transform == null) {
                    DrawAvatarLine(mouth.lowerLipLeft, mouth.lowerLipRight);
                }
                else {
                    DrawAvatarLine(mouth.lowerLipLeft, mouth.lowerLip);
                    DrawAvatarLine(mouth.lowerLip, mouth.lowerLipRight);
                }
                DrawAvatarLine(mouth.lowerLipRight, mouth.lipRight);
            }
            else if (mouth.lowerLip.bone.transform != null) {
                DrawAvatarLine(mouth.lipLeft, mouth.lowerLip);
                DrawAvatarLine(mouth.lowerLip, mouth.lipRight);
            }
        }

        private static void DrawAvatarLine(FaceBone start, FaceBone end) {
            Debug.DrawLine(start.bone.transform.position, end.bone.transform.position, Color.cyan);
        }

        #endregion
    }

    [System.Serializable]
    public class EyeBrow {
        public FaceBone outer = new FaceBone();
        public FaceBone center = new FaceBone();
        public FaceBone inner = new FaceBone();

        public EyeBrow(bool isLeft) {
            outer.boneId = isLeft ? Bone.LeftOuterBrow : Bone.RightOuterBrow;
            center.boneId = isLeft ? Bone.LeftBrow : Bone.RightBrow;
            inner.boneId = isLeft ? Bone.LeftInnerBrow : Bone.RightInnerBrow;
        }
    }

    [System.Serializable]
    public class Nose {
        public FaceBone top = new FaceBone();

        public FaceBone tip = new FaceBone();

        public FaceBone bottomLeft = new FaceBone();
        public FaceBone bottom = new FaceBone();
        public FaceBone bottomRight = new FaceBone();

        public Nose() {
            top.boneId = Bone.NoseTop;
            tip.boneId = Bone.NoseTip;
            bottomLeft.boneId = Bone.NoseBottomLeft;
            bottom.boneId = Bone.NoseBottom;
            bottomRight.boneId = Bone.NoseBottomRight;
        }
    }

    [System.Serializable]
    public class Mouth {
        public FaceBone upperLipLeft = new FaceBone();
        public FaceBone upperLip = new FaceBone();
        public FaceBone upperLipRight = new FaceBone();

        public FaceBone lipLeft = new FaceBone();
        public FaceBone lipRight = new FaceBone();

        public FaceBone lowerLipLeft = new FaceBone();
        public FaceBone lowerLip = new FaceBone();
        public FaceBone lowerLipRight = new FaceBone();
        public FaceBone[] bones;

        public Mouth() {
            bones = new FaceBone[] {
                upperLipLeft,
                upperLip,
                upperLipRight,
                lipLeft,
                lipRight,
                lowerLipLeft,
                lowerLip,
                lowerLipRight
            };

            upperLipLeft.boneId = Bone.UpperLipLeft;
            upperLip.boneId = Bone.UpperLip;
            upperLipRight.boneId = Bone.UpperLipRight;

            lipLeft.boneId = Bone.LipLeft;
            lipRight.boneId = Bone.LipRight;

            lowerLipLeft.boneId = Bone.LowerLipLeft;
            lowerLip.boneId = Bone.LowerLip;
            lowerLipRight.boneId = Bone.LowerLipRight;
        }
    }

    [System.Serializable]
    public class FaceBone : HumanoidTarget.TargetedBone {
        public Vector3 startPosition;

        public override void MatchTargetToAvatar() {
            if (bone.transform == null || target.transform == null)
                return;

            target.transform.position = bone.transform.position;
            target.transform.rotation = bone.targetRotation;

            DetermineBasePosition();
            DetermineBaseRotation();
        }
    }

}
#endif