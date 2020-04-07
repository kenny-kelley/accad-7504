#if hOPTITRACK
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OptitrackLeg : UnityLegSensor {
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

        private Bone upperLegBoneID;
        private Bone lowerLegBoneID;
        private Bone footBoneID;
        private Bone toesBoneID;

        #region Start
        public override void Init(FootTarget _footTarget) {
            base.Init(_footTarget);
            optitrackTracker = footTarget.humanoid.optitrack;
            tracker = optitrackTracker;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            optitrackTracker = footTarget.humanoid.optitrack;
            tracker = optitrackTracker;

            if (footTarget.isLeft) {
                upperLegBoneID = Bone.LeftUpperLeg;
                lowerLegBoneID = Bone.LeftLowerLeg;
                footBoneID = Bone.LeftFoot;
                toesBoneID = Bone.LeftToes;
            } else {
                upperLegBoneID = Bone.RightUpperLeg;
                lowerLegBoneID = Bone.RightLowerLeg;
                footBoneID = Bone.RightFoot;
                toesBoneID = Bone.RightToes;
            }

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
                UpdateTarget(footTarget.foot.target, sensorTransform);
                return;
            }

            optitrackRigidbody.UpdateComponent();
            if (optitrackRigidbody.status != Status.Tracking)
                return;

            UpdateTarget(footTarget.foot.target, optitrackRigidbody);
        }

        private void UpdateSkeleton() {
            float confidence = 1;

            footTarget.upperLeg.target.transform.position = footTarget.humanoid.optitrack.GetTargetPosition(upperLegBoneID);
            footTarget.upperLeg.target.confidence.position = confidence;
            footTarget.upperLeg.target.transform.rotation = footTarget.humanoid.optitrack.GetTargetRotation(upperLegBoneID);
            footTarget.upperLeg.target.confidence.rotation = confidence;

            footTarget.lowerLeg.target.transform.position = footTarget.humanoid.optitrack.GetTargetPosition(lowerLegBoneID);
            footTarget.lowerLeg.target.confidence.position = confidence;
            footTarget.lowerLeg.target.transform.rotation = footTarget.humanoid.optitrack.GetTargetRotation(lowerLegBoneID);
            footTarget.lowerLeg.target.confidence.rotation = confidence;

            footTarget.foot.target.transform.position = footTarget.humanoid.optitrack.GetTargetPosition(footBoneID);
            footTarget.foot.target.confidence.position = confidence;
            footTarget.foot.target.transform.rotation = footTarget.humanoid.optitrack.GetTargetRotation(footBoneID);
            footTarget.foot.target.confidence.rotation = confidence;

            footTarget.toes.target.transform.position = footTarget.humanoid.optitrack.GetTargetPosition(toesBoneID);
            footTarget.toes.target.confidence.position = confidence;
            footTarget.toes.target.transform.rotation = footTarget.humanoid.optitrack.GetTargetRotation(toesBoneID);
            footTarget.toes.target.confidence.rotation = confidence;
        }
        #endregion
    }
}
#endif