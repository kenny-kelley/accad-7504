using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class SteamVrControllerComponent : SensorComponent {
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        protected const string resourceName = "Vive Controller";
        public int trackerId = -1;

        public bool isLeft;

        public enum ControllerType {
            SteamVRController,
            OculusTouch,
            MixedReality
        }
        public ControllerType controllerType;

        public Vector3 touchPad;
        public float trigger;
        public float gripButton;
        public float menuButton;
        public float aButton;

        public static SteamVrControllerComponent NewController(HumanoidControl humanoid, int trackerId = -1) {
            GameObject trackerPrefab = Resources.Load(resourceName) as GameObject;
            GameObject trackerObject = (trackerPrefab == null) ? new GameObject(resourceName) : Instantiate(trackerPrefab);

            trackerObject.name = resourceName;

            SteamVrControllerComponent trackerComponent = trackerObject.GetComponent<SteamVrControllerComponent>();
            if (trackerComponent == null)
                trackerComponent = trackerObject.AddComponent<SteamVrControllerComponent>();

            if (trackerId != -1)
                trackerComponent.trackerId = trackerId;
            trackerObject.transform.parent = humanoid.steam.trackerTransform;

            trackerComponent.StartComponent(humanoid.steam.trackerTransform);

            return trackerComponent;
        }

        public override void UpdateComponent() {
            if (SteamDevice.status == Status.Unavailable)
                status = Status.Unavailable;

            //if (trackerId < 0)
            //    FindOutermostController(isLeft);

            if (SteamDevice.GetConfidence(trackerId) == 0) {
                status = SteamDevice.IsPresent(trackerId) ? Status.Present : Status.Unavailable;
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            status = Status.Tracking;
            Vector3 localSensorPosition = Target.ToVector3(SteamDevice.GetPosition(trackerId));
            Quaternion localSensorRotation = Target.ToQuaternion(SteamDevice.GetRotation(trackerId));
            transform.position = trackerTransform.TransformPoint(localSensorPosition);
            transform.rotation = trackerTransform.rotation * localSensorRotation;

            positionConfidence = SteamDevice.GetConfidence(trackerId);
            rotationConfidence = SteamDevice.GetConfidence(trackerId);
            gameObject.SetActive(true);

            Passer.VRControllerState_t controllerState = new Passer.VRControllerState_t();
            var system = Passer.OpenVR.System;
            uint controllerStateSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Passer.VRControllerState_t));
            bool newControllerState = system.GetControllerState((uint)trackerId, ref controllerState, controllerStateSize);
            if (system != null && newControllerState)
                UpdateInput(controllerState);
        }

        private void FindOutermostController(bool isLeft) {
            Vector outermostLocalPos = new Vector(isLeft ? -0.1F : 0.1F, 0, 0);

            for (int i = 0; i < Passer.OpenVR.k_unMaxTrackedDeviceCount; i++) {
                if (SteamDevice.GetDeviceClass(i) != Passer.ETrackedDeviceClass.Controller)
                    continue;

                Passer.ETrackedControllerRole role = Passer.OpenVR.System.GetControllerRoleForTrackedDeviceIndex((uint)i);
                if ((isLeft && role == Passer.ETrackedControllerRole.LeftHand) ||
                    (!isLeft && role == Passer.ETrackedControllerRole.RightHand)) {

                    trackerId = i;
                    return;
                }

                Vector sensorLocalPos = Rotation.Inverse(SteamDevice.GetRotation(0)) * (SteamDevice.GetPosition(i) - SteamDevice.GetPosition(0)); // 0 = HMD

                if ((isLeft && sensorLocalPos.x < outermostLocalPos.x && role != Passer.ETrackedControllerRole.RightHand) ||
                    (!isLeft && sensorLocalPos.x > outermostLocalPos.x) && role != Passer.ETrackedControllerRole.LeftHand) {

                    trackerId = i;
                    outermostLocalPos = sensorLocalPos;
                }
            }
        }

        public static bool IsLeftController(uint sensorId) {
            Passer.ETrackedControllerRole role = Passer.OpenVR.System.GetControllerRoleForTrackedDeviceIndex(sensorId);
            return (role == Passer.ETrackedControllerRole.LeftHand);
        }

        public static bool IsRightController(uint sensorId) {
            Passer.ETrackedControllerRole role = Passer.OpenVR.System.GetControllerRoleForTrackedDeviceIndex(sensorId);
            return (role == Passer.ETrackedControllerRole.RightHand);
        }

        public void UpdateInput(Passer.VRControllerState_t controllerState) {
            Vector2 thumbstickPosition = GetAxis(controllerState, Passer.EVRButtonId.k_EButton_Axis2);
            Vector2 touchPadPosition = GetAxis(controllerState, Passer.EVRButtonId.k_EButton_SteamVR_Touchpad);
            touchPadPosition += thumbstickPosition;

            float touchPadButton =
                GetPress(controllerState, Passer.EVRButtonId.k_EButton_SteamVR_Touchpad) ? 1 :
                (GetTouch(controllerState, Passer.EVRButtonId.k_EButton_SteamVR_Touchpad) ? 0 : -1);
            touchPad = new Vector3(touchPadPosition.x, touchPadPosition.y, touchPadButton);

            gripButton = GetPress(controllerState, Passer.EVRButtonId.k_EButton_Grip) ? 1 : 0;
            trigger = GetAxisX(controllerState, Passer.EVRButtonId.k_EButton_SteamVR_Trigger);

            menuButton =
                GetPress(controllerState, Passer.EVRButtonId.k_EButton_ApplicationMenu) ? 1 :
                (GetTouch(controllerState, Passer.EVRButtonId.k_EButton_ApplicationMenu) ? 0 : -1);
            aButton =
                GetPress(controllerState, Passer.EVRButtonId.k_EButton_A) ? 1 :
                (GetTouch(controllerState, Passer.EVRButtonId.k_EButton_A) ? 0 : -1);

        }

        private float GetAxisX(Passer.VRControllerState_t state, Passer.EVRButtonId button) {
            var axisId = (uint)button - (uint)Passer.EVRButtonId.k_EButton_Axis0;
            switch (axisId) {
                case 0: return state.rAxis0.x;
                case 1: return state.rAxis1.x;
                case 2: return state.rAxis2.x;
                case 3: return state.rAxis3.x;
                case 4: return state.rAxis4.x;
            }
            return 0;
        }

        private Vector2 GetAxis(Passer.VRControllerState_t state, Passer.EVRButtonId button) {
            var axisId = (uint)button - (uint)Passer.EVRButtonId.k_EButton_Axis0;
            switch (axisId) {
                case 0: return new Vector2(state.rAxis0.x, state.rAxis0.y);
                case 1: return new Vector2(state.rAxis1.x, state.rAxis1.y);
                case 2: return new Vector2(state.rAxis2.x, state.rAxis2.y);
                case 3: return new Vector2(state.rAxis3.x, state.rAxis3.y);
                case 4: return new Vector2(state.rAxis4.x, state.rAxis4.y);
            }
            return Vector2.zero;
        }

        private bool GetPress(Passer.VRControllerState_t controllerState, Passer.EVRButtonId button) {
            return (controllerState.ulButtonPressed & ButtonMaskFromId(button)) != 0;
        }

        private bool GetTouch(Passer.VRControllerState_t controllerState, Passer.EVRButtonId button) {
            return (controllerState.ulButtonTouched & ButtonMaskFromId(button)) != 0;
        }

        private ulong ButtonMaskFromId(Passer.EVRButtonId id) {
            return (ulong)1 << (int)id;
        }
#endif
    }
}