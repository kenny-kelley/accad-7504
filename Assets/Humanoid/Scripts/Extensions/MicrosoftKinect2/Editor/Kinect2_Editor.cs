using UnityEngine;
using UnityEditor;

namespace Passer {
    using Humanoid;

    public class Kinect2_Editor : Tracker_Editor {
   
#if hKINECT2

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, Kinect2Tracker kinectTracker)
                : base(serializedObject, targetObjs, kinectTracker, "kinectTracker") {
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "Kinect2");
                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    humanoid.kinectTracker.trackerTransform = (Transform)EditorGUILayout.ObjectField("Tracker Transform", humanoid.kinectTracker.trackerTransform, typeof(Transform), true);
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                    if (humanoid.steam.enabled)
                        EditorGUILayout.HelpBox("Kinect interferes with SteamVR tracking", MessageType.Warning);
#endif
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget handTarget)
                : base(serializedObject, handTarget.kinect, handTarget, "kinect") {
            }

            public override void Inspector() {
                if (headTarget.humanoid.kinectTracker.enabled) {
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
                    if (enabledProp.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal();
                        headTarget.kinect.headTracking = EditorGUILayout.ToggleLeft("Head Tracking", headTarget.kinect.headTracking, GUILayout.MinWidth(80));
                        if (headTarget.kinect.headTracking)
                            headTarget.kinect.rotationTrackingAxis = (Kinect2Head.RotationTrackingAxis)EditorGUILayout.EnumPopup(headTarget.kinect.rotationTrackingAxis);
                        EditorGUILayout.EndHorizontal();
#if hFACE
                        headTarget.kinectFace.faceTracking = EditorGUILayout.ToggleLeft("Face Tracking", headTarget.kinectFace.faceTracking);
                        headTarget.kinectFace.audioInput = EditorGUILayout.ToggleLeft("Audio Input", headTarget.kinectFace.audioInput);
#endif
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }
        #endregion

        #region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.kinect, handTarget, "kinect") {
            }

            public override void Inspector() {
                if (handTarget.humanoid.kinectTracker.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, handTarget);
            }
        }
        #endregion

        #region Hips
        public class HipsTargetProps : HipsTarget_Editor.TargetProps {
            public HipsTargetProps(SerializedObject serializedObject, HipsTarget hipsTarget)
                : base(serializedObject, hipsTarget.kinect, hipsTarget, "kinect") {
            }

            public override void Inspector() {
                if (hipsTarget.humanoid.kinectTracker.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, hipsTarget);
            }
        }
        #endregion

        #region Foot

        public class FootTargetProps : FootTarget_Editor.TargetProps {
            public FootTargetProps(SerializedObject serializedObject, FootTarget footTarget)
                : base(serializedObject, footTarget.kinect, footTarget, "kinect") {
            }

            public override void Inspector() {
                if (footTarget.humanoid.kinectTracker.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, footTarget);
            }
        }

        #endregion

#endif
    }
}
