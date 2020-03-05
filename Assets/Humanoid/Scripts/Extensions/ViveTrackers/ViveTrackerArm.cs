#if (hSTEAMVR || hOPENVR) && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class ViveTrackerArm : UnityArmSensor {
        public ViveTrackerArm() {
            enabled = false;
        }

        public override string name {
            get { return "Vive Tracker"; }
        }

        private ViveTrackerComponent viveTracker;

        public override Status status {
            get {
                if (viveTracker == null)
                    return Status.Unavailable;
                return viveTracker.status;
            }
            set { viveTracker.status = value; }
        }

        private static readonly Vector3 defaultLeftTrackerPosition = new Vector3(0, 0.05F, 0F);
        private static readonly Quaternion defaultLeftTrackerRotation = Quaternion.identity;
        private static readonly Vector3 defaultRightTrackerPosition = new Vector3(0, 0.05F, 0F);
        private static readonly Quaternion defaultRightTrackerRotation = Quaternion.identity;

        public ArmBones attachedBone;

#region Start
        public override void Init(HandTarget _handTarget) {
            base.Init(_handTarget);
#if hSTEAMVR
            tracker = handTarget.humanoid.steam;
#elif hOPENVR
            tracker = handTarget.humanoid.openVR;
#endif
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
#if hSTEAMVR
            tracker = handTarget.humanoid.steam;
#elif hOPENVR
            tracker = handTarget.humanoid.openVR;
#endif

            if (sensorTransform != null) {
                viveTracker = sensorTransform.GetComponent<ViveTrackerComponent>();
                if (viveTracker != null)
                    viveTracker.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            if (handTarget.isLeft)
                CreateSensorTransform("Vive Tracker", defaultLeftTrackerPosition, defaultLeftTrackerRotation);
            else
                CreateSensorTransform("Vive Tracker", defaultRightTrackerPosition, defaultRightTrackerRotation);

            ViveTrackerComponent viveTracker = sensorTransform.GetComponent<ViveTrackerComponent>();
            if (viveTracker == null)
                sensorTransform.gameObject.AddComponent<ViveTrackerComponent>();
        }
#endregion

#region Update

        public override void Update() {
#if UNITY_2017_2_OR_NEWER
            if (tracker == null || !tracker.enabled || !enabled || XRSettings.loadedDeviceName != "OpenVR")
#else
            if (tracker == null || !tracker.enabled || !enabled || VRSettings.loadedDeviceName != "OpenVR")
#endif
                return;

            HumanoidTarget.TargetedBone targetBone = handTarget.GetTargetBone(attachedBone);
            if (viveTracker == null) {
                UpdateTarget(targetBone.target, sensorTransform);
                return;
            }

            if (viveTracker.trackerId < 0)
                viveTracker.trackerId = FindArmTracker(handTarget.isLeft);

            viveTracker.UpdateComponent();
            if (viveTracker.status != Status.Tracking)
                return;

            UpdateTarget(targetBone.target, viveTracker);
        }

        protected int FindArmTracker(bool isLeft) {
#if hSTEAMVR
            List<ViveTrackerComponent> viveTrackers = handTarget.humanoid.steam.viveTrackers;

            // We need the hmd to find the arm
            if (handTarget.humanoid.steam.hmd == null)
                return -1;

            Transform hmdTransform = handTarget.humanoid.steam.hmd.transform;
#elif hOPENVR
            List<ViveTrackerComponent> viveTrackers = handTarget.humanoid.openVR.viveTrackers;

            // We need the hmd to find the arm
            if (handTarget.humanoid.openVR.hmd == null)
                return -1;

            Transform hmdTransform = handTarget.humanoid.openVR.hmd.transform;
#endif
            ViveTrackerComponent foundTracker = null;
            // Finds the left or rightmost tracker, at least 0.2m left or right of the HMD
            Vector3 outermostLocalPos = new Vector3(isLeft ? -0.1F : 0.1F, 0, 0);
            foreach (ViveTrackerComponent viveTracker in viveTrackers) {
                // Is it tracking??
                if (viveTracker.positionConfidence <= 0)
                    continue;

                Vector3 sensorPos = viveTracker.transform.position;

                // Get HMD rotation projected on XZ plane
                Vector3 hmdForward = new Vector3(hmdTransform.forward.x, 0, hmdTransform.forward.z);
                Quaternion hmdFwdRotation = Quaternion.LookRotation(hmdForward);

                // Get Vive tracker local to the HMD position
                Vector3 sensorLocalPos = Quaternion.Inverse(hmdFwdRotation) * (sensorPos - hmdTransform.position);

                if ((isLeft && sensorLocalPos.x < outermostLocalPos.x && sensorLocalPos.x < -0.2F) ||
                    (!isLeft && sensorLocalPos.x > outermostLocalPos.x && sensorLocalPos.x > 0.2F)) {

                    foundTracker = viveTracker;
                    outermostLocalPos = sensorLocalPos;
                }
            }
            if (foundTracker != null) {
                int trackerId = foundTracker.trackerId;
                viveTrackers.Remove(foundTracker);
                Object.Destroy(foundTracker.gameObject);
                return trackerId;
            }
            else
                return -1;
        }

#endregion

        public void ResetSensor() {
#if hOPENVR
            if (viveTracker != null) {
                List<ViveTrackerComponent> viveTrackers = handTarget.humanoid.openVR.viveTrackers;
                viveTrackers.Add(ViveTracker.NewViveTracker(handTarget.humanoid, (uint)viveTracker.trackerId));
                viveTracker.trackerId = -1;
            }
#endif
        }
    }
}
#endif