#if hFACE
using UnityEditor;
using UnityEngine;
using Passer.Humanoid;

namespace Passer {

    public class FaceTarget_Editor {

        private static SerializedProperty leftEyeBlinkProp;
        private static SerializedProperty rightEyeBlinkProp;

        public static void OnEnable(SerializedObject serializedObject, HeadTarget headTarget) {
            leftEyeBlinkProp = serializedObject.FindProperty("face.leftEye.blink");
            rightEyeBlinkProp = serializedObject.FindProperty("face.rightEye.blink");

            InitExpressions(headTarget.face);
        }

        public static void OnDisable(SerializedObject serializedObject, HeadTarget headTarget) {
            //if (currentPose != null && currentPose.configure) {
            //    currentPose.configure = false;
            //    currentPose = null;
            //}

            //Expression_Editor.UpdateExpression(headTarget.face.expressions, currentPose);
            headTarget.face.poseMixer.Cleanup();
            if (!Application.isPlaying)
                headTarget.face.poseMixer.ShowPose(headTarget.humanoid);

            headTarget.face.UpdateMovements();
        }

        #region Inspector
        public static void FocusObjectInspector(FaceTarget faceTarget) {
            EditorGUILayout.ObjectField("Focus Object", faceTarget.focusObject, typeof(GameObject), true);
        }

        #endregion

        #region SubTargets
        private static bool showTargets = false;
        private static bool showLeftEyeTargets = true;
        private static bool showRightEyeTargets = true;

        public static void OnInspectorGUI(HeadTarget headTarget) {
            showTargets = EditorGUILayout.Foldout(showTargets, "Sub targets", true);
            if (showTargets) {
                EditorGUI.indentLevel++;
                showLeftEyeTargets = EditorGUILayout.Foldout(showLeftEyeTargets, "Left Eye", true);
                if (showLeftEyeTargets) {
                    EyeTargetsInspector(headTarget.face.leftEye);
                }

                showRightEyeTargets = EditorGUILayout.Foldout(showRightEyeTargets, "Right Eye", true);
                if (showRightEyeTargets) {
                    EyeTargetsInspector(headTarget.face.rightEye);
                }

                headTarget.stress = EditorGUILayout.Slider("Stress", headTarget.stress, 0, 1);
                headTarget.smileValue = EditorGUILayout.Slider("Happiness", headTarget.smileValue, 0, 1);
                AudioEnergyInspector(headTarget);
                EditorGUI.indentLevel--;
            }
        }

        private static void EyeTargetsInspector(EyeTarget eyeTarget) {
            EditorGUI.indentLevel++;
            eyeTarget.closed = EditorGUILayout.Slider("Close", eyeTarget.closed, 0, 1);
            EditorGUI.indentLevel--;
        }

        private static void AudioEnergyInspector(HeadTarget headTarget) {
            headTarget.audioEnergy = EditorGUILayout.Slider("Audio Enery", headTarget.audioEnergy, 0, 1);
        }
        #endregion

        #region Configuration
        public static void ConfigurationInspector(FaceTarget faceTarget) {
            HumanoidControl humanoid = faceTarget.headTarget.humanoid;
            SkinnedMeshRenderer[] avatarMeshes = HeadTarget.FindAvatarMeshes(humanoid);
            string[] avatarMeshNames = HeadTarget.DistillAvatarMeshNames(avatarMeshes);
            int meshWithBlendshapes = HeadTarget.FindBlendshapemesh(avatarMeshes, faceTarget.headTarget.smRenderer); //HeadTarget.FindMeshWithBlendshapes(avatarMeshes);

            meshWithBlendshapes = EditorGUILayout.Popup("Head Mesh", meshWithBlendshapes, avatarMeshNames);
            string[] blendshapes;
            if (meshWithBlendshapes < avatarMeshes.Length) {
                faceTarget.headTarget.smRenderer = avatarMeshes[meshWithBlendshapes];
                blendshapes = HeadTarget.GetBlendshapes(avatarMeshes[meshWithBlendshapes]);
            } else {
                blendshapes = new string[0];
            }

            EyeBrowsInspector(faceTarget);
            EyesInspector(faceTarget, blendshapes);
            CheeksInspector(faceTarget);
            NoseInspector(faceTarget);
            MouthInspector(faceTarget.mouth);
            faceTarget.jaw.bone.transform = (Transform)EditorGUILayout.ObjectField("Jaw", faceTarget.jaw.bone.transform, typeof(Transform), true);
        }

        private static bool showLeftBrow;
        private static bool showRightBrow;
        private static void EyeBrowsInspector(FaceTarget faceTarget) {
            showLeftBrow = EditorGUILayout.Foldout(showLeftBrow, "Left Eye Brow", true);
            if (showLeftBrow) {
                EditorGUI.indentLevel++;
                FaceBoneInspector("Brow Outer", faceTarget.leftBrow.outer);
                FaceBoneInspector("Brow", faceTarget.leftBrow.center);
                FaceBoneInspector("Brow Inner", faceTarget.leftBrow.inner);
                EditorGUI.indentLevel--;
            }

            showRightBrow = EditorGUILayout.Foldout(showRightBrow, "Right Eye Brow", true);
            if (showRightBrow) {
                EditorGUI.indentLevel++;
                FaceBoneInspector("Brow Inner", faceTarget.rightBrow.inner);
                FaceBoneInspector("Brow", faceTarget.rightBrow.center);
                FaceBoneInspector("Brow Outer", faceTarget.rightBrow.outer);
                EditorGUI.indentLevel--;
            }
        }

        private static bool showLeftEye;
        private static bool showRightEye;
        private static void EyesInspector(FaceTarget faceTarget, string[] blendshapes) {
            showLeftEye = EditorGUILayout.Foldout(showLeftEye, "Left Eye", true);
            if (showLeftEye) {
                EditorGUI.indentLevel++;
                faceTarget.leftEye.upperLid.bone.transform = (Transform)EditorGUILayout.ObjectField("Upper Lid", faceTarget.leftEye.upperLid.bone.transform, typeof(Transform), true);
                faceTarget.leftEye.bone.transform = (Transform)EditorGUILayout.ObjectField("Eye", faceTarget.leftEye.bone.transform, typeof(Transform), true);
                faceTarget.leftEye.lowerLid.bone.transform = (Transform)EditorGUILayout.ObjectField("Lower Lid", faceTarget.leftEye.lowerLid.bone.transform, typeof(Transform), true);
                leftEyeBlinkProp.intValue = EditorGUILayout.Popup("Eye Closed Blendshape", leftEyeBlinkProp.intValue, blendshapes);
                EditorGUI.indentLevel--;
            }

            showRightEye = EditorGUILayout.Foldout(showRightEye, "Right Eye", true);
            if (showRightEye) {
                EditorGUI.indentLevel++;
                faceTarget.rightEye.upperLid.bone.transform = (Transform)EditorGUILayout.ObjectField("Upper Lid", faceTarget.rightEye.upperLid.bone.transform, typeof(Transform), true);
                faceTarget.rightEye.bone.transform = (Transform)EditorGUILayout.ObjectField("Eye", faceTarget.rightEye.bone.transform, typeof(Transform), true);
                faceTarget.rightEye.lowerLid.bone.transform = (Transform)EditorGUILayout.ObjectField("Lower Lid", faceTarget.rightEye.lowerLid.bone.transform, typeof(Transform), true);
                rightEyeBlinkProp.intValue = EditorGUILayout.Popup("Eye Closed Blendshape", rightEyeBlinkProp.intValue, blendshapes);
                EditorGUI.indentLevel--;
            }
        }

        private static bool showCheeks = false;
        private static void CheeksInspector(FaceTarget faceTarget) {
            showCheeks = EditorGUILayout.Foldout(showCheeks, "Cheeck", true);
            if (showCheeks) {
                EditorGUI.indentLevel++;
                FaceBoneInspector("Left", faceTarget.leftCheek);
                FaceBoneInspector("Right", faceTarget.rightCheek);
                EditorGUI.indentLevel--;
            }
        }

        private static bool showNose = false;
        private static void NoseInspector(FaceTarget faceTarget) {
            showNose = EditorGUILayout.Foldout(showNose, "Nose", true);
            if (showNose) {
                EditorGUI.indentLevel++;
                FaceBoneInspector("Top", faceTarget.nose.top);

                FaceBoneInspector("Tip", faceTarget.nose.tip);

                FaceBoneInspector("Bottom Left", faceTarget.nose.bottomLeft);
                FaceBoneInspector("Bottom", faceTarget.nose.bottom);
                FaceBoneInspector("Bottom Right", faceTarget.nose.bottomRight);
                EditorGUI.indentLevel--;
            }
        }

        private static bool showMouth = false;
        private static void MouthInspector(Mouth mouth) {
            showMouth = EditorGUILayout.Foldout(showMouth, "Mouth", true);
            if (showMouth) {
                EditorGUI.indentLevel++;
                FaceBoneInspector("Upper Lip Left", mouth.upperLipLeft);
                FaceBoneInspector("Upper Lip", mouth.upperLip);
                FaceBoneInspector("Upper Lip Right", mouth.upperLipRight);

                FaceBoneInspector("Left Lip Corner", mouth.lipLeft);
                FaceBoneInspector("Right Lip Corner", mouth.lipRight);

                FaceBoneInspector("Lower Lip Left", mouth.lowerLipLeft);
                FaceBoneInspector("Lower Lip", mouth.lowerLip);
                FaceBoneInspector("Lower Lip Right", mouth.lowerLipRight);
                EditorGUI.indentLevel--;
            }
        }

        private static void FaceBoneInspector(string name, FaceBone faceBone) {
            Transform faceBoneTransform = (Transform)EditorGUILayout.ObjectField(name, faceBone.bone.transform, typeof(Transform), true);
            if (faceBoneTransform != faceBone.bone.transform) {
                faceBone.bone.transform = faceBoneTransform;
                faceBone.CopyBonePositionToTarget();
                faceBone.DoMeasurements();
            }
        }
        #endregion

        #region Expressions
        public static BonePose selectedBone = null;
        private string[] poseNames;

        private static void InitExpressions(FaceTarget faceTarget) {
        }

        private static bool showExpressions = false;
        public static void ExpressionsInspector(FaceTarget faceTarget) {
            EditorGUILayout.BeginHorizontal();
            showExpressions = EditorGUILayout.Foldout(showExpressions, "Expression", true);

            EditorGUI.indentLevel++;
            string[] poseNames = faceTarget.poseMixer.GetPoseNames();
            int poseIx = EditorGUILayout.Popup(faceTarget.pose, poseNames);
            if (poseIx != faceTarget.pose) {
                faceTarget.pose = poseIx;
                faceTarget.poseMixer.SetPoseValue(poseIx);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndHorizontal();

            if (showExpressions) {
                EditorGUI.indentLevel++;
                Pose_Editor.PoseMixerInspector(faceTarget.poseMixer, faceTarget.headTarget.humanoid);
                EditorGUI.indentLevel--;
            }

            if (!Application.isPlaying) {
                faceTarget.poseMixer.ShowPose(faceTarget.headTarget.humanoid);
                faceTarget.UpdateMovements();
                SceneView.RepaintAll();
            }
        }
        #endregion

        #region Scene
        public static void UpdateScene(FaceTarget faceTarget) {

            if (Application.isPlaying)
                return;

            Pose_Editor.UpdateScene(faceTarget.headTarget.humanoid, faceTarget, faceTarget.poseMixer, ref selectedBone);

            //faceTarget.UpdateMovements();
        }
        #endregion
    }
}
#endif