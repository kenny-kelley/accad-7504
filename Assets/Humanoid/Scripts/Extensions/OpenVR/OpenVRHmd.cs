using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    public class OpenVRHmd : SensorComponent {
#if hOPENVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        protected const string resourceName = "OpenVR HMD";
        public int trackerId = 0;

        public static OpenVRHmd NewHmd(HumanoidControl humanoid, int trackerId = -1) {
            GameObject trackerPrefab = Resources.Load(resourceName) as GameObject;
            GameObject trackerObject = (trackerPrefab == null) ? new GameObject(resourceName) : Instantiate(trackerPrefab);

            trackerObject.name = resourceName;

            OpenVRHmd trackerComponent = trackerObject.GetComponent<OpenVRHmd>();
            if (trackerComponent == null)
                trackerComponent = trackerObject.AddComponent<OpenVRHmd>();

            if (trackerId != -1)
                trackerComponent.trackerId = trackerId;
            trackerObject.transform.parent = humanoid.openVR.trackerTransform;

            trackerComponent.StartComponent(humanoid.openVR.trackerTransform);

            return trackerComponent;
        }

        public override void UpdateComponent() {
            if (OpenVRDevice.status == Status.Unavailable)
                status = Status.Unavailable;

            if (OpenVRDevice.GetConfidence(trackerId) == 0) {
                status = OpenVRDevice.IsPresent(trackerId) ? Status.Present : Status.Unavailable;
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            status = Status.Tracking;
            Vector3 localSensorPosition = Target.ToVector3(OpenVRDevice.GetPosition(trackerId));
            Quaternion localSensorRotation = Target.ToQuaternion(OpenVRDevice.GetRotation(trackerId));
            transform.position = trackerTransform.TransformPoint(localSensorPosition);
            transform.rotation = trackerTransform.rotation * localSensorRotation;

            positionConfidence = OpenVRDevice.GetConfidence(trackerId);
            rotationConfidence = OpenVRDevice.GetConfidence(trackerId);
            gameObject.SetActive(true);

            FuseWithUnityCamera();
        }

        protected virtual void FuseWithUnityCamera() {
            if (Camera.main == null || Camera.main.transform == null)
                return;

            Vector3 deltaPos = Camera.main.transform.position - transform.position;
            trackerTransform.position += deltaPos;
        }
#endif
    }
}