using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Passer.Humanoid.Tracking {

    public class NeuronDevice : TrackingDevice {
        public static string name = "Perception Neuron";
        public static IntPtr pNeuron;

        public static void LoadDlls() {
            LoadLibrary("Assets/Humanoid/Plugins/NeuronDataReader.dll");
            LoadLibrary("Assets/Humanoid/Plugins/Humanoid.dll");
            LoadLibrary("Assets/Humanoid/Plugins/HumanoidNeuron.dll");
        }
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        public NeuronDevice() {
            pNeuron = Neuron_Constructor();
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Neuron_Constructor();

        ~NeuronDevice() {
            Neuron_Destructor(pNeuron);
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Neuron_Destructor(IntPtr pNeuron);

        public void Init(string ipAddress, int port) {
            Neuron_Init(pNeuron, ipAddress, port);
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Neuron_Init(IntPtr pNeuron, string ipAddress, int port);

        public override void Stop() {
            Neuron_Stop(pNeuron);
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Neuron_Stop(IntPtr pNeuron);

        #region Tracker
        public override void Update() {
            Neuron_Update(pNeuron);
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Neuron_Update(IntPtr pNeuron);

        public override Vector3 position {
            set {
                Neuron_SetPosition(pNeuron, new Vec3(value));
            }
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Neuron_SetPosition(IntPtr pNeuron, Vec3 position);

        public override Quaternion rotation {
            set {
                Neuron_SetRotation(pNeuron, new Quat(value));
            }
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Neuron_SetRotation(IntPtr pNeuron, Quat rotation);

        public override TrackerTransformC GetTrackerData() {
            TrackerTransformC trackerTransform = Neuron_GetTrackerData(pNeuron);
            return trackerTransform;
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern TrackerTransformC Neuron_GetTrackerData(IntPtr pNeuron);

        #endregion

        #region Bones
#if hUNSAFE
        unsafe public override SensorBone GetBone(uint actorId,Bone boneId) {
            SensorTransformC* pTargetTransform = Neuron_GetBone(actorId, boneId);
            return new SensorBone(pTargetTransform);
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        unsafe private static extern SensorTransformC* Neuron_GetBone(uint actorId, Bone boneId);
#else
        public override SensorTransformC GetBoneData(uint actorId, Bone boneId) {
            SensorTransformC sensorTransform = Neuron_GetBoneData(actorId, boneId);
            return sensorTransform;
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern SensorTransformC Neuron_GetBoneData(uint actorId, Bone boneId);

#endif

#if hUNSAFE
        unsafe public override SensorBone GetBone(uint actorId, Side side, SideBone boneId) {
            SensorTransformC* pTargetTransform = Neuron_GetSideBone(actorId, side, boneId);
            return new SensorBone(pTargetTransform);
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        unsafe private static extern SensorTransformC* Neuron_GetSideBone(uint actorId, Side side, SideBone boneId);
#else
        public override SensorTransformC GetBoneData(uint actorId, Side side, SideBone boneId) {
            SensorTransformC sensorTransform = Neuron_GetSideBoneData(actorId, side, boneId);
            return sensorTransform;
        }
        [DllImport("HumanoidNeuron", CallingConvention = CallingConvention.Cdecl)]
        private static extern SensorTransformC Neuron_GetSideBoneData(uint actorId, Side side, SideBone boneId);
#endif
#endregion
    }
}