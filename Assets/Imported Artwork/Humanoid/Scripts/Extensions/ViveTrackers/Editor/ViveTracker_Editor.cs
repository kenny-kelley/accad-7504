#if (hSTEAMVR || hOPENVR) && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)

using UnityEditor;
using UnityEngine;

namespace Passer {
    using Humanoid;

    public class ViveTracker_Editor : Tracker_Editor {

#region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
#if hSTEAMVR
            private SteamVRTracker steamTracker;

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, SteamVRTracker _steamTracker)
#elif hOPENVR
            private OpenVRHumanoidTracker steamTracker;

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, OpenVRHumanoidTracker _steamTracker)
#endif
                : base(serializedObject, targetObjs, _steamTracker, "steam") {
                steamTracker = _steamTracker;
                tracker = steamTracker;
            }

            public override void Inspector(HumanoidControl humanoid) { }

            public override void InitControllers() {
                HumanoidControl humanoid = steamTracker.humanoid;

                steamTracker.headSensorVive.InitController(headSensorProp, humanoid.headTarget);
                steamTracker.leftHandSensorVive.InitController(leftHandSensorProp, humanoid.leftHandTarget);
                steamTracker.rightHandSensorVive.InitController(rightHandSensorProp, humanoid.rightHandTarget);
                steamTracker.hipsSensorVive.InitController(hipsSensorProp, humanoid.hipsTarget);
                steamTracker.leftFootSensorVive.InitController(leftFootSensorProp, humanoid.leftFootTarget);
                steamTracker.rightFootSensorVive.InitController(rightFootSensorProp, humanoid.rightFootTarget);
            }

            public override void RemoveControllers() {
                RemoveTransform(steamTracker.headSensorVive.sensorTransform);
                RemoveTransform(steamTracker.leftHandSensorVive.sensorTransform);
                RemoveTransform(steamTracker.rightHandSensorVive.sensorTransform);
                RemoveTransform(steamTracker.hipsSensorVive.sensorTransform);
                RemoveTransform(steamTracker.leftFootSensorVive.sensorTransform);
                RemoveTransform(steamTracker.rightFootSensorVive.sensorTransform);
            }

            public override void SetSensors2Target() {
                steamTracker.headSensorVive.SetSensor2Target();
                steamTracker.leftHandSensorVive.SetSensor2Target();
                steamTracker.rightHandSensorVive.SetSensor2Target();
                steamTracker.hipsSensorVive.SetSensor2Target();
                steamTracker.leftFootSensorVive.SetSensor2Target();
                steamTracker.rightFootSensorVive.SetSensor2Target();
            }
        }
#endregion

#region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.viveTracker, headTarget, "viveTracker") {
            }

            public override void Inspector() {
#if hSTEAMVR
                if (!PlayerSettings.virtualRealitySupported || !headTarget.humanoid.steam.enabled)
                    return;
#elif hOPENVR
                if (!PlayerSettings.virtualRealitySupported || !headTarget.humanoid.openVR.enabled)
                    return;
#endif

                CheckSensorComponent(headTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
                sensor.enabled = enabledProp.boolValue;
                sensor.CheckSensorTransform();
                if (!Application.isPlaying) {
                    sensor.SetSensor2Target();
                    sensor.ShowSensor(headTarget.humanoid.showRealObjects && headTarget.showRealObjects);
                }


                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensor.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            private static void CheckSensorComponent(HeadTarget headTarget) {
                if (headTarget.viveTracker.sensorTransform == null)
                    return;

                ViveTrackerComponent sensorComponent = headTarget.viveTracker.sensorTransform.GetComponent<ViveTrackerComponent>();
                if (sensorComponent == null)
                    sensorComponent = headTarget.viveTracker.sensorTransform.gameObject.AddComponent<ViveTrackerComponent>();
            }
        }
#endregion

#region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            private static SerializedProperty attachedBoneProp;

            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.viveTracker, handTarget, "viveTracker") {

                attachedBoneProp = serializedObject.FindProperty("viveTracker.attachedBone");
            }

            public override void Inspector() {
#if hSTEAMVR
                if (!PlayerSettings.virtualRealitySupported || !handTarget.humanoid.steam.enabled)
                    return;
#elif hOPENVR
                if (!PlayerSettings.virtualRealitySupported || !handTarget.humanoid.openVR.enabled)
                    return;
#endif
                CheckSensorComponent(handTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, handTarget);
                sensor.enabled = enabledProp.boolValue;
                sensor.CheckSensorTransform();
                if (!Application.isPlaying) {
                    sensor.SetSensor2Target();
                    sensor.ShowSensor(handTarget.humanoid.showRealObjects && handTarget.showRealObjects);
                }


                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensor.sensorTransform, typeof(Transform), true);
                    attachedBoneProp.intValue = (int)(ArmBones)EditorGUILayout.EnumPopup("Bone", (ArmBones)attachedBoneProp.intValue);
#if hSTEAMVR
                    if ((ArmBones)attachedBoneProp.intValue == ArmBones.Hand && handTarget.steamVR.enabled) 
                        EditorGUILayout.HelpBox("SteamVR Controller and Vive Tracker are both on the hand", MessageType.Warning);
#elif hOPENVR
                    if ((ArmBones)attachedBoneProp.intValue == ArmBones.Hand && handTarget.openVR.enabled)
                        EditorGUILayout.HelpBox("OpenVR Controller and Vive Tracker are both on the hand", MessageType.Warning);
#endif
                    EditorGUI.indentLevel--;
                }
            }

            private static void CheckSensorComponent(HandTarget handTarget) {
                if (handTarget.viveTracker.sensorTransform == null)
                    return;

                ViveTrackerComponent sensorComponent = handTarget.viveTracker.sensorTransform.GetComponent<ViveTrackerComponent>();
                if (sensorComponent == null)
                    sensorComponent = handTarget.viveTracker.sensorTransform.gameObject.AddComponent<ViveTrackerComponent>();
            }
        }
#endregion

#region Hips
        public class HipsTargetProps : HipsTarget_Editor.TargetProps {
            public HipsTargetProps(SerializedObject serializedObject, HipsTarget hipsTarget)
                : base(serializedObject, hipsTarget.viveTracker, hipsTarget, "viveTracker") {
            }

            public override void Inspector() {
#if hSTEAMVR
                if (!PlayerSettings.virtualRealitySupported || !hipsTarget.humanoid.steam.enabled)
                    return;
#elif hOPENVR
                if (!PlayerSettings.virtualRealitySupported || !hipsTarget.humanoid.openVR.enabled)
                    return;
#endif
                CheckSensorComponent(hipsTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, hipsTarget);
                sensor.enabled = enabledProp.boolValue;
                sensor.CheckSensorTransform();
                if (!Application.isPlaying) {
                    sensor.SetSensor2Target();
                    sensor.ShowSensor(hipsTarget.humanoid.showRealObjects && hipsTarget.showRealObjects);
                }


                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensor.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            private static void CheckSensorComponent(HipsTarget hipsTarget) {
                if (hipsTarget.viveTracker.sensorTransform == null)
                    return;

                ViveTrackerComponent sensorComponent = hipsTarget.viveTracker.sensorTransform.GetComponent<ViveTrackerComponent>();
                if (sensorComponent == null)
                    sensorComponent = hipsTarget.viveTracker.sensorTransform.gameObject.AddComponent<ViveTrackerComponent>();
            }

        }
#endregion

#region Foot
        public class FootTargetProps : FootTarget_Editor.TargetProps {
            public FootTargetProps(SerializedObject serializedObject, FootTarget footTarget)
                : base(serializedObject, footTarget.viveTracker, footTarget, "viveTracker") {
            }

            public override void Inspector() {
#if hSTEAMVR
                if (!PlayerSettings.virtualRealitySupported || !footTarget.humanoid.steam.enabled)
                    return;
#elif hOPENVR
                if (!PlayerSettings.virtualRealitySupported || !footTarget.humanoid.openVR.enabled)
                    return;
#endif
                CheckSensorComponent(footTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, footTarget);
                sensor.enabled = enabledProp.boolValue;
                sensor.CheckSensorTransform();
                if (!Application.isPlaying) {
                    sensor.SetSensor2Target();
                    sensor.ShowSensor(footTarget.humanoid.showRealObjects && footTarget.showRealObjects);
                }

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensor.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            private static void CheckSensorComponent(FootTarget footTarget) {
                if (footTarget.viveTracker.sensorTransform == null)
                    return;

                ViveTrackerComponent sensorComponent = footTarget.viveTracker.sensorTransform.GetComponent<ViveTrackerComponent>();
                if (sensorComponent == null)
                    sensorComponent = footTarget.viveTracker.sensorTransform.gameObject.AddComponent<ViveTrackerComponent>();
            }
        }
#endregion

#region Object
        /*
        private static SerializedProperty objectEnabledProp;
        private static SerializedProperty objectSensorTransformProp;
        private static SerializedProperty objectSensor2TargetPositionProp;
        private static SerializedProperty objectSensor2TargetRotationProp;

        public static void InitObject(ObjectTarget objectTarget, SerializedObject serializedObject) {
            objectEnabledProp = serializedObject.FindProperty("viveTracker.enabled");
            objectSensorTransformProp = serializedObject.FindProperty("viveTracker.sensorTransform");
            objectSensor2TargetPositionProp = serializedObject.FindProperty("viveTracker.sensor2TargetPosition");
            objectSensor2TargetRotationProp = serializedObject.FindProperty("viveTracker.sensor2TargetRotation");

            objectTarget.viveTracker.Init(objectTarget);
        }

        public static void ObjectInspector(ViveTrackerSensor viveTracker) {
            objectEnabledProp.boolValue = Target_Editor.ControllerInspector(viveTracker);
            viveTracker.CheckSensorTransform();

            if (objectEnabledProp.boolValue) {
                EditorGUI.indentLevel++;
                viveTracker.trackerId = EditorGUILayout.IntField("Tracker Id", viveTracker.trackerId);
                objectSensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", viveTracker.sensorTransform, typeof(Transform), true);
                EditorGUI.indentLevel--;
            }
        }

        public static void SetSensor2Target(ViveTrackerSensor viveTracker) {
            viveTracker.SetSensor2Target();
            objectSensor2TargetRotationProp.quaternionValue = viveTracker.sensor2TargetRotation;
            objectSensor2TargetPositionProp.vector3Value = viveTracker.sensor2TargetPosition;
        }
        */
#endregion

#region Sensor Component
        [CustomEditor(typeof(ViveTrackerComponent))]
        public class ViveTrackerComponent_Editor : Editor {
            ViveTrackerComponent sensorComponent;

            private void OnEnable() {
                sensorComponent = (ViveTrackerComponent)target;
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", sensorComponent.status);
                EditorGUILayout.FloatField("Position Confidence", sensorComponent.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", sensorComponent.rotationConfidence);
                EditorGUILayout.IntField("Tracker Id", sensorComponent.trackerId);

                EditorGUILayout.Toggle("Pogo Pin 3", sensorComponent.pogo3);
                EditorGUILayout.Toggle("Pogo Pin 4", sensorComponent.pogo4);
                EditorGUILayout.Toggle("Pogo Pin 5", sensorComponent.pogo5);
                EditorGUILayout.Toggle("Pogo Pin 6", sensorComponent.pogo6);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
            }
        }
#endregion

    }

    /*
#region Sensor Component
    [CustomEditor(typeof(ViveTrackerComponent))]
    public class ViveTrackerComponent_Editor : Editor {
        ViveTrackerComponent sensorComponent;

        private void OnEnable() {
            sensorComponent = (ViveTrackerComponent)target;
        }
        public override void OnInspectorGUI() {

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup("Status", sensorComponent.status);
            EditorGUILayout.IntField("Tracker Id", sensorComponent.trackerId);
            EditorGUILayout.Toggle("Pogo Pin 3", sensorComponent.pogo3);
            EditorGUILayout.Toggle("Pogo Pin 4", sensorComponent.pogo4);
            EditorGUILayout.Toggle("Pogo Pin 5", sensorComponent.pogo5);
            EditorGUILayout.Toggle("Pogo Pin 6", sensorComponent.pogo6);
            EditorGUI.EndDisabledGroup();
        }

        public void OnSceneGUI() {
            if (!Application.isPlaying && sensorComponent.target != null) {
                System.Type targetType = typeof(sensorComponent.target);
                if (targetType == typeof(HeadTarget)) { 
                    HeadTarget headTarget = (HeadTarget)sensorComponent.target;
                    sensorComponent.SetSensor2Target(headTarget.viveTracker);
                }
                //sensorComponent.sensor.SetSensor2Target();
            }
        }
    }
#endregion
    */

}
#endif
