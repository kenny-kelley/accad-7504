/* Intel Realsense controller
 * copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 4.0.0
 * date: December 29, 2016
 *
 */

/*
#if hREALSENSE
using UnityEngine;
using Humanoid.Tracking;
using Humanoid.Tracking.Realsense;

namespace Passer {

    [System.Serializable]
    public class IntelRealsenseHand : UnityArmController {
        public bool handTracking;

        //public override void Init(HumanoidControl _humanoid) {
        //    base.Init(_humanoid);
        //    name = humanoid.realsenseTracker.name;
        //}

        private RealsenseArm realsenseArm;

#if (UNITY_STANDALONE_WIN)
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);

            realsenseArm = new RealsenseArm(handTarget.isLeft, humanoid.realsenseTracker.realsenseDevice, handTracking);
        }

        public override void Update() {
            if (handTarget == null || !handTarget.enabled || !enabled)
                return;

            if (RealsenseDevice.GetHandTargetConfidence(handTarget.isLeft) <= 0) {
                status = RealsenseDevice.present ? Status.Present : Status.Unavailable;
                return;
            }

            status = Status.Tracking;


            if (handTracking) {
                UpdateHand(realsenseArm);
                UpdateFingers(realsenseArm);
            }
        }
#endif
    }
}
#endif
*/