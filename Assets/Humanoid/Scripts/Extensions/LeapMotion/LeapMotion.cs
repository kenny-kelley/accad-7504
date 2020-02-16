#if hLEAP
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class LeapTracker : Tracker {
        public override string name {
            get { return LeapDevice.name; }
        }

        public LeapDevice device;
        public TrackerTransform leapTransform;

        public bool isHeadMounted = true;

        public bool useLeapPackage = false;
        private Leap.Unity.LeapProvider leapProvider;

        private readonly Vector3 defaultTrackerPosition = new Vector3(0, 1.2F, 0.3F);
        private readonly Quaternion defaultTrackerRotation = Quaternion.identity;

        private readonly Vector3 defaultHeadTrackerPosition = new Vector3(0, 0, 0.07F);
        private readonly Quaternion defaultHeadTrackerRotation = Quaternion.Euler(270, 0, 180);

        public override bool AddTracker(HumanoidControl humanoid, string resourceName) {
            bool trackerAdded = base.AddTracker(humanoid, resourceName);
            if (trackerAdded) {
                PlaceTrackerTransform(isHeadMounted);
                //trackerTransform.transform.localPosition = defaultTrackerPosition;
                //trackerTransform.transform.localRotation = defaultTrackerRotation;
            }
            return trackerAdded;
        }

#region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (!enabled)
                return;

            if (useLeapPackage && isHeadMounted) {
                AddXRServiceProvider();

                if (leapProvider == null) {
                    leapProvider = Leap.Unity.Hands.Provider;
                }

                leapProvider.OnUpdateFrame -= OnUpdateFrame;
                leapProvider.OnUpdateFrame += OnUpdateFrame;

            }
            else {
                device = new LeapDevice();
                device.Init();

                leapTransform = device.GetTracker();

                AddTracker(humanoid, "LeapMotion");
                if (isHeadMounted)
                    SetTrackerOnCamera();
            }
        }
#endregion

#region Update

        private bool policySet = false;

        // This is a (strange) correction factor to get the tracking of the leap hands at the right position
        // The values seem to depend on the tracking system
        // SteamVR = (0, 0, -0.05)
        // Oculus = (0, 0.05, 0)        
        // This needs more investigation
        public Vector3 delta = new Vector3(0, 0, 0);

        public override void UpdateTracker() {
            if (!enabled || device == null)
                return;

            if (!policySet && leapTransform.status != Status.Unavailable) {
                if (UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.None && isHeadMounted)
                    device.SetPolicy(true);
                else
                    device.SetPolicy(false);
                policySet = true;
            }

            device.position = trackerTransform.position + trackerTransform.transform.rotation * delta;
            device.rotation = trackerTransform.rotation;
            device.Update();

            status = leapTransform.status;
            if (trackerTransform != null)
                trackerTransform.gameObject.SetActive(status != Status.Unavailable);
        }

#endregion

        public override void StopTracker() {
            if (device != null)
                device.Stop();
        }

        private void SetTrackerOnCamera() {
            if (trackerTransform == null)
                return;

#if hSTEAMVR || hOCULUS
            if (humanoid.headTarget.unityVRHead.cameraTransform != null)
                trackerTransform.SetParent(humanoid.headTarget.unityVRHead.cameraTransform, true);
            else
#endif
            {
                Camera camera = humanoid.GetComponentInChildren<Camera>();
                if (camera != null)
                    trackerTransform.SetParent(camera.transform, true);
                else
                    return;
            }
        }

        public void PlaceTrackerTransform(bool isHeadMounted) {
            if (trackerTransform == null)
                return;

            if (isHeadMounted) {
                Transform cameraTransform;
#if hSTEAMVR || hOCULUS
                if (humanoid.headTarget.unityVRHead.cameraTransform != null)
                    cameraTransform = humanoid.headTarget.unityVRHead.cameraTransform;
                else
#endif
                {
                    Camera camera = humanoid.GetComponentInChildren<Camera>();
                    if (camera != null)
                        cameraTransform = camera.transform;
                    else
                        return;
                }
                trackerTransform.rotation = cameraTransform.rotation * defaultHeadTrackerRotation;
                trackerTransform.position = cameraTransform.position + cameraTransform.rotation * defaultHeadTrackerPosition;
            }
            else {
                trackerTransform.localPosition = defaultTrackerPosition;
                trackerTransform.localRotation = defaultTrackerRotation;
            }
        }

        public override void AdjustTracking(Vector3 v, Quaternion q) {
            if (isHeadMounted)
                return;

            if (trackerTransform != null) {
                trackerTransform.position += v;
                trackerTransform.rotation *= q;
            }
        }

#region Leap Package Support

        private void AddXRServiceProvider() {
            Transform cameraTransform = humanoid.headTarget.unityVRHead.cameraTransform;
            if (cameraTransform != null) {
                Leap.Unity.LeapXRServiceProvider serviceProvider = cameraTransform.GetComponent<Leap.Unity.LeapXRServiceProvider>();
                if (serviceProvider == null)
                    cameraTransform.gameObject.AddComponent<Leap.Unity.LeapXRServiceProvider>();
            }
        }

        private void OnUpdateFrame(Leap.Frame frame) {
            if (frame == null)
                return;

            humanoid.leftHandTarget.leap.SetHand(null);
            humanoid.rightHandTarget.leap.SetHand(null);

            for (int i = 0; i < frame.Hands.Count; i++) {
                Leap.Hand curHand = frame.Hands[i];
                if (curHand.IsLeft)
                    humanoid.leftHandTarget.leap.SetHand(curHand);
                if (curHand.IsRight)
                    humanoid.rightHandTarget.leap.SetHand(curHand);
            }
        }

#endregion
    }
}
#endif