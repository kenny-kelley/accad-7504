#if hKINECT2

using UnityEngine;
//using Microsoft.Kinect.Face;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class Kinect2Tracker : Tracker {
        public KinectDeviceView kinectDevice;

        public int bodyID;

        public Kinect2Tracker() {
            kinectDevice = new KinectDeviceView();
        }

        public override string name {
            get { return NativeKinectDevice.name; }
        }

        //public NativeKinectDevice device;
        public TrackerTransform kinectTransform;

        public override void Enable() {
            base.Enable();
            AddTracker(humanoid);
        }

        private readonly Vector3 defaultTrackerPosition = new Vector3(0, 1.2F, 1);
        private readonly Quaternion defaultTrackerRotation = Quaternion.Euler(0, 180, 0);

        public override bool AddTracker(HumanoidControl humanoid, string resourceName) {
            bool trackerAdded = base.AddTracker(humanoid, resourceName);
            if (trackerAdded) {
                trackerTransform.transform.localPosition = defaultTrackerPosition;
                trackerTransform.transform.localRotation = defaultTrackerRotation;
            }
            return trackerAdded;
        }

        public static GameObject AddTracker(HumanoidControl humanoid) {
            GameObject realWorld = HumanoidControl.GetRealWorld(humanoid.transform);

            humanoid.kinectTracker.trackerTransform = FindTrackerObject(realWorld, NativeKinectDevice.name);
            if (humanoid.kinectTracker.trackerTransform == null) {
                humanoid.kinectTracker.trackerTransform = CreateTracker();
                humanoid.kinectTracker.trackerTransform.transform.parent = realWorld.transform;
            }
            return humanoid.kinectTracker.trackerTransform.gameObject;
        }

        public static Transform CreateTracker() {
            GameObject kinect2Model = Resources.Load("Kinect2") as GameObject;

            GameObject trackerObject = Object.Instantiate(kinect2Model);
            trackerObject.name = NativeKinectDevice.name;
            return trackerObject.transform;
        }

        public static void RemoveTracker(HumanoidControl humanoid) {
            Object.DestroyImmediate(humanoid.kinectTracker.trackerTransform, true);
        }

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            _humanoid = humanoid;

            if (!enabled)
                return;

            AddTracker(humanoid);
            KinectDevice.Start();

            //device = new KinectInterface();
            //device.Init();
            //kinectTransform = device.GetTracker();

            //status = kinectTransform.status;
        }
        #endregion

        #region Stop
        public override void StopTracker() {
            //if (device != null)
            //    device.Stop();
            KinectDevice.Stop();
        }
        #endregion

        #region Update
        public override void UpdateTracker() {
            if (!enabled)
                return;

            //status = kinectTransform.status;

            if (trackerTransform != null) {
                if (KinectDevice.status == Status.Unavailable)
                    trackerTransform.gameObject.SetActive(false);
                else
                    trackerTransform.gameObject.SetActive(true);
            }

            if (KinectDevice.status != Status.Unavailable)
                if ((humanoid.headTarget.kinect.status == Status.Tracking) ||
                    (humanoid.leftHandTarget.kinect.status == Status.Tracking) ||
                    (humanoid.rightHandTarget.kinect.status == Status.Tracking) ||
                    (humanoid.hipsTarget.kinect.status == Status.Tracking) ||
                    (humanoid.leftFootTarget.kinect.status == Status.Tracking) ||
                    (humanoid.rightFootTarget.kinect.status == Status.Tracking))
                    status = Status.Tracking;
                else
                    status = Status.Present;
            else
                status = Status.Unavailable;

            if (trackerTransform != null) {
                kinectDevice.position = Target.ToVector(trackerTransform.position);
                kinectDevice.orientation = Target.ToRotation(trackerTransform.rotation);

                //device.position = trackerTransform.position;
                //device.rotation = trackerTransform.rotation;
            }
            //device.Update();
            //status = kinectTransform.status;

            KinectDevice.Update();
        }
        #endregion

        //public Vector3 Target2WorldPosition(Vector3 localPosition) {
        //    return trackerTransform.transform.position + trackerTransform.transform.rotation * Quaternion.AngleAxis(180, Vector3.up) * localPosition;
        //}

        public override void Calibrate() {
            //if (kinectTransform.status != Status.Unavailable)
            //    ;//KinectDevice.Calibrate();
        }

        #region Smoothing
        public static Vector3 SmoothPosition(Vector3 lastTargetPosition, Vector sensorPosition) {
            return SmoothPosition(lastTargetPosition, Target.ToVector3(sensorPosition));
        }
        public static Vector3 SmoothPosition(Vector3 lastTargetPosition, Vector3 sensorPosition) {
            // complementary filter
            return Vector3.Lerp(lastTargetPosition, sensorPosition, Time.deltaTime * 20);
        }

        public static Quaternion SmoothRotation(Quaternion lastTargetRotation, Rotation sensorRotation) {
            return SmoothRotation(lastTargetRotation, Target.ToQuaternion(sensorRotation));
        }
        public static Quaternion SmoothRotation(Quaternion lastTargetRotation, Quaternion sensorRotation) {
            // complementary filter
            return Quaternion.Slerp(lastTargetRotation, sensorRotation, Time.deltaTime * 20);
        }
        #endregion
    }
}
#endif