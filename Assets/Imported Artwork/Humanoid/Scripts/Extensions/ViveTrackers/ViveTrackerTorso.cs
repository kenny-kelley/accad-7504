#if (hSTEAMVR || hOPENVR) && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif
using Passer.Humanoid.Tracking;

namespace Passer {

    [System.Serializable]
    public class ViveTrackerTorso : UnityTorsoSensor {
        public ViveTrackerTorso() {
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

        private static readonly Vector3 defaultLocalTrackerPosition = new Vector3(0, 0F, 0.17F);
        private static readonly Quaternion defaultLocalTrackerRotation = Quaternion.Euler(270, 180, 0);

#region Start

        public override void Init(HipsTarget _hipsTarget) {
            base.Init(_hipsTarget);
#if hSTEAMVR
            tracker = hipsTarget.humanoid.steam;
#elif hOPENVR
            tracker = hipsTarget.humanoid.openVR;
#endif
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
#if hSTEAMVR
            tracker = hipsTarget.humanoid.steam;
#elif hOPENVR
            tracker = hipsTarget.humanoid.openVR;
#endif

            if (sensorTransform != null) {
                viveTracker = sensorTransform.GetComponent<ViveTrackerComponent>();
                if (viveTracker != null)
                    viveTracker.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            CreateSensorTransform("Vive Tracker", defaultLocalTrackerPosition, defaultLocalTrackerRotation);

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
                UpdateTarget(hipsTarget.hips.target, sensorTransform);
                return;
            }

            if (viveTracker.trackerId < 0)
                viveTracker.trackerId = FindTorsoTracker();

            viveTracker.UpdateComponent();
            if (viveTracker.status != Status.Tracking)
                return;

            UpdateTarget(hipsTarget.hips.target, viveTracker);
        }

        protected override void UpdateTarget(HumanoidTarget.TargetTransform target, SensorComponent sensorComponent) {
            if (target.transform == null || sensorComponent.rotationConfidence + sensorComponent.positionConfidence <= 0)
                return;

            Quaternion sensorRotation = GetTargetRotation(sensorComponent.transform);
            Vector3 sensorForward = sensorRotation * Vector3.forward;
            target.transform.rotation = Quaternion.LookRotation(sensorForward, hipsTarget.humanoid.up);
            target.confidence.rotation = sensorComponent.rotationConfidence;

            target.transform.position = GetTargetPosition(sensorComponent.transform);
            target.confidence.position = sensorComponent.positionConfidence;
        }


        protected int FindTorsoTracker() {
#if hSTEAMVR
            List<ViveTrackerComponent> viveTrackers = hipsTarget.humanoid.steam.viveTrackers;

            // We need the hmd
            if (hipsTarget.humanoid.steam.hmd == null)
                return -1;

            Transform hmdTransform = hipsTarget.humanoid.steam.hmd.transform;
#elif hOPENVR
            List<ViveTrackerComponent> viveTrackers = hipsTarget.humanoid.openVR.viveTrackers;

            // We need the hmd
            if (hipsTarget.humanoid.openVR.hmd == null)
                return -1;

            Transform hmdTransform = hipsTarget.humanoid.openVR.hmd.transform;
#endif
            ViveTrackerComponent foundTracker = null;
            // Finds a tracker between 0.3 and 1 meter from the ground and 0.2m around hmd
            // HMD is no good to final solution, because it could be that a Vive Tracker is used on the head
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
                Vector2 sensorLocalPosPlane = new Vector2(sensorLocalPos.x,sensorLocalPos.z);

                float sensorTrackingHeight = sensorPos.y - tracker.trackerTransform.position.y;
                if (sensorTrackingHeight > 0.3F && sensorTrackingHeight < 1.2F && sensorLocalPosPlane.magnitude < 0.2F) {
                    foundTracker = viveTracker;
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
                List<ViveTrackerComponent> viveTrackers = hipsTarget.humanoid.openVR.viveTrackers;
                viveTrackers.Add(ViveTracker.NewViveTracker(hipsTarget.humanoid, (uint)viveTracker.trackerId));
                viveTracker.trackerId = -1;
            }
#endif
        }

    }
}
#endif