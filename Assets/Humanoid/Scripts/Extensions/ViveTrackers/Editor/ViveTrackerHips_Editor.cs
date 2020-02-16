///* Humanoid Vive Tracker editor
// * Copyright (c) 2017 by Passer VR
// * author: Pascal Serrarens
// * email: support@passervr.com
// * version: 4.0.0
// * date: May 27, 2017
// * 
// */

//#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)

//using UnityEngine;
//using UnityEditor;

//namespace Passer {

//    public class ViveTrackerHips_Editor : Editor {

//        private static SerializedProperty enabledProp;
//        private static SerializedProperty sensorTransformProp;
//        private static SerializedProperty sensor2TargetPositionProp;
//        private static SerializedProperty sensor2TargetRotationProp;

//        public static void InitHips(SerializedObject serializedObject) {
//            enabledProp = serializedObject.FindProperty("viveTracker.enabled");

//            sensorTransformProp = serializedObject.FindProperty("viveTracker.sensorTransform");
//            sensor2TargetPositionProp = serializedObject.FindProperty("viveTracker.sensor2TargetPosition");
//            sensor2TargetRotationProp = serializedObject.FindProperty("viveTracker.sensor2TargetRotation");
//        }

//        public static void HipsInspector(HumanoidControl humanoid, HipsTarget hipsTarget) {
//            enabledProp.boolValue = Target_Editor.ControllerInspector(hipsTarget.viveTracker, hipsTarget);

//            if (enabledProp.boolValue) {
//                CheckSensorTransform(hipsTarget);
//                EditorGUI.indentLevel++;
//                sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensorTransformProp.objectReferenceValue, typeof(Transform), true);
//                EditorGUI.indentLevel--;
//            } else {
//                sensorTransformProp.objectReferenceValue = SteamVR_Editor.RemoveTracker(hipsTarget.viveTracker.sensorTransform);
//            }
//        }

//        private static void CheckSensorTransform(HipsTarget hipsTarget) {
//            if (sensorTransformProp.objectReferenceValue == null)
//                sensorTransformProp.objectReferenceValue = ViveTrackerTorso.CreateTrackerObject(hipsTarget);
//            SetSensor2Target(hipsTarget);
//        }

//        public static void SetSensor2Target(HipsTarget hipsTarget) {
//            if (hipsTarget.viveTracker.sensorTransform == null)
//                return;

//            sensor2TargetRotationProp.quaternionValue = Quaternion.Inverse(hipsTarget.viveTracker.sensorTransform.rotation) * hipsTarget.hips.target.transform.rotation;
//            sensor2TargetPositionProp.vector3Value = Quaternion.Inverse(sensor2TargetRotationProp.quaternionValue) * hipsTarget.viveTracker.sensorTransform.InverseTransformPoint(hipsTarget.hips.target.transform.position);
//        }
//    }
//}
//#endif
