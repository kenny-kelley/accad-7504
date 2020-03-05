#if hKINECT2
using UnityEngine;
namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class Kinect2Head : UnityHeadSensor {
        public override string name {
            get { return NativeKinectDevice.name; }
        }

        public enum RotationTrackingAxis {
            XYZ,
            XY
        }

        private Kinect2Tracker kinectTracker;
        //private SensorBone headSensor;
        //private SensorBone neckSensor;

        public bool headTracking = true;
        public RotationTrackingAxis rotationTrackingAxis = RotationTrackingAxis.XY;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            kinectTracker = headTarget.humanoid.kinectTracker;
            tracker = kinectTracker;

            sensor = new KinectHead(headTarget.humanoid.kinectTracker.kinectDevice, headTracking);

            //if (kinectTracker.device == null)
            //    return;

            //headSensor = kinectTracker.device.GetBone(0, Bone.Head);
            //neckSensor = kinectTracker.device.GetBone(0, Bone.Neck);
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled) // || kinectTracker.device == null)
                return;

            status = sensor.Update();
            if (status != Status.Tracking)
                return;

            if (headTracking) {
                UpdateBones(sensor);

                //UpdateNeck(headTarget.neck.target);
                //UpdateHead(headTarget.head.target, headTarget.humanoid);
            }
        }

        #region Bones
        protected void UpdateBones(HeadSensor sensor) {
            if (headTarget.head.target.confidence.position > KinectDevice.TrackingConfidence(KinectDevice.JointID.Head)) {
                if (headTarget.humanoid.leftHandTarget.hand.target.confidence.position > KinectDevice.TrackingConfidence(KinectDevice.JointID.WristLeft) &&
                    headTarget.humanoid.rightHandTarget.hand.target.confidence.position > KinectDevice.TrackingConfidence(KinectDevice.JointID.WristRight))
                    CalibrateWithHeadAndHands();
                else
                    CalibrateWithHead();
                return;
            }

            //if (headTarget.head.target.confidence.rotation > 0) {
            //    headTarget.head.target.transform.rotation = Kinect2Tracker.SmoothRotation(headTarget.head.target.transform.rotation, sensor.head.rotation);
            //    headTarget.head.target.confidence.rotation = sensor.head.confidence.rotation;

            //    headTarget.head.target.transform.position = Kinect2Tracker.SmoothPosition(headTarget.head.target.transform.position, sensor.head.position);
            //    headTarget.head.target.confidence.position = sensor.head.confidence.position;
            //}

            //if (headTarget.neck.target.confidence.rotation > 0) { 
            //    headTarget.neck.target.transform.rotation = Kinect2Tracker.SmoothRotation(headTarget.neck.target.transform.rotation, sensor.neck.rotation);
            //    headTarget.neck.target.confidence.rotation = sensor.neck.confidence.rotation;

            //    headTarget.neck.target.transform.position = Kinect2Tracker.SmoothPosition(headTarget.neck.target.transform.position, sensor.neck.position);
            //    headTarget.neck.target.confidence.position = sensor.neck.confidence.position;
            //}
            else {
             
                if (headTarget.neck.target.transform != null) {
                    headTarget.neck.target.transform.position = Target.ToVector3(sensor.neck.position);
                    headTarget.neck.target.transform.rotation = Target.ToQuaternion(sensor.neck.rotation);
                    headTarget.neck.target.confidence = sensor.neck.confidence;
                }

                if (headTarget.head.target.transform != null) {
                    headTarget.head.target.transform.position = Target.ToVector3(sensor.head.position);
                    headTarget.head.target.transform.rotation = Target.ToQuaternion(sensor.head.rotation);
                    headTarget.head.target.confidence = sensor.head.confidence;
                }


                if (rotationTrackingAxis == RotationTrackingAxis.XY)
                    headTarget.head.target.transform.rotation = Quaternion.LookRotation(headTarget.head.target.transform.rotation * Vector3.forward);

            }
            if (headTarget.virtual3d) {
                UpdateVirtual3D();
            }
        }

        /*
        protected void UpdateHead(HumanoidTarget.TargetTransform headTarget, HumanoidControl humanoid) {

            //float confidence = kinectTracker.device.GetBoneConfidence(0, Bone.Head);
            float confidence = headSensor.positionConfidence;

            //if (headTarget.confidence.position > confidence) {
            //    if (humanoid.leftHandTarget.hand.target.confidence.position > kinectTracker.device.GetBoneConfidence(0, Side.Left, SideBone.Hand) && 
            //        humanoid.rightHandTarget.hand.target.confidence.position > kinectTracker.device.GetBoneConfidence(0, Side.Right, SideBone.Hand)) 
            //        CalibrateWithHeadAndHands();
            //    else
            //        CalibrateWithHead();
            //    return;
            //}

            if (confidence > 0) {
                headTarget.transform.position = headSensor.position;//kinectTracker.device.GetBonePosition(0, Bone.Head);
                headTarget.transform.rotation = headSensor.rotation;//  kinectTracker.device.GetBoneRotation(0, Bone.Head);
                headTarget.confidence.position = headSensor.positionConfidence;
                headTarget.confidence.rotation = headSensor.rotationConfidence;
            }
        }

        protected void UpdateNeck(HumanoidTarget.TargetTransform neckTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, Bone.Neck);
            //if (confidence > 0) {
            //    headTarget.head.target.transform.position = kinectTracker.device.GetBonePosition(0, Bone.Neck);
            //    headTarget.head.target.confidence.position = confidence;
            //}
            float confidence = neckSensor.positionConfidence;
            if (confidence > 0) {
                neckTarget.transform.position = neckSensor.position;
                neckTarget.confidence.position = confidence;
            }
        }
        */
        private void CalibrateWithHeadAndHands() {
            HumanoidControl humanoid = headTarget.humanoid;

            Vector3 trackingNormal = TrackingNormal(headTarget.head.target.transform.position, humanoid.leftHandTarget.transform.position, humanoid.rightHandTarget.transform.position);

            Vector3 kinectHeadPosition = Target.ToVector3(sensor.head.position);
            Vector3 kinectLeftHandPosition = Target.ToVector3(headTarget.humanoid.kinectTracker.kinectDevice.GetTargetPosition(KinectDevice.JointID.WristLeft)); // kinectTracker.device.GetBonePosition(0, Side.Left, SideBone.Hand);
            Vector3 kinectRightHandPosition = Target.ToVector3(headTarget.humanoid.kinectTracker.kinectDevice.GetTargetPosition(KinectDevice.JointID.WristRight));  //kinectTracker.device.GetBonePosition(0, Side.Right, SideBone.Hand);

            Vector3 kinectTrackingNormal = TrackingNormal(kinectHeadPosition, kinectLeftHandPosition, kinectRightHandPosition);

            Quaternion rotation = Quaternion.FromToRotation(kinectTrackingNormal, trackingNormal);
            float rotY = Angle.Normalize(rotation.eulerAngles.y);
            float rotX = Angle.Normalize(rotation.eulerAngles.x);

            tracker.trackerTransform.RotateAround(headTarget.head.target.transform.position, humanoid.up, rotY * 0.01F);
            tracker.trackerTransform.RotateAround(headTarget.head.target.transform.position, humanoid.transform.right, rotX * 0.01F);

            Vector3 delta = headTarget.head.target.transform.position - kinectHeadPosition;
            tracker.trackerTransform.transform.position += (delta * 0.01F);
        }

        private void CalibrateWithHead() {
            Vector3 kinectHeadPosition = Target.ToVector3(sensor.head.position);  //kinectTracker.device.GetBonePosition(0, Bone.Head);

            Vector3 delta = headTarget.head.target.transform.position - kinectHeadPosition;
            tracker.trackerTransform.transform.position += (delta * 0.01F);

            // Rotation calibration is not reliable enough and is therefore not implemented
            // Rotation needs to be set manually.
            // Kinect head rotation based on body tracking is not accurate enough
            // Kinect head rotation based on face tracking does not work with HMDs
        }

        private Vector3 TrackingNormal(Vector3 neckPosition, Vector3 leftHandPosition, Vector3 rightHandPosition) {
            Vector3 neck2leftHand = leftHandPosition - neckPosition;
            Vector3 neck2rightHand = rightHandPosition - neckPosition;

            Vector3 trackingNormal = Vector3.Cross(neck2leftHand, neck2rightHand);
            return trackingNormal;
        }

        #region Virtual3D
        private void UpdateVirtual3D() {
            Vector3 lookDirection = headTarget.screenTransform.position - headTarget.GetEyePosition();
            lookDirection = headTarget.screenTransform.forward;

            // We need to do this to prevent the camera position being influences by the neck rotation
            headTarget.neck.target.transform.rotation = Quaternion.LookRotation(lookDirection);

            headTarget.unityVRHead.cameraTransform.rotation = Quaternion.LookRotation(headTarget.screenTransform.forward);
            headTarget.neck.target.transform.position = new Vector3(headTarget.neck.target.transform.position.x, headTarget.neck.target.transform.position.y, headTarget.humanoid.transform.position.z);

            CalculateProjectionMatrix(headTarget.unityVRHead.camera, headTarget.screenTransform, headTarget.GetEyePosition());
        }

        private void CalculateProjectionMatrix(Camera camera, Transform screenTransform, Vector3 eyePosition) {
            float halfWidth = screenTransform.lossyScale.x / 2;
            float halfHeight = screenTransform.lossyScale.y / 2;

            Vector3 screenBottomleft = screenTransform.position + new Vector3(-halfWidth, -halfHeight, 0); //Bottom-Left
            Vector3 screenBottomRight = screenTransform.position + new Vector3(halfWidth, -halfHeight, 0);//Bottom-Right
            Vector3 screenTopLeft = screenTransform.position + new Vector3(-halfWidth, halfHeight, 0); //Top-Left
            Vector3 screeenTopRight = screenTransform.position + new Vector3(halfWidth, halfHeight, 0); //Top-Right

            Vector3 toBottomLeft = screenBottomleft - eyePosition;
            Vector3 toBottomRight = screenBottomRight - eyePosition;
            Vector3 toTopLeft = screenTopLeft - eyePosition;
            Vector3 toTopRight = screeenTopRight - eyePosition;

            float dEyeScreen = Vector3.Dot(toBottomLeft, screenTransform.forward); // distance from eye to screen
            float dLeft = Vector3.Dot(screenTransform.right, toBottomLeft) * camera.nearClipPlane / dEyeScreen; // distance to left screen edge from the 'center'
            float dRight = Vector3.Dot(screenTransform.right, toBottomRight) * camera.nearClipPlane / dEyeScreen; // distance to right screen edge from 'center'
            float dBottom = Vector3.Dot(screenTransform.up, toBottomLeft) * camera.nearClipPlane / dEyeScreen; // distance to bottom screen edge from 'center'
            float dTop = Vector3.Dot(screenTransform.up, toTopLeft) * camera.nearClipPlane / dEyeScreen; // distance to top screen edge from 'center'

            Matrix4x4 p = new Matrix4x4(); // Projection matrix
            p[0, 0] = 2.0f * camera.nearClipPlane / (dRight - dLeft);
            p[0, 2] = (dRight + dLeft) / (dRight - dLeft);
            p[1, 1] = 2.0f * camera.nearClipPlane / (dTop - dBottom);
            p[1, 2] = (dTop + dBottom) / (dTop - dBottom);
            p[2, 2] = (camera.farClipPlane + camera.nearClipPlane) / (camera.nearClipPlane - camera.farClipPlane);
            p[2, 3] = 2.0f * camera.farClipPlane * camera.nearClipPlane / (camera.nearClipPlane - camera.farClipPlane);
            p[3, 2] = -1.0f;

            Debug.DrawRay(eyePosition, toBottomLeft, Color.blue);
            Debug.DrawRay(eyePosition, toBottomRight, Color.blue);
            Debug.DrawRay(eyePosition, toTopLeft, Color.blue);
            Debug.DrawRay(eyePosition, toTopRight, Color.blue);

            camera.projectionMatrix = p;
        }
        #endregion

        #endregion

        #endregion
    }
}
#endif