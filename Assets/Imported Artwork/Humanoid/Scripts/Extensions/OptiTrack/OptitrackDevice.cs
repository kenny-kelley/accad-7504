#if hOPTITRACK
using System.Runtime.InteropServices;
using UnityEngine;

namespace Passer.Humanoid.Tracking {

    public class OptitrackSensor : Sensor {
        public TargetData sensorTransform;

        private OptitrackStreamingClient streamingClient;

        public OptitrackSensor(DeviceView device, OptitrackStreamingClient _streamingClient) : base(device) {
            streamingClient = _streamingClient;
        }

        private const float maxAge_sec = 0.01F; // if a measurement is older then this value, it will not be used

        public void Update(int trackerId) {
            if (streamingClient == null)
                return;

            OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(trackerId);
            if (rbState == null || rbState.DeliveryTimestamp.AgeSeconds > maxAge_sec) {
                _rotationConfidence = 0;
                _positionConfidence = 0;
                status = Status.Present;
                return;
            }

            _localSensorPosition = Passer.Target.ToVector(rbState.Pose.Position);
            _localSensorRotation = Passer.Target.ToRotation(rbState.Pose.Orientation);
            _rotationConfidence = 0.99F; // only native tracking is better
            _positionConfidence = 1;

            UpdateSensor();

            status = Status.Tracking;
        }
    }

    public static class OptitrackDevice {

        public static string name = "OptiTrack";

        public static string SkeletonAssetName;

        public static void Start(string localAddress, string serverAddress, int serverCommandPort, int serverDataPort) {
        }

        public static void Update() {
        }

        public static void Stop() {
        }

        public static Vector GetPosition(OptitrackStreamingClient streamingClient, int trackerId) {
            OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(trackerId);
            return Passer.Target.ToVector(rbState.Pose.Position);
        }

        public static Rotation GetRotation(OptitrackStreamingClient streamingClient, int trackerId) {
            OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(trackerId);
            return Passer.Target.ToRotation(rbState.Pose.Orientation);
        }

        public static void GetPositionRotation(OptitrackStreamingClient streamingClient, int trackerId, out Vector position, out Rotation rotation) {
            OptitrackRigidBodyState rbState = streamingClient.GetLatestRigidBodyState(trackerId);
            position = Passer.Target.ToVector(rbState.Pose.Position);
            rotation = Passer.Target.ToRotation(rbState.Pose.Orientation);
        }

        public static Rotation GetLocalOrientation(Bone bone) {
            return Rotation.identity;
        }

        public static Rotation GetLocalRotation(Bone bone) {
            return Rotation.identity;
        }

        public static Rotation GetTargetRotation(Bone bone, Rotation parentRotation) {
            return GetTargetRotation(GetLocalRotation(bone), parentRotation);
        }

        public static Rotation GetTargetRotation(Rotation receivedRotation, Rotation parentOrientation) {
            return parentOrientation * receivedRotation;
        }
    }
    
    public class NativeOptitrackDevice {
        public static string name = "OptiTrack";
        private static System.IntPtr pOptitrack;

        public NativeOptitrackDevice() {
            pOptitrack = Optitrack_Constructor();
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern System.IntPtr Optitrack_Constructor();

        ~NativeOptitrackDevice() {
            Optitrack_Destructor(pOptitrack);
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Optitrack_Destructor(System.IntPtr pOptitrack);

        public void Init(string localAddress, string serverAddress, int serverCommandPort, int serverDataPort) {
            Optitrack_Init(pOptitrack, localAddress, serverAddress, serverCommandPort, serverDataPort);
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Optitrack_Init(System.IntPtr pOptitrack, string localAddress, string serverAddress, int serverCommandPort, int serverDataPort);

        public void Stop() {
            Optitrack_Stop(pOptitrack);
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Optitrack_Stop(System.IntPtr pOptitrack);

        #region Tracker
        public void SetPosition(Vector3 position) {
            Optitrack_SetPosition(pOptitrack, new Vec3(position));
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Optitrack_SetPosition(System.IntPtr pOptitrack, Vec3 position);

        public void SetRotation(Quaternion rotation) {
            Optitrack_SetRotation(pOptitrack, new Quat(rotation));
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Optitrack_SetRotation(System.IntPtr pOptitrack, Quat rotation);
        #endregion

        #region Sensors
        public Vector3 GetSensorPosition(int sensorId) {
            return Optitrack_GetSensorPosition(pOptitrack, sensorId).Vector3;
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern Vec3 Optitrack_GetSensorPosition(System.IntPtr pOptitrack, int sensorId);

        public Quaternion GetSensorRotation(int sensorId) {
            return Optitrack_GetSensorRotation(pOptitrack, sensorId).Quaternion;
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern Quat Optitrack_GetSensorRotation(System.IntPtr pOptitrack, int sensorId);

        public float GetSensorConfidence(int sensorId) {
            return Optitrack_GetSensorConfidence(pOptitrack, sensorId);
        }
        [DllImport("HumanoidOptitrack", CallingConvention = CallingConvention.Cdecl)]
        private static extern float Optitrack_GetSensorConfidence(System.IntPtr pOptitrack, int sensorId);
        #endregion
    }    
}
#endif