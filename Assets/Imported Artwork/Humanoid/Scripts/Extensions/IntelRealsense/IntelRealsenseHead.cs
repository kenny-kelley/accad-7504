/* Intel Realsense head controller
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
    public class IntelRealsenseHead : UnityHeadSensor {
        public override string name {
            get { return RealsenseDevice.name; }
        }

        public enum RotationTrackingAxis {
            XYZ,
            XY
        };

        public bool headTracking = true;
        public RotationTrackingAxis rotationTrackingAxis = RotationTrackingAxis.XY;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = headTarget.humanoid.realsenseTracker;

            sensor = new RealsenseHead(_humanoid.realsenseTracker.realsenseDevice, headTracking);
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            sensor.Update();
            if (status != Status.Tracking)
                return;

            if (headTracking) {
                UpdateHeadTargetTransform(sensor);

                if (rotationTrackingAxis == RotationTrackingAxis.XY)
                    headTarget.neck.target.transform.rotation = Quaternion.LookRotation(headTarget.neck.target.transform.rotation * Vector3.forward);
            }
        }
        #endregion
    }
}
#endif