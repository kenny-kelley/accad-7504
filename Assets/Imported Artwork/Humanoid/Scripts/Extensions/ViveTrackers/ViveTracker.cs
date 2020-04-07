#if (hSTEAMVR || hOPENVR) && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using UnityEngine;

namespace Passer {
    public static class ViveTracker {
        private const string resourceName = "Vive Tracker";

        public static Transform AddTracker(HumanoidTarget target, Vector3 localPosition, Quaternion localRotation) {
            Transform trackerTransform = AddViveTracker(target.humanoid);
            trackerTransform.rotation = target.transform.rotation * localRotation;
            trackerTransform.position = target.transform.position + target.transform.rotation * localPosition;
            return trackerTransform;
        }

        public static Transform AddViveTracker(HumanoidControl humanoid, int trackerId = -1) {
            GameObject trackerPrefab = Resources.Load(resourceName) as GameObject;
            GameObject trackerObject = (trackerPrefab == null) ? new GameObject(resourceName) : Object.Instantiate(trackerPrefab);

            trackerObject.name = resourceName;

            ViveTrackerComponent trackerComponent = trackerObject.GetComponent<ViveTrackerComponent>();
            if (trackerComponent == null)
                trackerComponent = trackerObject.AddComponent<ViveTrackerComponent>();

            if (trackerId != -1)
                trackerComponent.trackerId = trackerId;
#if hSTEAMVR
            trackerObject.transform.parent = humanoid.steam.trackerTransform;

            trackerComponent.StartComponent(humanoid.steam.trackerTransform);
#elif hOPENVR
            trackerObject.transform.parent = humanoid.openVR.trackerTransform;

            trackerComponent.StartComponent(humanoid.openVR.trackerTransform);
#endif
            return trackerObject.transform;
        }

        public static void ShowTracker(HumanoidControl humanoid, bool shown) {
            if (humanoid.headTarget.viveTracker != null && humanoid.headTarget.viveTracker.sensorTransform != null)
                Tracker.ShowTracker(humanoid.headTarget.viveTracker.sensorTransform.gameObject, shown);
            if (humanoid.leftHandTarget.viveTracker != null && humanoid.leftHandTarget.viveTracker.sensorTransform != null)
                Tracker.ShowTracker(humanoid.leftHandTarget.viveTracker.sensorTransform.gameObject, shown);
            if (humanoid.rightHandTarget.viveTracker != null && humanoid.rightHandTarget.viveTracker.sensorTransform != null)
                Tracker.ShowTracker(humanoid.rightHandTarget.viveTracker.sensorTransform.gameObject, shown);
            if (humanoid.hipsTarget.viveTracker != null && humanoid.hipsTarget.viveTracker.sensorTransform != null)
                Tracker.ShowTracker(humanoid.hipsTarget.viveTracker.sensorTransform.gameObject, shown);
            if (humanoid.leftFootTarget.viveTracker != null && humanoid.leftFootTarget.viveTracker.sensorTransform != null)
                Tracker.ShowTracker(humanoid.leftFootTarget.viveTracker.sensorTransform.gameObject, shown);
            if (humanoid.rightFootTarget.viveTracker != null && humanoid.rightFootTarget.viveTracker.sensorTransform != null)
                Tracker.ShowTracker(humanoid.rightFootTarget.viveTracker.sensorTransform.gameObject, shown);
        }

        public static ViveTrackerComponent NewViveTracker(HumanoidControl humanoid, uint trackerId) {
            Transform viveTrackerTransform = AddViveTracker(humanoid, (int)trackerId);
            ViveTrackerComponent viveTracker = viveTrackerTransform.GetComponent<ViveTrackerComponent>();
            return viveTracker;
        }
    }
}
#endif