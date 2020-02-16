using System.IO;
using UnityEngine;
using UnityEditor;

namespace Passer.Humanoid {

    public class LeapMotion_Editor : Tracker_Editor {

#if hLEAP

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {

            private SerializedProperty headMountedProp;

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, LeapTracker leapTracker)
                : base(serializedObject, targetObjs, leapTracker, "leapTracker") {

                headMountedProp = serializedObject.FindProperty("leapTracker.isHeadMounted");
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "LeapMotion");

                bool wasHeadMounted = humanoid.leapTracker.isHeadMounted;
                if (enabledProp.boolValue) {

                    EditorGUI.indentLevel++;
                    {
                        GUIContent label = new GUIContent(
                            "Tracker Transform",
                            "The leap camera position in the real world"
                            );
                        trackerTransfromProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField(label, humanoid.leapTracker.trackerTransform, typeof(Transform), true);
                    }

                    if (PlayerSettings.virtualRealitySupported && humanoid.headTarget.unityVRHead.enabled) {
                        GUIContent label = new GUIContent(
                            "HMD mounted",
                            "Puts the leap camera on the Headset"
                            );
                        headMountedProp.boolValue = EditorGUILayout.Toggle(label, humanoid.leapTracker.isHeadMounted, GUILayout.MinWidth(80));
                    }
                    else
                        headMountedProp.boolValue = false;
                    EditorGUI.indentLevel--;
                }
                else {
                    headMountedProp.boolValue = false;
                }
                if (wasHeadMounted != headMountedProp.boolValue)
                    humanoid.leapTracker.PlaceTrackerTransform(headMountedProp.boolValue);
            }
        }

        private bool hmdMounted(HumanoidControl humanoid) {
            return (
                PlayerSettings.virtualRealitySupported &&
                humanoid.headTarget.unityVRHead.enabled &&
                humanoid.leapTracker.isHeadMounted
                );
        }
        #endregion

        #region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.leap, handTarget, "leap") {
            }

            public override void Inspector() {
                if (handTarget.humanoid.leapTracker.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, handTarget);
            }
        }
        #endregion

#endif
    }
}
