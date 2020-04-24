#if hNEURON
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class NeuronHand : UnityArmSensor {
        public override string name {
            get { return NeuronDevice.name; }
        }

        private NeuronTracker neuronTracker;

        private SensorBone shoulderSensor;
        private SensorBone upperArmSensor;
        private SensorBone forearmSensor;
        private SensorBone handSensor;
        private readonly SensorBone[,] fingerSensors = new SensorBone[5, 3];

        //Transform chestTargetTransform;

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            neuronTracker = handTarget.humanoid.neuronTracker;
            tracker = neuronTracker;

            //if (handTarget.humanoid.hipsTarget.chest.target.transform != null) {
            //    chestTargetTransform = handTarget.humanoid.hipsTarget.chest.target.transform;
            //}
            //else {
            //    chestTargetTransform = handTarget.humanoid.hipsTarget.hips.target.transform;
            //}

            if (neuronTracker.device == null)
                return;

            Side side = handTarget.isLeft ? Side.Left : Side.Right;

            shoulderSensor = neuronTracker.device.GetBone(0, side, SideBone.Shoulder);
            upperArmSensor = neuronTracker.device.GetBone(0, side, SideBone.UpperArm);
            forearmSensor = neuronTracker.device.GetBone(0, side, SideBone.Forearm);
            handSensor = neuronTracker.device.GetBone(0, side, SideBone.Hand);

            for (int i = 0; i < (int)Finger.Count; i++)
                StartFinger(side, handTarget.fingers.allFingers[i], i);
        }

        private void StartFinger(Side side, FingersTarget.TargetedFinger finger, int fingerIx) {
            SideBone sideBoneId = BoneReference.HumanoidSideBone((Finger)fingerIx, FingerBone.Proximal);
            fingerSensors[fingerIx, 0] = neuronTracker.device.GetBone(0, side, sideBoneId);

            sideBoneId = BoneReference.HumanoidSideBone((Finger)fingerIx, FingerBone.Intermediate);
            fingerSensors[fingerIx, 1] = neuronTracker.device.GetBone(0, side, sideBoneId);

            sideBoneId = BoneReference.HumanoidSideBone((Finger)fingerIx, FingerBone.Distal);
            fingerSensors[fingerIx, 2] = neuronTracker.device.GetBone(0, side, sideBoneId);
        }

        public override void Update() {
            if (tracker == null ||
                    !tracker.enabled ||
                    !enabled ||
                    neuronTracker.device == null ||
                    tracker.status == Status.Unavailable)
                return;

            status = Status.Present;
            if (handSensor.rotationConfidence == 0)
                return;

            float armConfidence = ArmConfidence();

            if (handTarget.hand.target.confidence.position > armConfidence)
                FusedUpdatArm();            
            else 
                UpdateArm();

            UpdateFingers();
            status = Status.Tracking;
        }

        private float ArmConfidence() {
            float armOrientationsConfidence =
                shoulderSensor.rotationConfidence *
                upperArmSensor.rotationConfidence *
                forearmSensor.rotationConfidence;
            return armOrientationsConfidence;
        }

        #region Fusion
        private void FusedUpdatArm() {
            // save hand pos/rot to restore after IK.
            Vector3 externalHandTargetPosition = handTarget.hand.target.transform.position;
            Quaternion externalHandTargetRotation = handTarget.hand.target.transform.rotation;

            //UpdateShoulder(handTarget.shoulder.target);
            // I am not happy with the Neuron shoulder rotation in combination with IK,
            // Probably it is better to use the IK rotation
            // But for now, the shoulders are fixed

            Quaternion neuronUpperArmRotation = tracker.trackerTransform.rotation * upperArmSensor.rotation;
            Vector3 elbowAxis = neuronUpperArmRotation * Vector3.up;

            Quaternion upperArmRotation = ArmMovements.UpperArmRotationIK(
                    handTarget.upperArm.target.transform.position, externalHandTargetPosition, elbowAxis, handTarget.upperArm.target.length, handTarget.forearm.target.length, handTarget.isLeft);
            upperArmBias = CalculateBias(upperArmBias, tracker.trackerTransform.rotation * upperArmSensor.rotation, upperArmRotation);
            UpdateUpperArm(handTarget.upperArm.target);

            Quaternion forearmRotation = ArmMovements.ForearmRotationIK(
                    handTarget.forearm.target.transform.position, externalHandTargetPosition, elbowAxis, handTarget.isLeft);
            forearmBias = CalculateBias(forearmBias, tracker.trackerTransform.rotation * forearmSensor.rotation, forearmRotation);
            UpdateForearm(handTarget.forearm.target);

            Debug.DrawRay(handTarget.forearm.target.transform.position, elbowAxis, Color.magenta);

            if (handTarget.hand.target.confidence.rotation < handSensor.rotationConfidence)
                handBias = Quaternion.identity;
            else
                handBias = CalculateBias(handBias, tracker.trackerTransform.rotation * handSensor.rotation, externalHandTargetRotation);
            UpdateHand(handTarget.hand.target);
        }

        // We use a complementary filter to fight drift in the Neuron sensors
        const float beta = 4F; // the percentage of external tracking used per second, currently a constant, may be derived from the time of the latest external measurement

        Quaternion upperArmBias = Quaternion.identity;
        Quaternion forearmBias = Quaternion.identity;
        Quaternion handBias = Quaternion.identity;

        private Quaternion CalculateBias(Quaternion bias, Quaternion sensorBoneRotation, Quaternion targetBoneRotation) {
            Quaternion deltaRot = Quaternion.Inverse(sensorBoneRotation) * targetBoneRotation;
            bias = Quaternion.Slerp(bias, deltaRot, beta * Time.deltaTime);
            // No complementary filter atm
            bias = deltaRot;
            return bias;
        }
        #endregion

        #region Biased Update
        private void UpdateArm() {
            UpdateShoulder(handTarget.shoulder.target);
            UpdateUpperArm(handTarget.upperArm.target);
            UpdateForearm(handTarget.forearm.target);
            UpdateHand(handTarget.hand.target);
        }

        protected void UpdateShoulder(HumanoidTarget.TargetTransform shoulderTarget) {
            shoulderTarget.confidence.rotation = shoulderSensor.rotationConfidence;
            if (shoulderTarget.confidence.rotation > 0)
                shoulderTarget.transform.rotation = tracker.trackerTransform.rotation * shoulderSensor.rotation;
        }

        protected void UpdateUpperArm(HumanoidTarget.TargetTransform upperArmTarget) { 
            upperArmTarget.confidence.rotation = upperArmSensor.rotationConfidence;
            if (upperArmTarget.confidence.rotation > 0)
                upperArmTarget.transform.rotation = tracker.trackerTransform.rotation * upperArmSensor.rotation * upperArmBias;
        }

        protected void UpdateForearm(HumanoidTarget.TargetTransform forearmTarget) {
            forearmTarget.confidence.rotation = forearmSensor.rotationConfidence;
            if (forearmTarget.confidence.rotation > 0)
                forearmTarget.transform.rotation = tracker.trackerTransform.rotation * forearmSensor.rotation * forearmBias;
        }

        protected void UpdateHand(HumanoidTarget.TargetTransform handTarget) {
            handTarget.confidence.rotation = handSensor.rotationConfidence;
            if (handTarget.confidence.rotation > 0)
                handTarget.transform.rotation = tracker.trackerTransform.rotation * handSensor.rotation * handBias;
        }

        protected void UpdateFingers() {
            if (handTarget.hand.target.confidence.rotation == 0)
                return;

            for (int i = 0; i < (int)Finger.Count; i++)
                UpdateFinger(handTarget.fingers.allFingers[i], i);
        }

        private void UpdateFinger(FingersTarget.TargetedFinger finger, int fingerIx) {
            if (Quaternion.Angle(fingerSensors[fingerIx, 0].rotation, Quaternion.identity) == 0)
                return;

            finger.proximal.target.transform.localRotation = Quaternion.Inverse(handSensor.rotation) * fingerSensors[fingerIx, 0].rotation;
            finger.intermediate.target.transform.localRotation = Quaternion.Inverse(fingerSensors[fingerIx, 0].rotation) * fingerSensors[fingerIx, 1].rotation;
            finger.distal.target.transform.localRotation = Quaternion.Inverse(fingerSensors[fingerIx, 1].rotation) * fingerSensors[fingerIx, 2].rotation;
        }


        private Vector3 CalculatePosition(Transform parentTransform, float parentBoneLength) {
            Vector3 targetPosition = parentTransform.position + parentTransform.rotation * handTarget.outward * parentBoneLength;
            return targetPosition;
        }

        #endregion
    }
}
#endif