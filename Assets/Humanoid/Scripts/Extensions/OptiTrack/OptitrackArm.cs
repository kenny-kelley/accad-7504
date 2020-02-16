#if hOPTITRACK
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OptitrackArm : UnityArmSensor {
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

        private Bone shoulderBoneID;
        private Bone upperArmBoneID;
        private Bone forearmBoneID;
        private Bone handBoneID;

        #region Start
        public override void Init(HandTarget _handTarget) {
            base.Init(_handTarget);
            optitrackTracker = handTarget.humanoid.optitrack;
            tracker = optitrackTracker;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            optitrackTracker = handTarget.humanoid.optitrack;
            tracker = optitrackTracker;

            if (handTarget.isLeft) {
                shoulderBoneID = Bone.LeftShoulder;
                upperArmBoneID = Bone.LeftUpperArm;
                forearmBoneID = Bone.LeftForearm;
                handBoneID = Bone.LeftHand;
            } else {
                shoulderBoneID = Bone.RightShoulder;
                upperArmBoneID = Bone.RightUpperArm;
                forearmBoneID = Bone.RightForearm;
                handBoneID = Bone.RightHand;
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
                UpdateTarget(handTarget.hand.target, sensorTransform);
                return;
            }

            optitrackRigidbody.UpdateComponent();
            if (optitrackRigidbody.status != Status.Tracking)
                return;

            UpdateTarget(handTarget.hand.target, optitrackRigidbody);
        }

        private void UpdateSkeleton() {
            float confidence = 1;
            handTarget.shoulder.target.transform.position = optitrackTracker.GetTargetPosition(shoulderBoneID);
            handTarget.shoulder.target.confidence.position = confidence;
            handTarget.shoulder.target.transform.rotation = optitrackTracker.GetTargetRotation(shoulderBoneID);
            handTarget.shoulder.target.confidence.rotation = confidence;

            handTarget.upperArm.target.transform.position = optitrackTracker.GetTargetPosition(upperArmBoneID);
            handTarget.upperArm.target.confidence.position = confidence;
            handTarget.upperArm.target.transform.rotation = optitrackTracker.GetTargetRotation(upperArmBoneID);
            handTarget.upperArm.target.confidence.rotation = confidence;

            handTarget.forearm.target.transform.position = optitrackTracker.GetTargetPosition(forearmBoneID);
            handTarget.forearm.target.confidence.position = confidence;
            handTarget.forearm.target.transform.rotation = optitrackTracker.GetTargetRotation(forearmBoneID);
            handTarget.forearm.target.confidence.rotation = confidence;

            handTarget.hand.target.transform.position = optitrackTracker.GetTargetPosition(handBoneID);
            handTarget.hand.target.confidence.position = confidence;
            handTarget.hand.target.transform.rotation = optitrackTracker.GetTargetRotation(handBoneID);
            handTarget.hand.target.confidence.rotation = confidence;

            UpdateHand();
        }

        private void UpdateHand() {
            for (int fingerNr = 0; fingerNr < 5; fingerNr++)
                UpdateFinger(fingerNr);
        }

        private void UpdateFinger(int fingerNr) {
            UpdateFingerBone(handTarget.fingers.allFingers[fingerNr].proximal, fingerNr, 0);
            UpdateFingerBone(handTarget.fingers.allFingers[fingerNr].proximal, fingerNr, 1);
            UpdateFingerBone(handTarget.fingers.allFingers[fingerNr].proximal, fingerNr, 2);
        }

        private void UpdateFingerBone(FingersTarget.TargetedPhalanges phalange, int fingerNr, int boneNr) {
            Vector3 position = handTarget.humanoid.optitrack.GetTargetPosition(FingerBone(fingerNr, boneNr, handTarget.isLeft));
            if (position != Vector3.zero) {
                phalange.target.transform.position = position;
                phalange.target.transform.rotation = handTarget.humanoid.optitrack.GetTargetRotation(FingerBone(fingerNr, boneNr, handTarget.isLeft));
            }
        }

        public Bone FingerBone(int finger, int bone, bool isLeft) {
            int offset = 0;
            if (isLeft)
                offset = 40;
            else
                offset = 17;

            offset += finger * 4 + bone;

            return (Bone)offset;
        }
        #endregion
    }
}
#endif