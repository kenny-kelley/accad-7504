#if hNEURON
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class PerceptionNeuronHead : UnityHeadSensor {
        public override string name {
            get { return NeuronDevice.name; }
        }

        private NeuronTracker neuronTracker;
        private SensorBone headSensor;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            neuronTracker = headTarget.humanoid.neuronTracker;
            tracker = neuronTracker;

            if (neuronTracker.device == null)
                return;

            headSensor = neuronTracker.device.GetBone(0, Bone.Head);
        }
        #endregion

        #region Update
        public override void Update() {
            status = Status.Unavailable;
            if (tracker == null ||
                    !tracker.enabled ||
                    !enabled ||
                    neuronTracker.device == null ||
                    tracker.status == Status.Unavailable)                    
                return;

            status = Status.Present;
            if (headSensor.rotationConfidence == 0)
                return;

            if (headTarget.head.target.confidence.position >= 0.9F)
                FusePosition();
            if (headTarget.head.target.confidence.rotation >= 0.9F)
                FuseRotation();

            UpdateHead(headTarget.head.target);

            status = Status.Tracking;
        }

        public void UpdateHead(HumanoidTarget.TargetTransform headTarget) {
            float confidence = headSensor.rotationConfidence;
            if (confidence > headTarget.confidence.rotation) {
                headTarget.confidence.rotation = confidence;
                headTarget.transform.rotation = tracker.trackerTransform.rotation * headSensor.rotation;
            }
        }
        #endregion

        #region Fusion
        // We use a complementary filter to fight drift in the Neuron sensors
        const float beta = 2F; // the percentage of external tracking used per second, currently a constant, may be derived from the time of the latest external measurement


        private void FuseRotation() {
            Quaternion neuronHeadRotation = tracker.trackerTransform.rotation * headSensor.rotation;
            float neuronYrot = neuronHeadRotation.eulerAngles.y;

            float externalYrot = headTarget.neck.target.transform.eulerAngles.y;
            //if (headTarget.head.target.confidence.rotation > headTarget.neck.target.confidence.rotation)
                externalYrot = headTarget.head.target.transform.eulerAngles.y;

            float resultingRot = Mathf.LerpAngle(neuronYrot, externalYrot, beta * Time.deltaTime);

            float deltaY = -UnityAngles.Difference(resultingRot, neuronYrot);
            tracker.trackerTransform.RotateAround(headTarget.head.target.transform.position, Vector3.up, deltaY);
        }

        private void FusePosition() {
            HipsTarget hipsTarget = headTarget.humanoid.hipsTarget;
            hipsTarget.chest.target.length = 0.4F;

            Vector3 neuronNeckPos = hipsTarget.chest.target.transform.position + hipsTarget.chest.target.transform.rotation * Vector3.up * (hipsTarget.chest.target.length - 0.08F);

            Vector3 externalNeckPos = headTarget.neck.target.transform.position;
            if (headTarget.head.target.confidence.position > headTarget.neck.target.confidence.position) {
                Vector3 externalHeadPos = headTarget.head.target.transform.position;
                externalNeckPos = externalHeadPos + headTarget.neck.target.transform.rotation * Vector3.down * headTarget.neck.target.length;
            }
            Vector3 resultNeckPos = Vector3.Lerp(neuronNeckPos, externalNeckPos, beta * Time.deltaTime);

            Vector3 deltaPos = resultNeckPos - neuronNeckPos;

            tracker.trackerTransform.transform.Translate(deltaPos, Space.World);
        }
        #endregion
    }
}
#endif