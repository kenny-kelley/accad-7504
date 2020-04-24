#if hHYDRA

using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class HydraTracker : Tracker {
        public override string name {
            get { return "Razer Hydra"; }
        }

        public HydraTracker() {
            deviceView = new DeviceView();
        }

        public override UnityArmSensor leftHandSensor {
            get { return humanoid.leftHandTarget.hydra; }
        }
        public override UnityArmSensor rightHandSensor {
            get {
                return humanoid.rightHandTarget.hydra;
            }
        }
        [System.NonSerialized]
        private UnitySensor[] _sensors;
        public override UnitySensor[] sensors {
            get {
                if (_sensors == null)
                    _sensors = new UnitySensor[] {
                        leftHandSensor,
                        rightHandSensor,
                    };

                return _sensors;
            }
        }

        private readonly Vector3 defaultBaseStationPosition = new Vector3(0, 1.2F, 0.3F);
        private readonly Quaternion defaultBaseStationRotation = Quaternion.identity;

        public override bool AddTracker(HumanoidControl humanoid, string resourceName) {
            bool trackerAdded = base.AddTracker(humanoid, resourceName);
            if (trackerAdded) {
                trackerTransform.transform.localPosition = defaultBaseStationPosition;
                trackerTransform.transform.localRotation = defaultBaseStationRotation;
            }
            return trackerAdded;
        }

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (!enabled)
                return;

            HydraDevice.Start();

            AddTracker(humanoid, "Hydra BaseStation");
        }
        #endregion

        #region Update
        public override void UpdateTracker() {
            if (!enabled || trackerTransform == null)
                return;

            if (trackerTransform != null) {
                if (HydraDevice.status == Status.Unavailable)
                    trackerTransform.gameObject.SetActive(false);
                else
                    trackerTransform.gameObject.SetActive(true);
            }

            if (HydraDevice.status != Status.Unavailable) {
                if ((leftHandSensor.enabled && leftHandSensor.status == Status.Tracking) ||
                    (rightHandSensor.enabled && rightHandSensor.status == Status.Tracking))
                    status = Status.Tracking;
                else
                    status = Status.Present;
            }
            else
                status = Status.Unavailable;

            deviceView.position = Target.ToVector(trackerTransform.position);
            deviceView.orientation = Target.ToRotation(trackerTransform.rotation);

            HydraDevice.Update();
        }
        #endregion

        public override void ShowTracker(bool shown) {
            if (trackerTransform == null)
                return;

            ShowTracker(trackerTransform.gameObject, shown);
            leftHandSensor.ShowSensor(shown);
            rightHandSensor.ShowSensor(shown);
        }
    }
}
#endif