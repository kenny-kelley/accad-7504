#if hOCULUS && (UNITY_STANDALONE_WIN || UNITY_ANDROID)

using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OculusTracker : Tracker {
        public OculusTracker() {
            deviceView = new DeviceView();
        }

        public override string name {
            get { return OculusDevice.name; }
        }

        public override UnityHeadSensor headSensor {
            get { return humanoid.headTarget.oculus; }
        }
        public override UnityArmSensor leftHandSensor {
            get { return humanoid.leftHandTarget.oculus; }
        }
        public override UnityArmSensor rightHandSensor {
            get { return humanoid.rightHandTarget.oculus; }
        }
        [System.NonSerialized]
        private UnitySensor[] _sensors;
        public override UnitySensor[] sensors {
            get {
                if (_sensors == null)
                    _sensors = new UnitySensor[] {
                        headSensor,
                        leftHandSensor,
                        rightHandSensor
                    };

                return _sensors;
            }
        }

#if UNITY_ANDROID
        public enum AndroidDeviceType {
            GearVR,
            OculusGo,
            OculusQuest,
        }
        public AndroidDeviceType androidDeviceType = AndroidDeviceType.OculusQuest;
#endif

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (humanoid.headTarget.unityVRHead.enabled && UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.Oculus)
                enabled = true;

            if (!enabled || UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.Oculus)
                return;

            OculusDevice.Start();

            AddTracker(humanoid, "Oculus");
            StartCameras(trackerTransform);
        }

        private void StartCameras(Transform trackerTransform) {
            //subTrackers = new OculusCameraComponent[(int)OculusDevice.Tracker.Count];
            for (int i = 0; i < OculusCameraComponent.GetCount(); i++) {
                OculusCameraComponent oculusCamera = OculusCameraComponent.Create(this);
                oculusCamera.subTrackerId = i;
                subTrackers.Add(oculusCamera);
                //subTrackers[i] = OculusCameraComponent.Create(this);
                //subTrackers[i].subTrackerId = i;
            }
        }
        #endregion

        #region Update
        public override void UpdateTracker() {
            if (!enabled || trackerTransform == null)
                return;

            if (UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.Oculus) {
                status = Status.Unavailable;
                return;
            }

            status = OculusDevice.status;

            trackerTransform.localPosition = new Vector3(0, OculusDevice.eyeHeight, 0);
            deviceView.position = Target.ToVector(trackerTransform.position);
            deviceView.orientation = Target.ToRotation(trackerTransform.rotation);

            OculusDevice.Update();

            foreach (SubTracker subTracker in subTrackers) {
                if (subTracker != null)
                    subTracker.UpdateTracker(humanoid.showRealObjects);
            }

            if (OculusDevice.ovrp_GetAppShouldRecenter() == OculusDevice.Bool.True) {
                humanoid.Calibrate();
            }
        }
        #endregion

        public override void Calibrate() {
            base.Calibrate();

            if (enabled && UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.Oculus)
                OculusDevice.ovrp_RecenterTrackingOrigin(unchecked((uint)OculusDevice.RecenterFlags.IgnoreAll));
        }

        public override void AdjustTracking(Vector3 v, Quaternion q) {
            if (trackerTransform != null) {
                trackerTransform.position += v;
                trackerTransform.rotation *= q;
            }
        }
    }
}
#endif