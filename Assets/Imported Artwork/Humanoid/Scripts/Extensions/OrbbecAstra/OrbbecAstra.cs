#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_WSA_10_0)
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class AstraTracker : Tracker {
        public override string name {
            get { return AstraDevice.name; }
        }

        public AstraDevice device;
        public TrackerTransform astraTransform;

        private readonly Vector3 defaultTrackerPosition = new Vector3(0, 1.5F, 1);
        private readonly Quaternion defaultTrackerRotation = Quaternion.Euler(0, 180, 0);

        public AstraTracker() {
            deviceView = new DeviceView();//new AstraDeviceView();
        }

        public override bool AddTracker(HumanoidControl humanoid, string resourceName) {
            bool trackerAdded = base.AddTracker(humanoid, resourceName);
            if (trackerAdded) {
                trackerTransform.transform.localPosition = defaultTrackerPosition;
                trackerTransform.transform.localRotation = defaultTrackerRotation;
            }
            return trackerAdded;
        }

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (!enabled)
                return;

            device = new AstraDevice();
            device.Init();

            astraTransform = device.GetTracker();

            AddTracker(humanoid, "Orbbec Astra");
        }
        #endregion

        #region Stop
        public override void StopTracker() {
            if (device != null)
                device.Stop();
        }
        #endregion

        #region Update

        public override void UpdateTracker() {
            if (!enabled ||
                device == null ||
                trackerTransform == null)
                return;

            device.position = trackerTransform.position;
            device.rotation = trackerTransform.rotation;
            device.Update();

            status = astraTransform.status;
            trackerTransform.gameObject.SetActive(status != Status.Unavailable);
        }

        #endregion

        #region Sensor Fusion
        public void CalibrateWithHead(SensorBone headSensor) {
            Vector3 delta = humanoid.headTarget.head.target.transform.position - device.GetBonePosition(0, Bone.Head);
            trackerTransform.position += (delta * 0.01F);
        }

        public void CalibrateWithHeadAndHands(SensorBone headSensor, SensorBone leftSensor, SensorBone rightSensor) {
            Vector3 trackingNormal = TrackingNormal(humanoid.headTarget.head.target.transform.position, humanoid.leftHandTarget.transform.position, humanoid.rightHandTarget.transform.position);

            Vector3 astraHeadPosition = headSensor.position;
            Vector3 astraLeftHandPosition = leftSensor.position;
            Vector3 astraRightHandPosition = rightSensor.position;

            Vector3 astraTrackingNormal = TrackingNormal(astraHeadPosition, astraLeftHandPosition, astraRightHandPosition);

            Quaternion rotation = Quaternion.FromToRotation(astraTrackingNormal, trackingNormal);
            float rotY = Angle.Normalize(rotation.eulerAngles.y);
            float rotX = Angle.Normalize(rotation.eulerAngles.x);

            trackerTransform.RotateAround(humanoid.headTarget.head.target.transform.position, humanoid.up, rotY * 0.01F);
            trackerTransform.RotateAround(humanoid.headTarget.head.target.transform.position, humanoid.transform.right, rotX * 0.01F);

            Vector3 delta = humanoid.headTarget.head.target.transform.position - astraHeadPosition;
            trackerTransform.transform.position += (delta * 0.01F);
        }

        public void CalibrateWithHands(SensorBone leftSensor, SensorBone rightSensor) {
            Vector3 astraLeftHandPosition = leftSensor.position;
            Vector3 astraRightHandPosition = rightSensor.position;
            Vector3 astraAvgHandPosition = (astraLeftHandPosition + astraRightHandPosition) / 2;
            Debug.DrawLine(astraLeftHandPosition, astraRightHandPosition, Color.red);

            Vector3 targetLeftHandPosition = humanoid.leftHandTarget.hand.target.transform.position;
            Vector3 targetRightHandPosition = humanoid.rightHandTarget.hand.target.transform.position;
            Vector3 targetAvgHandPosition = (targetLeftHandPosition + targetRightHandPosition) / 2;
            Debug.DrawLine(targetLeftHandPosition, targetRightHandPosition, Color.magenta);

            Vector3 delta = targetAvgHandPosition - astraAvgHandPosition;
            Debug.DrawRay(astraAvgHandPosition, delta, Color.blue);

            trackerTransform.position += (delta * 0.01F);

            // Just positional calibration for now
        }

        private Vector3 TrackingNormal(Vector3 neckPosition, Vector3 leftHandPosition, Vector3 rightHandPosition) {
            Vector3 neck2leftHand = leftHandPosition - neckPosition;
            Vector3 neck2rightHand = rightHandPosition - neckPosition;

            Vector3 trackingNormal = Vector3.Cross(neck2leftHand, neck2rightHand);
            return trackingNormal;
        }
        #endregion
    }
}
#endif