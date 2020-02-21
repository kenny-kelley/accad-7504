#if hOPENVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    public class OpenVRTracker : SubTracker {
        protected const string resourceName = "Lighthouse";

        public static OpenVRTracker Create(Tracker tracker) {
            Object lighthousePrefab = Resources.Load(resourceName);
            GameObject lighthouseObject = (lighthousePrefab == null) ? new GameObject(resourceName) : (GameObject)Instantiate(lighthousePrefab);

            lighthouseObject.name = "Lighthouse";
            lighthouseObject.transform.parent = tracker.trackerTransform;

            lighthouseObject.SetActive(false);

            OpenVRTracker subTracker = lighthouseObject.AddComponent<OpenVRTracker>();
            subTracker.tracker = tracker;

            return subTracker;
        }

        public override bool IsPresent() {
            bool isPresent = OpenVRDevice.IsPresent(subTrackerId);
            return isPresent;
        }

        public override void UpdateTracker(bool showRealObjects) {
            if (subTrackerId == -1)
                return;

            bool isPresent = IsPresent();
            if (!isPresent)
                return;

            if (gameObject != null)
                gameObject.SetActive(showRealObjects);

            transform.localPosition = Target.ToVector3(OpenVRDevice.GetPosition(subTrackerId));
            transform.localRotation = Target.ToQuaternion(OpenVRDevice.GetRotation(subTrackerId));
        }
    }
}
#endif