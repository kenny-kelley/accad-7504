#if hNEURON
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class PerceptionNeuronLeg : UnityLegSensor {

        public override string name {
            get { return NeuronDevice.name; }
        }

        private NeuronTracker neuronTracker;

        private SensorBone upperLegSensor;
        private SensorBone lowerLegSensor;
        private SensorBone footSensor;

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            neuronTracker = footTarget.humanoid.neuronTracker;
            tracker = neuronTracker;

            if (neuronTracker.device == null)
                return;

            Side side = footTarget.isLeft ? Side.Left : Side.Right;
            upperLegSensor = neuronTracker.device.GetBone(0, side, SideBone.UpperLeg);
            lowerLegSensor = neuronTracker.device.GetBone(0, side, SideBone.LowerLeg);
            footSensor = neuronTracker.device.GetBone(0, side, SideBone.Foot);
        }

        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled || neuronTracker.device == null)
                return;

            UpdateUpperLeg(footTarget.upperLeg.target);
            UpdateLowerLeg(footTarget.lowerLeg.target, footTarget.upperLeg);
            UpdateFoot(footTarget.foot.target, footTarget.lowerLeg);

            if (footTarget.humanoid.headTarget.head.target.confidence.position < 0.9 && footTarget.isLeft)
                FootOnGroundCheck(footTarget);

            status = neuronTracker.device.status;
        }

        private void UpdateUpperLeg(HumanoidTarget.TargetTransform upperLegTarget) {
            upperLegTarget.confidence.rotation = upperLegSensor.rotationConfidence;
            if (upperLegTarget.confidence.rotation > 0)
                upperLegTarget.transform.rotation = tracker.trackerTransform.rotation * upperLegSensor.rotation;
        }

        private void UpdateLowerLeg(HumanoidTarget.TargetTransform lowerLegTarget, FootTarget.TargetedUpperLegBone upperLeg) {
            lowerLegTarget.confidence.rotation = lowerLegSensor.rotationConfidence;
            if (lowerLegTarget.confidence.rotation > 0) {
                lowerLegTarget.transform.rotation = tracker.trackerTransform.rotation * lowerLegSensor.rotation;
            }
        }

        private void UpdateFoot(HumanoidTarget.TargetTransform footTarget, FootTarget.TargetedLowerLegBone lowerLeg) {
            footTarget.confidence.rotation = footSensor.rotationConfidence;
            footTarget.confidence.position = lowerLeg.target.confidence.rotation - 0.1F;
            if (footTarget.confidence.rotation > 0) {
                footTarget.transform.position = CalculatePosition(lowerLeg.target.transform, lowerLeg.bone.length);
                footTarget.transform.rotation = tracker.trackerTransform.rotation * footSensor.rotation;
            }
        }

        private Vector3 CalculatePosition(Transform parentTransform, float parentBoneLength) {
            Vector3 targetPosition = parentTransform.position + parentTransform.rotation * Vector3.down * parentBoneLength;
            return targetPosition;
        }

        // This function ensures that the avatar is on the ground. It will adjust the tracking root vertically
        private void FootOnGroundCheck(FootTarget footTarget) {
            if (footSensor.rotationConfidence <= 0.1F)
                return;

            HipsTarget hipsTarget = footTarget.humanoid.hipsTarget;
            float upperLegAngle = Quaternion.Angle(hipsTarget.hips.target.transform.rotation, footTarget.upperLeg.target.transform.rotation);
            float lowerLegAngle = Quaternion.Angle(footTarget.upperLeg.target.transform.rotation, footTarget.lowerLeg.target.transform.rotation);
            float angle = upperLegAngle + lowerLegAngle;
            if (angle < 20) { // stretched leg
                float footY = footTarget.foot.target.transform.position.y - footTarget.soleThicknessFoot;
                float diff = footY - hipsTarget.humanoid.transform.position.y;
                neuronTracker.trackerTransform.Translate(0, -diff, 0);
            }

        }
    }

}
#endif