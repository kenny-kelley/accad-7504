#if hOPTITRACK
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OptitrackTorso : UnityTorsoSensor {
        public override string name {
            get { return "OptiTrack"; }
        }

        private OptitrackRigidbodyComponent optitrackRigidbody;

        public override Status status {
            get {
                if (optitrackRigidbody == null)
                    return Status.Unavailable;
                return optitrackRigidbody.status;
            }
            set { optitrackRigidbody.status = value; }
        }

        private OptiTracker optitrackTracker;
        
        #region Start
        public override void Init(HipsTarget _hipsTarget) {
            base.Init(_hipsTarget);
            optitrackTracker = hipsTarget.humanoid.optitrack;
            tracker = optitrackTracker;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            optitrackTracker = hipsTarget.humanoid.optitrack;
            tracker = optitrackTracker;

            if (sensorTransform != null) {
                optitrackRigidbody = sensorTransform.GetComponent<OptitrackRigidbodyComponent>();
                if (optitrackRigidbody != null)
                    optitrackRigidbody.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            CreateSensorTransform(null, Vector3.zero, Quaternion.identity);

            OptitrackRigidbodyComponent optitrackRigidbody = sensorTransform.GetComponent<OptitrackRigidbodyComponent>();
            if (optitrackRigidbody == null)
                sensorTransform.gameObject.AddComponent<OptitrackRigidbodyComponent>();
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            switch (optitrackTracker.trackingType) {
                case OptiTracker.TrackingType.Rigidbody:
                    UpdateRigidbody();
                    break;

                case OptiTracker.TrackingType.Skeleton:
                    float confidence = 1;

                    hipsTarget.hips.target.transform.position = hipsTarget.humanoid.optitrack.GetTargetPosition(Bone.Hips);
                    hipsTarget.hips.target.confidence.position = confidence;
                    hipsTarget.hips.target.transform.rotation = hipsTarget.humanoid.optitrack.GetTargetRotation(Bone.Hips);
                    hipsTarget.hips.target.confidence.rotation = confidence;

                    hipsTarget.spine.target.transform.position = hipsTarget.humanoid.optitrack.GetTargetPosition(Bone.Spine);
                    hipsTarget.spine.target.confidence.position = confidence;
                    hipsTarget.spine.target.transform.rotation = hipsTarget.humanoid.optitrack.GetTargetRotation(Bone.Spine);
                    hipsTarget.spine.target.confidence.rotation = confidence;

                    hipsTarget.chest.target.transform.position = hipsTarget.humanoid.optitrack.GetTargetPosition(Bone.Chest);
                    hipsTarget.chest.target.confidence.position = confidence;
                    hipsTarget.chest.target.transform.rotation = hipsTarget.humanoid.optitrack.GetTargetRotation(Bone.Chest);
                    hipsTarget.chest.target.confidence.rotation = confidence;
                    break;
                default:
                    break;
            }
        }

        private void UpdateRigidbody() {
            if (optitrackRigidbody == null) {
                UpdateTarget(hipsTarget.hips.target, sensorTransform);
                return;
            }

            optitrackRigidbody.UpdateComponent();
            if (optitrackRigidbody.status != Status.Tracking)
                return;

            UpdateTarget(hipsTarget.hips.target, optitrackRigidbody);
        }

        #endregion
    }
}
#endif