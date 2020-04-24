#if hOPTITRACK
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OptitrackSensor : UnitySensor {
        private new OptiTracker tracker;

        private HumanoidControl humanoid;
        private Transform targetTransform;

        private OptitrackStreamingClient streamingClient;
        public int trackerId;

        public override void Start(HumanoidControl _humanoid, Transform _targetTransform) {
            base.Start(_humanoid, _targetTransform);
            humanoid = _humanoid;

            tracker = humanoid.optitrack;

            streamingClient = tracker.streamingClient;

            targetTransform = _targetTransform;
        }

        public override void Update() {
            if (!humanoid.optitrack.enabled || !enabled)
                return;

            //float confidence = 1;

            OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(trackerId);
            if (rbState != null) {
                targetTransform.position = rbState.Pose.Position;
                targetTransform.rotation = rbState.Pose.Orientation;
                // architecture does not allow confidence here...
            }
        }
    }
}
#endif