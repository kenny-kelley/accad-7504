#if hKINECT2
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class Kinect2Foot : UnityLegSensor {
        public override string name {
            get { return NativeKinectDevice.name; }
        }

        private Kinect2Tracker kinectTracker;
        private KinectLeg kinectLeg;

        //private SensorBone upperLegSensor;
        //private SensorBone lowerLegSensor;
        //private SensorBone footSensor;

        #region Start
        public override void Start(HumanoidControl humanoid, Transform targetTransform) {
            base.Start(humanoid, targetTransform);
            kinectTracker = footTarget.humanoid.kinectTracker;
            tracker = kinectTracker;

            sensor = new KinectLeg(footTarget.isLeft, footTarget.humanoid.kinectTracker.kinectDevice);
            kinectLeg = (KinectLeg)sensor;

            //if (kinectTracker.device == null)
            //    return;

            //upperLegSensor = kinectTracker.device.GetBone(0, footTarget.side, SideBone.UpperLeg);
            //lowerLegSensor = kinectTracker.device.GetBone(0, footTarget.side, SideBone.LowerLeg);
            //footSensor = kinectTracker.device.GetBone(0, footTarget.side, SideBone.Foot);
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            status = kinectLeg.Update(Target.ToRotation(footTarget.humanoid.hipsTarget.transform.rotation));
            if (status != Status.Tracking)
                return;

            UpdateUpperLeg(sensor);
            UpdateLowerLeg(sensor);
            UpdateFoot(sensor);

            //    UpdateUpperLeg(footTarget.upperLeg.target);
            //    UpdateLowerLeg(footTarget.lowerLeg.target);
            //    UpdateFoot(footTarget.foot.target);
        }


        protected Vector3 lastUpperLegPosition;
        protected Quaternion lastUpperLegRotation;
        protected override void UpdateUpperLeg(LegSensor legSensor) {
            legSensor.upperLeg.position = Target.ToVector(Kinect2Tracker.SmoothPosition(lastUpperLegPosition, legSensor.upperLeg.position));
            legSensor.upperLeg.rotation = Target.ToRotation(Kinect2Tracker.SmoothRotation(lastUpperLegRotation, legSensor.upperLeg.rotation));
            base.UpdateUpperLeg(legSensor);

            lastUpperLegPosition = footTarget.upperLeg.target.transform.position;
            lastUpperLegRotation = footTarget.upperLeg.target.transform.rotation;
        }

        protected Vector3 lastLowerLegPosition;
        protected Quaternion lastLowerLegRotation;
        protected override void UpdateLowerLeg(LegSensor legSensor) {
            legSensor.lowerLeg.position = Target.ToVector(Kinect2Tracker.SmoothPosition(lastLowerLegPosition, legSensor.lowerLeg.position));
            legSensor.lowerLeg.rotation = Target.ToRotation(Kinect2Tracker.SmoothRotation(lastLowerLegRotation, legSensor.lowerLeg.rotation));
            base.UpdateLowerLeg(legSensor);

            lastLowerLegPosition = footTarget.lowerLeg.target.transform.position;
            lastLowerLegRotation = footTarget.lowerLeg.target.transform.rotation;
        }

        protected Vector3 lastFootPosition;
        protected Quaternion lastFootRotation;
        protected override void UpdateFoot(LegSensor legSensor) {
            legSensor.foot.position = Target.ToVector(Kinect2Tracker.SmoothPosition(lastFootPosition, legSensor.foot.position));
            legSensor.foot.rotation = Target.ToRotation(Kinect2Tracker.SmoothRotation(lastFootRotation, legSensor.foot.rotation));
            base.UpdateFoot(legSensor);

            lastFootPosition = footTarget.foot.target.transform.position;
            lastFootRotation = footTarget.foot.target.transform.rotation;
        }
        /*
        protected void UpdateUpperLeg(HumanoidTarget.TargetTransform upperLegTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, footTarget.side, SideBone.UpperLeg);
            //if (confidence > 0) {
            //    footTarget.upperLeg.target.transform.position = kinectTracker.device.GetBonePosition(0, footTarget.side, SideBone.UpperLeg);
            //    footTarget.upperLeg.target.confidence.position = confidence;
            //}
            float confidence = upperLegSensor.positionConfidence;
            if (confidence > 0) {
                upperLegTarget.transform.position = upperLegSensor.position;
                upperLegTarget.confidence.position = confidence;
            }
        }

        protected void UpdateLowerLeg(HumanoidTarget.TargetTransform lowerLegTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, footTarget.side, SideBone.LowerLeg);
            //if (confidence > 0) {
            //    footTarget.lowerLeg.target.transform.position = kinectTracker.device.GetBonePosition(0, footTarget.side, SideBone.LowerLeg);
            //    footTarget.lowerLeg.target.confidence.position = confidence;
            //}
            float confidence = lowerLegSensor.positionConfidence;
            if (confidence > 0) {
                lowerLegTarget.transform.position = lowerLegSensor.position;
                lowerLegTarget.confidence.position = confidence;
            }
        }

        protected void UpdateFoot(HumanoidTarget.TargetTransform footTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, footTarget.side, SideBone.Foot);
            //if (confidence > 0) {
            //    footTarget.foot.target.transform.position = kinectTracker.device.GetBonePosition(0, footTarget.side, SideBone.Foot);
            //    footTarget.foot.target.confidence.position = confidence;
            //}
            float confidence = footSensor.positionConfidence;
            if (confidence > 0) {
                footTarget.transform.position = footSensor.position;
                footTarget.confidence.position = confidence;
            }
        }
        */
        #endregion
    }
}
#endif