using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class OptitrackRigidbodyComponent : SensorComponent {
#if hOPTITRACK

        private OptitrackStreamingClient streamingClient;
        public int streamingID;

        private void Awake() {
            streamingClient = FindObjectOfType<OptitrackStreamingClient>();
        }

        private const float maxAge_sec = 0.01F; // if a measurement is older then this value, it will not be used

        public override void UpdateComponent() {
            OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(streamingID);

            if (rbState == null || rbState.DeliveryTimestamp.AgeSeconds > maxAge_sec) {
                status = Status.Present;
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            status = Status.Tracking;
            transform.position = trackerTransform.TransformPoint(rbState.Pose.Position);
            transform.rotation = trackerTransform.rotation * rbState.Pose.Orientation;

            positionConfidence = 1F;
            rotationConfidence = 0.99F;
            gameObject.SetActive(true);
        }

#endif
    }
}
