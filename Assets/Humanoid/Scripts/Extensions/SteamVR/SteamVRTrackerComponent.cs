using UnityEngine;

namespace Passer {

    public class SteamVRTrackerComponent : MonoBehaviour {
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        public SteamVRTracker tracker = new SteamVRTracker();

        private void Start() {
            tracker.trackerTransform = this.transform;
            tracker.StartTracker();
        }

        private void Update() {
            tracker.UpdateTracker();
        }
#endif
    }
}