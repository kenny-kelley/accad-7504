using System.IO;
using UnityEditor;
using UnityEngine;

namespace Passer {
    using Passer.Humanoid;

    public class Pupil_Editor : Tracker_Editor {

#if hPUPIL

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, Tracker pupil) 
                : base(serializedObject, targetObjs, pupil, "pupil") {
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, humanoid.pupil.name);

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    CheckScript();

                    GUIContent text;
                    text = new GUIContent(
                        "Tracking Mode",
                        "Pupil Labs tracking mode or Detection and Mapping mode. " +
                        "This will also determine which kine of calibration will be executed"
                        );
                    humanoid.pupil.trackingMode = (Device.TrackingMode) EditorGUILayout.EnumPopup(text, humanoid.pupil.trackingMode, GUILayout.MinWidth(80));
                    text = new GUIContent(
                        "Auto Calibration",
                        "This will start calibration as soon as device is active"
                        );
                    humanoid.pupil.autoCalibration = EditorGUILayout.Toggle(text, humanoid.pupil.autoCalibration);
                    EditorGUI.indentLevel--;
                } else 
                {
                    RemoveScript(humanoid);
                }
            }
        }
        #endregion

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.pupil, headTarget, "pupil") {
            }

            public override void Inspector() {
                if (!headTarget.humanoid.pupil.enabled)
                    return;

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
                if (enabledProp.boolValue && !headTarget.unityVRHead.enabled)
                    EditorGUILayout.HelpBox("Pupil eye tracking requires a First Person Camera", MessageType.Error);                

            }
        }
        #endregion

         private static PupilGazeTracker CheckScript() {
            PupilGazeTracker client = FindObjectOfType<PupilGazeTracker>();
            if (client == null) {
                GameObject rootGameObject = new GameObject("Humanoid Pupil Gaze Tracker");
                client = rootGameObject.AddComponent<PupilGazeTracker>();
            }

            return client;
        }

        private static void RemoveScript(HumanoidControl humanoid) {
            PupilGazeTracker client = FindObjectOfType<PupilGazeTracker>();
            if (client != null && client.gameObject.name == "Humanoid Pupil Gaze Tracker")
                DestroyImmediate(client.gameObject, true);
        }

#endif
    }

}
