#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using System.Collections.Generic;
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class SteamVRTracker : Tracker {
        public SteamVRTracker() {
            deviceView = new DeviceView();
        }

        public override string name {
            get { return SteamDevice.name; }
        }

        public override UnityHeadSensor headSensor {
            get { return humanoid.headTarget.steamVR; }
        }
        public override UnityArmSensor leftHandSensor {
            get { return humanoid.leftHandTarget.steamVR; }
        }
        public override UnityArmSensor rightHandSensor {
            get { return humanoid.rightHandTarget.steamVR; }
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

        public SteamVrHmdComponent hmd = null;
        public List<SteamVrControllerComponent> controllers = new List<SteamVrControllerComponent>();

#if hVIVETRACKER
        public UnityHeadSensor headSensorVive {
            get { return humanoid.headTarget.viveTracker; }
        }
        public UnityArmSensor leftHandSensorVive {
            get { return humanoid.leftHandTarget.viveTracker; }
        }
        public UnityArmSensor rightHandSensorVive {
            get { return humanoid.rightHandTarget.viveTracker; }
        }
        public UnityTorsoSensor hipsSensorVive {
            get { return humanoid.hipsTarget.viveTracker; }
        }
        public UnityLegSensor leftFootSensorVive {
            get { return humanoid.leftFootTarget.viveTracker; }
        }
        public UnityLegSensor rightFootSensorVive {
            get { return humanoid.rightFootTarget.viveTracker; }
        }

        public List<ViveTrackerComponent> viveTrackers = new List<ViveTrackerComponent>();
#endif

        public override bool AddTracker(HumanoidControl humanoid, string resourceName) {
            bool trackerAdded = base.AddTracker(humanoid, resourceName);
            if (trackerAdded) {
                /*SteamVRTrackerComponent trackerComponent = */
                trackerTransform.gameObject.AddComponent<SteamVRTrackerComponent>();
            }
            return trackerAdded;
        }

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (humanoid.headTarget.unityVRHead.enabled && UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.OpenVR)
                enabled = true;

            if (!enabled || UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.OpenVR)
                return;

            TraditionalDevice.gameControllerEnabled = false;
            // Game controllers interfere with SteamVR Controller Input ... :-(

            SteamDevice.Start();

            AddTracker(humanoid, "SteamVR");

            SteamDevice.onNewSensor += OnNewSensor; // trackerId => ViveTracker.NewViveTracker(humanoid, trackerId);
#if hVIVETRACKER
            Debug.Log("Detecting Vive Tracker positions.\nMake sure the Vive HMD is looking in the same direction as the user!");
#endif
        }

        public void StartTracker() {
            if (!enabled || UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.OpenVR)
                return;

            TraditionalDevice.gameControllerEnabled = false;
            // Game controllers interfere with SteamVR Controller Input ... :-(

            SteamDevice.Start();
        }

        protected virtual void OnNewSensor(uint sensorId) {
            Passer.ETrackedDeviceClass deviceClass = Passer.OpenVR.System.GetTrackedDeviceClass(sensorId);
            switch (deviceClass) {
                case Passer.ETrackedDeviceClass.HMD:
                    hmd = FindHmd(sensorId);
                    if (hmd == null) {
                        hmd = SteamVrHmdComponent.NewHmd(humanoid, (int)sensorId);
                    }
                    break;
                case Passer.ETrackedDeviceClass.TrackingReference:
                    SubTracker subTracker = FindLighthouse(sensorId);
                    if (subTracker == null) {
                        subTracker = NewLighthouse(humanoid, sensorId);
                        subTrackers.Add(subTracker);
                    }
                    break;
                case Passer.ETrackedDeviceClass.Controller:
                    SteamVrControllerComponent controller = FindController(sensorId);
                    if (controller == null) {
                        controller = SteamVrControllerComponent.NewController(humanoid, (int)sensorId);
                        controllers.Add(controller);
                    }
                    break;
#if hVIVETRACKER
                case Passer.ETrackedDeviceClass.GenericTracker:
                    ViveTrackerComponent viveTracker = FindViveTracker(sensorId);
                    if (viveTracker == null) {
                        viveTracker = ViveTracker.NewViveTracker(humanoid, sensorId);
                        viveTrackers.Add(viveTracker);
                    }
                    break;
#endif
                default:
                    break;
            }
        }

        protected SubTracker NewLighthouse(HumanoidControl humanoid, uint sensorId) {
            SubTracker subTracker = SteamVRSubTracker.Create(this);
            subTracker.subTrackerId = (int)sensorId;
            return subTracker;
        }

        protected SteamVrHmdComponent FindHmd(uint sensorId) {
            if (hmd != null && hmd.trackerId == sensorId)
                return hmd;

            SteamVRHead steamVrHead = humanoid.headTarget.steamVR;
            if (steamVrHead.steamVrHmd != null) {
                steamVrHead.steamVrHmd.trackerId = (int)sensorId;
                hmd = steamVrHead.steamVrHmd;
                return hmd;
            }
            return null;
        }

        protected SteamVrControllerComponent FindController(uint sensorId) {
            foreach (SteamVrControllerComponent controller in controllers) {
                if (controller != null && controller.trackerId == sensorId)
                    return controller;
            }
            if (SteamVrControllerComponent.IsLeftController(sensorId)) {
                SteamVRHand leftHand = humanoid.leftHandTarget.steamVR;
                if (leftHand != null && leftHand.steamVrController != null) {
                    leftHand.steamVrController.trackerId = (int)sensorId;
                    // leftHand Controller was not in controller list yet
                    controllers.Add(leftHand.steamVrController);
                    return leftHand.steamVrController;
                }
            }
            else
            if (SteamVrControllerComponent.IsRightController(sensorId)) {
                SteamVRHand rightHand = humanoid.rightHandTarget.steamVR;
                if (rightHand != null && rightHand.steamVrController != null) {
                    rightHand.steamVrController.trackerId = (int)sensorId;
                    // leftHand Controller was not in controller list yet
                    controllers.Add(rightHand.steamVrController);
                    return rightHand.steamVrController;
                }
            }
            return null;
        }

#if hVIVETRACKER
        protected ViveTrackerComponent FindViveTracker(uint sensorId) {
            foreach (ViveTrackerComponent viveTracker in viveTrackers) {
                if (viveTracker != null && viveTracker.trackerId == sensorId)
                    return viveTracker;
            }
            return null;
        }
#endif

        #endregion

        public override void ShowTracker(bool shown) {
            if (!enabled)
                return;
#if hVIVETRACKER
            if (humanoid == null)
                return;

            ViveTracker.ShowTracker(humanoid, shown);
#endif
        }


        #region Update
        public override void UpdateTracker() {
            if (!enabled || UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.OpenVR) {
                status = Status.Unavailable;
                return;
            }

            status = SteamDevice.status;

            SteamDevice.Update();

            deviceView.position = Target.ToVector(trackerTransform.position);
            deviceView.orientation = Target.ToRotation(trackerTransform.rotation);

            bool showRealObjects = humanoid == null ? true : humanoid.showRealObjects;

            if (hmd != null)
                hmd.UpdateComponent();
            foreach (SubTracker subTracker in subTrackers)
                subTracker.UpdateTracker(showRealObjects);
            foreach (SteamVrControllerComponent controller in controllers)
                controller.UpdateComponent();
#if hVIVETRACKER
            foreach (ViveTrackerComponent viveTracker in viveTrackers)
                viveTracker.UpdateComponent();
#endif
        }

        protected SubTracker FindLighthouse(uint sensorId) {
            foreach (SubTracker subTracker in subTrackers) {
                if (subTracker != null && subTracker.subTrackerId == sensorId)
                    return subTracker;
            }
            return null;
        }

        private bool IsTracking() {
            if (!humanoid.leftHandTarget.steamVR.enabled || humanoid.leftHandTarget.steamVR.status == Status.Tracking ||
#if hVIVETRACKER
                humanoid.headTarget.viveTracker.status == Status.Tracking ||
                humanoid.leftHandTarget.viveTracker.status == Status.Tracking ||
                humanoid.rightHandTarget.viveTracker.status == Status.Tracking ||
                humanoid.leftHandTarget.viveTracker.status == Status.Tracking ||
                humanoid.hipsTarget.viveTracker.status == Status.Tracking ||
                humanoid.leftFootTarget.viveTracker.status == Status.Tracking ||
                humanoid.rightFootTarget.viveTracker.status == Status.Tracking ||
#endif
                !humanoid.rightHandTarget.steamVR.enabled || humanoid.rightHandTarget.steamVR.status == Status.Tracking)
                return true;
            else
                return false;
        }
        #endregion 

        public static GameObject CreateControllerObject() {
            GameObject trackerPrefab = Resources.Load("Vive Controller") as GameObject;
            if (trackerPrefab == null)
                return null;

            GameObject trackerObject = Object.Instantiate(trackerPrefab);
            trackerObject.name = "Vive Controller";

            return trackerObject;
        }

        public override void Calibrate() {
            SteamDevice.ResetSensors();
        }

        public override void AdjustTracking(Vector3 v, Quaternion q) {
            if (trackerTransform != null) {
                trackerTransform.position += v;
                trackerTransform.rotation *= q;
            }
        }

        public Transform GetTrackingTransform() {
            if (!enabled ||
                subTrackers == null ||
                subTrackers.Count <= 0 ||
                subTrackers[0] == null ||
                subTrackers[0].subTrackerId == -1) {

                return null;
            }

            return subTrackers[0].transform;
        }

        public void SyncTracking(Vector3 position, Quaternion rotation) {
            if (!enabled)
                return;


            // rotation

            // Not stable
            //Quaternion deltaRotation = Quaternion.Inverse(lighthouses[0].transform.rotation) * rotation;
            //unityVRroot.rotation *= deltaRotation;

            // stable
            float angle = (-subTrackers[0].transform.eulerAngles.y) + rotation.eulerAngles.y;
            trackerTransform.Rotate(Vector3.up, angle, Space.World);

            // position
            Vector3 deltaPosition = position - subTrackers[0].transform.position;

            trackerTransform.Translate(deltaPosition, Space.World);
        }
    }
}
#endif