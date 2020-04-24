#if hKINECT2
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class Kinect2Torso : UnityTorsoSensor {
        public override string name {
            get { return NativeKinectDevice.name; }
        }

        private Kinect2Tracker kinectTracker;
        //private SensorBone hipsSensor;
        //private SensorBone spineSensor;
        //private SensorBone chestSensor;


        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            kinectTracker = hipsTarget.humanoid.kinectTracker;
            tracker = kinectTracker;

            sensor = new KinectTorso(hipsTarget.humanoid.kinectTracker.kinectDevice);

            //if (kinectTracker.device == null)
            //    return;

            //hipsSensor = kinectTracker.device.GetBone(0, Bone.Hips);
            //spineSensor = kinectTracker.device.GetBone(0, Bone.Spine);
            //chestSensor = kinectTracker.device.GetBone(0, Bone.Chest);
        }

        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            sensor.Update();
            if (status != Status.Tracking)
                return;

            UpdateHips(sensor);
            UpdateChest(sensor);

            //UpdateHips(hipsTarget.hips.target);
            //UpdateSpine(hipsTarget.spine.target);
            //UpdateChest(hipsTarget.chest.target);
        }

        protected Vector3 lastHipsPosition;
        protected Quaternion lastHipsRotation;
        protected void UpdateHips(TorsoSensor torsoSensor) {
            if (hipsTarget.hips.target.transform != null) {
                torsoSensor.hips.position = Target.ToVector(Kinect2Tracker.SmoothPosition(lastHipsPosition, torsoSensor.hips.position));
                torsoSensor.hips.rotation = Target.ToRotation(Kinect2Tracker.SmoothRotation(lastHipsRotation, torsoSensor.hips.rotation));

                hipsTarget.hips.target.transform.position = Target.ToVector3(torsoSensor.hips.position);
                hipsTarget.hips.target.transform.rotation = Target.ToQuaternion(torsoSensor.hips.rotation);
                hipsTarget.hips.target.confidence = torsoSensor.hips.confidence;

                lastHipsPosition = hipsTarget.hips.target.transform.position;
                lastHipsRotation = hipsTarget.hips.target.transform.rotation;
            }
        }

        protected Vector3 lastChestPosition;
        protected Quaternion lastChestRotation;
        protected void UpdateChest(TorsoSensor torsoSensor) {
            if (hipsTarget.chest.target.transform != null) {
                torsoSensor.chest.position = Target.ToVector(Kinect2Tracker.SmoothPosition(lastChestPosition, torsoSensor.chest.position));
                torsoSensor.chest.rotation = Target.ToRotation(Kinect2Tracker.SmoothRotation(lastChestRotation, torsoSensor.chest.rotation));

                hipsTarget.chest.target.transform.rotation = Target.ToQuaternion(torsoSensor.chest.rotation);
                hipsTarget.chest.target.confidence = torsoSensor.chest.confidence;

                lastChestPosition = hipsTarget.chest.target.transform.position;
                lastChestRotation = hipsTarget.chest.target.transform.rotation;
            }
        }

        /*
        protected void UpdateHips(HumanoidTarget.TargetTransform hipsTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, Bone.Hips);
            //if (confidence > 0) {
            //    hipsTarget.hips.target.transform.position = kinectTracker.device.GetBonePosition(0, Bone.Hips);
            //    hipsTarget.hips.target.confidence.position = confidence;
            //}
            float confidence = hipsSensor.positionConfidence;
            if (confidence > 0) {
                hipsTarget.transform.position = hipsSensor.position;
                hipsTarget.confidence.position = confidence;
            }
        }

        protected void UpdateSpine(HumanoidTarget.TargetTransform spineTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, Bone.Spine);
            //if (confidence > 0) {
            //    hipsTarget.spine.target.transform.position = kinectTracker.device.GetBonePosition(0, Bone.Spine);
            //    hipsTarget.spine.target.confidence.position = confidence;
            //}
            float confidence = spineSensor.positionConfidence;
            if (confidence > 0) {
                spineTarget.transform.position = spineSensor.position;
                spineTarget.confidence.position = confidence;
            }
        }

        protected void UpdateChest(HumanoidTarget.TargetTransform chestTarget) {
            //float confidence = kinectTracker.device.GetBoneConfidence(0, Bone.Chest);
            //if (confidence > 0) {
            //    hipsTarget.chest.target.transform.position = kinectTracker.device.GetBonePosition(0, Bone.Chest);
            //    hipsTarget.chest.target.confidence.position = confidence;
            //}
            float confidence = chestSensor.positionConfidence;
            if (confidence > 0) {
                chestTarget.transform.position = chestSensor.position;
                chestTarget.confidence.position = confidence;
            }
        }
        */
    }
}
#endif