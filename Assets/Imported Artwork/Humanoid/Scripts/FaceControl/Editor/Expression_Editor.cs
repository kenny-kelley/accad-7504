/*
#if hFACE
using UnityEngine;
using UnityEditor;
using Passer;

namespace Humanoid {

public class Expression_Editor : Editor {
        
                public static void ExpressionListInspector(FaceExpressions faceExpressions, Expression[] expressionList, FaceTarget faceTarget, ref Pose currentPose, ref Pose.Bone selectedBone) {
                    if (expressionList == null)
                        return;

                    Expression poseToRemove = null;
                    for (int i = 0; i < expressionList.Length; i++) {
                        if (expressionList[i].name[0] == '_')
                            continue;
                        if (ExpressionInspector(faceExpressions, i, expressionList[i], ref currentPose, ref selectedBone, faceTarget))
                            poseToRemove = expressionList[i];
                    }
                    if (poseToRemove != null) {
                        faceExpressions.customPoses.Remove(poseToRemove);
                        faceExpressions.InitPoses(faceTarget);
                    }

                    if (currentPose == null) {
                        if (GUILayout.Button("Add new Pose")) {
                            Expression newPose = new Expression(faceExpressions.rest, "New Pose");
                            faceExpressions.customPoses.Add(newPose);
                            faceExpressions.InitPoses(faceTarget);
                        }
                    }
                }

                private static bool ExpressionInspector(FaceExpressions faceExpressions, int poseIndex, Expression pose, ref Pose currentPose, ref Pose.Bone selectedBone, FaceTarget faceTarget) {
                    bool toRemove = Pose_Editor.PoseInspector(faceExpressions, faceExpressions.expressions, poseIndex, pose, ref currentPose, ref selectedBone, faceTarget);
                    if (pose.configure) {
                        EditorGUI.indentLevel++;
                        currentPose = pose;
                        ExpressionEditor(pose, ref currentPose, ref selectedBone, faceTarget);
                        EditorGUI.indentLevel--;
                    }
                    return toRemove;
                }

                private static void ExpressionEditor(Expression pose, ref Pose currentPose, ref Pose.Bone selectedBone, FaceTarget faceTarget) {
                    HumanoidTarget.TargetedBone[] bones = pose.target.GetBones();
                    if (bones == null || bones.Length == 0)
                        return;

                    Pose_Editor.BlendshapeEditor(pose);

                    // We cannot reset one bone in a default expression yet
                    //EditorGUI.BeginDisabledGroup(selectedBone == null);
                    //if (GUILayout.Button("Reset Bone"))
                    //    Pose_Editor.ResetBone(selectedBone);
                    //EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Reset Expression")) {
                        selectedBone = null;
                        pose.Reset(faceTarget);
                        pose.rest.ShowPose(1);
                        pose.ShowPose();
                        faceTarget.UpdateMovements();
                    }
                }

                public static void UpdateExpression(FaceExpressions faceExpressions, Pose currentPose) {
                    if (!Application.isPlaying) {
                        SceneView.RepaintAll();

                        if (currentPose == null || !currentPose.configure)
                            faceExpressions.Update();
                    }
                }
                
        #region Scene
        static int lastControlID;
        public static void DotHandleCapSaveID(int controlID, Vector3 position, Quaternion rotation, float size, EventType et) {
            lastControlID = controlID;
            Handles.DotHandleCap(controlID, position, rotation, size, et);
        }

        static int boneIndex = -1;
        public static void UpdateScene(ITarget target, HumanoidPoseMixer poseMixer, ref Pose.Bone selectedBone) {
            MixedHumanoidPose currentPose = poseMixer.GetEditedPose();
            if (currentPose == null || !currentPose.isEdited) {
                Tools.hidden = false;
                return;
            }

            Tools.hidden = true;

            HumanoidTarget.TargetedBone[] bones = target.GetBones();
            int[] controlIds = new int[bones.Length];
            Tracking.Bone[] boneIds = new Tracking.Bone[bones.Length];

            for (int i = 0; i < bones.Length; i++) {
                if (bones[i] == null || bones[i].bone == null || bones[i].bone.transform == null)
                    continue;

                Handles.FreeMoveHandle(bones[i].bone.transform.position, bones[i].bone.transform.rotation, 0.001F, Vector3.zero, DotHandleCapSaveID);
                controlIds[i] = lastControlID;
                boneIds[i] = bones[i].boneId;
            }


            FindSelectedHandle(controlIds, boneIds, ref boneIndex);
            if (boneIndex == -1)
                return;

            //for (int i = 0; i < bones.Length; i++) {
            //    if (bones[i] == null || bones[i].bone == null || bones[i].bone.transform == null)
            //        continue;

            //    GUI.SetNextControlName(bones[i].name);
            //    bones[i].target.transform.position = Handles.FreeMoveHandle(bones[i].target.transform.position, bones[i].target.transform.rotation, 0.001F, Vector3.zero, Handles.DotHandleCap);
            //    if (selectedBone != null && selectedBone.targetedBone.bone.transform == bones[i].bone.transform) {
            //        GUIStyle style = new GUIStyle();
            //        style.normal.textColor = Color.yellow;
            //        Handles.Label(bones[i].bone.transform.position + Vector3.up * 0.01F, bones[i].name, style);
            //        Handles.color = Color.white;
            //        selectedBone.UpdatePosition(restPose);

            //        switch (Tools.current) {
            //            case Tool.Rotate:
            //                bones[i].target.transform.rotation = Handles.RotationHandle(bones[i].target.transform.rotation, bones[i].bone.transform.position);
            //                selectedBone.UpdateRotation(restPose);

            //                //bones[i].bone.transform.rotation = Handles.RotationHandle(bones[i].bone.transform.rotation, bones[i].bone.transform.position);
            //                //selectedBone.relativeRotation = Quaternion.Inverse(selectedBone.restRotation) * selectedBone.boneTransform.localRotation;
            //                break;
            //                //    case Tool.Scale:
            //                //        //Handles.ScaleHandle(selectedBone.transform.localScale, selectedBone.transform.position, selectedBone.transform.rotation, HandleUtility.GetHandleSize(selectedBone.transform.position));
            //                //        // need to all morphScale first...
            //                //        break;
            //        }
            //    }
            //}
        }

        private static void FindSelectedHandle(int[] controlIds, Tracking.Bone[] boneIds, ref int boneIndex) {
            for (int i = 0; i < controlIds.Length; i++) {
                if (controlIds[i] == GUIUtility.hotControl) {
                    boneIndex = i;
                    return;
                }
            }
            return;
        }

        //private static void FindSelectedHandle(HumanoidTarget.TargetedBone[] bones, Pose currentPose, ref Pose.Bone selectedBone) {
        //    string boneName = GUI.GetNameOfFocusedControl();
        //    int bondeIndex = -1;
        //    for (int i = 0; i < bones.Length; i++) {
        //        if (bones[i].name == boneName)
        //            bondeIndex = i;
        //    }
        //    if (bondeIndex == -1) {
        //        return;
        //    }

        //    selectedBone = currentPose.CheckBone(bones[bondeIndex]);
        //}
        #endregion
    }
}
#endif
*/