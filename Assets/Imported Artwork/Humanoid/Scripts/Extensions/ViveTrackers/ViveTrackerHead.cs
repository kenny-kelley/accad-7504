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
    public class ViveTrackerHead : UnityHeadSensor {
        public ViveTrackerHead() {
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

        private static readonly Vector3 defaultLocalTrackerPosition = new Vector3(0, 0.1F, 0.17F);
        private static readonly Quaternion defaultLocalTrackerRotation = Quaternion.Euler(270, 180, 0);

        #region Start
        public override void Init(HeadTarget _headTarget) {
            base.Init(_headTarget);
            if (headTarget.humanoid != null)
#if hSTEAMVR
                tracker = headTarget.humanoid.steam;
#elif hOPENVR
                tracker = headTarget.humanoid.openVR;
#endif
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
#if hSTEAMVR
                tracker = headTarget.humanoid.steam;
#elif hOPENVR
            tracker = headTarget.humanoid.openVR;
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
                UpdateTarget(headTarget.head.target, sensorTransform);
                return;
            }

            if (viveTracker.trackerId < 0)
                viveTracker.trackerId = FindHeadTracker();

            viveTracker.UpdateComponent();
            if (viveTracker.status != Status.Tracking)
                return;

            UpdateTarget(headTarget.head.target, viveTracker);
            UpdateNeckTargetFromHead();
        }

        public int FindHeadTracker() {
#if hSTEAMVR
            List<ViveTrackerComponent> viveTrackers = headTarget.humanoid.steam.viveTrackers;
#elif hOPENVR
            List<ViveTrackerComponent> viveTrackers = headTarget.humanoid.openVR.viveTrackers;
#endif

            ViveTrackerComponent foundTracker = null;
            // Finds a tracker at least 1.2m above the ground
            foreach (ViveTrackerComponent viveTracker in viveTrackers) {
                // Is it tracking??
                if (viveTracker.positionConfidence <= 0)
                    continue;

                Vector3 sensorPos = viveTracker.transform.position;
                float sensorTrackingHeight = sensorPos.y - tracker.trackerTransform.position.y;

                if (sensorTrackingHeight > 1.2F) // head is more than 1.2 meter above the ground
                    foundTracker = viveTracker;
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
    }
}
#endif