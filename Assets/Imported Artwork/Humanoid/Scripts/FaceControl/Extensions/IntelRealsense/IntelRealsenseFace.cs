/* Intel Realsense face sensor
 * copyright (c) 2017 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 1.1
 * date: August 16, 2017
 * 
 */

#if hREALSENSE && hFACE
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;
    using Humanoid.Tracking.Realsense;

    [System.Serializable]
    public class IntelRealsenseFace : UnityFaceSensor {
        private RealsenseTracker realsenseTracker;
        private RealsenseFace realsenseFace;

        public override string name {
            get { return RealsenseDevice.name; }
        }

        public bool faceTracking = false;
        public bool eyeTracking = false;

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            realsenseTracker = _humanoid.realsenseTracker;

            realsenseFace = new RealsenseFace(realsenseTracker.realsenseDevice, faceTracking, eyeTracking);
            SetFaceSensor(realsenseFace);
        }
        #endregion

        #region Update
        public override void Update() {
            if (!realsenseTracker.enabled || !enabled)
                return;

            status = realsenseFace.Update();
            if (status != Status.Tracking)
                return;

            if (faceTracking)
                UpdateFace();
            if (eyeTracking)
                UpdateEyes();
        }
        #endregion

        #region Face
        private void UpdateFace() {
            DrawFace();

            headTarget.smileValue = realsenseFace.smile;
            headTarget.puckerValue = realsenseFace.pucker;
            headTarget.frownValue = realsenseFace.frown;

            UpdateEyeBrows(faceTarget);
            UpdateEyeLids(realsenseFace);

            UpdateNose(faceTarget.nose);

            UpdateJaw(faceTarget.jaw);
            UpdateMouth(faceTarget.mouth);
        }

        private void DrawFace() {
            //Debug.DrawRay(headTarget.neck.target.transform.position + Target.ToVector3(RealsenseDevice.facePosition), Vector3.up, Color.red);
            Vector point;
            Vector3 referencePoint = headTarget.face.nose.tip.target.transform.position;
            for (int i = 0; i < RealsenseDevice.FacePointsCount(); i++) {
                if (RealsenseDevice.GetFacePoint(i, out point)) {
                    Vector3 facePoint = Target.ToVector3(point) + headTarget.head.target.transform.position;
                    Debug.DrawRay(facePoint, Vector3.forward * 0.01F);
                }
            }
        }

        #endregion

        #region Eyes
        private void UpdateEyes() {
            Debug.Log("A");
            if (headTarget.unityVRHead.camera == null)
                return;

            float eyesTurnLeft = RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.EyesTurnLeft);
            float eyesTurnRight = RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.EyesTurnRight);
            float eyesX = eyesTurnRight - eyesTurnLeft;
            float eyesUp = RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.EyesUp);
            float eyesDown = RealsenseDevice.GetFaceExpression(RealsenseDevice.FaceExpression.EyesDown);
            float eyesY = eyesDown - eyesUp;

            Vector3 lookDirection = headTarget.unityVRHead.camera.ViewportPointToRay(new Vector3(eyesX, eyesY, 2)).direction;
            headTarget.face.SetGazeDirection(lookDirection, 0.8F);
            Debug.Log("B");
        }
        #endregion
    }
}
#endif
