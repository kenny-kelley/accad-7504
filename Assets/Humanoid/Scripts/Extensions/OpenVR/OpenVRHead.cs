#if hOPENVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class OpenVRHead : UnityHeadSensor {
        public override string name {
            get { return "OpenVR HMD"; }
        }

        public OpenVRHmd hmd;

        public override Status status {
            get {
                if (hmd == null)
                    return Status.Unavailable;
                return hmd.status;
            }
            set { hmd.status = value; }
        }

        #region Start
        public override void Init(HeadTarget headTarget) {
            base.Init(headTarget);
            if (headTarget.humanoid != null)
                tracker = headTarget.humanoid.openVR;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = headTarget.humanoid.openVR;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            SetSensor2Target();
            CheckSensorTransform();
            sensor2TargetPosition = -headTarget.head2eyes;

            if (sensorTransform != null) {
                hmd = sensorTransform.GetComponent<OpenVRHmd>();
                if (hmd != null)
                    hmd.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            CreateSensorTransform("OpenVR HMD", headTarget.head2eyes, Quaternion.identity);
            OpenVRHmd openVRHmd = sensorTransform.GetComponent<OpenVRHmd>();
            if (openVRHmd == null)
                sensorTransform.gameObject.AddComponent<OpenVRHmd>();
        }
        #endregion

        #region Update
        protected bool calibrated = false;

        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (hmd == null) {
                hmd = sensorTransform.GetComponent<OpenVRHmd>();
                UpdateTarget(headTarget.head.target, sensorTransform);
            }

            hmd.UpdateComponent();
            if (hmd.status != Status.Tracking)
                return;

            if (!calibrated && tracker.humanoid.calibrateAtStart) {
                tracker.humanoid.Calibrate();
                calibrated = true;
            }

            UpdateTarget(headTarget.head.target, hmd);
            UpdateNeckTargetFromHead();
        }
        #endregion
    }
}
#endif