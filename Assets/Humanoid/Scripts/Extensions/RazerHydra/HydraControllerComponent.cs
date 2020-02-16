using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class HydraControllerComponent : SensorComponent {
#if hHYDRA
        public bool isLeft;

        public Vector3 joystick;
        public float bumper;
        public float trigger;
        public float button1;
        public float button2;
        public float button3;
        public float button4;
        public float option;

        public virtual void StartComponent(Transform trackerTransform, bool isLeft) {
            StartComponent(trackerTransform);
            this.isLeft = isLeft;
        }

        public override void UpdateComponent() {
            status = Status.Tracking;
            if (HydraDevice.status == Status.Unavailable)
                status = Status.Unavailable;

            if (HydraDevice.GetConfidence(isLeft) == 0)
                status = Status.Present;

            if (status == Status.Present || status == Status.Unavailable) {
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            Vector3 localSensorPosition = Target.ToVector3(HydraDevice.GetControllerLocalPosition(isLeft));
            Quaternion localSensorRotation = Target.ToQuaternion(HydraDevice.GetControllerLocalOrientation(isLeft));
            transform.position = trackerTransform.TransformPoint(localSensorPosition);
            transform.rotation = trackerTransform.rotation * localSensorRotation;

            positionConfidence = HydraDevice.GetConfidence(isLeft);
            rotationConfidence = HydraDevice.GetConfidence(isLeft);
            gameObject.SetActive(true);

            UpdateInput();
        }

        private void UpdateInput() {
            HydraDevice.Controller hydraController = HydraDevice.GetController(isLeft);
            if (hydraController == null)
                return;

            float stickPress = hydraController.GetButton(HydraDevice.HydraButtons.JOYSTICK) ? 1 : 0;
            float joystickX = (Mathf.Abs(hydraController.m_JoystickX) < 0.1F) ? 0 : hydraController.m_JoystickX;
            float joystickY = (Mathf.Abs(hydraController.m_JoystickY) < 0.1F) ? 0 : hydraController.m_JoystickY;
            joystick = new Vector3(joystickX, joystickY, stickPress);

            bumper = hydraController.GetButton(HydraDevice.HydraButtons.BUMPER) ? 1 : 0;
            trigger = hydraController.m_Trigger;

            button1 = hydraController.GetButton(HydraDevice.HydraButtons.ONE) ? 1 : 0;
            button2 = hydraController.GetButton(HydraDevice.HydraButtons.TWO) ? 1 : 0;
            button3 = hydraController.GetButton(HydraDevice.HydraButtons.THREE) ? 1 : 0;
            button4 = hydraController.GetButton(HydraDevice.HydraButtons.FOUR) ? 1 : 0;

            option = hydraController.GetButton(HydraDevice.HydraButtons.START) ? 1 : 0;
        }
#endif
    }
}