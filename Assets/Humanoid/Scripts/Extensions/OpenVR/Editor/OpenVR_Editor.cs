#if hOPENVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
    using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

namespace Passer.Humanoid {

    public class OpenVR_Editor : Tracker_Editor {

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {

#if hVIVETRACKER
            private ViveTracker_Editor.TrackerProps viveTrackerProps;
#endif

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, OpenVRHumanoidTracker _openVR)
                : base(serializedObject, targetObjs, _openVR, "openVR") {
                tracker = _openVR;

                headSensorProp = targetObjs.headTargetObj.FindProperty("openVR");
                leftHandSensorProp = targetObjs.leftHandTargetObj.FindProperty("openVR");
                rightHandSensorProp = targetObjs.rightHandTargetObj.FindProperty("openVR");

#if hVIVETRACKER
                viveTrackerProps = new ViveTracker_Editor.TrackerProps(serializedObject, targetObjs, _openVR);
#endif
            }

            public override void Inspector(HumanoidControl humanoid) {
                bool openVRSupported = OpenVRSupported();
                if (openVRSupported) {
                    if (humanoid.headTarget.unityVRHead.enabled)
                        humanoid.openVR.enabled = true;

                    EditorGUI.BeginDisabledGroup(humanoid.headTarget.unityVRHead.enabled);
                    Inspector(humanoid, "TrackerModels/Lighthouse");
                    EditorGUI.EndDisabledGroup();

#if hVIVETRACKER
                    viveTrackerProps.Inspector(humanoid);
#endif
                }
                else
                    enabledProp.boolValue = false;
            }

            public override void InitControllers() {
                base.InitControllers();
#if hVIVETRACKER
                viveTrackerProps.InitControllers();
#endif
            }

            public override void RemoveControllers() {
                base.RemoveControllers();
#if hVIVETRACKER
                viveTrackerProps.RemoveControllers();
#endif
            }

            public override void SetSensors2Target() {
                base.SetSensors2Target();
#if hVIVETRACKER
                viveTrackerProps.SetSensors2Target();
#endif
            }
        }
        #endregion

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.openVR, headTarget, "openVR") {
            }

            public override void Inspector() {
                if (!headTarget.humanoid.openVR.enabled || !OpenVRSupported())
                    return;

                CheckHmdComponent(headTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(headTarget.openVR, headTarget);
                headTarget.openVR.enabled = enabledProp.boolValue;
                headTarget.openVR.CheckSensorTransform();
                if (!Application.isPlaying) {
                    headTarget.openVR.SetSensor2Target();
                    headTarget.openVR.ShowSensor(headTarget.humanoid.showRealObjects && headTarget.showRealObjects);
                }

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", headTarget.openVR.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckHmdComponent(HeadTarget headTarget) {
                if (headTarget.openVR.sensorTransform == null)
                    return;

                OpenVRHmd sensorComponent = headTarget.openVR.sensorTransform.GetComponent<OpenVRHmd>();
                if (sensorComponent == null)
                    headTarget.openVR.sensorTransform.gameObject.AddComponent<OpenVRHmd>();
            }
        }

        #region HMD Component
        [CustomEditor(typeof(OpenVRHmd))]
        public class OpenVRHmd_Editor : Editor {
            OpenVRHmd sensorComponent;

            private void OnEnable() {
                sensorComponent = (OpenVRHmd)target;
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", sensorComponent.status);
                EditorGUILayout.FloatField("Position Confidence", sensorComponent.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", sensorComponent.rotationConfidence);
                EditorGUILayout.Space();
                EditorGUILayout.IntField("Tracker Id", sensorComponent.trackerId);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion
        #endregion

        #region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            protected SerializedProperty useSkeletalInputProp;

            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.openVR, handTarget, "openVR") {

                useSkeletalInputProp = serializedObject.FindProperty("openVR.useSkeletalInput");
            }

            public override void Inspector() {
                if (!handTarget.humanoid.openVR.enabled || !OpenVRSupported())
                    return;

                CheckControllerComponent(handTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(handTarget.openVR, handTarget);
                handTarget.openVR.enabled = enabledProp.boolValue;
                handTarget.openVR.CheckSensorTransform();
                if (!Application.isPlaying) {
                    handTarget.openVR.SetSensor2Target();
                    handTarget.openVR.ShowSensor(handTarget.humanoid.showRealObjects && handTarget.showRealObjects);
                }

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    // For this, the controller meshes need to have the same origin which is currently not the case
                    //controllerTypeProp.intValue = (int)(OpenVRController.ControllerType)EditorGUILayout.EnumPopup("Controller Type", handTarget.openVR.controllerType);
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", handTarget.openVR.sensorTransform, typeof(Transform), true);
                    useSkeletalInputProp.boolValue = EditorGUILayout.Toggle("Use Skeletal Input", useSkeletalInputProp.boolValue);   
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckControllerComponent(HandTarget handTarget) {
                if (handTarget.openVR.sensorTransform == null)
                    return;

                OpenVRController sensorComponent = handTarget.openVR.sensorTransform.GetComponent<OpenVRController>();
                if (sensorComponent == null)
                    sensorComponent = handTarget.openVR.sensorTransform.gameObject.AddComponent<OpenVRController>();
                sensorComponent.isLeft = handTarget.isLeft;
            }
        }

        #region Controller Component
        [CustomEditor(typeof(OpenVRController))]
        public class OpenVRController_Editor : Editor {
            OpenVRController controllerComponent;

            //SerializedProperty controllerTypeProp;

            private void OnEnable() {
                controllerComponent = (OpenVRController)target;

                //controllerTypeProp = serializedObject.FindProperty("controllerType");
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", controllerComponent.status);
                EditorGUILayout.FloatField("Position Confidence", controllerComponent.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", controllerComponent.rotationConfidence);
                EditorGUILayout.IntField("Tracker Id", controllerComponent.trackerId);
                EditorGUILayout.Space();
                EditorGUILayout.Toggle("Is Left", controllerComponent.isLeft);
                EditorGUILayout.Vector3Field("Joystick", controllerComponent.joystick);
                EditorGUILayout.Vector3Field("Touchpad", controllerComponent.touchpad);
                EditorGUILayout.Slider("Trigger", controllerComponent.trigger, -1, 1);
                EditorGUILayout.Slider("Grip", controllerComponent.grip, -1, 1);
                EditorGUILayout.Slider("Button A", controllerComponent.aButton, -1, 1);
                EditorGUILayout.Slider("Button B", controllerComponent.bButton, -1, 1);
                EditorGUI.EndDisabledGroup();
                // For this, the controller meshes need to have the same origin which is currently not the case
                //controllerTypeProp.intValue = (int)(OPenVRController.ControllerType)EditorGUILayout.EnumPopup("View Controller Type", controllerComponent.controllerType);

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion
        #endregion

        private static bool OpenVRSupported() {
#if UNITY_2017_2_OR_NEWER
            string[] supportedDevices = XRSettings.supportedDevices;
#else
            string[] supportedDevices = VRSettings.supportedDevices;
#endif
            foreach (string supportedDevice in supportedDevices) {
                if (supportedDevice == "OpenVR")
                    return true;
            }
            return false;
        }

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
            FileInfo fileInfo = new FileInfo(pathToBuiltProject);
            string buildPath = fileInfo.Directory.FullName;


            CopyFilesToPath(buildPath, true);
        }

        private static void CopyManifestsToBuild(string buildPath) {

        }

        public static void CopyFilesToPath(string toPath, bool overwrite) {
            string humanoidPath = Configuration_Editor.FindHumanoidFolder();
            string[] files = GetFilesToCopy();

            foreach (string file in files) {
                string fullFile = Application.dataPath + humanoidPath + "Extensions/OpenVR/" + file;
                FileInfo bindingInfo = new FileInfo(fullFile);
                string newFilePath = Path.Combine(toPath, bindingInfo.Name);

                bool exists = false;
                if (File.Exists(newFilePath))
                    exists = true;

                if (exists) {
                    if (overwrite) {
                        FileInfo existingFile = new FileInfo(newFilePath) {
                            IsReadOnly = false
                        };
                        existingFile.Delete();

                        File.Copy(fullFile, newFilePath);

                        Debug.Log("Copied (overwrote) manifest to build: " + newFilePath);
                    }
                    else
                        Debug.Log("Skipped writing existing manifest in build: " + newFilePath);
                }
                else {
                    File.Copy(fullFile, newFilePath);

                    Debug.Log("Copied manifest to buld: " + newFilePath);
                }

            }
        }

        private static string[] GetFilesToCopy() {
            string[] files = {
                "actions.json",
                "binding_vive.json",
                "binding_vive_pro.json",
                "bindings_holographic_controller.json",
                "bindings_knuckles.json",
                "bindings_oculus_touch.json",
                "bindings_vive_controller.json"
            };
            return files;
        }
    }
}
#endif