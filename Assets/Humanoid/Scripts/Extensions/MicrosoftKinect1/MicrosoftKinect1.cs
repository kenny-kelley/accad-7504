#if hKINECT1
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [Serializable]
    public class Kinect1Tracker : Tracker {
        public override string name {
            get { return Kinect1Device.name; }
        }

        public Kinect1Device device;
        public TrackerTransform kinect1Transform;

        private readonly Vector3 defaultTrackerPosition = new Vector3(0, 1.5F, 1);
        private readonly Quaternion defaultTrackerRotation = Quaternion.Euler(0, 180, 0);

        public Kinect1Tracker() {
            deviceView = new DeviceView();
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

            device = new Kinect1Device();
            device.Init();

            kinect1Transform = device.GetTracker();

            AddTracker(humanoid, "Microsoft Kinect 1");
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

            status = kinect1Transform.status;
            trackerTransform.gameObject.SetActive(status != Status.Unavailable);
        }
        #endregion

        #region Sensor Fusion
        public void CalibrateWithHead(SensorBone headSensor) {
            Vector3 delta = humanoid.headTarget.head.target.transform.position - headSensor.position;
            trackerTransform.position += (delta * 0.01F);
        }

        public void CalibrateWithHeadAndHands(SensorBone headSensor, SensorBone leftHandSensor, SensorBone rightHandSensor) {
            Vector3 trackingNormal = TrackingNormal(humanoid.headTarget.head.target.transform.position, humanoid.leftHandTarget.transform.position, humanoid.rightHandTarget.transform.position);

            Vector3 TrackingSensorsNormal = TrackingNormal(headSensor.position, leftHandSensor.position, rightHandSensor.position);

            Quaternion rotation = Quaternion.FromToRotation(TrackingSensorsNormal, trackingNormal);
            float rotY = Angle.Normalize(rotation.eulerAngles.y);
            float rotX = Angle.Normalize(rotation.eulerAngles.x);

            trackerTransform.RotateAround(humanoid.headTarget.head.target.transform.position, humanoid.up, rotY * 0.01F);
            trackerTransform.RotateAround(humanoid.headTarget.head.target.transform.position, humanoid.transform.right, rotX * 0.01F);

            Vector3 delta = humanoid.headTarget.head.target.transform.position - headSensor.position;
            trackerTransform.transform.position += (delta * 0.01F);
        }

        public void CalibrateWithHands(SensorBone leftHandSensor, SensorBone rightHandSensor) {
            Vector3 avgHandPosition = (leftHandSensor.position + rightHandSensor.position) / 2;

            Vector3 targetLeftHandPosition = humanoid.leftHandTarget.hand.target.transform.position;
            Vector3 targetRightHandPosition = humanoid.rightHandTarget.hand.target.transform.position;
            Vector3 targetAvgHandPosition = (targetLeftHandPosition + targetRightHandPosition) / 2;

            Vector3 delta = targetAvgHandPosition - avgHandPosition;

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

    namespace Tracking {
        public class Kinect1Device : TrackingDevice {
            public static string name = "Microsoft Kinect 1";
            private static IntPtr pKinect1;

            public static void LoadDlls() {
                LoadLibrary("Assets/Humanoid/Plugins/Humanoid.dll");
                LoadLibrary("Assets/Humanoid/Plugins/HumanoidKinect1.dll");
            }
            [DllImport("kernel32.dll")]
            private static extern IntPtr LoadLibrary(string dllToLoad);

            public Kinect1Device() {
                pKinect1 = Kinect1_Constructor();
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Kinect1_Constructor();

            ~Kinect1Device() {
                Kinect1_Destructor(pKinect1);
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Kinect1_Destructor(IntPtr pKinect1);

            public override void Init() {
                Kinect1_Init(pKinect1);
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Kinect1_Init(IntPtr pKinect1);

            public override void Stop() {
                Debug.Log("Stop");
                Kinect1_Stop(pKinect1);
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Kinect1_Stop(IntPtr pKinect1);

            public override void Update() {
                Kinect1_Update(pKinect1);
                Kinect1_Update(pKinect1);
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Kinect1_Update(IntPtr pKinect1);

            #region Tracker
            public override Vector3 position {
                set {
                    Kinect1_SetPosition(pKinect1, new Vec3(value));
                }
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern void Kinect1_SetPosition(IntPtr pKinect1, Vec3 position);

            public override Quaternion rotation {
                set {
                    Kinect1_SetRotation(pKinect1, new Quat(value));
                }
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern void Kinect1_SetRotation(IntPtr pKinect1, Quat rotation);

            public override TrackerTransformC GetTrackerData() {
                return Kinect1_GetTrackerData(pKinect1);
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern TrackerTransformC Kinect1_GetTrackerData(IntPtr pKinect1);

            #endregion

            #region Bones
            public override SensorTransformC GetBoneData(uint actorId, Bone boneId) {
                return Kinect1_GetBoneData(actorId, boneId);
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern SensorTransformC Kinect1_GetBoneData(uint actorId, Bone boneId);

            public override SensorTransformC GetBoneData(uint actorId, Side side, SideBone boneId) {
                return Kinect1_GetSideBoneData(actorId, side, boneId);
            }
            [DllImport("HumanoidKinect1", CallingConvention = CallingConvention.Cdecl)]
            private static extern SensorTransformC Kinect1_GetSideBoneData(uint actorId, Side side, SideBone boneId);

            #endregion
        }
    }
}
#endif