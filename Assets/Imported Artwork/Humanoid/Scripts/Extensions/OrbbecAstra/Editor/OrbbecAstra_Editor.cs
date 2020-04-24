using System.IO;
using UnityEngine;
using UnityEditor;

namespace Passer.Humanoid {

    public class Astra_Editor : Tracker_Editor {

#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_WSA_10_0)

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, AstraTracker _tracker)
                : base(serializedObject, targetObjs, _tracker, "astra") {
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "Orbbec Astra");
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
                : base(serializedObject, headTarget.astra, headTarget, "astra") {
            }

            public override void Inspector() {
                if (headTarget.humanoid.astra.enabled) {
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
                    if (enabledProp.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal();
                        headTarget.astra.headTracking = EditorGUILayout.ToggleLeft("Head Tracking", headTarget.astra.headTracking, GUILayout.MinWidth(80));
                        if (headTarget.astra.headTracking)
                            headTarget.astra.rotationTrackingAxis = (AstraHead.RotationTrackingAxis)EditorGUILayout.EnumPopup(headTarget.astra.rotationTrackingAxis);
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }
        #endregion

        #region Arm
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.astra, handTarget, "astra") {
            }

            public override void Inspector() {
                if (handTarget.humanoid.astra.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, handTarget);
            }
        }
        #endregion

        #region Torso
        public class HipsTargetProps : HipsTarget_Editor.TargetProps {
            public HipsTargetProps(SerializedObject serializedObject, HipsTarget hipsTarget)
                : base(serializedObject, hipsTarget.astra, hipsTarget, "astra") {
            }

            public override void Inspector() {
                if (hipsTarget.humanoid.astra.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, hipsTarget);
            }
        }
        #endregion

        #region Leg
        public class FootTargetProps : FootTarget_Editor.TargetProps {
            public FootTargetProps(SerializedObject serializedObject, FootTarget footTarget)
                : base(serializedObject, footTarget.astra, footTarget, "astra") {
            }

            public override void Inspector() {
                if (footTarget.humanoid.astra.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, footTarget);
            }
        }
        #endregion

#endif
    }

}
