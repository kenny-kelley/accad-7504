#if hHYDRA

using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class RazerHydraHand : UnityArmController {
        public override string name {
            get { return "Razer Hydra"; }
        }

        public override Status status {
            get {
                if (hydraController == null)
                    return Status.Unavailable;
                return hydraController.status;
            }
            set { hydraController.status = value; }
        }

        public HydraControllerComponent hydraController;

        #region Start
        public override void Init(HandTarget handTarget) {
            base.Init(handTarget);
            tracker = handTarget.humanoid.hydra;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = handTarget.humanoid.hydra;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            SetSensor2Target();
            CheckSensorTransform();
            ShowSensor(handTarget.humanoid.showRealObjects && target.showRealObjects);

            if (sensorTransform != null) {
                hydraController = sensorTransform.GetComponent<HydraControllerComponent>();
                if (hydraController != null)
                    hydraController.StartComponent(tracker.trackerTransform, handTarget.isLeft);
            }
        }

        protected override void CreateSensorTransform() {
            if (handTarget.isLeft)
                CreateSensorTransform("Hydra Controller", new Vector3(-0.01F, -0.04F, -0.01F), Quaternion.Euler(180, 110, 90));
            else
                CreateSensorTransform("Hydra Controller", new Vector3(0.01F, -0.04F, -0.01F), Quaternion.Euler(0, 70, 90));

            HydraControllerComponent hydraController = sensorTransform.GetComponent<HydraControllerComponent>();
            if (hydraController == null)
                hydraController = sensorTransform.gameObject.AddComponent<HydraControllerComponent>();
            hydraController.isLeft = handTarget.isLeft;
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (hydraController == null) {
                UpdateTarget(handTarget.hand.target, sensorTransform);
                return;
            }

            hydraController.UpdateComponent();
            status = hydraController.status;
            if (hydraController.status != Status.Tracking)
                return;

            UpdateTarget(handTarget.hand.target, hydraController);
            UpdateInput();
        }

        protected void UpdateInput() {
            if (handTarget.isLeft)
                SetControllerInput(controllerInput.left);
            else
                SetControllerInput(controllerInput.right);
        }

        protected void SetControllerInput(ControllerSide controllerSide) {
            controllerSide.stickHorizontal += hydraController.joystick.x;
            controllerSide.stickVertical += hydraController.joystick.y;
            controllerSide.stickButton |= (hydraController.joystick.z > 0.5F);
            controllerSide.stickTouch = true;

            controllerSide.buttons[0] |= hydraController.button1 > 0.5F;
            controllerSide.buttons[1] |= hydraController.button2 > 0.5F;
            controllerSide.buttons[2] |= hydraController.button3 > 0.5F;
            controllerSide.buttons[3] |= hydraController.button4 > 0.5F;

            controllerSide.trigger1 += hydraController.bumper;
            controllerSide.trigger2 += hydraController.trigger;

            controllerSide.option |= hydraController.option > 0.5F;
        }
        #endregion
    }

}
#endif