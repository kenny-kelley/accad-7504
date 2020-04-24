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
    public class ViveTrackerLeg : UnityLegSensor {
        public ViveTrackerLeg() {
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

        private static readonly Vector3 defaultLeftTrackerPosition = new Vector3(0, 0, 0.13F);
        private static readonly Quaternion defaultLeftTrackerRotation = Quaternion.identity;
        private static readonly Vector3 defaultRightTrackerPosition = new Vector3(0, 0, 0.13F);
        private static readonly Quaternion defaultRightTrackerRotation = Quaternion.identity;

#region Start

        public override void Init(FootTarget _footTarget) {
            base.Init(_footTarget);
#if hSTEAMVR
            tracker = footTarget.humanoid.steam;
#elif hOPENVR
            tracker = footTarget.humanoid.openVR;
#endif
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
#if hSTEAMVR
            tracker = footTarget.humanoid.steam;
#elif hOPENVR
            tracker = footTarget.humanoid.openVR;
#endif

            if (sensorTransform != null) {
                viveTracker = sensorTransform.GetComponent<ViveTrackerComponent>();
                if (viveTracker != null)
                    viveTracker.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            if (footTarget.isLeft)
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

            if (viveTracker == null) {
                UpdateTarget(footTarget.foot.target, sensorTransform);
                return;
            }

            if (viveTracker.trackerId < 0)
                viveTracker.trackerId = FindFootTracker(footTarget.isLeft);

            viveTracker.UpdateComponent();
            if (viveTracker.status != Status.Tracking)
                return;

            UpdateTarget(footTarget.foot.target, viveTracker);
        }

        protected override void UpdateTarget(HumanoidTarget.TargetTransform target, SensorComponent sensorComponent) {
            if (target.transform == null || sensorComponent.rotationConfidence + sensorComponent.positionConfidence <= 0)
                return;

            // Workaround for 90 degrees issue in SteamVR without Action Manifests

            target.transform.rotation = GetTargetRotation(sensorComponent.transform);
            target.confidence.rotation = sensorComponent.rotationConfidence;

            // Foot rotation is derived from lowerleg
            //target.transform.rotation = footTarget.lowerLeg.bone.targetRotation; //Quaternion.identity;
            //target.confidence.rotation = 0.5F;

            target.transform.position = GetTargetPosition(sensorComponent.transform);
            target.confidence.position = sensorComponent.positionConfidence;
        }

        public int FindFootTracker(bool isLeft) {
            Debug.Log("Seachting " + footTarget);
#if hSTEAMVR
            List<ViveTrackerComponent> viveTrackers = footTarget.humanoid.steam.viveTrackers;

            // We need the hmd
            if (footTarget.humanoid.steam.hmd == null)
                return -1;

            Transform hmdTransform = footTarget.humanoid.steam.hmd.transform;
#elif hOPENVR
            List<ViveTrackerComponent> viveTrackers = footTarget.humanoid.openVR.viveTrackers;

            // We need the hmd
            if (footTarget.humanoid.openVR.hmd == null)
                return -1;

            Transform hmdTransform = footTarget.humanoid.openVR.hmd.transform;
#endif
            ViveTrackerComponent foundTracker = null;
            Vector3 outermostLocalPos = Vector3.zero;
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
                Vector2 sensorLocalPosPlane = new Vector2(sensorLocalPos.x, sensorLocalPos.z);

                // foot is no more than 20cm above ground and 0.5m around hmd
                float sensorTrackingHeight = sensorPos.y - tracker.trackerTransform.position.y;
                if (sensorTrackingHeight < 0.2F && sensorLocalPosPlane.magnitude < 0.5F &&
                    (isLeft && sensorLocalPos.x < outermostLocalPos.x ||
                    !isLeft && sensorLocalPos.x > outermostLocalPos.x)) {

                    foundTracker = viveTracker;
                    outermostLocalPos = sensorLocalPos;
                }
            }
            if (footTarget.otherFoot.viveTracker.viveTracker == foundTracker) {
                Debug.Log("Tracker already on other foot!");
                // already assigned to the other foot
                footTarget.otherFoot.viveTracker.viveTracker.trackerId = -1;
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
                List<ViveTrackerComponent> viveTrackers = footTarget.humanoid.openVR.viveTrackers;
                viveTrackers.Add(ViveTracker.NewViveTracker(footTarget.humanoid, (uint)viveTracker.trackerId));
                viveTracker.trackerId = -1;
            }
#endif
        }

    }
}
#endif