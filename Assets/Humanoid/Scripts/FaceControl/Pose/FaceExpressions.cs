/*
#if hFACE
using System.Collections.Generic;
using UnityEngine;
using Passer;

namespace Humanoid {
    [System.Serializable]
    public class FaceExpressions : Poses {
        public enum ExpressionId {
            Unknown,
            // Built-in poses
            Neutral,
            Smile,
            Pucker,
            Frown,
            BlinkLeft,
            BlinkRight,
            // Visemes
            BMP_Closed,
            EE_Wide,
            FV,
            OO_Narrow,
            AW_OH_UH,
            // to do
        };
        private const int nBuiltinPoses = (int)ExpressionId.AW_OH_UH + 1;

        public Expression[] expressions;
        public Expression rest = new Expression(null, "_rest", null, false);
        public Expression neutral = new Expression("Neutral", null, false);
        public Expression smile = new Expression("Smile", DefaultSmile, false);
        public Expression pucker = new Expression("Pucker", DefaultPucker, false);
        public Expression frown = new Expression("Frown", DefaultFrown, false);
        public Expression blinkLeft = new Expression("Blink Left", (face, pose) => DefaultBlink(face, pose, true), false);
        public Expression blinkRight = new Expression("Blink Right", (face, pose) => DefaultBlink(face, pose, false), false);
        public List<Expression> customPoses = new List<Expression>();

        public Expression BMP_Closed = new Expression("B/M/P/Closed", null, false);
        public Expression EE_Wide = new Expression("EE/Wide", DefaultWide, false);
        public Expression FV = new Expression("F/V", null, false);
        public Expression OO_Narrow = new Expression("oo/Narrow", DefaultNarrow, false);
        public Expression AW_OH_UH = new Expression("AW/OH/UH", DefaultAwOhUh, false);

        public void Update() {
            rest.ShowPose(1);
            foreach (Expression expression in expressions)
                expression.ShowPose();
        }

        #region Init
        public void InitPoses(FaceTarget faceTarget) {
            bool wasInitialized = expressions != null;

            if (expressions == null || expressions.Length < nBuiltinPoses + customPoses.Count)
            expressions = new Expression[nBuiltinPoses + customPoses.Count];
            expressions[(int)ExpressionId.Unknown] = new Expression(null, "_unknown"); // null, false);
            expressions[(int)ExpressionId.Neutral] = neutral;
            expressions[(int)ExpressionId.Smile] = smile;
            expressions[(int)ExpressionId.Pucker] = pucker;
            expressions[(int)ExpressionId.Frown] = frown;
            expressions[(int)ExpressionId.BlinkLeft] = blinkLeft;
            expressions[(int)ExpressionId.BlinkRight] = blinkRight;

            expressions[(int)ExpressionId.BMP_Closed] = BMP_Closed;
            expressions[(int)ExpressionId.EE_Wide] = EE_Wide;
            expressions[(int)ExpressionId.FV] = FV;
            expressions[(int)ExpressionId.OO_Narrow] = OO_Narrow;
            expressions[(int)ExpressionId.AW_OH_UH] = AW_OH_UH;
            for (int i = 0; i < customPoses.Count; i++)
                expressions[nBuiltinPoses + i] = customPoses[i];


            foreach (Expression pose in expressions) {
                pose.rest = rest;
                if (!wasInitialized) {
                    pose.value = 0;
                    pose.Reset(faceTarget);
                }
            }

            if (!isAnyPoseSet(expressions)) {
                neutral.value = 1;
                neutral.isCurrent = true;
            }

            if (!Application.isPlaying && faceTarget.expressions.neutral.value == 1)
                CheckRestPose(faceTarget.expressions.rest, faceTarget);
        }

        private static void DefaultSmile(FaceTarget faceTarget, Pose pose) {
            pose.bones = new List<Pose.Bone>();

            AddBoneToExpression(pose, faceTarget.mouth.lipLeft, new Vector3(-0.005F, 0.01F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lipRight, new Vector3(0.005F, 0.01F, 0));

            AddBoneToExpression(pose, faceTarget.mouth.upperLipLeft, new Vector3(-0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.2F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.upperLipRight, new Vector3(0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.2F, 0));

            AddBoneToExpression(pose, faceTarget.mouth.lowerLipLeft, new Vector3(-0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.3F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lowerLipRight, new Vector3(0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.3F, 0));

            AddBoneToExpression(pose, faceTarget.leftCheek, new Vector3(-0.005F, 0.005F, 0));
            AddBoneToExpression(pose, faceTarget.rightCheek, new Vector3(0.005F, 0.005F, 0));
        }

        private static void DefaultFrown(FaceTarget faceTarget, Pose pose) {
            pose.bones = new List<Pose.Bone>();

            AddBoneToExpression(pose, faceTarget.leftBrow.outer, new Vector3(0.002F, -0.002F, 0));
            AddBoneToExpression(pose, faceTarget.leftBrow.center, new Vector3(0.003F, -0.004F, 0));
            AddBoneToExpression(pose, faceTarget.leftBrow.inner, new Vector3(0.004F, -0.006F, 0));

            AddBoneToExpression(pose, faceTarget.rightBrow.inner, new Vector3(-0.004F, -0.006F, 0));
            AddBoneToExpression(pose, faceTarget.rightBrow.center, new Vector3(0.003F, -0.004F, 0));
            AddBoneToExpression(pose, faceTarget.rightBrow.outer, new Vector3(0.002F, -0.002F, 0));

            AddBoneToExpression(pose, faceTarget.mouth.lipLeft, new Vector3(0.005F, 0, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lipRight, new Vector3(-0.005F, 0F, 0));

            AddBoneToExpression(pose, faceTarget.mouth.upperLipLeft, new Vector3(-0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.2F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.upperLipRight, new Vector3(0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.2F, 0));

            AddBoneToExpression(pose, faceTarget.mouth.lowerLipLeft, new Vector3(-0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.3F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lowerLipRight, new Vector3(0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.3F, 0));

            AddBoneToExpression(pose, faceTarget.leftCheek, new Vector3(-0.005F, 0.005F, 0));
            AddBoneToExpression(pose, faceTarget.rightCheek, new Vector3(0.005F, 0.005F, 0));

        }

        private static void DefaultPucker(FaceTarget faceTarget, Pose pose) {
            pose.bones = new List<Pose.Bone>();

            AddBoneToExpression(pose, faceTarget.mouth.lipLeft, new Vector3(0.005F, 0, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lipRight, new Vector3(-0.005F, 0, 0));

            AddBoneToExpression(pose, faceTarget.mouth.upperLipLeft, new Vector3(0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.2F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.upperLipRight, new Vector3(-0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.2F, 0));

            AddBoneToExpression(pose, faceTarget.mouth.lowerLipLeft, new Vector3(0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.3F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lowerLipRight, new Vector3(-0.002F, 0.000F, 0), Quaternion.identity, new Vector3(0, -0.3F, 0));

            AddBoneToExpression(pose, faceTarget.leftCheek, new Vector3(0.005F, 0.005F, 0));
            AddBoneToExpression(pose, faceTarget.rightCheek, new Vector3(-0.005F, 0.005F, 0));

        }

        public static void DefaultBlink(FaceTarget faceTarget, Pose pose, bool isLeft) {
            pose.bones = new List<Pose.Bone>();

            EyeTarget eyeTarget = isLeft ? faceTarget.leftEye : faceTarget.rightEye;
            AddBoneToExpression(pose, eyeTarget.upperLid, Quaternion.Euler(30, 0, 0));
            AddBoneToExpression(pose, eyeTarget.lowerLid, Quaternion.Euler(-5, 0, 0));

            FaceBone cheek = isLeft ? faceTarget.leftCheek : faceTarget.rightCheek;
            AddBoneToExpression(pose, cheek, new Vector3(0, 0.005F, 0));

            EyeBrow eyeBrow = isLeft ? faceTarget.leftBrow : faceTarget.rightBrow;
            AddBoneToExpression(pose, eyeBrow.outer, new Vector3(0, -0.007F, 0));
            AddBoneToExpression(pose, eyeBrow.center, new Vector3(0, -0.005F, 0));
            AddBoneToExpression(pose, eyeBrow.inner, new Vector3(0, -0.002F, 0));

            FaceBone lipCorner = isLeft ? faceTarget.mouth.lipLeft : faceTarget.mouth.lipRight;
            AddBoneToExpression(pose, lipCorner, new Vector3(0, 0.005F, 0));
        }

        public static void DefaultWide(FaceTarget faceTarget, Pose pose) {
            pose.bones = new List<Pose.Bone>();

            AddBoneToExpression(pose, faceTarget.mouth.lipLeft, new Vector3(-0.008F, 0, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lipRight, new Vector3(0.008F, 0, 0));

            AddBoneToExpression(pose, faceTarget.mouth.lowerLipLeft, new Vector3(0, -0.005F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lowerLip, new Vector3(0, -0.005F, 0));
            AddBoneToExpression(pose, faceTarget.mouth.lowerLipRight, new Vector3(0, -0.005F, 0));

            AddBoneToExpression(pose, faceTarget.jaw, Quaternion.Euler(5, 0, 0));
        }

        public static void DefaultNarrow(FaceTarget faceTarget, Pose pose) {
            pose.bones = new List<Pose.Bone>();

            AddBoneToExpression(pose, faceTarget.jaw, Quaternion.Euler(5, 0, 0));

            AddBoneToExpression(pose, faceTarget.mouth.upperLipLeft, new Vector3(0.002F, 0, 0.005F));
            AddBoneToExpression(pose, faceTarget.mouth.upperLip, new Vector3(0, 0, 0.005F));
            AddBoneToExpression(pose, faceTarget.mouth.upperLipRight, new Vector3(-0.002F, 0, 0.005F));

            AddBoneToExpression(pose, faceTarget.mouth.lipLeft, new Vector3(0.008F, 0, 0.002F));
            AddBoneToExpression(pose, faceTarget.mouth.lipRight, new Vector3(-0.008F, 0, 0.002F));

            AddBoneToExpression(pose, faceTarget.mouth.lowerLipLeft, new Vector3(0.002F, 0, 0.005F));
            AddBoneToExpression(pose, faceTarget.mouth.lowerLip, new Vector3(0, 0, 0.005F));
            AddBoneToExpression(pose, faceTarget.mouth.lowerLipRight, new Vector3(-0.002F, 0, 0.005F));
        }

        public static void DefaultAwOhUh(FaceTarget faceTarget, Pose pose) {
            pose.bones = new List<Pose.Bone>();

            AddBoneToExpression(pose, faceTarget.mouth.upperLipLeft, new Vector3(0.000F, 0.001F, 0.005F));
            AddBoneToExpression(pose, faceTarget.mouth.upperLip, new Vector3(0, 0.001F, 0.005F));
            AddBoneToExpression(pose, faceTarget.mouth.upperLipRight, new Vector3(-0.000F, 0.001F, 0.005F));

            AddBoneToExpression(pose, faceTarget.mouth.lipLeft, new Vector3(0.008F, 0, 0.002F));
            AddBoneToExpression(pose, faceTarget.mouth.lipRight, new Vector3(-0.008F, 0, 0.002F));

            //AddBoneToExpression(pose, faceTarget.mouth.lowerLipLeft, new Vector3(0.000F, 0, 0.005F));
            //AddBoneToExpression(pose, faceTarget.mouth.lowerLip, new Vector3(0, 0, 0.005F));
            //AddBoneToExpression(pose, faceTarget.mouth.lowerLipRight, new Vector3(-0.000F, 0, 0.005F));

            AddBoneToExpression(pose, faceTarget.jaw, Quaternion.Euler(15, 0, 0));
        }

        private static void AddBoneToExpression(Pose pose, HumanoidTarget.TargetedBone bone, Quaternion targetRotation) {
            if (bone == null)
                return;

            Pose.Bone poseBone = pose.AddBone(bone);
            poseBone.targetRotation = targetRotation;
        }

        private static void AddBoneToExpression(Pose pose, HumanoidTarget.TargetedBone bone, Vector3 targetPosition) {
            if (bone == null)
                return;

            Pose.Bone poseBone = pose.AddBone(bone);
            poseBone.targetPosition = targetPosition;
            poseBone.targetRotation = Quaternion.identity;
            poseBone.targetScale = Vector3.one;
        }

        private static void AddBoneToExpression(Pose pose, HumanoidTarget.TargetedBone bone, Vector3 targetPosition, Quaternion targetRotation) {
            if (bone == null)
                return;

            Pose.Bone poseBone = pose.AddBone(bone);
            poseBone.targetPosition = targetPosition;
            poseBone.targetRotation = targetRotation;
            poseBone.targetScale = Vector3.one;
        }

        private static void AddBoneToExpression(Pose pose, HumanoidTarget.TargetedBone bone, Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale) {
            if (bone == null)
                return;

            Pose.Bone poseBone = pose.AddBone(bone);
            poseBone.targetPosition = targetPosition;
            poseBone.targetRotation = targetRotation;
            poseBone.targetScale = targetScale;
        }
        #endregion
    }

    
    public class FaceExpressions_MB : MonoBehaviour {
        public Expression rest = new Expression(null, "_rest");
        public Expression smile = new Expression("Smile");
        public Expression pucker = new Expression("Pucker");
        public Expression frown = new Expression("Frown");
        public Expression blinkLeft = new Expression("Blink Left");
        public Expression blinkRight = new Expression("Blink Right");
        public Expression[] expressions;

        // visemes
        public Expression BMP_Closed = new Expression("B/M/P/Closed");
        public Expression EE_Wide = new Expression("EE/Wide");
        public Expression FV = new Expression("F/V");
        public Expression OO_Narrow = new Expression("oo/Narrow");
        public Expression AW_OH_UH = new Expression("AW/OH/UH");
        public Expression[] visemes;

        // visemes
        //public Expression sil;
        //public Expression PP; 
        //public Expression FF;
        //public Expression TH;
        //public Expression DD;
        //public Expression kk;
        //public Expression CH;
        //public Expression SS;
        //public Expression nn;
        //public Expression RR;
        //public Expression aa;
        //public Expression E;
        //public Expression ih;
        //public Expression oh;
        //public Expression ou;

        public Expression testExpression = new Expression("Mouth open");
        public List<Expression> customExpressions = new List<Expression>();

        public FaceTarget faceTarget;
#if hSALSA
        private CrazyMinnow.SALSA.Salsa3D salsa;
#endif

        public void Show(Expression[] expressions) {
            rest.ShowPose(1);
            foreach (Expression expression in expressions)
                expression.ShowPose();
        }

        private void Start() {
            HeadTarget headTarget = GetComponent<HeadTarget>();
            faceTarget = headTarget.face;

            InitVisemes();
            faceTarget.directFaceMovements = false;
#if hSALSA
            salsa = GetComponent<CrazyMinnow.SALSA.Salsa3D>();
            if (salsa == null) {
                salsa = faceTarget.headTarget.humanoid.GetComponent<CrazyMinnow.SALSA.Salsa3D>();
            }
#endif
        }

        public void InitExpressions() {
            expressions = new Expression[] {
                smile,
                pucker,
                frown,
                blinkLeft,
                blinkRight
            };
            foreach (Expression expression in expressions)
                expression.rest = rest;

            DefaultBlinkExpression(blinkLeft, faceTarget.leftEye);
            DefaultBlinkExpression(blinkRight, faceTarget.rightEye);
        }

        public void InitVisemes() {
            visemes = new Expression[] {
                BMP_Closed,
                EE_Wide,
                FV,
                OO_Narrow,
                AW_OH_UH
            };
        }

        private void Update() {
#if hSALSA
            EE_Wide.value = salsa.sayAmount.sayMedium / 100;
            OO_Narrow.value = salsa.sayAmount.saySmall / 100;
            AW_OH_UH.value = salsa.sayAmount.sayLarge / 100;
            Show(visemes);
#else
            smile.value = faceTarget.headTarget.smileValue;
            pucker.value = faceTarget.headTarget.puckerValue;
            frown.value = faceTarget.headTarget.frownValue;
            blinkLeft.value = faceTarget.headTarget.face.leftEye.closed;
            blinkRight.value = faceTarget.headTarget.face.leftEye.closed;
            Show(expressions);
#endif
        }

        #region Blink
        public static void DefaultBlinkExpression(Expression blink, EyeTarget eyeTarget) {
            if (eyeTarget.lowerLid.target.transform == null || eyeTarget.upperLid.target.transform == null)
                return;

            if (eyeTarget.lowerLid != null && eyeTarget.upperLid != null) // && (blink.bones == null || blink.bones.Count == 0)) 
            {
                blink.bones = new List<Pose.Bone>();
                Vector3 lidDistance = eyeTarget.upperLid.target.transform.position - eyeTarget.lowerLid.target.transform.position;

                AddBoneToExpression(blink, eyeTarget.upperLid.bone.transform, Vector3.Lerp(Vector3.zero, -lidDistance, 0.9F));
                AddBoneToExpression(blink, eyeTarget.lowerLid.bone.transform, Vector3.Lerp(Vector3.zero, lidDistance, 0.1F));
            }
        }
        #endregion

        #region Visemes
        public void DefaultVisemes() {
            DefaultWide(EE_Wide, faceTarget);
            DefaultNarrow(OO_Narrow, faceTarget);
            DefaultAwOhUh(AW_OH_UH, faceTarget);
        }

        public static void DefaultWide(Expression visemeWide, FaceTarget faceTarget) {
            //if (eyeTarget.lowerLid != null && eyeTarget.upperLid != null && (visemeWide.bones == null || visemeWide.bones.Count == 0)) 
            {
                visemeWide.bones = new List<Pose.Bone>();

                AddBoneToExpression(visemeWide, faceTarget.jaw.bone.transform, Quaternion.Euler(5, 0, 0));

                AddBoneToExpression(visemeWide, faceTarget.mouth.lipLeft.bone.transform, new Vector3(-0.008F, 0, 0));
                AddBoneToExpression(visemeWide, faceTarget.mouth.lipRight.bone.transform, new Vector3(0.008F, 0, 0));

                AddBoneToExpression(visemeWide, faceTarget.mouth.lowerLipLeft.bone.transform, new Vector3(0, -0.005F, 0));
                AddBoneToExpression(visemeWide, faceTarget.mouth.lowerLip.bone.transform, new Vector3(0, -0.005F, 0));
                AddBoneToExpression(visemeWide, faceTarget.mouth.lowerLipRight.bone.transform, new Vector3(0, -0.005F, 0));
            }
        }

        public static void DefaultNarrow(Expression visemeWide, FaceTarget faceTarget) {
            //if (eyeTarget.lowerLid != null && eyeTarget.upperLid != null && (visemeWide.bones == null || visemeWide.bones.Count == 0)) 
            {
                visemeWide.bones = new List<Pose.Bone>();

                AddBoneToExpression(visemeWide, faceTarget.jaw.bone.transform, Quaternion.Euler(5, 0, 0));

                AddBoneToExpression(visemeWide, faceTarget.mouth.upperLipLeft.bone.transform, new Vector3(0.002F, 0, 0.005F));
                AddBoneToExpression(visemeWide, faceTarget.mouth.upperLip.bone.transform, new Vector3(0, 0, 0.005F));
                AddBoneToExpression(visemeWide, faceTarget.mouth.upperLipRight.bone.transform, new Vector3(-0.002F, 0, 0.005F));

                AddBoneToExpression(visemeWide, faceTarget.mouth.lipLeft.bone.transform, new Vector3(0.008F, 0, 0.002F));
                AddBoneToExpression(visemeWide, faceTarget.mouth.lipRight.bone.transform, new Vector3(-0.008F, 0, 0.002F));

                AddBoneToExpression(visemeWide, faceTarget.mouth.lowerLipLeft.bone.transform, new Vector3(0.002F, 0, 0.005F));
                AddBoneToExpression(visemeWide, faceTarget.mouth.lowerLip.bone.transform, new Vector3(0, 0, 0.005F));
                AddBoneToExpression(visemeWide, faceTarget.mouth.lowerLipRight.bone.transform, new Vector3(-0.002F, 0, 0.005F));
            }
        }

        public static void DefaultAwOhUh(Expression visemeAhOhUh, FaceTarget faceTarget) {
            //if (eyeTarget.lowerLid != null && eyeTarget.upperLid != null && (visemeWide.bones == null || visemeWide.bones.Count == 0)) 
            {
                visemeAhOhUh.bones = new List<Pose.Bone>();

                AddBoneToExpression(visemeAhOhUh, faceTarget.jaw.bone.transform, Quaternion.Euler(15, 0, 0));

                AddBoneToExpression(visemeAhOhUh, faceTarget.mouth.upperLipLeft.bone.transform, new Vector3(0.000F, 0.001F, 0.005F));
                AddBoneToExpression(visemeAhOhUh, faceTarget.mouth.upperLip.bone.transform, new Vector3(0, 0.001F, 0.005F));
                AddBoneToExpression(visemeAhOhUh, faceTarget.mouth.upperLipRight.bone.transform, new Vector3(-0.000F, 0.001F, 0.005F));

                AddBoneToExpression(visemeAhOhUh, faceTarget.mouth.lipLeft.bone.transform, new Vector3(0.008F, 0, 0.002F));
                AddBoneToExpression(visemeAhOhUh, faceTarget.mouth.lipRight.bone.transform, new Vector3(-0.008F, 0, 0.002F));

                AddBoneToExpression(visemeAhOhUh, faceTarget.mouth.lowerLipLeft.bone.transform, new Vector3(0.000F, 0, 0.005F));
                AddBoneToExpression(visemeAhOhUh, faceTarget.mouth.lowerLip.bone.transform, new Vector3(0, 0, 0.005F));
                AddBoneToExpression(visemeAhOhUh, faceTarget.mouth.lowerLipRight.bone.transform, new Vector3(-0.000F, 0, 0.005F));
            }
        }

        #endregion

        private static void AddBoneToExpression(Expression expression, Transform boneTransform, Vector3 relativePosition) {
            if (boneTransform == null)
                return;
            //Pose.Bone poseBone = expression.AddBone(boneTransform);
            //poseBone.relativePosition = boneTransform.parent.InverseTransformVector(relativePosition);
        }
        private static void AddBoneToExpression(Expression expression, Transform boneTransform, Quaternion relativeRotation) {
            if (boneTransform == null)
                return;
            //Pose.Bone poseBone = expression.AddBone(boneTransform);
            //poseBone.relativeRotation = Quaternion.Inverse(boneTransform.rotation) * relativeRotation * boneTransform.rotation;
        }
        private static void AddBoneToExpression(Expression expression, Transform boneTransform, Vector3 relativePosition, Quaternion relativeRotation) {
            if (boneTransform == null)
                return;
            //Pose.Bone poseBone = expression.AddBone(boneTransform);
            //poseBone.relativePosition = boneTransform.TransformVector(relativePosition);
            //poseBone.relativeRotation = Quaternion.Inverse(boneTransform.rotation) * relativeRotation * boneTransform.rotation;
        }
    }
    

    [System.Serializable]
    public class Expression : Pose {
        public Expression(string _name, ExpressionReset _reset = null, bool _isCustom = true) : base(_name, null, _isCustom) {
            if (_reset == null)
                reset = DefaultReset;
            else
                reset = _reset;
        }
        public Expression(Expression _base, string _name, ExpressionReset _reset = null, bool _isCustom = true) : base(_base, _name, null, _isCustom) {
            if (_reset == null)
                reset = DefaultReset;
            else
                reset = _reset;
        }

        public new void Reset(FaceTarget face) {
            if (reset != null)
                reset(face, this);
        }
        public delegate void ExpressionReset(FaceTarget face, Pose pose);
        private ExpressionReset reset;
        private static void DefaultReset(FaceTarget face, Pose pose) {
            pose.bones = new List<Pose.Bone>();
            //foreach (Bone bone in pose.bones)
            //    bone.Reset();

        }
    }
    // We call facial poses expressions

}
#endif
*/