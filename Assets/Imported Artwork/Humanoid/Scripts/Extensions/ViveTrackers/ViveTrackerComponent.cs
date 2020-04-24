using UnityEngine;

namespace Passer {
    using Humanoid;
    using Humanoid.Tracking;

    public class ViveTrackerComponent : SensorComponent {
#if (hSTEAMVR || hOPENVR) && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        public int trackerId = -1;

        public bool pogo3;
        public bool pogo4;
        public bool pogo5;
        public bool pogo6;

        public override void UpdateComponent() {
#if hSTEAMVR
            if (SteamDevice.status == Status.Unavailable)
                status = Status.Unavailable;

            if (SteamDevice.GetConfidence(trackerId) == 0) {
                status = SteamDevice.IsPresent(trackerId) ? Status.Present : Status.Unavailable;
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            status = Status.Tracking;
            Vector3 localSensorPosition = Target.ToVector3(SteamDevice.GetPosition(trackerId));
            Quaternion localSensorRotation = Target.ToQuaternion(SteamDevice.GetRotation(trackerId)) * Quaternion.Euler(270, 0, 180);
            transform.position = trackerTransform.TransformPoint(localSensorPosition);
            transform.rotation = trackerTransform.rotation * localSensorRotation;

            positionConfidence = SteamDevice.GetConfidence(trackerId);
            rotationConfidence = SteamDevice.GetConfidence(trackerId) * 0.6F;
#elif hOPENVR
            if (OpenVRDevice.status == Status.Unavailable)
                status = Status.Unavailable;

            if (OpenVRDevice.GetConfidence(trackerId) == 0) {
                status = OpenVRDevice.IsPresent(trackerId) ? Status.Present : Status.Unavailable;
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            status = Status.Tracking;
            Vector3 localSensorPosition = Target.ToVector3(OpenVRDevice.GetPosition(trackerId));
            Quaternion localSensorRotation = Target.ToQuaternion(OpenVRDevice.GetRotation(trackerId)) * Quaternion.Euler(270, 0, 180);
            transform.position = trackerTransform.TransformPoint(localSensorPosition);
            transform.rotation = trackerTransform.rotation * localSensorRotation;

            positionConfidence = OpenVRDevice.GetConfidence(trackerId);
            rotationConfidence = OpenVRDevice.GetConfidence(trackerId);
#endif

            gameObject.SetActive(true);

            Passer.VRControllerState_t controllerState = new Passer.VRControllerState_t();
            var system = Passer.OpenVR.System;
            uint controllerStateSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Passer.VRControllerState_t));
            bool newControllerState = system.GetControllerState((uint)trackerId, ref controllerState, controllerStateSize);
            if (system != null && newControllerState)
                UpdateInput(controllerState);
        }

        public void UpdateInput(Passer.VRControllerState_t controllerState) {
            pogo3 = GetPress(controllerState, Passer.EVRButtonId.k_EButton_Grip);
            pogo4 = GetPress(controllerState, Passer.EVRButtonId.k_EButton_SteamVR_Trigger);
            pogo5 = GetPress(controllerState, Passer.EVRButtonId.k_EButton_SteamVR_Touchpad);
            pogo6 = GetPress(controllerState, Passer.EVRButtonId.k_EButton_ApplicationMenu);
        }

        private bool GetPress(Passer.VRControllerState_t controllerState, Passer.EVRButtonId button) {
            return (controllerState.ulButtonPressed & ButtonMaskFromId(button)) != 0;
        }

        private ulong ButtonMaskFromId(Passer.EVRButtonId id) {
            return (ulong)1 << (int)id;
        }
#endif
        }
}
