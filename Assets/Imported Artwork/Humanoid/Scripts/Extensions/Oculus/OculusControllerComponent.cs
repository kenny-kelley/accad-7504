﻿using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class OculusControllerComponent : SensorComponent {
#if hOCULUS
        public bool isLeft;

        public Vector3 joystick;
        public float indexTrigger;
        public float handTrigger;
        public float buttonAX;
        public float buttonBY;
        public float thumbrest;

        public bool positionalTracking = true;

        public override void UpdateComponent() {
            status = Status.Tracking;
            if (OculusDevice.status == Status.Unavailable)
                status = Status.Unavailable;

            Sensor.ID sensorID = isLeft ? Sensor.ID.LeftHand : Sensor.ID.RightHand;

            if (OculusDevice.GetRotationalConfidence(sensorID) == 0)
                status = Status.Present;

            if (status == Status.Present || status == Status.Unavailable) {
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            Vector3 localSensorPosition = Target.ToVector3(OculusDevice.GetPosition(sensorID));
            Quaternion localSensorRotation = Target.ToQuaternion(OculusDevice.GetRotation(sensorID));
            transform.position = trackerTransform.TransformPoint(localSensorPosition);
            transform.rotation = trackerTransform.rotation * localSensorRotation;

            positionConfidence = OculusDevice.GetPositionalConfidence(sensorID);
            rotationConfidence = OculusDevice.GetRotationalConfidence(sensorID);
            gameObject.SetActive(true);

            UpdateInput(sensorID);
        }

        private void UpdateInput(Sensor.ID sensorID) {
            switch (sensorID) {
                case Sensor.ID.LeftHand:
                    UpdateLeftInput();
                    return;
                case Sensor.ID.RightHand:
                    UpdateRightInput();
                    return;
                default:
                    return;
            }
        }

        private void UpdateLeftInput() {
            OculusDevice.Controller controllerMask;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!positionalTracking)
                controllerMask = OculusDevice.Controller.LTrackedRemote;
            else
#endif
                controllerMask = OculusDevice.Controller.LTouch;
            OculusDevice.ControllerState4 controllerState = OculusDevice.GetControllerState(controllerMask);

            float stickButton =
            OculusDevice.GetStickPress(controllerState) ? 1 : (
            OculusDevice.GetStickTouch(controllerState) ? 0 : -1);

            if (positionalTracking)
                joystick = new Vector3(
                    OculusDevice.GetHorizontalStick(controllerState, true),
                    OculusDevice.GetVerticalStick(controllerState, true),
                    stickButton);
            else
                joystick = new Vector3(
                    OculusDevice.GetHorizontalTouch(controllerState, true),
                    OculusDevice.GetVerticalTouch(controllerState, true),
                    stickButton);


            indexTrigger = OculusDevice.GetTrigger1(controllerState, true);
            handTrigger = OculusDevice.GetTrigger2(controllerState, true);

            buttonAX =
                OculusDevice.GetButton1Press(controllerState) ? 1 : (
                OculusDevice.GetButton1Touch(controllerState) ? 0 : -1);
            buttonBY =
                OculusDevice.GetButton2Press(controllerState) ? 1 : (
                OculusDevice.GetButton2Touch(controllerState) ? 0 : -1);
            thumbrest =
                OculusDevice.GetThumbRest(controllerState) ? 0 : -1;
        }

        private void UpdateRightInput() {
            OculusDevice.Controller controllerMask;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!positionalTracking)
                controllerMask = OculusDevice.Controller.RTrackedRemote;
            else
#endif
                controllerMask = OculusDevice.Controller.RTouch;

            OculusDevice.ControllerState4 controllerState = OculusDevice.GetControllerState(controllerMask);


            float stickButton =
                OculusDevice.GetStickPress(controllerState) ? 1 : (
                OculusDevice.GetStickTouch(controllerState) ? 0 : -1);

            if (positionalTracking) {
                joystick = new Vector3(
                    OculusDevice.GetHorizontalStick(controllerState, false),
                    OculusDevice.GetVerticalStick(controllerState, false),
                    stickButton);
            }
            else {
                joystick = new Vector3(
                    OculusDevice.GetHorizontalTouch(controllerState, false),
                    OculusDevice.GetVerticalTouch(controllerState, false),
                    stickButton);
            }

            indexTrigger = OculusDevice.GetTrigger1(controllerState, false);
            handTrigger = OculusDevice.GetTrigger2(controllerState, false);

            buttonAX =
                OculusDevice.GetButton1Press(controllerState) ? 1 : (
                OculusDevice.GetButton1Touch(controllerState) ? 0 : -1);
            buttonBY =
                OculusDevice.GetButton2Press(controllerState) ? 1 : (
                OculusDevice.GetButton2Touch(controllerState) ? 0 : -1);
            thumbrest =
                OculusDevice.GetThumbRest(controllerState) ? 0 : -1;
        }
#endif
    }
}