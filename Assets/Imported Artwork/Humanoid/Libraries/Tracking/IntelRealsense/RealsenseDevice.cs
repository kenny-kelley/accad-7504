using System;

namespace Passer.Humanoid.Tracking.Realsense {
    public class RealsenseDeviceView : DeviceView {
    }

    public static class RealsenseDevice {
        public readonly static string name = "Intel RealSense";
        public static bool present;

        private readonly static Vector faceScale = new Vector(0.001F, 0.001F, 0.001F);

        //public static bool bodyTrackingEnabled = false; // does not work yet
        //private static bool handTrackingEnabled = false; // needs to be true at startup, can be disabled later on
        private static bool faceTrackingEnabled = true; // needs to be true at startup, can be disabled later on

        public static PXCMSenseManager senseManager;

        private static System.Threading.Thread thread;
        public static void Start() {
            // Create an instance of the SenseManager
            PXCMSession session = PXCMSession.CreateInstance();
            if (session == null) {
                present = false;
                return;
            }

            senseManager = session.CreateSenseManager();
            if (senseManager == null) {
                present = false;
                return;
            }

            //if (bodyTrackingEnabled)
            //    StartBodyTracking();
            //if (handTrackingEnabled)
            //    StartHandTracking();
            if (faceTrackingEnabled)
                StartFace();


            present = true;

            thread = new System.Threading.Thread(Fetcher);
            thread.Start();

        }

        private static bool fetching = true;
        private static void Fetcher() {
            // It is yet unclear why we need the handler here
            // But without it, we get no data
            PXCMSenseManager.Handler handler = new PXCMSenseManager.Handler();
            senseManager.Init(handler);    // Register the handler object

            while (fetching && senseManager.IsConnected()) {
                // This is a blocking operation
                // We need to do this is a separate thread
                if (senseManager.AcquireFrame(true) < pxcmStatus.PXCM_STATUS_NO_ERROR)
                    break;

                if (!senseManager.IsConnected())
                    continue;

                senseManager.ReleaseFrame();

                //if (bodyTrackingEnabled)
                //    UpdateBodyTracking();
                //if (handTrackingEnabled)
                //    UpdateHandTracking();
                if (faceTrackingEnabled)
                    UpdateFace();
            }

            if (senseManager != null) {
                //if (handTrackingEnabled)
                //    StopHandTracking();
                if (faceTrackingEnabled)
                    StopFaceTracking();
                senseManager.Close();
                senseManager = null;
            }
        }

        public static void Update() {
            //senseManager.FlushFrame();
            //senseManager.AcquireFrame(false, 0);

            ////if (bodyTrackingEnabled)
            ////    UpdateBodyTracking();
            ////if (handTrackingEnabled)
            ////    UpdateHandTracking();
            //if (faceTrackingEnabled)
            //    UpdateFace();

            //// This is a blocking operation
            //// We need to do this is a separate thread
            //senseManager.ReleaseFrame();
        }

        public static void Stop() {
            fetching = false;
            // Wait for a second to ensure that the fetching thread is finished before quitting the application
            System.Threading.Thread.Sleep(1000);
            if (senseManager != null) {
                // if the fetching is still not terminated, abort it
                thread.Abort();
            }
            //if (senseManager != null) {
            //    //if (handTrackingEnabled)
            //    //    StopHandTracking();
            //    if (faceTrackingEnabled)
            //        StopFaceTracking();
            //    senseManager.Close();
            //    senseManager = null;
            //}
        }
        #region BodyTracking
        /*
        public static PXCMPersonTrackingData bodyData;
        private static PXCMPersonTrackingData.PersonJoints.SkeletonPoint[] joints;

        private static void StartBodyTracking() {
            pxcmStatus status = senseManager.EnablePersonTracking();
            PXCMPersonTrackingModule bodyModule = senseManager.QueryPersonTracking();
            if (status != pxcmStatus.PXCM_STATUS_NO_ERROR || bodyModule == null) {
                return;
            }

            PXCMPersonTrackingConfiguration ptc = bodyModule.QueryConfiguration();
            ptc.SetTrackedAngles(PXCMPersonTrackingConfiguration.TrackingAngles.TRACKING_ANGLES_FRONTAL);

            bodyData = bodyModule.QueryOutput(); // CreateOutput();
        }

        private static void UpdateBodyTracking() {
            if (bodyData == null)
                return;

            PXCMPersonTrackingData.Person person = bodyData.QueryPersonData(PXCMPersonTrackingData.AccessOrderType.ACCESS_ORDER_BY_ID, 0);
            PXCMPersonTrackingData.PersonJoints personJoints = person.QuerySkeletonJoints();

            int njoints = personJoints.QueryNumJoints();

            joints = new PXCMPersonTrackingData.PersonJoints.SkeletonPoint[njoints];

            personJoints.QueryJoints(joints);
        }
        
        public static float GetBodyJointConfidence(PXCMPersonTrackingData.PersonJoints.JointType jointID) {
            if (joints == null)
                return 0;

            PXCMPersonTrackingData.PersonJoints.SkeletonPoint joint = joints[(int) jointID];
            if (joint == null)
                return 0;

            return joint.confidenceWorld / 100;
        }
        
        public static Vector GetBodyTargetLocalPosition(PXCMPersonTrackingData.PersonJoints.JointType jointID) {
            if (joints == null)
                return Vector.zero;

            PXCMPersonTrackingData.PersonJoints.SkeletonPoint joint = joints[(int) jointID];
            if (joint == null)
                return Vector.zero;

            return new Vector(
                joint.world.x,
                joint.world.y,
                joint.world.z
                );
        }              
        #endregion

        #region HandTracking
        public static PXCMHandData handData;
        private static PXCMHandData.IHand leftIhand;
        private static PXCMHandData.IHand rightIhand;

        private static PXCMHandData.JointType[][] realSenseBones = new PXCMHandData.JointType[][] {
            new PXCMHandData.JointType[] { PXCMHandData.JointType.JOINT_THUMB_BASE, PXCMHandData.JointType.JOINT_THUMB_JT1, PXCMHandData.JointType.JOINT_THUMB_JT2, PXCMHandData.JointType.JOINT_THUMB_TIP },
            new PXCMHandData.JointType[] { PXCMHandData.JointType.JOINT_INDEX_BASE, PXCMHandData.JointType.JOINT_INDEX_JT1, PXCMHandData.JointType.JOINT_INDEX_JT2, PXCMHandData.JointType.JOINT_INDEX_TIP },
            new PXCMHandData.JointType[] { PXCMHandData.JointType.JOINT_MIDDLE_BASE, PXCMHandData.JointType.JOINT_MIDDLE_JT1, PXCMHandData.JointType.JOINT_MIDDLE_JT2, PXCMHandData.JointType.JOINT_MIDDLE_TIP },
            new PXCMHandData.JointType[] { PXCMHandData.JointType.JOINT_RING_BASE, PXCMHandData.JointType.JOINT_RING_JT1, PXCMHandData.JointType.JOINT_RING_JT2, PXCMHandData.JointType.JOINT_RING_TIP },
            new PXCMHandData.JointType[] { PXCMHandData.JointType.JOINT_PINKY_BASE, PXCMHandData.JointType.JOINT_PINKY_JT1, PXCMHandData.JointType.JOINT_PINKY_JT2, PXCMHandData.JointType.JOINT_PINKY_TIP }
        };

        private static void StartHandTracking() {
            pxcmStatus status = senseManager.EnableHand();
            PXCMHandModule handModule = senseManager.QueryHand();
            if (status != pxcmStatus.PXCM_STATUS_NO_ERROR || handModule == null) {
                return;
            }

            PXCMHandConfiguration handConfig = handModule.CreateActiveConfiguration();
            handConfig.SetTrackingMode(PXCMHandData.TrackingModeType.TRACKING_MODE_FULL_HAND);
            handConfig.ApplyChanges();

            // Create the hand data instance
            handData = handModule.CreateOutput();
        }

        private static void UpdateHandTracking() {
            if (handData == null)
                return;

            handData.Update();

            UpdateLeftHandTracking();
            UpdateRightHandTracking();
        }

        private static void UpdateLeftHandTracking() {
            int handId;
            handData.QueryHandId(PXCMHandData.AccessOrderType.ACCESS_ORDER_LEFT_HANDS, 0, out handId);
            handData.QueryHandDataById(handId, out leftIhand);
            if (leftIhand != null && leftIhand.QueryBodySide() != PXCMHandData.BodySideType.BODY_SIDE_LEFT)
                leftIhand = null;
        }
        private static void UpdateRightHandTracking() {
            int handId;
            handData.QueryHandId(PXCMHandData.AccessOrderType.ACCESS_ORDER_RIGHT_HANDS, 0, out handId);
            handData.QueryHandDataById(handId, out rightIhand);
            if (rightIhand != null && rightIhand.QueryBodySide() != PXCMHandData.BodySideType.BODY_SIDE_RIGHT)
                rightIhand = null;
        }

        private static void StopHandTracking() {
            if (handData == null)
                return;

            handData.Dispose();
        }
        
        public static float GetHandTargetConfidence(bool isLeft) {
            PXCMHandData.IHand ihand = isLeft ? leftIhand : rightIhand;
            if (ihand == null)
                return 0;
            else
                return 0.8F;
        }
        
        public static Vector GetHandTargetLocalPosition(PXCMHandData.JointType jointType, bool isLeft) {
            PXCMHandData.IHand ihand = isLeft ? leftIhand : rightIhand;
            if (ihand == null)
                return Vector.zero;

            PXCMHandData.JointData jointData;
            ihand.QueryTrackedJoint(jointType, out jointData);

            Vector handPosition = new Vector(
                jointData.positionWorld.x,
                jointData.positionWorld.y,
                jointData.positionWorld.z
                );
            return handPosition;
        }
        

        public static Rotation GetHandTargetLocalOrientation(PXCMHandData.JointType jointType, bool isLeft) {
            PXCMHandData.IHand ihand = isLeft ? leftIhand : rightIhand;
            if (ihand == null)
                return Rotation.identity;

            PXCMHandData.JointData jointData;
            ihand.QueryTrackedJoint(jointType, out jointData);

            Rotation rotation = new Rotation(
                jointData.globalOrientation.x,
                jointData.globalOrientation.y,
                jointData.globalOrientation.z,
                jointData.globalOrientation.w
                );
            Vector eulerAngles = Rotation.ToAngles(rotation);
            rotation = Rotation_.Euler(-eulerAngles.x, eulerAngles.y, eulerAngles.z);

            if (isLeft)
                rotation *= Rotation_.Euler(0, 270, 90);
            else
                rotation *= Rotation_.Euler(0, 90, 270);
            return rotation;
        }
        
        public static Rotation GetFingerTargetLocalOrientation(bool isLeft, int fingerIndex, FingerBones boneID) {
            PXCMHandData.JointType jointType;
            switch (boneID) {
                case FingerBones.Proximal:
                    jointType = realSenseBones[fingerIndex][1];
                    break;
                case FingerBones.Intermediate:
                    jointType = realSenseBones[fingerIndex][2];
                    break;
                case FingerBones.Distal:
                    jointType = realSenseBones[fingerIndex][3];
                    break;
                default:
                    return Rotation.identity;
            }
            return GetHandTargetLocalOrientation(jointType, isLeft);
        }
        */
        #endregion

        #region FaceTracking
        private static PXCMFaceData faceData;
        private static PXCMFaceData.ExpressionsData expressionData;
        private static PXCMFaceData.LandmarksData landmarksData;
        private static PXCMFaceData.PoseData poseData;

        private const int maxFacePoints = 80; //currently limited to 76 face points (max nr. facepoint at time of writing)
        private static Vector[] faceGeometry = new Vector[maxFacePoints];
        //private static Vector[] lastFacePointPositions = new Vector[maxFacePoints];

        public static Vector facePosition;
        public static Rotation faceOrientation;

        #region Calibration
        private static Vector[] faceBias = {
            new Vector(0.016F, -0.009F, 0.000F),            // left outer brow
            new Vector(-0.006F, -0.017F, 0.000F),           // left brow
            new Vector(-0.025F, -0.009F, 0.000F),           // left inner brow

            new Vector(0.025F, -0.009F, 0.000F),            // right inner brow
            new Vector(0.006F, -0.017F, 0.000F),            // right brow
            new Vector(-0.016F,-0.009F, 0.000F),            // right outer brow

            new Vector(0, 0, 0),                            // left cheek
            new Vector(0, 0, 0),                            // right cheek

            new Vector(0, 0, 0),                            // nose top left
            new Vector(0, 0, 0),                            // nose top
            new Vector(0, 0, 0),                            // nose top right

            new Vector(0, 0, 0),                            // nose tip(reference point)

            new Vector(0, 0, 0),                            // nose bottom left
            new Vector(0.050F, -0.003F, 0),                 // nose bottom
            new Vector(0, 0, 0),                            // nose bottom right

            new Vector(0.034F, 0.068F, 0.000F),             // upper lip left
            new Vector(0.025F, 0.068F, 0.000F),             // upper lip
            new Vector(0.018F, 0.068F, 0.000F),             // upper lip right

            new Vector(0.042F, 0.068F, 0.000F),             // lip left
            new Vector(0.010F, 0.068F, 0.000F),             // lip right

            new Vector(0.034F, 0.068F, 0.000F),             // lower lip left
            new Vector(0.025F, 0.068F, 0.000F),             // lower lip
            new Vector(0.018F, 0.068F, 0.000F),             // lower lip right
        };

        public static void Calibrate() {
            CalibrateEyeBrows();
            CalibrateNose();
            CalibrateMouth();
        }

        private static void CalibrateEyeBrows() {
            Vector leftEyePositon = GetFacePoint(76);
            for (int i = (int)FaceBone.LeftOuterBrow; i <= (int)FaceBone.LeftInnerBrow; i++) {
                faceBias[i] = leftEyePositon - GetFacePointRaw((FaceBone)i);
            }

            Vector rightEyePosition = GetFacePoint(77);
            for (int i = (int)FaceBone.RightInnerBrow; i <= (int)FaceBone.RightOuterBrow; i++) {
                faceBias[i] = rightEyePosition - GetFacePointRaw((FaceBone)i);
            }
        }

        private static void CalibrateNose() {
            Vector referencePoint = GetFacePoint(facePoints[(int)FaceBone.NoseTip]);

            for (int i = (int)FaceBone.NoseTopLeft; i <= (int)FaceBone.NoseBottomRight; i++) {
                faceBias[i] = referencePoint - GetFacePointRaw((FaceBone)i);
            }
        }

        private static void CalibrateMouth() {
            Vector referencePoint = GetFacePoint(facePoints[(int)FaceBone.NoseTip]);

            for (int i = (int)FaceBone.UpperLipLeft; i < (int)FaceBone.LastBone; i++)
                faceBias[i] = referencePoint - GetFacePointRaw((FaceBone)i);
        }
        #endregion

        #region FacePointMapping
        private static int[] facePoints = {
            0,
            2,
            4,

            9,
            7,
            5,

            -1,
            -1,

            -1,
            (int)PXCMFaceData.LandmarkType.LANDMARK_NOSE_TOP,
            -1,
            (int)PXCMFaceData.LandmarkType.LANDMARK_NOSE_TIP,
            (int)PXCMFaceData.LandmarkType.LANDMARK_NOSE_LEFT,
            (int)PXCMFaceData.LandmarkType.LANDMARK_NOSE_BOTTOM,
            (int)PXCMFaceData.LandmarkType.LANDMARK_NOSE_RIGHT,

            48,
            47,
            46,

            49,
            45,

            50,
            51,
            52,
        };
        #endregion

        #region Start
        private static void StartFace() {
            pxcmStatus status = senseManager.EnableFace();
            PXCMFaceModule faceModule = senseManager.QueryFace();
            if (status != pxcmStatus.PXCM_STATUS_NO_ERROR || faceModule == null) {
                return;
            }

            PXCMFaceConfiguration faceConfig = faceModule.CreateActiveConfiguration();
            if (faceConfig == null) {                
                return;
            }

            faceConfig.SetTrackingMode(PXCMFaceConfiguration.TrackingModeType.FACE_MODE_COLOR_PLUS_DEPTH);
            faceConfig.strategy = PXCMFaceConfiguration.TrackingStrategyType.STRATEGY_RIGHT_TO_LEFT;

            PXCMFaceConfiguration.ExpressionsConfiguration econfiguration = faceConfig.QueryExpressions();
            if (econfiguration == null) {
                return;
            }
            econfiguration.properties.maxTrackedFaces = 1;
            econfiguration.EnableAllExpressions();
            econfiguration.Enable();
            faceConfig.ApplyChanges();

            faceConfig.pose.isEnabled = true;
            faceConfig.landmarks.isEnabled = true;

            faceData = faceModule.CreateOutput();
        }
        #endregion

        #region Update
        private static void UpdateFace() {
            landmarksData = null;
            if (faceData == null)
                return;

            faceData.Update();

            PXCMFaceData.Face face = faceData.QueryFaceByIndex(0);
            if (face != null) {
                landmarksData = face.QueryLandmarks();
                expressionData = face.QueryExpressions();
                poseData = face.QueryPose();

                UpdateFaceOrientation(poseData);
                UpdateFacePosition(poseData);
                UpdateFaceGeometry(landmarksData);
            }
        }


        private static void UpdateFacePosition(PXCMFaceData.PoseData poseData) {
            if (poseData == null)
                return;

            PXCMFaceData.HeadPosition posePosition;
            if (!poseData.QueryHeadPosition(out posePosition))
                return;

            Vector position = new Vector(
                posePosition.headCenter.x * faceScale.x,
                posePosition.headCenter.y * faceScale.y,
                posePosition.headCenter.z * faceScale.z
                );
            facePosition = position + faceOrientation * new Vector(0, -0.13F, 0.05F);
        }

        private static Rotation UpdateFaceOrientation(PXCMFaceData.PoseData poseData) {
            if (poseData == null)
                return Rotation.identity;

            PXCMFaceData.PoseEulerAngles poseAngles;
            if (!poseData.QueryPoseAngles(out poseAngles))
                return Rotation.identity;

            Rotation orientation = Rotation_.Euler(-poseAngles.pitch, poseAngles.yaw, -poseAngles.roll);
            faceOrientation = orientation;
            return orientation;
        }

        private static void UpdateFaceGeometry(PXCMFaceData.LandmarksData landmarksData) {
            if (landmarksData == null)
                return;

            for (int i = 0; i < landmarksData.QueryNumPoints(); i++) {
                PXCMFaceData.LandmarkPoint outPoint;
                if (landmarksData.QueryPoint(i, out outPoint))
                    faceGeometry[i] = new Vector(-outPoint.world.x, outPoint.world.y, -outPoint.world.z);
            }
        }
        #endregion

        #region Stop
        private static void StopFaceTracking() {
            if (faceData == null)
                return;

            faceData.Dispose();
        }
        #endregion

        public static bool FaceTrackingDataAvailable() {
            return (landmarksData != null);
        }

        #region Geometry
        public static int FacePointsCount() {
            if (landmarksData == null)
                return 0;

            return landmarksData.QueryNumPoints();

        }
        public static int FacePointName(PXCMFaceData.LandmarkType landmarkID) {
            if (landmarksData == null)
                return 0;

            return landmarksData.QueryPointIndex(landmarkID);
        }

        public static bool GetFacePoint(FaceBone faceBone, out Vector position) {
            position = GetFacePointRaw(faceBone) + faceBias[(int)faceBone];
            return true;
        }

        public static Vector GetFacePointRaw(FaceBone faceBone) {
            int facePoint = facePoints[(int)faceBone];
            if (facePoint < 0)
                return Vector.zero;

            Vector facePointPosition = GetFacePoint(facePoint);
            return facePointPosition;
        }

        public static Vector GetFacePoint(int facePoint) {
            Rotation orientation = GetFaceLocalOrientation();
            Vector facePointPosition = Rotation.Inverse(orientation) * faceGeometry[facePoint];
            return facePointPosition;
        }

        public static bool GetFacePoint(int facePoint, out Vector position) {
            position = GetFacePoint(facePoint);
            return true;
        }

        public static Rotation GetFaceLocalOrientation() {
            return faceOrientation;
        }
        
        public static Vector GetFaceLocalPosition() {            
            return facePosition;
        }
        
        public static float GetFaceTargetConfidence() {
            if (poseData == null)
                return 0;

            PXCMFaceData.HeadPosition headPosition;
            if (!poseData.QueryHeadPosition(out headPosition))
                return 0;

            return (float)headPosition.confidence / 100;
        }
        #endregion

        #region Expressions
        public enum FaceExpression {
            BrowRaiserLeft = 0,
            BrowRaiserRight = 1,
            BrowLowererLeft = 2,
            BrowLowererRight = 3,
            Smile = 4,
            Kiss = 5,
            MouthOpen = 6,
            EyeClosedLeft = 7,
            EyeClosedRight = 8,
            HeadTurnLeft = 9,
            HeadTurnRight = 10,
            HeadUp = 11,
            HeadDown = 12,
            HeadTiltLeft = 13,
            HeadTiltRight = 14,
            EyesTurnLeft = 15,
            EyesTurnRight = 16,
            EyesUp = 17,
            EyesDown = 18,
            TongueOut = 19,
            RightCheekPuff = 20,
            LeftCheekPuff = 21
        }

        public static float GetFaceExpression(FaceExpression expressionID) {
            if (expressionData == null)
                return 0;

            PXCMFaceData.ExpressionsData.FaceExpressionResult score;
            expressionData.QueryExpression((PXCMFaceData.ExpressionsData.FaceExpression)expressionID, out score);

            return ((float)score.intensity) / 100;
        }
        #endregion
        #endregion
    }
}