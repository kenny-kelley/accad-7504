using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class SteamVrHmdComponent : SensorComponent {
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        protected const string resourceName = "SteamVR HMD";
        public int trackerId = 0;

        public static SteamVrHmdComponent NewHmd(HumanoidControl humanoid, int trackerId = -1) {
            GameObject trackerPrefab = Resources.Load(resourceName) as GameObject;
            GameObject trackerObject = (trackerPrefab == null) ? new GameObject(resourceName) : Instantiate(trackerPrefab);
            
            trackerObject.name = resourceName;

            SteamVrHmdComponent trackerComponent = trackerObject.GetComponent<SteamVrHmdComponent>();
            if (trackerComponent == null)
                trackerComponent = trackerObject.AddComponent<SteamVrHmdComponent>();

            if (trackerId != -1)
                trackerComponent.trackerId = trackerId;
            trackerObject.transform.parent = humanoid.steam.trackerTransform;

            trackerComponent.StartComponent(humanoid.steam.trackerTransform);

            return trackerComponent;
        }

        public override void UpdateComponent() {
            if (SteamDevice.status == Status.Unavailable)
                status = Status.Unavailable;

            if (SteamDevice.GetConfidence(trackerId) == 0) {
                status = SteamDevice.IsPresent(trackerId) ? Status.Present : Status.Unavailable;
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            status = Status.Tracking;
            Vector3 localSensorPosition = Target.ToVector3(SteamDevice.GetPosition(trackerId));
            Quaternion localSensorRotation = Target.ToQuaternion(SteamDevice.GetRotation(trackerId));
            transform.position = trackerTransform.TransformPoint(localSensorPosition);
            transform.rotation = trackerTransform.rotation * localSensorRotation;

            positionConfidence = SteamDevice.GetConfidence(trackerId);
            rotationConfidence = SteamDevice.GetConfidence(trackerId);
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