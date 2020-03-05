/* Intel RealSense extension
 * copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 4.0.0
 * date: September 4, 2016
 * 
 */

#if hREALSENSE
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;
    using Humanoid.Tracking.Realsense;

    [System.Serializable]
    public class RealsenseTracker : Tracker {
        public RealsenseDeviceView realsenseDevice;

        public RealsenseTracker() {
            realsenseDevice = new RealsenseDeviceView();
        }

        public override string name {
            get { return RealsenseDevice.name; }
        }

        public override void Enable() {
            base.Enable();
            AddTracker(humanoid);
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

        public override void StartTracker(HumanoidControl humanoid) {
            base.StartTracker(humanoid);

            if (!enabled)
                return;

            RealsenseDevice.Start();
        }

        public override void UpdateTracker() {
            if (!enabled)
                return;

            if (RealsenseDevice.present) {
                //if ((humanoid.leftHandTarget.realsenseHand.enabled && humanoid.leftHandTarget.realsenseHand.status == Status.Tracking) ||
                //    (humanoid.rightHandTarget.realsenseHand.enabled && humanoid.rightHandTarget.realsenseHand.status == Status.Tracking))
                //    status = Status.Tracking;
                //else
                status = Status.Present;
            } else
                status = Status.Unavailable;

            realsenseDevice.position = Target.ToVector(trackerTransform.transform.position);
            realsenseDevice.orientation = Target.ToRotation(trackerTransform.transform.rotation);

            RealsenseDevice.Update();
        }

        public static GameObject AddTracker(HumanoidControl humanoid) {
            GameObject realWorld = HumanoidControl.GetRealWorld(humanoid.transform);

            humanoid.realsenseTracker.trackerTransform = FindTrackerObject(realWorld, RealsenseDevice.name);
            if (humanoid.realsenseTracker.trackerTransform == null) {
                humanoid.realsenseTracker.trackerTransform = CreateTracker();
                humanoid.realsenseTracker.trackerTransform.transform.parent = realWorld.transform;
            }
            return humanoid.realsenseTracker.trackerTransform.gameObject;
        }

        public static Transform CreateTracker() {
            GameObject realsenseModel = Resources.Load("IntelRealsense") as GameObject;

            GameObject trackerObject = Object.Instantiate(realsenseModel);
            trackerObject.name = RealsenseDevice.name;
            return trackerObject.transform;
        }

        public override void Calibrate() {
            RealsenseDevice.Calibrate();
        }
    }
}
#endif
