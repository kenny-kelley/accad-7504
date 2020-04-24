using UnityEngine;
using UnityEditor;

namespace Passer.Humanoid {

    public class Neuron_Editor : Tracker_Editor {

#if hNEURON
#region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, NeuronTracker neuronTracker)
                : base(serializedObject, targetObjs, neuronTracker, "neuronTracker") {
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "PerceptionNeuron");
                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    humanoid.neuronTracker.address = EditorGUILayout.TextField("Address", humanoid.neuronTracker.address);
                    EditorGUILayout.BeginHorizontal();
                    humanoid.neuronTracker.port = EditorGUILayout.IntField("Port", humanoid.neuronTracker.port);
                    //EditorGUI.indentLevel--;
                    //humanoid.neuronTracker.socketType = (Neuron.NeuronConnection.SocketType)EditorGUILayout.EnumPopup(humanoid.neuronTracker.socketType, GUILayout.Width(60));
                    //EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();
                    //humanoid.neuronTracker.actorName = EditorGUILayout.TextField("Actor Name", humanoid.neuronTracker.actorName);
                    EditorGUI.indentLevel--;
                }
            }
        }
#endregion

#region Head

        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.neuron, headTarget, "neuron") {
            }

            public override void Inspector() {
                if (headTarget.humanoid.neuronTracker.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, headTarget);
            }
        }

#endregion

#region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.neuron, handTarget, "neuron") {
            }

            public override void Inspector() {
                if (handTarget.humanoid.neuronTracker.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, handTarget);
            }
        }
#endregion

#region Hips
        public class HipsTargetProps : HipsTarget_Editor.TargetProps {
            public HipsTargetProps(SerializedObject serializedObject, HipsTarget hipsTarget)
                : base(serializedObject, hipsTarget.neuron, hipsTarget, "neuron") {
            }

            public override void Inspector() {
                if (hipsTarget.humanoid.neuronTracker.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, hipsTarget);
            }
        }
#endregion

#region Foot
        public class FootTargetProps : FootTarget_Editor.TargetProps {
            public FootTargetProps(SerializedObject serializedObject, FootTarget footTarget)
                : base(serializedObject, footTarget.neuron, footTarget, "neuron") {
            }

            public override void Inspector() {
                if (footTarget.humanoid.neuronTracker.enabled)
                    enabledProp.boolValue = Target_Editor.ControllerInspector(sensor, footTarget);
            }
        }
#endregion

#endif
    }
}
