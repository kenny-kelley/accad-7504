#if hOPENVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using UnityEngine;

namespace Passer.Humanoid {
    using Tracking;

    [System.Serializable]
    public class OpenVRHand : UnityArmController {
        public override string name {
            get { return "OpenVR Controller"; }
        }

        public OpenVRController openVRController;

        public override Status status {
            get {
                if (openVRController == null)
                    return Status.Unavailable;
                return openVRController.status;
            }
            set { openVRController.status = value; }
        }

        public OpenVRController.ControllerType controllerType;
        public bool useSkeletalInput = true;

        #region Start
        public override void Init(HandTarget handTarget) {
            base.Init(handTarget);
            tracker = handTarget.humanoid.openVR;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = handTarget.humanoid.openVR;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            SetSensor2Target();
            CheckSensorTransform();
            ShowSensor(handTarget.humanoid.showRealObjects && target.showRealObjects);

            if (sensorTransform != null) {
                openVRController = sensorTransform.GetComponent<OpenVRController>();
                if (openVRController != null)
                    openVRController.StartComponent(tracker.trackerTransform);
            }
        }

        //        public override void RefreshSensor() {
        //            if (openVRController != null) {
        //#if UNITY_EDITOR
        //                Object.DestroyImmediate(openVRController, true);
        //#else
        //                Object.Destroy(openVRController);
        //#endif
        //            }

        //            CreateSensorTransform();
        //        }

        protected override void CreateSensorTransform() {
            string prefabLeftName = "Vive Controller";
            string prefabRightName = "Vive Controller";
            switch (controllerType) {
                case OpenVRController.ControllerType.ValveIndex:
                    break;
                case OpenVRController.ControllerType.MixedReality:
                    prefabLeftName = "Windows MR Controller Left";
                    prefabRightName = "Windows MR Controller Right";
                    break;
                case OpenVRController.ControllerType.OculusTouch:
                    prefabLeftName = "Left Touch Controller";
                    prefabRightName = "Right Touch Controller";
                    break;
                case OpenVRController.ControllerType.SteamVRController:
                default:
                    break;
            }

            if (handTarget.isLeft)
                CreateSensorTransform(prefabLeftName, new Vector3(-0.14F, -0.04F, 0.08F), Quaternion.Euler(0, -30, -90));
            else
                CreateSensorTransform(prefabRightName, new Vector3(0.14F, -0.04F, 0.08F), Quaternion.Euler(0, 30, 90));

            openVRController = sensorTransform.GetComponent<OpenVRController>();
            if (openVRController == null)
                openVRController = sensorTransform.gameObject.AddComponent<OpenVRController>();
            openVRController.isLeft = handTarget.isLeft;
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (openVRController == null) {
                UpdateTarget(handTarget.hand.target, sensorTransform);
                return;
            }

            openVRController.UpdateComponent();
            if (openVRController.status != Status.Tracking)
                return;

            UpdateTarget(handTarget.hand.target, openVRController);
            UpdateInput();
            if (useSkeletalInput)
                UpdateHand();
        }

        protected void UpdateInput() {
            if (handTarget.isLeft)
                SetControllerInput(controllerInput.left);
            else
                SetControllerInput(controllerInput.right);
        }

        protected void SetControllerInput(ControllerSide controllerSide) {
            controllerSide.stickHorizontal += Mathf.Clamp(openVRController.joystick.x + openVRController.touchpad.x, -1, 1);
            controllerSide.stickVertical += Mathf.Clamp(openVRController.joystick.y + openVRController.touchpad.y, -1, 1);
            controllerSide.stickButton |= (openVRController.joystick.z > 0.5F) || (openVRController.touchpad.z > 0.5F);

            controllerSide.buttons[0] |= openVRController.aButton > 0.5F;
            controllerSide.buttons[1] |= openVRController.bButton > 0.5F;

            controllerSide.trigger1 += openVRController.trigger;
            controllerSide.trigger2 += openVRController.grip;

            controllerSide.option |= openVRController.aButton > 0.5F;
        }

        protected void UpdateHand() {
            for (int i = 0; i < (int)Finger.Count; i++)
                UpdateFinger(handTarget.fingers.allFingers[i], i);

            handTarget.fingers.DetermineFingerCurl();
        }

        private void UpdateFinger(FingersTarget.TargetedFinger finger, int fingerIx) {
            finger.proximal.target.transform.localRotation = GetFingerRotation(openVRController, fingerIx, 0);
            finger.intermediate.target.transform.localRotation = GetFingerRotation(openVRController, fingerIx, 1);
            finger.distal.target.transform.localRotation = GetFingerRotation(openVRController, fingerIx, 2);
        }

        protected Quaternion GetFingerRotation(OpenVRController openVRController, int fingerIx, int boneIx) {
            int ix = fingerIx * 5 + boneIx + 2;
            Passer.VRBoneTransform_t boneTransform = openVRController.tempBoneTransforms[ix];

            Quaternion q = new Quaternion(
                boneTransform.orientation.x,
                boneTransform.orientation.y,
                boneTransform.orientation.z,
                boneTransform.orientation.w
                );
            if (!handTarget.isLeft)
                q = new Quaternion(q.x, -q.y, -q.z, q.w);
            if (fingerIx == 0) {
                if (boneIx == 0) {
                    q = Rotations.Rotate(q, Quaternion.Euler(90, 0, 0));
                    if (handTarget.isLeft)
                        q = Quaternion.Euler(0, -180, -90) * q;
                    else
                        q = Quaternion.Euler(180, -180, -90) * q;
                }
                else
                    q = Rotations.Rotate(q, Quaternion.Euler(90, 0, 0));
            }
            return q;
        }

        #endregion

        //length is how long the vibration should go for in seconds
        //strength is vibration strength from 0-1
        public override void Vibrate(float length, float strength) {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            for (float i = 0; i < length; i += Time.deltaTime)
                controller.Vibrate(length, strength);
        }
    }
}

#endif