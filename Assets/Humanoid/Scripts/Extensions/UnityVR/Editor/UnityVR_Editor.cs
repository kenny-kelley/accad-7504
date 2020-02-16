using UnityEditor;
using UnityEngine;

namespace Passer {

    public class UnityVR_Editor : Editor {

        public static void AddTracker(HumanoidControl humanoid) {
            // you cannot find a tracker in a disabled gameObject
            if (!humanoid.gameObject.activeInHierarchy)
                return;

            GameObject realWorld = HumanoidControl.GetRealWorld(humanoid.transform);

            Transform trackerTransform = realWorld.transform.Find(UnityVRDevice.trackerName);
            if (trackerTransform == null) {
                UnityVRDevice.trackerObject = new GameObject {
                    name = UnityVRDevice.trackerName
                };
                UnityVRDevice.trackerObject.transform.parent = realWorld.transform;
                UnityVRDevice.trackerObject.transform.localPosition = Vector3.zero;
            }
            else
                UnityVRDevice.trackerObject = trackerTransform.gameObject;
        }

        private static void RemoveTracker() {
            DestroyImmediate(UnityVRDevice.trackerObject, true);
        }

        public static void ShowTracker(bool show) {
            if (UnityVRDevice.trackerObject == null)
                return;

            if (show && !UnityVRDevice.trackerObject.activeSelf && UnityVRDevice.present)
                HumanoidControl_Editor.ShowTracker(UnityVRDevice.trackerObject, true);

            else if (!show && UnityVRDevice.trackerObject.activeSelf)
                HumanoidControl_Editor.ShowTracker(UnityVRDevice.trackerObject, false);
        }

        public static void Inspector(HumanoidControl humanoid) {
            if (humanoid.headTarget == null)
                return;

            FirstPersonCameraInspector(humanoid.headTarget);
#if (UNITY_STANDALONE_WIN || UNITY_ANDROID)
            if (PlayerSettings.virtualRealitySupported)
                AddTracker(humanoid);
            else
                RemoveTracker();

            ShowTracker(humanoid.showRealObjects);
#endif
        }

        private static void FirstPersonCameraInspector(HeadTarget headTarget) {
            if (headTarget.unityVRHead == null || headTarget.humanoid == null)
                return;

#if hSTEAMVR && hVIVETRACKER && UNITY_STANDALONE_WIN
            EditorGUI.BeginDisabledGroup(headTarget.humanoid.steam.enabled && headTarget.viveTracker.enabled);
#endif
            bool wasEnabled = headTarget.unityVRHead.enabled;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
#if hSTEAMVR && hVIVETRACKER && UNITY_STANDALONE_WIN
            if (headTarget.humanoid.steam.enabled && headTarget.viveTracker.enabled)
                headTarget.unityVRHead.enabled = false;
#endif
            GUIContent text = new GUIContent(
                "First Person Camera",
                "Enables a first person camera. Disabling and enabling again reset the camera position"
                );
            bool enabled = EditorGUILayout.ToggleLeft(text, headTarget.unityVRHead.enabled, GUILayout.Width(200));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(headTarget, enabled ? "Enabled " : "Disabled " + headTarget.unityVRHead.name);
                headTarget.unityVRHead.enabled = enabled;
            }
            EditorGUILayout.EndHorizontal();

            if (!Application.isPlaying && !HumanoidControl_Editor.IsPrefab(headTarget.humanoid)) {
                UnityVRHead.CheckCamera(headTarget);
                if (!wasEnabled && headTarget.unityVRHead.enabled) {
                    UnityVRHead.AddCamera(headTarget);
                } else if (wasEnabled && !headTarget.unityVRHead.enabled) {
                    UnityVRHead.RemoveCamera(headTarget);
                }
            }
#if hSTEAMVR && hVIVETRACKER && UNITY_STANDALONE_WIN
            EditorGUI.EndDisabledGroup();
#endif

        }
    }
}