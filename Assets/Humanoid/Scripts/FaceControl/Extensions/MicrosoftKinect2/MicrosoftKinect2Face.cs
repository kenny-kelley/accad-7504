#define USE_LIBRARY

#if hKINECT2 && hFACE
using UnityEngine;
using Microsoft.Kinect.Face;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class Kinect2Face : UnityFaceSensor {
        private Kinect2Tracker kinectTracker;
        private KinectFace kinectFace;

        public override string name {
            get { return NativeKinectDevice.name; }
        }

        public bool faceTracking = false;
        public bool audioInput = true;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            kinectTracker = headTarget.humanoid.kinectTracker;

            kinectFace = new KinectFace(kinectTracker.kinectDevice, faceTracking, audioInput);
            SetFaceSensor(kinectFace);
        }
        #endregion

        #region Update
        public override void Update() {
            if (!kinectTracker.enabled || !enabled)
                return;

            status = kinectFace.Update();
            if (status != Status.Tracking)
                return;

            if (faceTracking)
                UpdateFace();
            if (audioInput)
                UpdateAudio();
        }
        #endregion

        #region Face
        private void UpdateFace() {
            if (!KinectDevice.faceIsTracked)
                return;

            //#if DEBUG
            //DrawFace();
            //#endif

            UpdateEyeBrows(faceTarget);
            UpdateEyelids(faceTarget);

            UpdateCheeks(faceTarget);

            UpdateNose(faceTarget.nose);

            UpdateJaw(faceTarget.jaw);
            UpdateMouth(faceTarget.mouth);

            faceTarget.headTarget.smileValue = kinectFace.smile;
        }

        private void DrawFace() {
            for (int i = 0; i < FaceModel.VertexCount; i++) {
                Vector3 point = Target.ToVector3(KinectDevice.GetNeckLocalFacePoint(i));
                Debug.DrawRay(headTarget.neck.target.transform.position + headTarget.neck.target.transform.rotation * point, Vector3.forward * 0.01F);
            }
        }

        private void UpdateEyelids(FaceTarget face) {
            face.leftEye.closed = Mathf.Clamp01(KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.EyeClosedLeft) * 4 - 2.5F);
            face.rightEye.closed = Mathf.Clamp01(KinectDevice.GetFaceExpression(KinectDevice.FaceExpression.EyeClosedRight) * 4 - 2.5F);
        }
                #endregion

        #region Audio
        private void UpdateAudio() {
            //headTarget.audioEnergy = KinectDevice.GetAudioEnergy();
        }
        #endregion

    }
}
#endif