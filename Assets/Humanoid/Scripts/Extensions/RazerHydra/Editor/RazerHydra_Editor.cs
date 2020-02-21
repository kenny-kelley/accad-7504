using UnityEngine;
using UnityEditor;

namespace Passer {
    using Humanoid;

    public class Hydra_Editor : Tracker_Editor {

#if hHYDRA

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, HydraTracker _hydraTracker)
                : base(serializedObject, targetObjs, _hydraTracker, "hydra") {
             //   tracker = _hydraTracker;
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "Hydra BaseStation");

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    trackerTransfromProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", humanoid.hydra.trackerTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion

        #region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.hydra, handTarget, "hydra") {
            }

            public override void Inspector() {
                if (!handTarget.humanoid.hydra.enabled)
                    return;

                CheckSensorComponent(handTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(handTarget.hydra, handTarget);
                handTarget.hydra.enabled = enabledProp.boolValue;
                handTarget.hydra.CheckSensorTransform();

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", handTarget.hydra.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
                else {
                    RemoveTransform(handTarget.hydra.sensorTransform);
                    sensorTransformProp.objectReferenceValue = null;
                }
            }

            protected static void CheckSensorComponent(HandTarget handTarget) {
                if (handTarget.hydra.sensorTransform == null)
                    return;

                HydraControllerComponent sensorComponent = handTarget.hydra.sensorTransform.GetComponent<HydraControllerComponent>();
                if (sensorComponent == null)
                    sensorComponent = handTarget.hydra.sensorTransform.gameObject.AddComponent<HydraControllerComponent>();
            }
        }

        public static Transform RemoveController(UnitySensor sensor) {
            if (sensor.sensorTransform != null)
                DestroyImmediate(sensor.sensorTransform.gameObject, true);
            return null;
        }
        #endregion

        #region Object
        /*
        private static SerializedProperty objectEnabledProp;
        private static SerializedProperty objectSensorTransformProp;
        private static SerializedProperty objectSensor2TargetPositionProp;
        private static SerializedProperty objectSensor2TargetRotationProp;

        public static void InitObject(SerializedObject serializedObject, ObjectTarget objectTarget) {
            objectEnabledProp = serializedObject.FindProperty("hydraController.enabled");
            objectSensorTransformProp = serializedObject.FindProperty("hydraController.sensorTransform");
            objectSensor2TargetPositionProp = serializedObject.FindProperty("hydraController.sensor2TargetPosition");
            objectSensor2TargetRotationProp = serializedObject.FindProperty("hydraController.sensor2TargetRotation");

            objectTarget.hydra.Init(objectTarget);
        }

        private enum LeftRight {
            Left,
            Right
        }

        public static void ObjectInspector(RazerHydraController controller) {
            objectEnabledProp.boolValue = Target_Editor.ControllerInspector(controller);
            controller.CheckSensorTransform();

            if (objectEnabledProp.boolValue) {
                EditorGUI.indentLevel++;
                LeftRight leftRight = controller.isLeft ? LeftRight.Left : LeftRight.Right;
                leftRight = (LeftRight)EditorGUILayout.EnumPopup("Tracker Id", leftRight);
                controller.isLeft = leftRight == LeftRight.Left;
                objectSensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", controller.sensorTransform, typeof(Transform), true);
                EditorGUI.indentLevel--;
            }
        }

        public static void SetSensor2Target(RazerHydraController controller) {
            controller.SetSensor2Target();
            objectSensor2TargetRotationProp.quaternionValue = controller.sensor2TargetRotation;
            objectSensor2TargetPositionProp.vector3Value = controller.sensor2TargetPosition;
        }
        */
        #endregion

        #region Sensor Component
        [CustomEditor(typeof(HydraControllerComponent))]
        public class HydraControllerComponent_Editor : Editor {
            HydraControllerComponent hydraController;

            private void OnEnable() {
                hydraController = (HydraControllerComponent)target;
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", hydraController.status);
                EditorGUILayout.FloatField("Position Confidence", hydraController.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", hydraController.rotationConfidence);
                EditorGUILayout.Toggle("Is Left", hydraController.isLeft);

                EditorGUILayout.Vector3Field("Joystick", hydraController.joystick);
                EditorGUILayout.Slider("Bumper", hydraController.bumper, -1, 1);
                EditorGUILayout.Slider("Trigger", hydraController.trigger, -1, 1);
                EditorGUILayout.Slider("Button 1", hydraController.button1, -1, 1);
                EditorGUILayout.Slider("Button 2", hydraController.button2, -1, 1);
                EditorGUILayout.Slider("Button 3", hydraController.button3, -1, 1);
                EditorGUILayout.Slider("Button 4", hydraController.button4, -1, 1);
                EditorGUILayout.Slider("Option", hydraController.option, -1, 1);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion

#endif
    }
}
