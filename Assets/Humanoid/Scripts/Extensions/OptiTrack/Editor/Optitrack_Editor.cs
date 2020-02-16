using System.IO;
using UnityEngine;
using UnityEditor;

namespace Passer {
    using Humanoid;

    public class Optitrack_Editor : Tracker_Editor {

#if hOPTITRACK

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            private OptiTracker optiTracker;

            private SerializedProperty trackingTypeProp;
            private SerializedProperty skeletonNameProp;

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, OptiTracker _optiTracker)
                : base(serializedObject, targetObjs, _optiTracker, "optitrack") {
                optiTracker = _optiTracker;
                tracker = optiTracker;

                trackingTypeProp = serializedObject.FindProperty("optitrack.trackingType");
                skeletonNameProp = serializedObject.FindProperty("optitrack.skeletonName");
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "OptiTrack");
                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    OptitrackStreamingClient client = CheckScript();

                    optiTracker.streamingClient = (OptitrackStreamingClient)EditorGUILayout.ObjectField("Streaming Client", optiTracker.streamingClient, typeof(OptitrackStreamingClient), true);
                    if (optiTracker.streamingClient == null)
                        optiTracker.streamingClient = client;

                    trackingTypeProp.intValue = (int)(OptiTracker.TrackingType)EditorGUILayout.EnumPopup("Tracking Type", (OptiTracker.TrackingType)trackingTypeProp.intValue);
                    if ((OptiTracker.TrackingType)trackingTypeProp.intValue == OptiTracker.TrackingType.Skeleton) {
                        if (optiTracker.skeletonName == null)
                            optiTracker.skeletonName = "";
                        optiTracker.skeletonName = EditorGUILayout.TextField("Skeleton name", optiTracker.skeletonName);
                    }
                    else
                        optiTracker.skeletonName = null;

                    skeletonNameProp.stringValue = optiTracker.skeletonName;
                    EditorGUI.indentLevel--;
                }
                else {
                    RemoveScript(humanoid);
                }
            }
        }
        #endregion

        private static OptitrackStreamingClient CheckScript() {
            OptitrackStreamingClient client = FindObjectOfType<OptitrackStreamingClient>();
            if (client == null) {
                GameObject rootGameObject = new GameObject("Humanoid OptitrackStreamingClient");
                client = rootGameObject.AddComponent<OptitrackStreamingClient>();
            }

            return client;
        }

        private static void RemoveScript(HumanoidControl humanoid) {
            OptitrackStreamingClient client = FindObjectOfType<OptitrackStreamingClient>();
            if (client != null && client.gameObject.name == "Humanoid OptitrackStreamingClient")
                DestroyImmediate(client.gameObject, true);
        }

        //public static void SensorInspector(ObjectTracker objectTracker) {
        //    //ObjectTracker_Editor.SensorInspector(objectTracker.optitrack, "OptiTrack");

        //    //if (objectTracker.optitrack.enabled) {
        //    //    EditorGUI.indentLevel++;
        //    //    objectTracker.optitrack.trackerId = EditorGUILayout.IntField("Tracker ID", objectTracker.optitrack.trackerId);
        //    //    objectTracker.optitrack.sensorTransform = (Transform)EditorGUILayout.ObjectField("Tracker Transform", objectTracker.optitrack.sensorTransform, typeof(Transform), true);
        //    //    EditorGUI.indentLevel--;
        //    //}
        //}

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public SerializedProperty trackerIdProp;

            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.optitrack, headTarget, "optitrack") {

            }

            public override void Inspector() {
                if (!headTarget.humanoid.optitrack.enabled)
                    return;

                CheckRigidbodyComponent(headTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
                sensor.enabled = enabledProp.boolValue;
                sensor.CheckSensorTransform();

                if (enabledProp.boolValue && (headTarget.humanoid.optitrack.trackingType == OptiTracker.TrackingType.Rigidbody)) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Sensor Transform", sensor.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckRigidbodyComponent(HeadTarget headTarget) {
                if (headTarget.optitrack.sensorTransform == null)
                    return;

                OptitrackRigidbodyComponent sensorComponent = headTarget.optitrack.sensorTransform.GetComponent<OptitrackRigidbodyComponent>();
                if (sensorComponent == null)
                    headTarget.optitrack.sensorTransform.gameObject.AddComponent<OptitrackRigidbodyComponent>();
            }
        }
        #endregion

        #region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            //public SerializedProperty trackerIdProp;

            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.optitrack, handTarget, "optitrack") {

                //trackerIdProp = serializedObject.FindProperty("optitrack.trackerId");
            }

            public override void Inspector() {
                if (!handTarget.humanoid.optitrack.enabled)
                    return;

                CheckRigidbodyComponent(handTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, handTarget);
                sensor.enabled = enabledProp.boolValue;
                sensor.CheckSensorTransform();

                if (enabledProp.boolValue && (handTarget.humanoid.optitrack.trackingType == OptiTracker.TrackingType.Rigidbody)) {
                    EditorGUI.indentLevel++;
                    //trackerIdProp.intValue = EditorGUILayout.IntField("Tracker Id", trackerIdProp.intValue);
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensor.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckRigidbodyComponent(HandTarget handTarget) {
                if (handTarget.optitrack.sensorTransform == null)
                    return;

                OptitrackRigidbodyComponent sensorComponent = handTarget.optitrack.sensorTransform.GetComponent<OptitrackRigidbodyComponent>();
                if (sensorComponent == null)
                    handTarget.optitrack.sensorTransform.gameObject.AddComponent<OptitrackRigidbodyComponent>();
            }

        }
        #endregion

        #region Hips
        public class HipsTargetProps : HipsTarget_Editor.TargetProps {
            //public SerializedProperty trackerIdProp;

            public HipsTargetProps(SerializedObject serializedObject, HipsTarget hipsTarget)
                : base(serializedObject, hipsTarget.optitrack, hipsTarget, "optitrack") {

                //trackerIdProp = serializedObject.FindProperty("optitrack.trackerId");
            }

            public override void Inspector() {
                if (!hipsTarget.humanoid.optitrack.enabled)
                    return;

                CheckRigidbodyComponent(hipsTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, hipsTarget);
                sensor.enabled = enabledProp.boolValue;
                sensor.CheckSensorTransform();

                if (enabledProp.boolValue && (hipsTarget.humanoid.optitrack.trackingType == OptiTracker.TrackingType.Rigidbody)) {
                    EditorGUI.indentLevel++;
                    //trackerIdProp.intValue = EditorGUILayout.IntField("Tracker Id", trackerIdProp.intValue);
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensor.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckRigidbodyComponent(HipsTarget hipsTarget) {
                if (hipsTarget.optitrack.sensorTransform == null)
                    return;

                OptitrackRigidbodyComponent sensorComponent = hipsTarget.optitrack.sensorTransform.GetComponent<OptitrackRigidbodyComponent>();
                if (sensorComponent == null)
                    hipsTarget.optitrack.sensorTransform.gameObject.AddComponent<OptitrackRigidbodyComponent>();
            }

        }
        #endregion

        #region Foot
        public class FootTargetProps : FootTarget_Editor.TargetProps {
            public SerializedProperty trackerIdProp;

            public FootTargetProps(SerializedObject serializedObject, FootTarget footTarget)
                : base(serializedObject, footTarget.optitrack, footTarget, "optitrack") {

                //trackerIdProp = serializedObject.FindProperty("optitrack.trackerId");
            }

            public override void Inspector() {
                if (!footTarget.humanoid.optitrack.enabled)
                    return;

                CheckRigidbodyComponent(footTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, footTarget);
                sensor.enabled = enabledProp.boolValue;
                sensor.CheckSensorTransform();

                if (enabledProp.boolValue && (footTarget.humanoid.optitrack.trackingType == OptiTracker.TrackingType.Rigidbody)) {
                    EditorGUI.indentLevel++;
                    //trackerIdProp.intValue = EditorGUILayout.IntField("Tracker Id", trackerIdProp.intValue);
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", sensor.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckRigidbodyComponent(FootTarget footTarget) {
                if (footTarget.optitrack.sensorTransform == null)
                    return;

                OptitrackRigidbodyComponent sensorComponent = footTarget.optitrack.sensorTransform.GetComponent<OptitrackRigidbodyComponent>();
                if (sensorComponent == null)
                    footTarget.optitrack.sensorTransform.gameObject.AddComponent<OptitrackRigidbodyComponent>();
            }
        }
        #endregion

        #region Rigidbody Component
        [CustomEditor(typeof(OptitrackRigidbodyComponent))]
        public class OptitrackSensorComponent_Editor : Editor {
            OptitrackRigidbodyComponent sensorComponent;

            SerializedProperty streamingIdProp;

            private void OnEnable() {
                sensorComponent = (OptitrackRigidbodyComponent)target;

                streamingIdProp = serializedObject.FindProperty("streamingID");
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", sensorComponent.status);
                EditorGUILayout.FloatField("Position Confidence", sensorComponent.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", sensorComponent.rotationConfidence);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
                streamingIdProp.intValue = EditorGUILayout.IntField("Streaming ID", streamingIdProp.intValue);

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion

#endif
    }
}
