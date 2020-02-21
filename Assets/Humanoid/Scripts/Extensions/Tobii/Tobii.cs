#if hTOBII
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class TobiiTracker : Tracker {
        public override string name {
            get { return TobiiDevice.name; }
        }

        public override void Enable() {
            base.Enable();
            AddTracker(humanoid, name);
        }

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (!enabled)
                return;

            AddTracker(humanoid, name);
        }
        #endregion

        #region Update
        public override void UpdateTracker() {
            if (!enabled)
                return;

            if ((humanoid.headTarget.tobiiHead.enabled && humanoid.headTarget.tobiiHead.status == Status.Tracking))
                status = Status.Tracking;
            else
                status = Status.Present;
        }

        private readonly Vector3 defaultTrackerPosition = new Vector3(0, 1.2F, 1);
        private readonly Quaternion defaultTrackerRotation = Quaternion.Euler(0, 180, 0);
        public override bool AddTracker(HumanoidControl humanoid, string resourceName) {
            bool trackerAdded = base.AddTracker(humanoid, resourceName);
            if (trackerAdded) {
                trackerTransform.transform.localPosition = defaultTrackerPosition;
                trackerTransform.transform.localRotation = defaultTrackerRotation;
            }
            return trackerAdded;
        }
        #endregion
    }
}
#endif