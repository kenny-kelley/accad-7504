using System.IO;
using UnityEngine;
using UnityEditor;

namespace Passer {
    using Humanoid;

    public class Tobii_Editor : Tracker_Editor {

#if hTOBII

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, TobiiTracker tobiiTracker)
                : base(serializedObject, targetObjs, tobiiTracker, "tobiiTracker") {
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, humanoid.tobiiTracker.name);
                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    humanoid.tobiiTracker.trackerTransform = (Transform)EditorGUILayout.ObjectField("Tracker Transform", humanoid.tobiiTracker.trackerTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.tobiiHead, headTarget, "tobiiHead") {
            }

            public override void Inspector() {
                if (!headTarget.humanoid.tobiiTracker.enabled)
                    return;

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
                headTarget.tobiiHead.enabled = enabledProp.boolValue;
                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    headTarget.tobiiHead.headTracking = EditorGUILayout.ToggleLeft("Head Tracking", headTarget.tobiiHead.headTracking, GUILayout.MinWidth(80));
                    if (headTarget.tobiiHead.headTracking)
                        headTarget.tobiiHead.rotationTrackingAxis = (TobiiHead.RotationTrackingAxis)EditorGUILayout.EnumPopup(headTarget.tobiiHead.rotationTrackingAxis);
                    EditorGUILayout.EndHorizontal();
#if hFACE
                    headTarget.tobiiHead.eyeTracking = EditorGUILayout.ToggleLeft("Eye Tracking", headTarget.tobiiHead.eyeTracking);
#endif
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion

#endif
    }
}
