using UnityEngine;
using UnityEditor;

namespace Passer {
    using Humanoid;

    public class Realsense_Editor : Tracker_Editor {

#if hREALSENSE

#region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, RealsenseTracker realsenseTracker)
                : base(serializedObject, targetObjs, realsenseTracker, "realsenseTracker") {
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "IntelRealsense");
                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    humanoid.realsenseTracker.trackerTransform = (Transform)EditorGUILayout.ObjectField("Tracker Transform", humanoid.realsenseTracker.trackerTransform, typeof(Transform), true);

#if hLEAP
                    if (humanoid.leapTracker.enabled)
                        EditorGUILayout.HelpBox("Leap Motion interferes with RealSense tracking", MessageType.Warning);
#endif
                    EditorGUI.indentLevel--;
                }
            }
        }
#endregion

#region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.realsense, headTarget, "realsense") {
            }

            public override void Inspector() {
                if (!headTarget.humanoid.realsenseTracker.enabled)
                    return;

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
                headTarget.realsense.enabled = enabledProp.boolValue;
                if (headTarget.realsense.enabled) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    headTarget.realsense.headTracking = EditorGUILayout.ToggleLeft("Head Tracking", headTarget.realsense.headTracking);
                    if (headTarget.realsense.headTracking)
                        headTarget.realsense.rotationTrackingAxis = (IntelRealsenseHead.RotationTrackingAxis)EditorGUILayout.EnumPopup(headTarget.realsense.rotationTrackingAxis);
                    EditorGUILayout.EndHorizontal();
#if hFACE
                    headTarget.face.realsenseFace.faceTracking = EditorGUILayout.ToggleLeft("Face Tracking", headTarget.face.realsenseFace.faceTracking);
                    headTarget.face.realsenseFace.eyeTracking = EditorGUILayout.ToggleLeft("Eye Tracking", headTarget.face.realsenseFace.eyeTracking);
#endif
                    EditorGUI.indentLevel--;
                }

            }
        }
#endregion

#endif
    }
}
