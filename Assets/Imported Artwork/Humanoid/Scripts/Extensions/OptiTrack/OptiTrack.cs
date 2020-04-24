#if hOPTITRACK

using System.Collections.Generic;
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OptiTracker : Tracker {
        public OptiTracker() {
            deviceView = new DeviceView();
        }

        public override string name {
            //get { return NativeOptitrackDevice.name; }
            get { return "OptiTrack"; }
        }

        public string localAddress = "127.0.0.1";
        public string serverAddress = "127.0.0.1";
        public int serverCommandPort = 1510;
        public int serverDataPort = 1511;

        public override UnityHeadSensor headSensor {
            get { return humanoid.headTarget.optitrack; }
        }
        public override UnityArmSensor leftHandSensor {
            get { return humanoid.leftHandTarget.optitrack; }
        }
        public override UnityArmSensor rightHandSensor {
            get { return humanoid.rightHandTarget.optitrack; }
        }
        public override UnityTorsoSensor hipsSensor {
            get { return humanoid.hipsTarget.optitrack; }
        }
        public override UnityLegSensor leftFootSensor {
            get { return humanoid.leftFootTarget.optitrack; }
        }
        public override UnityLegSensor rightFootSensor {
            get { return humanoid.rightFootTarget.optitrack; }
        }


        public OptitrackStreamingClient streamingClient;
        //public NativeOptitrackDevice device;

        public enum TrackingType {
            Skeleton,
            Rigidbody
        }
        public TrackingType trackingType = TrackingType.Skeleton;
        public string skeletonName = null;

        private OptitrackSkeletonDefinition m_skeletonDef;

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (!enabled)
                return;

            //if (device == null) {
            //    device = new NativeOptitrackDevice();
            //    device.Init(localAddress, serverAddress, serverCommandPort, serverDataPort);
            //}

            if (trackingType == TrackingType.Skeleton) {
                CacheHumanBodyBonesMap(streamingClient.BoneNamingConvention, skeletonName);

                m_skeletonDef = streamingClient.GetSkeletonDefinitionByName(skeletonName);
            }
        }
        #endregion

        private readonly Vector3[] bonePositions = new Vector3[(int)Bone.Count];
        private readonly Quaternion[] boneRotations = new Quaternion[(int)Bone.Count];

        #region Update
        public override void UpdateTracker() {
            base.UpdateTracker();

            status = Status.Unavailable;

            if (!enabled || streamingClient == null)
                return;

            status = Status.Present;

            deviceView.position = Target.ToVector(trackerTransform.position);
            deviceView.orientation = Target.ToRotation(trackerTransform.rotation);

            if (trackingType == TrackingType.Skeleton && m_skeletonDef != null) {
                OptitrackSkeletonState skelState = streamingClient.GetLatestSkeletonState(m_skeletonDef.Id);
                if (skelState == null)
                    return;

                status = Status.Tracking;


                // Update the transforms of the bone GameObjects.
                for (int i = 0; i < m_skeletonDef.Bones.Count; ++i) {
                    int optitrackBoneId = m_skeletonDef.Bones[i].Id;

                    OptitrackPose bonePose;
                    Bone boneId;

                    bool foundPose = skelState.BonePoses.TryGetValue(optitrackBoneId, out bonePose);
                    if (foundPose) {
                        bool foundBone = optitrackBoneMapping.TryGetValue(m_skeletonDef.Bones[i].Name, out boneId);
                        if (foundBone) {
                            bonePositions[(int)boneId] = bonePose.Position;
                            boneRotations[(int)boneId] = bonePose.Orientation;
                        }
                    }
                }
            }
        }
        #endregion

        public override void StopTracker() {
            //if (device == null)
            //    return;
            //device.Stop();
        }

        public Quaternion GetTargetRotation(Bone boneId) {
            if (boneRotations == null || (int)boneId > boneRotations.Length)
                return Quaternion.identity;

            return trackerTransform.rotation * boneRotations[(int)boneId];
        }

        public Vector3 GetTargetPosition(Bone boneId) {
            if (bonePositions == null || (int)boneId > bonePositions.Length)
                return Vector3.zero;

            return trackerTransform.position + trackerTransform.rotation * bonePositions[(int)boneId];
        }

        public override void AdjustTracking(Vector3 translation, Quaternion rotation) {
            if (!enabled)
                return;

            trackerTransform.position += translation;
            trackerTransform.rotation *= rotation;

            //device.SetPosition(trackerTransform.position);
            //device.SetRotation(trackerTransform.rotation);
        }

        private readonly Dictionary<string, Bone> optitrackBoneMapping = new Dictionary<string, Bone>();
        private void CacheHumanBodyBonesMap(OptitrackBoneNameConvention convention, string assetName) {
            optitrackBoneMapping.Clear();

            switch (convention) {
                case OptitrackBoneNameConvention.Motive:
                    optitrackBoneMapping.Add(assetName + "_Hip", Bone.Hips);
                    optitrackBoneMapping.Add(assetName + "_Ab", Bone.Spine);
                    optitrackBoneMapping.Add(assetName + "_Chest", Bone.Chest);
                    optitrackBoneMapping.Add(assetName + "_Neck", Bone.Neck);
                    optitrackBoneMapping.Add(assetName + "_Head", Bone.Head);
                    optitrackBoneMapping.Add(assetName + "_LShoulder", Bone.LeftShoulder);
                    optitrackBoneMapping.Add(assetName + "_LUArm", Bone.LeftUpperArm);
                    optitrackBoneMapping.Add(assetName + "_LFArm", Bone.LeftForearm);
                    optitrackBoneMapping.Add(assetName + "_LHand", Bone.LeftHand);
                    optitrackBoneMapping.Add(assetName + "_RShoulder", Bone.RightShoulder);
                    optitrackBoneMapping.Add(assetName + "_RUArm", Bone.RightUpperArm);
                    optitrackBoneMapping.Add(assetName + "_RFArm", Bone.RightForearm);
                    optitrackBoneMapping.Add(assetName + "_RHand", Bone.RightHand);
                    optitrackBoneMapping.Add(assetName + "_LThigh", Bone.LeftUpperLeg);
                    optitrackBoneMapping.Add(assetName + "_LShin", Bone.LeftLowerLeg);
                    optitrackBoneMapping.Add(assetName + "_LFoot", Bone.LeftFoot);
                    optitrackBoneMapping.Add(assetName + "_RThigh", Bone.RightUpperLeg);
                    optitrackBoneMapping.Add(assetName + "_RShin", Bone.RightLowerLeg);
                    optitrackBoneMapping.Add(assetName + "_RFoot", Bone.RightFoot);
                    optitrackBoneMapping.Add(assetName + "_LToe", Bone.LeftToes);
                    optitrackBoneMapping.Add(assetName + "_RToe", Bone.RightToes);

                    //optitrackBoneMapping.Add(assetName + "_LThumb1", HumanoidBones.LeftHandThumb1);
                    //optitrackBoneMapping.Add(assetName + "_LThumb2", HumanoidBones.LeftHandThumb2);
                    //optitrackBoneMapping.Add(assetName + "_LThumb3", HumanoidBones.LeftHandThumb3);
                    //optitrackBoneMapping.Add(assetName + "_RThumb1", HumanoidBones.RightHandThumb1);
                    //optitrackBoneMapping.Add(assetName + "_RThumb2", HumanoidBones.RightHandThumb2);
                    //optitrackBoneMapping.Add(assetName + "_RThumb3", HumanoidBones.RightHandThumb3);

                    //optitrackBoneMapping.Add(assetName + "_LIndex1", HumanoidBones.LeftHandIndex1);
                    //optitrackBoneMapping.Add(assetName + "_LIndex2", HumanoidBones.LeftHandIndex2);
                    //optitrackBoneMapping.Add(assetName + "_LIndex3", HumanoidBones.LeftHandIndex3);
                    //optitrackBoneMapping.Add(assetName + "_RIndex1", HumanoidBones.RightHandIndex1);
                    //optitrackBoneMapping.Add(assetName + "_RIndex2", HumanoidBones.RightHandIndex2);
                    //optitrackBoneMapping.Add(assetName + "_RIndex3", HumanoidBones.RightHandIndex3);

                    //optitrackBoneMapping.Add(assetName + "_LMiddle1", HumanoidBones.LeftHandMiddle1);
                    //optitrackBoneMapping.Add(assetName + "_LMiddle2", HumanoidBones.LeftHandMiddle2);
                    //optitrackBoneMapping.Add(assetName + "_LMiddle3", HumanoidBones.LeftHandMiddle3);
                    //optitrackBoneMapping.Add(assetName + "_RMiddle1", HumanoidBones.RightHandMiddle1);
                    //optitrackBoneMapping.Add(assetName + "_RMiddle2", HumanoidBones.RightHandMiddle2);
                    //optitrackBoneMapping.Add(assetName + "_RMiddle3", HumanoidBones.RightHandMiddle3);

                    //optitrackBoneMapping.Add(assetName + "_LRing1", HumanoidBones.LeftHandRing1);
                    //optitrackBoneMapping.Add(assetName + "_LRing2", HumanoidBones.LeftHandRing2);
                    //optitrackBoneMapping.Add(assetName + "_LRing3", HumanoidBones.LeftHandRing3);
                    //optitrackBoneMapping.Add(assetName + "_RRing1", HumanoidBones.RightHandRing1);
                    //optitrackBoneMapping.Add(assetName + "_RRing2", HumanoidBones.RightHandRing2);
                    //optitrackBoneMapping.Add(assetName + "_RRing3", HumanoidBones.RightHandRing3);

                    //optitrackBoneMapping.Add(assetName + "_LPinky1", HumanoidBones.LeftHandPinky1);
                    //optitrackBoneMapping.Add(assetName + "_LPinky2", HumanoidBones.LeftHandPinky2);
                    //optitrackBoneMapping.Add(assetName + "_LPinky3", HumanoidBones.LeftHandPinky3);
                    //optitrackBoneMapping.Add(assetName + "_RPinky1", HumanoidBones.RightHandPinky1);
                    //optitrackBoneMapping.Add(assetName + "_RPinky2", HumanoidBones.RightHandPinky2);
                    //optitrackBoneMapping.Add(assetName + "_RPinky3", HumanoidBones.RightHandPinky3);
                    break;
                    //case OptitrackBoneNameConvention.FBX:
                    //    m_cachedMecanimBoneNameMap.Add("Hips", assetName + "_Hips");
                    //    m_cachedMecanimBoneNameMap.Add("Spine", assetName + "_Spine");
                    //    m_cachedMecanimBoneNameMap.Add("Chest", assetName + "_Spine1");
                    //    m_cachedMecanimBoneNameMap.Add("Neck", assetName + "_Neck");
                    //    m_cachedMecanimBoneNameMap.Add("Head", assetName + "_Head");
                    //    m_cachedMecanimBoneNameMap.Add("LeftShoulder", assetName + "_LeftShoulder");
                    //    m_cachedMecanimBoneNameMap.Add("LeftUpperArm", assetName + "_LeftArm");
                    //    m_cachedMecanimBoneNameMap.Add("LeftLowerArm", assetName + "_LeftForeArm");
                    //    m_cachedMecanimBoneNameMap.Add("LeftHand", assetName + "_LeftHand");
                    //    m_cachedMecanimBoneNameMap.Add("RightShoulder", assetName + "_RightShoulder");
                    //    m_cachedMecanimBoneNameMap.Add("RightUpperArm", assetName + "_RightArm");
                    //    m_cachedMecanimBoneNameMap.Add("RightLowerArm", assetName + "_RightForeArm");
                    //    m_cachedMecanimBoneNameMap.Add("RightHand", assetName + "_RightHand");
                    //    m_cachedMecanimBoneNameMap.Add("LeftUpperLeg", assetName + "_LeftUpLeg");
                    //    m_cachedMecanimBoneNameMap.Add("LeftLowerLeg", assetName + "_LeftLeg");
                    //    m_cachedMecanimBoneNameMap.Add("LeftFoot", assetName + "_LeftFoot");
                    //    m_cachedMecanimBoneNameMap.Add("RightUpperLeg", assetName + "_RightUpLeg");
                    //    m_cachedMecanimBoneNameMap.Add("RightLowerLeg", assetName + "_RightLeg");
                    //    m_cachedMecanimBoneNameMap.Add("RightFoot", assetName + "_RightFoot");
                    //    m_cachedMecanimBoneNameMap.Add("LeftToeBase", assetName + "_LeftToeBase");
                    //    m_cachedMecanimBoneNameMap.Add("RightToeBase", assetName + "_RightToeBase");
                    //    break;
                    //case OptitrackBoneNameConvention.BVH:
                    //    m_cachedMecanimBoneNameMap.Add("Hips", assetName + "_Hips");
                    //    m_cachedMecanimBoneNameMap.Add("Spine", assetName + "_Chest");
                    //    m_cachedMecanimBoneNameMap.Add("Chest", assetName + "_Chest2");
                    //    m_cachedMecanimBoneNameMap.Add("Neck", assetName + "_Neck");
                    //    m_cachedMecanimBoneNameMap.Add("Head", assetName + "_Head");
                    //    m_cachedMecanimBoneNameMap.Add("LeftShoulder", assetName + "_LeftCollar");
                    //    m_cachedMecanimBoneNameMap.Add("LeftUpperArm", assetName + "_LeftShoulder");
                    //    m_cachedMecanimBoneNameMap.Add("LeftLowerArm", assetName + "_LeftElbow");
                    //    m_cachedMecanimBoneNameMap.Add("LeftHand", assetName + "_LeftWrist");
                    //    m_cachedMecanimBoneNameMap.Add("RightShoulder", assetName + "_RightCollar");
                    //    m_cachedMecanimBoneNameMap.Add("RightUpperArm", assetName + "_RightShoulder");
                    //    m_cachedMecanimBoneNameMap.Add("RightLowerArm", assetName + "_RightElbow");
                    //    m_cachedMecanimBoneNameMap.Add("RightHand", assetName + "_RightWrist");
                    //    m_cachedMecanimBoneNameMap.Add("LeftUpperLeg", assetName + "_LeftHip");
                    //    m_cachedMecanimBoneNameMap.Add("LeftLowerLeg", assetName + "_LeftKnee");
                    //    m_cachedMecanimBoneNameMap.Add("LeftFoot", assetName + "_LeftAnkle");
                    //    m_cachedMecanimBoneNameMap.Add("RightUpperLeg", assetName + "_RightHip");
                    //    m_cachedMecanimBoneNameMap.Add("RightLowerLeg", assetName + "_RightKnee");
                    //    m_cachedMecanimBoneNameMap.Add("RightFoot", assetName + "_RightAnkle");
                    //    m_cachedMecanimBoneNameMap.Add("LeftToeBase", assetName + "_LeftToe");
                    //    m_cachedMecanimBoneNameMap.Add("RightToeBase", assetName + "_RightToe");
                    //    break;
            }
        }

    }

}

#endif