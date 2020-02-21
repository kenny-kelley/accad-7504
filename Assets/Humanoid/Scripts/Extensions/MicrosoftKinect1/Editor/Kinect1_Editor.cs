using UnityEngine;
using UnityEditor;

namespace Passer.Humanoid {

    public class Kinect1_Editor : Tracker_Editor {

#if hKINECT1

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, Kinect1Tracker _tracker)
                : base(serializedObject, targetObjs, _tracker, "kinect1") {
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "Kinect1");
                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    tracker.trackerTransform = (Transform)EditorGUILayout.ObjectField("Tracker Transform", tracker.trackerTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.kinect1, headTarget, "kinect1") {
            }

            public override void Inspector() {
                if (headTarget.humanoid.kinect1.enabled) {
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
                    //if (enabledProp.boolValue) {
                    //    EditorGUI.indentLevel++;
                    //    EditorGUILayout.BeginHorizontal();
                    //    headTarget.kinect1.headTracking = EditorGUILayout.ToggleLeft("Head Tracking", headTarget.kinect1.headTracking, GUILayout.MinWidth(80));
                    //    if (headTarget.kinect1.headTracking)
                    //        headTarget.kinect1.rotationTrackingAxis = (Kinect1Head.RotationTrackingAxis)EditorGUILayout.EnumPopup(headTarget.kinect1.rotationTrackingAxis);
                    //    EditorGUILayout.EndHorizontal();
                    //    EditorGUI.indentLevel--;
                    //}
                }
            }
        }
        #endregion

        #region Arm
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.kinect1, handTarget, "kinect1") {
            }

            public override void Inspector() {
                if (handTarget.humanoid.kinect1.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, handTarget);
            }
        }
        #endregion

        #region Torso
        public class HipsTargetProps : HipsTarget_Editor.TargetProps {
            public HipsTargetProps(SerializedObject serializedObject, HipsTarget hipsTarget)
                : base(serializedObject, hipsTarget.kinect1, hipsTarget, "kinect1") {
            }

            public override void Inspector() {
                if (hipsTarget.humanoid.kinect1.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, hipsTarget);
            }
        }
        #endregion

        #region Leg

        public class FootTargetProps : FootTarget_Editor.TargetProps {
            public FootTargetProps(SerializedObject serializedObject, FootTarget footTarget)
                : base(serializedObject, footTarget.kinect1, footTarget, "kinect1") {
            }

            public override void Inspector() {
                if (footTarget.humanoid.kinect1.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, footTarget);
            }
        }

        #endregion

#endif
    }
}
