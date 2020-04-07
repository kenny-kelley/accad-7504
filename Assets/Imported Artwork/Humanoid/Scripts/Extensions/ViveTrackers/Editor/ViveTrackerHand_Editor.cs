///* Humanoid Vive Tracker editor
// * Copyright (c) 2017 by Passer VR
// * author: Pascal Serrarens
// * email: support@passervr.com
// * version: 4.0.0
// * date: May 27, 2017
// * 
// */

//using UnityEngine;
//using UnityEditor;

//#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)

//namespace Passer {

//    public class ViveTrackerHand_Editor : Editor {

//        private static SerializedProperty enabledProp;
//        private static SerializedProperty attachedBoneProp;
//        private static SerializedProperty sensorTransformProp;
//        private static SerializedProperty sensor2TargetPositionProp;
//        private static SerializedProperty sensor2TargetRotationProp;

//        public static void InitHand(SerializedObject serializedObject) {
//            enabledProp = serializedObject.FindProperty("viveTracker.enabled");

//            attachedBoneProp = serializedObject.FindProperty("viveTracker.attachedBone");
//            attachedBoneProp.intValue = (int)ArmBones.Hand;

//            sensorTransformProp = serializedObject.FindProperty("viveTracker.sensorTransform");
//            sensor2TargetPositionProp = serializedObject.FindProperty("viveTracker.sensor2TargetPosition");
//            sensor2TargetRotationProp = serializedObject.FindProperty("viveTracker.sensor2TargetRotation");
//        }

//        public static void HandInspector(HandTarget handTarget) {
//            enabledProp.boolValue = Target_Editor.ControllerInspector(handTarget.viveTracker, handTarget);

//            if (enabledProp.boolValue) {
//                CheckSensorTransform(handTarget);
//                EditorGUI.indentLevel++;
//                sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensorTransformProp.objectReferenceValue, typeof(Transform), true);
//                attachedBoneProp.intValue = (int)(ArmBones)EditorGUILayout.EnumPopup("Bone", (ArmBones)attachedBoneProp.intValue);
//                if ((ArmBones)attachedBoneProp.intValue == ArmBones.Hand && handTarget.steamVRHand.enabled) {
//                    EditorGUILayout.HelpBox("SteamVR Controller and Vive Tracker are both on the hand", MessageType.Warning);
//                }
//                EditorGUI.indentLevel--;
//            } else {
//                sensorTransformProp.objectReferenceValue = SteamVR_Editor.RemoveTracker((Transform) sensorTransformProp.objectReferenceValue);
//            }
//        }

//        private static void CheckSensorTransform(HandTarget handTarget) {
//            if (sensorTransformProp.objectReferenceValue == null)
//                sensorTransformProp.objectReferenceValue = ViveTrackerArm.CreateTrackerObject(handTarget);
//            SetSensor2Target(handTarget);
//        }

//        public static void SetSensor2Target(HandTarget handTarget) {
//            if (handTarget.viveTracker.sensorTransform == null)
//                return;

//            Target.TargetedBone targetBone = handTarget.GetTargetBone(handTarget.viveTracker.attachedBone);
//            sensor2TargetRotationProp.quaternionValue = Quaternion.Inverse(handTarget.viveTracker.sensorTransform.rotation) * targetBone.target.transform.rotation;
//            sensor2TargetPositionProp.vector3Value = Quaternion.Inverse(sensor2TargetRotationProp.quaternionValue) * handTarget.viveTracker.sensorTransform.InverseTransformPoint(targetBone.target.transform.position);
//        }
//    }
//}
//#endif
