using System;

namespace Passer.Humanoid.Tracking {

    public static class HydraDevice {
        public const string name = "Razer Hydra Basestation";

        public static Status status;
        public static bool started;

        // Is it OK that these are static or does every player have their own position/orientiation?
        private static Vector scale = new Vector(0.001F, 0.001F, 0.001F);

        private static Vector controller2LeftHand = Vector.zero; //new Vector(0.04F, 0.03F, 0.04F);
        private static Vector controller2RightHand = Vector.zero; // new Vector(-0.04F, 0.03F, 0.04F);

        public enum BodySide {
            Unknown = 0,
            Left = 1,
            Right = 2,
        };

        public enum HydraButtons {
            START = 1,
            ONE = 32,
            TWO = 64,
            THREE = 8,
            FOUR = 16,
            BUMPER = 128,
            JOYSTICK = 256,
            TRIGGER = 512,
        }

        private const uint MAX_CONTROLLERS = 2;
        private static Controller[] m_Controllers = new Controller[MAX_CONTROLLERS];
        private static Controller leftController;
        private static Controller rightController;

        private static bool dllPresent;

        public static void Start() {
            status = Status.Unavailable;

            SixensePlugin.sixenseInit();
            for (int i = 0; i < MAX_CONTROLLERS; i++) {
                m_Controllers[i] = new Controller();
            }
            dllPresent = true;

            started = true;

            bool baseConnected = SixensePlugin.sixenseIsBaseConnected(0) > 0;
            if (dllPresent && baseConnected)
                status = Status.Present;
        }

        private static bool Available() {
            bool baseConnected = SixensePlugin.sixenseIsBaseConnected(0) > 0;
            return (dllPresent && baseConnected);
        }

        private static bool ControllerManagerEnabled = true;
        private static ControllerManagerState m_ControllerManagerState = ControllerManagerState.NONE;
        private enum ControllerManagerState {
            NONE,
            BIND_CONTROLLER_ONE,
            BIND_CONTROLLER_TWO,
        }

        public static void Update() {
            if (dllPresent) {
                //present = Available();

                uint numControllersBound = 0;
                uint numControllersEnabled = 0;
                SixensePlugin.sixenseControllerData cd = new SixensePlugin.sixenseControllerData();
                for (int i = 0; i < MAX_CONTROLLERS; i++) {
                    if (m_Controllers[i] != null) {
                        if (SixensePlugin.sixenseIsControllerEnabled(i) == 1) {
                            status = Status.Tracking;
                            SixensePlugin.sixenseGetNewestData(i, ref cd);
                            m_Controllers[i].Update(ref cd);
                            m_Controllers[i].m_Enabled = true;
                            numControllersEnabled++;
                            if (ControllerManagerEnabled && (m_Controllers[i].bodySide != Controller.BodySide.Unknown)) {
                                numControllersBound++;
                            }
                        } else {
                            m_Controllers[i].m_Enabled = false;
                        }
                    }
                }

                if (ControllerManagerEnabled) {
                    if (numControllersEnabled < 2) {
                        m_ControllerManagerState = ControllerManagerState.NONE;
                    }

                    switch (m_ControllerManagerState) {
                        case ControllerManagerState.NONE:
                            if (SixensePlugin.sixenseIsBaseConnected(0) != 0 && (numControllersEnabled > 1)) {
                                if (numControllersBound == 0) {
                                    m_ControllerManagerState = ControllerManagerState.BIND_CONTROLLER_ONE;
                                } else if (numControllersBound == 1) {
                                    m_ControllerManagerState = ControllerManagerState.BIND_CONTROLLER_TWO;
                                }
                            }
                            break;

                        case ControllerManagerState.BIND_CONTROLLER_ONE:
                            if (numControllersBound > 0) {
                                m_ControllerManagerState = ControllerManagerState.BIND_CONTROLLER_TWO;
                            } else {
                                for (int i = 0; i < MAX_CONTROLLERS; i++) {
                                    if ((m_Controllers[i] != null) && m_Controllers[i].GetButtonDown(HydraButtons.TRIGGER) && (m_Controllers[i].bodySide == Controller.BodySide.Unknown)) {
                                        m_Controllers[i].m_HandBind = Controller.BodySide.Left;
                                        SixensePlugin.sixenseAutoEnableHemisphereTracking(i);
                                        m_ControllerManagerState = ControllerManagerState.BIND_CONTROLLER_TWO;
                                        break;
                                    }
                                }
                            }
                            break;

                        case ControllerManagerState.BIND_CONTROLLER_TWO:
                            if (numControllersBound > 1) {
                                m_ControllerManagerState = ControllerManagerState.NONE;
                            } else {
                                for (int i = 0; i < MAX_CONTROLLERS; i++) {
                                    if ((m_Controllers[i] != null) && m_Controllers[i].GetButtonDown(HydraButtons.TRIGGER) && (m_Controllers[i].bodySide == Controller.BodySide.Unknown)) {
                                        m_Controllers[i].m_HandBind = Controller.BodySide.Right;
                                        SixensePlugin.sixenseAutoEnableHemisphereTracking(i);
                                        m_ControllerManagerState = ControllerManagerState.NONE;
                                        break;
                                    }
                                }
                            }
                            break;

                    }
                }
            }
            leftController = GetController(true);
            rightController = GetController(false);
        }

        public static Controller GetController(bool isLeft) {
            Controller.BodySide bodySide = isLeft ? Controller.BodySide.Left : Controller.BodySide.Right;

            for (int i = 0; i < MAX_CONTROLLERS; i++) {

                if ((m_Controllers[i] != null) && (m_Controllers[i].bodySide == bodySide)) {
                    return m_Controllers[i];
                }
            }

            return null;
        }

        public static Vector GetControllerLocalPosition(bool isLeft) {
            Controller controller = isLeft ? leftController : rightController;

            Vector localPosition = Vector.Scale(controller.Position, scale);
            return localPosition;
        }

        public static Rotation GetControllerLocalOrientation(bool isLeft) {
            Controller controller = isLeft ? leftController : rightController;

            Rotation localOrientation = controller.Rotation;
            localOrientation *= Rotation_.Euler(-30, 0, 0);
            return localOrientation;
        }

        public static Vector GetTargetLocalPosition(bool isLeft) {
            Controller controller = isLeft ? leftController : rightController;

            Vector localPosition = Vector.Scale(controller.Position, scale);
            localPosition += GetTargetLocalOrientation(isLeft) * (isLeft ? controller2LeftHand : controller2RightHand);
            return localPosition;
        }

        public static Rotation GetTargetLocalOrientation(bool isLeft) {
            Controller controller = isLeft ? leftController : rightController;

            Rotation localOrientation = controller.Rotation;
            localOrientation *= isLeft ? Rotation_.Euler(-90, 0, 90) : Rotation_.Euler(-90, 0, -90);
            return localOrientation;
        }

        public static float GetConfidence(Sensor.ID sensorID) {
            Controller controller;
            if (sensorID == Sensor.ID.LeftHand)
                controller = leftController;
            else if (sensorID == Sensor.ID.RightHand)
                controller = rightController;
            else
                return 0;

            if (controller == null || controller.m_Docked)
                return 0;

            return 0.8F;
        }

        public static float GetConfidence(bool isLeft) {
            Controller controller = isLeft ? leftController : rightController;
            if (controller != null) {
                if (!controller.m_Docked)
                    return 0.8F;
            }
            return 0;
        }


        public class Controller {
            public enum BodySide {
                Unknown = 0,
                Left = 1,
                Right = 2,
            };

            internal Controller() {
                m_Enabled = false;
                m_Docked = false;
                m_Hand = BodySide.Unknown;
                m_HandBind = BodySide.Unknown;
                m_Buttons = 0;
                m_ButtonsPrevious = 0;
                m_Trigger = 0.0f;
                m_JoystickX = 0.0f;
                m_JoystickY = 0.0f;
                m_Position = new Vector(0.0f, 0.0f, 0.0f);
                m_Rotation = new Rotation(0.0f, 0.0f, 0.0f, 1.0f);
            }

            internal void Update(ref SixensePlugin.sixenseControllerData cd) {
                m_Docked = (cd.is_docked != 0);
                m_Hand = (BodySide)cd.which_hand;
                m_ButtonsPrevious = m_Buttons;
                m_Buttons = (HydraDevice.HydraButtons)cd.buttons;
                m_Trigger = cd.trigger;
                m_JoystickX = cd.joystick_x;
                m_JoystickY = cd.joystick_y;
                m_Position = new Vector(cd.pos[0], cd.pos[1], cd.pos[2]);
                m_Rotation = new Rotation(cd.rot_quat[0], cd.rot_quat[1], cd.rot_quat[2], cd.rot_quat[3]);
                if (m_Trigger > m_TriggerButtonThreshold) {
                    m_Buttons |= HydraDevice.HydraButtons.TRIGGER;
                }
            }

            public BodySide bodySide { get { return ((m_Hand == BodySide.Unknown) ? m_HandBind : m_Hand); } }
            public Vector Position { get { return new Vector(m_Position.x, m_Position.y, -m_Position.z); } }
            public Rotation Rotation { get { return new Rotation(-m_Rotation.x, -m_Rotation.y, m_Rotation.z, m_Rotation.w); } }
            public bool GetButton(HydraDevice.HydraButtons button) {
                return ((button & m_Buttons) != 0);
            }
            public bool GetButtonDown(HydraDevice.HydraButtons button) {
                return ((button & m_Buttons) != 0) && ((button & m_ButtonsPrevious) == 0);
            }

            public bool m_Enabled;
            public bool m_Docked;
            private BodySide m_Hand;
            internal BodySide m_HandBind;
            private HydraDevice.HydraButtons m_Buttons;
            private HydraDevice.HydraButtons m_ButtonsPrevious;
            public float m_Trigger;
            public const float DefaultTriggerButtonThreshold = 0.9f;
            private float m_TriggerButtonThreshold = DefaultTriggerButtonThreshold;
            public float m_JoystickX;
            public float m_JoystickY;
            private Vector m_Position;
            private Rotation m_Rotation;
        }
    }
}