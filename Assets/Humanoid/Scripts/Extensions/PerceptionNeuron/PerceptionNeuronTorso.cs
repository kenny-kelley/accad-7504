#if hNEURON
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class PerceptionNeuronTorso : UnityTorsoSensor {
        public override string name {
            get { return NeuronDevice.name; }
        }

        private NeuronTracker neuronTracker;

        private SensorBone chestSensor;
        private SensorBone spineSensor;
        private SensorBone hipsSensor;

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            neuronTracker = hipsTarget.humanoid.neuronTracker;
            tracker = neuronTracker;

            if (neuronTracker.device == null)
                return;

            hipsSensor = neuronTracker.device.GetBone(0, Bone.Hips);
            spineSensor = neuronTracker.device.GetBone(0, Bone.Spine);
            chestSensor = neuronTracker.device.GetBone(0, Bone.Chest);
        }

        public override void Update() {
            if (tracker == null ||
                    !tracker.enabled ||
                    !enabled ||
                    neuronTracker.device == null ||
                    tracker.status == Status.Unavailable)
                return;

            status = Status.Present;
            if (hipsSensor.rotationConfidence == 0)
                return;

            UpdateHips(hipsTarget.hips.target);
            UpdateSpine(hipsTarget.spine.target);
            UpdateChest(hipsTarget.chest.target);

            HumanoidTarget.TargetTransform neckTarget = hipsTarget.humanoid.headTarget.neck.target;
            neckTarget.transform.position = hipsTarget.chest.target.transform.position + hipsTarget.chest.target.transform.rotation * Vector3.up * (hipsTarget.chest.target.length);

            status = Status.Tracking;
        }

        private void UpdateHips(HumanoidTarget.TargetTransform hipsTarget) {
            float confidence = hipsSensor.rotationConfidence;
            if (confidence > 0) {
                hipsTarget.confidence.rotation = confidence;
                hipsTarget.transform.rotation = tracker.trackerTransform.rotation * hipsSensor.rotation; 
            }

            confidence = hipsSensor.positionConfidence;
            if (confidence > 0) {
                hipsTarget.confidence.position = confidence;
                // This is double, in the NeuronTracker this happens too...
                // Currently it is disabled in the NeuronTracker (see PerceptionNeuron.cs, line 47)
                hipsTarget.transform.position = neuronTracker.trackerTransform.TransformPoint(hipsSensor.position);
            }
        }

        private void UpdateSpine(HumanoidTarget.TargetTransform spineTarget) {
            float confidence = spineSensor.rotationConfidence;
            if (confidence > 0) {
                spineTarget.confidence.rotation = confidence;
                spineTarget.transform.rotation = tracker.trackerTransform.rotation * spineSensor.rotation;
            }
        }

        private void UpdateChest(HumanoidTarget.TargetTransform chestTarget) {
            float confidence = chestSensor.rotationConfidence;
            if (confidence > 0) {
                chestTarget.confidence.rotation = confidence;
                chestTarget.transform.rotation = tracker.trackerTransform.rotation * chestSensor.rotation;
            }
        }

    }
}
#endif