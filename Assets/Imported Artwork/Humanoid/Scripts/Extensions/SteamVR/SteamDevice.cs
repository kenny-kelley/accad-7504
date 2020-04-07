#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using System;

namespace Passer.Humanoid.Tracking {
    public class SteamDevice {
        public const string name = "SteamVR";

        public static bool present = true;
        public static Status status;

        private struct SensorState {
            public Passer.ETrackedDeviceClass deviceClass;
            public Vector position;
            public Rotation rotation;
            public float confidence;
            public bool present;
        }

        private static SensorState[] sensorStates = new SensorState[Passer.OpenVR.k_unMaxTrackedDeviceCount];

        public delegate void OnNewSensor(uint i);
        public static OnNewSensor onNewSensor;

        public static void Start() {
            status = Status.Unavailable;
        }

        public static void Update() {
            Passer.CVRCompositor compositor = Passer.OpenVR.Compositor;
            Passer.CVRSystem system = Passer.OpenVR.System;

            if (compositor != null) {
                status = Status.Present;
                Passer.TrackedDevicePose_t[] renderPoseArray = new Passer.TrackedDevicePose_t[16];
                Passer.TrackedDevicePose_t[] gamePoseArray = new Passer.TrackedDevicePose_t[16];
                compositor.GetLastPoses(renderPoseArray, gamePoseArray);

                for (uint i = 0; i < renderPoseArray.Length; i++) {
                    if (!sensorStates[i].present && renderPoseArray[i].bDeviceIsConnected) {
                        // Detected new sensor
                        onNewSensor(i);
                    }
                    sensorStates[i].present = renderPoseArray[i].bDeviceIsConnected;
                    if (renderPoseArray[i].bPoseIsValid) {
                        sensorStates[i].confidence = (renderPoseArray[i].eTrackingResult == Passer.ETrackingResult.Running_OK) ? 1 : 0;
                        StorePose(system, renderPoseArray[i].mDeviceToAbsoluteTracking, i);
                        status = Status.Tracking;
                    }
                    else
                        sensorStates[i].confidence = 0;
                }
            }
        }

        private static ISteamSensor[] sensors = new ISteamSensor[Passer.OpenVR.k_unMaxTrackedDeviceCount]; // SteamVR limits # sensors to 16
        //public static void AssignTrackerToSensor(int trackerId, ISteamSensor sensor) {
        //    if (trackerId < 0 || trackerId > OpenVR.k_unMaxTrackedDeviceCount)
        //        return;

        //    if (sensors[trackerId] != null && sensors[trackerId] != sensor) {
        //        // We already assigned this tracker to a sensor
        //        // now the old sensor is no longer valid
        //        sensors[trackerId].trackerId = -1;
        //    }
        //    sensor.trackerId = trackerId;
        //    sensors[trackerId] = sensor;
        //}

        public static void ResetSensors() {
            for (int i = 0; i < sensors.Length; i++) {
                if (sensors[i] != null) {
                    sensors[i].trackerId = -1;
                    sensors[i] = null;
                }
            }
        }

        private static void StorePose(Passer.CVRSystem system, Passer.HmdMatrix34_t pose, uint sensorID) {
            Matrix4x4 m = new Matrix4x4();

            m.m00 = pose.m0;
            m.m01 = pose.m1;
            m.m02 = -pose.m2;
            m.m03 = pose.m3;

            m.m10 = pose.m4;
            m.m11 = pose.m5;
            m.m12 = -pose.m6;
            m.m13 = pose.m7;

            m.m20 = -pose.m8;
            m.m21 = -pose.m9;
            m.m22 = pose.m10;
            m.m23 = -pose.m11;

            m.m30 = 0;
            m.m31 = 0;
            m.m32 = 0;
            m.m33 = 0;

            sensorStates[sensorID].position = GetPosition(m);
            sensorStates[sensorID].rotation = GetRotation(m);
            sensorStates[sensorID].deviceClass = system.GetTrackedDeviceClass(sensorID);
        }

        public static Passer.ETrackedDeviceClass GetDeviceClass(int sensorID) {
            if (sensorStates == null)
                return Passer.ETrackedDeviceClass.Invalid;

            return sensorStates[sensorID].deviceClass;
        }

        public static Vector GetPosition(int sensorID) {
            if (sensorStates == null)
                return Vector.zero;

            return sensorStates[sensorID].position;
        }

        public static Rotation GetRotation(int sensorID) {
            if (sensorStates == null)
                return Rotation.identity;

            return sensorStates[sensorID].rotation;
        }

        public static float GetConfidence(int sensorID) {
            if (sensorStates == null || sensorID < 0 || sensorID > sensorStates.Length)
                return 0;

            return sensorStates[sensorID].confidence;
        }

        public static bool IsPresent(int sensorID) {
            if (sensorStates == null || sensorID < 0 || sensorID > sensorStates.Length)
                return false;

            return sensorStates[sensorID].present;
        }

        private struct Matrix4x4 {
            public float m00;
            public float m01;
            public float m02;
            public float m03;
            public float m10;
            public float m11;
            public float m12;
            public float m13;
            public float m20;
            public float m21;
            public float m22;
            public float m23;
            public float m30;
            public float m31;
            public float m32;
            public float m33;
        }

        private static Rotation GetRotation(Matrix4x4 matrix) {
            Rotation q = Rotation.identity; // new Rotation();
            q.w = (float)Math.Sqrt(Math.Max(0, 1 + matrix.m00 + matrix.m11 + matrix.m22)) / 2;
            q.x = (float)Math.Sqrt(Math.Max(0, 1 + matrix.m00 - matrix.m11 - matrix.m22)) / 2;
            q.y = (float)Math.Sqrt(Math.Max(0, 1 - matrix.m00 + matrix.m11 - matrix.m22)) / 2;
            q.z = (float)Math.Sqrt(Math.Max(0, 1 - matrix.m00 - matrix.m11 + matrix.m22)) / 2;
            q.x = _copysign(q.x, matrix.m21 - matrix.m12);
            q.y = _copysign(q.y, matrix.m02 - matrix.m20);
            q.z = _copysign(q.z, matrix.m10 - matrix.m01);
            return q;
        }

        private static float _copysign(float sizeval, float signval) {
            if (float.IsNaN(signval))
                return Math.Abs(sizeval);
            else
                return Math.Sign(signval) == 1 ? Math.Abs(sizeval) : -Math.Abs(sizeval);
        }

        private static Vector GetPosition(Matrix4x4 matrix) {
            var x = matrix.m03;
            var y = matrix.m13;
            var z = matrix.m23;

            return new Vector(x, y, z);
        }
    }

    public interface ISteamSensor {
        int trackerId { get; set; }
    }
}
#endif