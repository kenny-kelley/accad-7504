#if hOPTITRACK
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OptitrackHead : UnityHeadSensor {
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

        public OptiTracker optitrackTracker;

        #region Start
        public override void Init(HeadTarget _headTarget) {
            base.Init(_headTarget);
            if (headTarget.humanoid != null) {
                optitrackTracker = headTarget.humanoid.optitrack;
                tracker = optitrackTracker;
            }
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            optitrackTracker = headTarget.humanoid.optitrack;
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
                    return;
                case OptiTracker.TrackingType.Skeleton:
                    UpdateSkeleton();
                    return;
            }
        }

        private void UpdateRigidbody() {
            if (optitrackRigidbody == null) {
                UpdateTarget(headTarget.head.target, sensorTransform);
                return;
            }

            optitrackRigidbody.UpdateComponent();
            if (optitrackRigidbody.status != Status.Tracking)
                return;

            UpdateTarget(headTarget.head.target, optitrackRigidbody);
        }

        private void UpdateSkeleton() {
            float confidence = 1;
            headTarget.neck.target.transform.position = optitrackTracker.GetTargetPosition(Bone.Neck);
            headTarget.neck.target.confidence.position = confidence;
            headTarget.neck.target.transform.rotation = optitrackTracker.GetTargetRotation(Bone.Neck);
            headTarget.neck.target.confidence.rotation = confidence;

            headTarget.head.target.transform.position = optitrackTracker.GetTargetPosition(Bone.Head);
            headTarget.head.target.confidence.position = confidence;
            headTarget.head.target.transform.rotation = optitrackTracker.GetTargetRotation(Bone.Head);
            headTarget.head.target.confidence.rotation = confidence;
        }
        #endregion
    }
}
#endif