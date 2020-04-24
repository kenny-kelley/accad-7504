/* Humanoid Vive Tracker editor
 * Copyright (c) 2017 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 4.0.0
 * date: May 27, 2017
 * 
 */
/*
using UnityEngine;
using UnityEditor;

#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
namespace Passer {

    public class ViveTrackerFoot_Editor : Editor {

        private static SerializedProperty enabledProp;
        private static SerializedProperty sensorTransformProp;
        private static SerializedProperty sensor2TargetPositionProp;
        private static SerializedProperty sensor2TargetRotationProp;

        public static void InitFoot(SerializedObject serializedObject) {
            enabledProp = serializedObject.FindProperty("viveTracker.enabled");

            sensorTransformProp = serializedObject.FindProperty("viveTracker.sensorTransform");
            sensor2TargetPositionProp = serializedObject.FindProperty("viveTracker.sensor2TargetPosition");
            sensor2TargetRotationProp = serializedObject.FindProperty("viveTracker.sensor2TargetRotation");
        }

        public static void FootInspector(HumanoidControl humanoid, FootTarget footTarget) {
            enabledProp.boolValue = Target_Editor.ControllerInspector(footTarget.viveTracker, footTarget);

            if (enabledProp.boolValue) {
                CheckSensorTransform(footTarget);
                EditorGUI.indentLevel++;
                sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensorTransformProp.objectReferenceValue, typeof(Transform), true);
                EditorGUI.indentLevel--;
            } else {
                sensorTransformProp.objectReferenceValue = Tracker_Editor.RemoveTracker((Transform)sensorTransformProp.objectReferenceValue);
            }
        }

        private static void CheckSensorTransform(FootTarget footTarget) {
            if (sensorTransformProp.objectReferenceValue == null)
                sensorTransformProp.objectReferenceValue = ViveTrackerLeg.CreateTrackerObject(footTarget);
            SetSensor2Target(footTarget);
        }

        public static void SetSensor2Target(FootTarget footTarget) {
            if (footTarget.viveTracker.sensorTransform == null)
                return;

            sensor2TargetRotationProp.quaternionValue = Quaternion.Inverse(footTarget.viveTracker.sensorTransform.rotation) * footTarget.foot.target.transform.rotation;
            sensor2TargetPositionProp.vector3Value = Quaternion.Inverse(sensor2TargetRotationProp.quaternionValue) * footTarget.viveTracker.sensorTransform.InverseTransformPoint(footTarget.foot.target.transform.position);
        }
    }
}
#endif
*/