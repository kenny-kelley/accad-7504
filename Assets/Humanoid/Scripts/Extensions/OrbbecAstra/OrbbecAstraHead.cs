#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_WSA_10_0)
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class AstraHead : UnityHeadSensor {
        public override string name {
            get { return AstraDevice.name; }
        }

        private AstraTracker astraTracker;
        private SensorBone neckSensor;
        private SensorBone headSensor;

        public bool headTracking = true;
        public enum RotationTrackingAxis {
            XYZ,
            XY
        }
        public RotationTrackingAxis rotationTrackingAxis = RotationTrackingAxis.XY;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            astraTracker = headTarget.humanoid.astra;
            tracker = astraTracker;

            if (astraTracker.device == null)
                return;

            neckSensor = astraTracker.device.GetBone(0, Bone.Neck);
            headSensor = astraTracker.device.GetBone(0, Bone.Head);
        }
        #endregion

        #region Update

        protected bool calibrated = false;

        public override void Update() {
            status = Status.Unavailable;
            if (tracker == null ||
                    !tracker.enabled ||
                    !enabled ||
                    astraTracker.device == null ||
                    tracker.status == Status.Unavailable)
                return;

            status = Status.Present;
            if (headSensor.positionConfidence == 0)
                return;

            UpdateBones();

            status = Status.Tracking;

            if (!calibrated && tracker.humanoid.calibrateAtStart) {
                tracker.humanoid.Calibrate();
                calibrated = true;
            }
        }

        protected void UpdateBones() {
            if (headTarget.head.target.confidence.position > headSensor.positionConfidence) {
                if (headTarget.humanoid.leftHandTarget.hand.target.confidence.position > astraTracker.device.GetBoneConfidence(0, Side.Left, SideBone.Hand) &&
                    headTarget.humanoid.rightHandTarget.hand.target.confidence.position > astraTracker.device.GetBoneConfidence(0, Side.Right, SideBone.Hand))
                    astraTracker.CalibrateWithHeadAndHands(headSensor, headTarget.humanoid.leftHandTarget.astra.handSensor, headTarget.humanoid.rightHandTarget.astra.handSensor);
                else
                    astraTracker.CalibrateWithHead(headSensor);
                return;
            }

            UpdateNeck(headTarget.neck.target);
            UpdateHead(headTarget.head.target);
        }

        private void UpdateNeck(HumanoidTarget.TargetTransform neckTarget) {
            neckTarget.confidence.position = neckSensor.positionConfidence;
            if (neckTarget.confidence.position > 0)
                neckTarget.transform.position = neckSensor.position;

            neckTarget.confidence.rotation = (neckSensor.positionConfidence + headSensor.positionConfidence) / 2 * 0.8F;
            if (neckTarget.confidence.rotation > 0) {
                Vector3 toHead = headSensor.position - neckSensor.position;
                Quaternion rotation =
                    Quaternion.LookRotation(toHead, -headTarget.humanoid.transform.forward) *
                    Quaternion.AngleAxis(90, headTarget.humanoid.transform.right);
                neckTarget.transform.rotation = rotation;

                if (rotationTrackingAxis == RotationTrackingAxis.XY)
                    neckTarget.transform.rotation = Quaternion.LookRotation(neckTarget.transform.rotation * Vector3.forward);
            }
        }

        private void UpdateHead(HumanoidTarget.TargetTransform headTarget) {
            headTarget.confidence.position = headSensor.positionConfidence;
            if (headTarget.confidence.position > 0)
                headTarget.transform.position = headSensor.position;
        }

        #endregion
    }
}
#endif