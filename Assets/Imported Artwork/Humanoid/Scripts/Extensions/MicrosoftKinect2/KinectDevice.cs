#if hKINECT2
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Passer.Humanoid.Tracking {
    
    public class KinectDeviceView : DeviceView {
        public Vector GetTargetPosition(KinectDevice.JointID boneType) {
            return ToWorldPosition(KinectDevice.GetTargetLocalPosition(boneType));
        }
    }

    public static class KinectDevice {
        public const string name = "Microsoft Kinect 2";

        public static Status status;
        private static bool started;

        private static bool bodyTrackingEnabled = true;
#if hFACE
        private static bool faceTrackingEnabled = true;
#else
        private static bool faceTrackingEnabled = false;
#endif
        private static bool audioTrackingEnabled = true;

        private static KinectSensor sensor;

        #region events
        public static event OnPlayerAppears onPlayerAppears;
        public delegate void OnPlayerAppears(int bodyID);

        public static event OnPlayerDisappears onPlayerDisappears;
        public delegate void OnPlayerDisappears(int bodyID);
        #endregion

        public static void Start() {
            status = Status.Unavailable;

            if (started || sensor != null)
                return;

            started = true;
            sensor = KinectSensor.GetDefault();
            if (sensor == null)
                return;

            if (sensor.IsAvailable)
                status = Status.Present;

            if (bodyTrackingEnabled)
                StartBody();
            if (faceTrackingEnabled)
                StartFace();
            if (audioTrackingEnabled)
                StartAudio();

            if (!sensor.IsOpen) {
                sensor.Open();
            }
        }

        public static void Update() {
            if (sensor != null && sensor.IsAvailable)
                status = Status.Present;

            if (bodyTrackingEnabled)
                UpdateBody();
            if (faceTrackingEnabled)
                UpdateFace();
            if (audioTrackingEnabled)
                UpdateAudio();
        }

        public static void Stop() {
            try {
                if (reader != null) {
                    reader.Dispose();
                    reader = null;
                }

                if (sensor != null) {
                    if (sensor.IsOpen) {
                        sensor.Close();
                    }

                    sensor = null;
                }
            }
            catch (Exception ex) {
                UnityEngine.Debug.LogError(ex.Message);
            }
        }

        #region BodyTracking
        public enum JointID {
            HipCenter = 0,
            SpineMid = 1,
            Neck = 2,
            Head = 3,
            ShoulderLeft = 4,
            ElbowLeft = 5,
            WristLeft = 6,
            HandLeft = 7,
            ShoulderRight = 8,
            ElbowRight = 9,
            WristRight = 10,
            HandRight = 11,
            HipLeft = 12,
            KneeLeft = 13,
            AnkleLeft = 14,
            FootLeft = 15,
            HipRight = 16,
            KneeRight = 17,
            AnkleRight = 18,
            FootRight = 19,
            SpineShoulder = 20,
            HandTipLeft = 21,
            ThumbLeft = 22,
            HandTipRight = 23,
            ThumbRight = 24
        }

        private static Body bodyData;
        private static BodyFrameReader reader;

        private static Body[] data = null;
        private static int bodyID = -1;

        private static void StartBody() {
            if (sensor != null) {
                reader = sensor.BodyFrameSource.OpenReader();
            }
        }

        private static void UpdateBody() {
            if (reader == null)
                return;

            BodyFrame frame = reader.AcquireLatestFrame();
            if (frame == null) {
                return;
            }

            if (data == null)
                data = new Body[sensor.BodyFrameSource.BodyCount];

            frame.GetAndRefreshBodyData(data);
            for (int bodyNr = 0; bodyNr < data.Length; bodyNr++) {
                if (data[bodyNr] == null)
                    continue;

                if (data[bodyNr].IsTracked) {
                    status = Status.Tracking;
                    if (bodyID == -1) {
                        bodyID = bodyNr;
#if hFACE
                        if (FaceFrameSource != null)
                            FaceFrameSource.TrackingId = data[bodyID].TrackingId;
#endif
                        if (onPlayerAppears != null)
                            onPlayerAppears(bodyID);
                    }
                }
                else {
                    if (bodyNr == bodyID) {
                        if (onPlayerDisappears != null)
                            onPlayerDisappears(bodyID);
                        bodyID = -1;
                        status = Status.Present;

#if hFACE
                        if (FaceFrameSource != null)
                            FaceFrameSource.TrackingId = 0;
#endif
                    }
                }
            }

            if (bodyID > -1)
                bodyData = data[bodyID];
            else
                bodyData = null;

            frame.Dispose();
        }


        public static Vector GetTargetLocalPosition(JointID boneType) {
            if (bodyData == null)
                return Vector.zero;

            Vector localPosition = new Vector(
                    -bodyData.Joints[(JointType)boneType].Position.X,
                    bodyData.Joints[(JointType)boneType].Position.Y,
                    bodyData.Joints[(JointType)boneType].Position.Z
                );
            return localPosition;
        }

        public static Vector Target2WorldPosition(Vector localPosition) {
            //return position + orientation 
            return Rotation.AngleAxis(180, Vector.up) * localPosition;
        }

        public static Vector GetTargetPosition(JointID boneType) {
            return Rotation.AngleAxis(180, Vector.up) * GetTargetLocalPosition(boneType);
        }

        public static bool JointIsTracked(JointID boneIndex) {
            if (bodyData == null)
                return false;

            return (bodyData.Joints[(JointType)boneIndex].TrackingState != TrackingState.NotTracked);
        }

        public static float TrackingConfidence(JointID boneIndex) {
            if (bodyData == null)
                return 0;

            switch (bodyData.Joints[(JointType)boneIndex].TrackingState) {
                case TrackingState.Tracked:
                    return 0.8F;
                case TrackingState.Inferred:
                    return 0.6F;
                case TrackingState.NotTracked:
                default:
                    return 0;
            }
        }

        public static float TrackingConfidence(Vector predictedPosition, Vector measuredPosition) {
            float distance = 0; // Vector.Distance(predictedPosition, measuredPosition)
            float confidence = 0.9F - distance * 10;
            confidence = confidence < 0 ? 0 : confidence;
            confidence = confidence > 1 ? 1 : confidence;
            return confidence;
        }

        public static HandState GetLeftHandState() {
            if (bodyData == null)
                return HandState.NotTracked;

            return bodyData.HandLeftState;
        }

        public static HandState GetRightHandState() {
            if (bodyData == null)
                return HandState.NotTracked;

            return bodyData.HandRightState;
        }
        #endregion

        #region FaceTracking
        private static HighDefinitionFaceFrameSource FaceFrameSource;
        private static HighDefinitionFaceFrameReader faceReader;
        private static FaceModel faceModel;
        private static FaceAlignment faceAlignment = null;
        private static Vector[] faceGeometry;

        private static bool _faceIsTracked = false;
        public static bool faceIsTracked {
            get { return _faceIsTracked; }
        }

        #region Calibration
        // This ought to be store for each player's face individually

        private static Vector[] faceBias = {
            new Vector(0.027F, -0.012F, 0.011F),            // left outer brow
            new Vector(0.005F, -0.020F, -0.007F),           // left brow
            new Vector(-0.018F, -0.012F, -0.013F),          // left inner brow

            new Vector(0.018F, -0.012F, -0.013F),           // right inner brow
            new Vector(-0.005F, -0.020F, -0.007F),          // right brow
            new Vector(-0.027F, -0.012F, 0.011F),           // right outer brow

            new Vector(0.057F, -0.025F, 0.032F),            // left cheek
            new Vector(-0.057F, -0.025F, 0.032F),           // right cheek

            new Vector(0.012F, -0.044F, 0.017F),            // nose top left
            new Vector(0, -0.051F, 0.007F),                 // nose top
            new Vector(-0.012F, -0.044F, 0.017F),           // nose top right

            new Vector(0, -0.015F, -0.013F),                // nose tip

            new Vector(0.017F, -0.003F, 0.009F),            // nose bottom left
            new Vector(0, 0, 0),                            // nose bottom
            new Vector(-0.017F, -0.003F, 0.009F),           // nose bottom right

            new Vector(0, 0.005F + 0.02F, 0.000F),          // upper lip left
            new Vector(0, 0.004F + 0.02F, -0.005F),         // upper lip
            new Vector(-0.007F, 0.004F + 0.02F, 0.001F),    // upper lip right

            new Vector(0.023F, 0.006F + 0.02F, 0.005F),     // lip left
            new Vector(-0.023F, 0.006F + 0.02F, 0.005F),    // lip right

            new Vector(0, 0.006F + 0.02F, -0.001F),         // lower lip left
            new Vector(0, 0.005F + 0.02F, -0.005F),         // lower lip
            new Vector(-0.002F, 0.006F + 0.02F, -0.001F),   // lower lip right
        };

        public static void Calibrate() {
#if hFACE
            if (faceIsTracked) {
                CalibrateEyeBrows();
                CalibrateCheeks();
                CalibrateNose();
                CalibrateMouth();
            }
#endif
        }

        private static void CalibrateEyeBrows() {
            Vector leftEyePositon = KinectDevice.GetFacePoint((int)HighDetailFacePoints.LefteyeMidbottom);

            for (int i = (int)FaceBone.LeftOuterBrow; i <= (int)FaceBone.LeftInnerBrow; i++)
                faceBias[i] = leftEyePositon - GetFacePointRaw((FaceBone)i);

            Vector rightEyePosition = KinectDevice.GetFacePoint((int)HighDetailFacePoints.RighteyeMidbottom);

            for (int i = (int)FaceBone.RightInnerBrow; i <= (int)FaceBone.RightOuterBrow; i++)
                faceBias[i] = rightEyePosition - GetFacePointRaw((FaceBone)i);

        }

        private static void CalibrateCheeks() {
            Vector referencePoint = GetFacePoint((int)HighDetailFacePoints.NoseBottom);

            for (int i = (int)FaceBone.LeftCheek; i <= (int)FaceBone.RightCheek; i++)
                faceBias[i] = referencePoint - GetFacePointRaw((FaceBone)i);
        }

        private static void CalibrateNose() {
            Vector referencePoint = GetFacePoint((int)HighDetailFacePoints.NoseBottom);

            for (int i = (int)FaceBone.NoseTopLeft; i <= (int)FaceBone.NoseBottomRight; i++)
                faceBias[i] = referencePoint - GetFacePointRaw((FaceBone)i);
        }

        private static void CalibrateMouth() {
            Vector referencePoint = GetFacePoint((int)HighDetailFacePoints.NoseBottom);

            for (int i = (int)FaceBone.UpperLipLeft; i <= (int)FaceBone.LowerLipRight; i++)
                faceBias[i] = referencePoint - GetFacePointRaw((FaceBone)i);
        }

        #endregion

        #region FacePointMapping

        public enum FaceBone {
            LeftOuterBrow,
            LeftBrow,
            LeftInnerBrow,
            RightInnerBrow,
            RightBrow,
            RightOuterBrow,

            LeftCheek,
            RightCheek,

            NoseTopLeft,
            NoseTop,
            NoseTopRight,
            NoseTip,
            NoseBottomLeft,
            NoseBottom,
            NoseBottomRight,

            UpperLipLeft,
            UpperLip,
            UpperLipRight,
            LipLeft,
            LipRight,
            LowerLipLeft,
            LowerLip,
            LowerLipRight,
        }

        private static int[] faceBoneIndex = {
            (int)HighDetailFacePoints.LefteyebrowOuter,
            (int)HighDetailFacePoints.LefteyebrowCenter,
            (int)HighDetailFacePoints.LefteyebrowInner,

            (int)HighDetailFacePoints.RighteyebrowInner,
            (int)HighDetailFacePoints.RighteyebrowCenter,
            (int)HighDetailFacePoints.RighteyebrowOuter,

            (int)HighDetailFacePoints.Leftcheekbone,
            (int)HighDetailFacePoints.Rightcheekbone,

            (int)HighDetailFacePoints.NoseTopleft,
            (int)HighDetailFacePoints.NoseTop,
            (int)HighDetailFacePoints.NoseTopright,
            (int)HighDetailFacePoints.NoseTip,
            (int)HighDetailFacePoints.NoseBottomleft,
            (int)HighDetailFacePoints.NoseBottom,
            (int)HighDetailFacePoints.NoseBottomright,

            1132,
            (int) HighDetailFacePoints.MouthUpperlipMidbottom,
            1146,
            (int)HighDetailFacePoints.MouthLeftcorner,
            (int)HighDetailFacePoints.MouthRightcorner,
            1133,
            (int)HighDetailFacePoints.MouthLowerlipMidtop,
            1149,
        };
        #endregion

        private static void StartFace() {
            FaceFrameSource = HighDefinitionFaceFrameSource.Create(sensor);
            if (FaceFrameSource != null) {
                faceReader = FaceFrameSource.OpenReader();
                faceModel = FaceModel.Create();
                faceAlignment = FaceAlignment.Create();
                faceGeometry = new Vector[FaceModel.VertexCount];
            }
        }

        private static void UpdateFace() {
            if (faceReader != null) {
                HighDefinitionFaceFrame frame = faceReader.AcquireLatestFrame();
                if (frame != null && frame.IsFaceTracked) {
                    _faceIsTracked = true;
                    frame.GetAndRefreshFaceAlignmentResult(faceAlignment);

                    IList<CameraSpacePoint> vertices = faceModel.CalculateVerticesForAlignment(faceAlignment);
                    for (int i = 0; i < FaceModel.VertexCount; i++)
                        faceGeometry[i] = new Vector(vertices[i].X, vertices[i].Y, -vertices[i].Z);
                }
            }
            else {
                _faceIsTracked = false;
            }
        }

        public enum FaceExpression : int {
            JawOpen = 0,
            LipPucker = 1,
            JawSlideRight = 2,
            LipStretcherRight = 3,
            LipStretcherLeft = 4,
            LipCornerPullerLeft = 5,
            LipCornerPullerRight = 6,
            LipCornerDepressorLeft = 7,
            LipCornerDepressorRight = 8,
            LeftCheekPuff = 9,
            RightCheekPuff = 10,
            EyeClosedLeft = 11,
            EyeClosedRight = 12,
            BrowLowererRight = 13,
            BrowLowererLeft = 14,
            LowerlipDepressorLeft = 15,
            LowerlipDepressorRight = 16,
        }

        public static float GetFaceExpression(FaceExpression expressionID) {
            if (faceAlignment == null)
                return 0;

            return faceAlignment.AnimationUnits[(Microsoft.Kinect.Face.FaceShapeAnimations)expressionID];
        }

        private static Vector GetFacePointRaw(FaceBone faceBone) {
            int facePoint = faceBoneIndex[(int)faceBone];
            if (facePoint < 0)
                return Vector.zero;

            Rotation orientation = GetFaceOrientationRaw();
            Vector facePointPosition = Rotation.Inverse(orientation) * faceGeometry[facePoint];
            return facePointPosition;
        }

        public static Vector GetFacePoint(FaceBone faceBone) {
            Rotation orientation = GetFaceOrientationRaw();
            int facePoint = faceBoneIndex[(int)faceBone];
            Vector facePointPosition = Rotation.Inverse(orientation) * faceGeometry[facePoint] + faceBias[(int)faceBone];
            return facePointPosition;
        }

        public static Vector GetFacePoint(int facePoint) {
            Rotation orientation = GetFaceOrientationRaw();
            Vector facePointPosition = Rotation.Inverse(orientation) * faceGeometry[facePoint];
            return facePointPosition;
        }

        public static Vector GetFaceWorldPoint(FaceBone facePoint) {
            return ToWorldPosition(GetFacePoint(facePoint));
        }

        public static Vector GetNeckLocalFacePoint(int facePoint) {
            Rotation faceOrientation = Rotation.AngleAxis(180, Vector.up) * GetFaceOrientation();

            Vector faceTargetPosition = ToWorldPosition(GetFacePosition());
            Vector worldFacePoint = ToWorldPosition(GetFacePoint(facePoint));
            Vector neckLocalFacePoint = worldFacePoint + faceTargetPosition;

            neckLocalFacePoint = Rotation.Inverse(faceOrientation) * neckLocalFacePoint;
            return neckLocalFacePoint;
        }

        public static Rotation GetFacePointOrientation(FaceBone facePoint) {
            Rotation faceOrientation = GetFaceOrientation();
            Vector facePosition = GetFacePosition();
            Vector worldFacePoint = GetFaceWorldPoint(facePoint);
            Vector neckLocalFacePoint = worldFacePoint + facePosition;

            Rotation facePointOrientation = Rotation_.FromToRotation(Vector.forward, neckLocalFacePoint);
            facePointOrientation = Rotation.Inverse(faceOrientation) * facePointOrientation;

            return facePointOrientation;
        }

        public static Vector GetFacePosition() {
            Vector facePosition = new Vector(
                -faceAlignment.HeadPivotPoint.X,
                -faceAlignment.HeadPivotPoint.Y,
                faceAlignment.HeadPivotPoint.Z);
            return facePosition;
        }

        public static Rotation GetFaceOrientation() {
            Rotation faceOrientation = new Rotation(
                faceAlignment.FaceOrientation.X,
                faceAlignment.FaceOrientation.Y,
                faceAlignment.FaceOrientation.Z,
                faceAlignment.FaceOrientation.W);
            Vector faceAngles = Rotation.ToAngles(Rotation.AngleAxis(180, Vector.up) * faceOrientation);
            Rotation newOrientation = Rotation_.Euler(-faceAngles.x, -faceAngles.y, faceAngles.z);
            return newOrientation;
        }

        public static Rotation GetFaceOrientationRaw() {
            if (faceAlignment == null)
                return Rotation.identity;

            Rotation faceOrientation = new Rotation(
                faceAlignment.FaceOrientation.X,
                faceAlignment.FaceOrientation.Y,
                faceAlignment.FaceOrientation.Z,
                faceAlignment.FaceOrientation.W);
            Vector faceAngles = Rotation.ToAngles(faceOrientation);
            faceOrientation = Rotation_.Euler(-faceAngles.x, -faceAngles.y, faceAngles.z);
            return faceOrientation;
        }

        #endregion

        public static Rotation GetHeadOrientation() {
            if (faceIsTracked)
                return GetFaceOrientation();
            else {
                Vector neckPos = GetTargetPosition(JointID.Neck);
                Vector headPos = GetTargetPosition(JointID.Head);

                Vector direction = headPos - neckPos;
                direction = Rotation.AngleAxis(180, Vector.up) * direction;
                Rotation neckRotation = Rotation.LookRotation(direction, Vector.forward);
                neckRotation *= Rotation.AngleAxis(90, Vector.right);
                return neckRotation;
            }
        }

        #region AudioTracking

        private static AudioBeamFrameReader audioReader;
        private static AudioBeamFrameReference audioFrameReference;
        private static bool audioFrameAvailable;
        private static AudioBeam audioBeam;
        private static float beamAngle;
        private static byte[] audioBuffer = null;

        private static void StartAudio() {
            Windows.Kinect.KinectAudioSource audioSource = sensor.AudioSource;
            if (audioSource != null) {
                audioReader = audioSource.OpenReader();
                //audioReader.FrameArrived += Reader_FrameArrived;

                audioBuffer = new byte[audioSource.SubFrameLengthInBytes];
            }
        }

        //private static void Reader_FrameArrived(object sender, AudioBeamFrameArrivedEventArgs e) {
        //}

        private static void UpdateAudio() {
            audioFrameAvailable = false;
            if (audioReader != null) {
                IList<AudioBeamFrame> frames = audioReader.AcquireLatestBeamFrames();
                for (int i = 0; i < frames.Count; i++) {
                    AudioBeamFrame frame = frames[i];
                    if (frame != null) {
                        audioFrameAvailable = true;
                        IList<AudioBeamSubFrame> subframes = frame.SubFrames;

                        for (int j = 0; j < subframes.Count; j++) {
                            AudioBeamSubFrame subframe = subframes[j];
                            if (subframe != null) {
                                beamAngle = subframe.BeamAngle;
                                ProcessAudio(subframe);
                                subframe.Dispose();
                            }
                        }
                        frame.Dispose();
                    }
                }
            }
        }

        public static bool GetNewAudioFrame() {
            return (audioFrameAvailable);
        }

        public static float GetBeamAngle() {
            return (beamAngle * (180 / (float)Math.PI));
        }

        public static float GetAudioEnergy() {
            return (1 + (energyAmount / 100));
        }

        private const int MinEnergy = -90;
        private const int EnergyBitmapWidth = 780;
        private const int BytesPerSample = sizeof(float);
        private const int SamplesPerColumn = 40;

        private static float accumulatedSquareSum;
        private static float accumulatedSampleCount;
        private static int energyIndex;
        private static float[] energy = new float[(uint)(EnergyBitmapWidth * 1.25)];
        private static object energyLock = new object();
        private static int newEnergyAvailable;
        private static float energyAmount;

        private static void ProcessAudio(AudioBeamSubFrame subFrame) {
            // Process audio buffer
            subFrame.CopyFrameDataToArray(audioBuffer);

            for (int i = 0; i < audioBuffer.Length; i += BytesPerSample) {
                // Extract the 32-bit IEEE float sample from the byte array
                float audioSample = BitConverter.ToSingle(audioBuffer, i);

                accumulatedSquareSum += audioSample * audioSample;
                ++accumulatedSampleCount;

                if (accumulatedSampleCount < SamplesPerColumn) {
                    continue;
                }

                float meanSquare = accumulatedSquareSum / SamplesPerColumn;

                if (meanSquare > 1.0f) {
                    // A loud audio source right next to the sensor may result in mean square values
                    // greater than 1.0. Cap it at 1.0f for display purposes.
                    meanSquare = 1.0f;
                }

                // Calculate energy in dB, in the range [MinEnergy, 0], where MinEnergy < 0
                energyAmount = MinEnergy;

                if (meanSquare > 0) {
                    energyAmount = (float)(10.0 * Math.Log10(meanSquare));
                }

                lock (energyLock) {
                    // Normalize values to the range [0, 1] for display
                    energy[energyIndex] = (MinEnergy - energyAmount) / MinEnergy;
                    energyIndex = (energyIndex + 1) % energy.Length;
                    ++newEnergyAvailable;
                }

                accumulatedSquareSum = 0;
                accumulatedSampleCount = 0;
            }
        }
        #endregion

        public static Vector ToWorldPosition(Vector localPosition) {
            //return position + orientation 
            return localPosition;
        }

        public static Rotation ToWorldOrientation(Rotation localRotation) {
            //return orientation *
            return localRotation;
        }
    }

    public class NativeKinectDevice : TrackingDevice {
        public static string name = "Microsoft Kinect 2";
        private static IntPtr pKinect;

        public NativeKinectDevice() {
            pKinect = Kinect_Constructor();
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Kinect_Constructor();

        ~NativeKinectDevice() {
            Kinect_Destructor(pKinect);
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Kinect_Destructor(IntPtr pKinect);

        public override void Init() {
            Kinect_Init(pKinect);
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Kinect_Init(IntPtr pKinect);

        public override void Stop() {
            Kinect_Stop(pKinect);
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Kinect_Stop(IntPtr pKinect);

        #region Tracker
        public override void Update() {
            Kinect_Update(pKinect);
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Kinect_Update(IntPtr pKinect);

        //public override Status status {
        //    get {
        //        return Kinect_GetStatus(pKinect);
        //    }
        //}
        //[DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        //private static extern Status Kinect_GetStatus(IntPtr pKinect);

        public override Vector3 position {
            set {
                Kinect_SetPosition(pKinect, new Vec3(value));
            }
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Kinect_SetPosition(IntPtr pKinect, Vec3 position);

        public override Quaternion rotation {
            set {
                Kinect_SetRotation(pKinect, new Quat(value));
            }
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Kinect_SetRotation(IntPtr pKinect, Quat rotation);
        #endregion

        #region Space
        public Vector3 GetSpacePoint(int i) {
            Vector3 spacePoint = Kinect_GetSpacePoint(pKinect, i).Vector3;
            return spacePoint;
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern Vec3 Kinect_GetSpacePoint(IntPtr pKinect, int i);

        Vector3[] spacePoints = new Vector3[512 * 424];
        public Vector3[] GetSpace() {
            IntPtr pSpacePoints = Kinect_GetSpace(pKinect);
            if (pSpacePoints == IntPtr.Zero)
                return null;

            for (int i = 0; i < 512 * 424; i++) {
                spacePoints[i] = GetSpacePoint(pSpacePoints, i);
            }
            return spacePoints;
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Kinect_GetSpace(IntPtr pKinect);
        
        private Vector3 GetSpacePoint(IntPtr ptr, int i) {
            int vector3size = Marshal.SizeOf(typeof(Vector3));
            IntPtr data = new IntPtr(ptr.ToInt64() + vector3size * i);
            Vector3 spacePoint = (Vector3)Marshal.PtrToStructure(data, typeof(Vector3));
            return spacePoint;
        }
        #endregion

        #region Bones
        //public override Vector3 GetBonePosition(uint actorId, Bone boneId) {
        //    return Kinect_GetBonePosition(pKinect, actorId, boneId).Vector3;
        //}
        //[DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        //private static extern Vec3 Kinect_GetBonePosition(IntPtr pKinect, uint actorId, Bone boneId);

        //public override Vector3 GetBonePosition(uint actorId, Side side, SideBone boneId) {
        //    return Kinect_GetSideBonePosition(pKinect, actorId, side, boneId).Vector3;
        //}
        //[DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        //private static extern Vec3 Kinect_GetSideBonePosition(IntPtr pKinect, uint actorId, Side side, SideBone boneId);

        //public override Quaternion GetBoneRotation(uint actorId, Bone boneId) {
        //    if (boneId != Bone.Head)
        //        return Quaternion.identity;

        //    Vector3 neckPos = GetBonePosition(actorId, Bone.Neck);
        //    Vector3 headPos = GetBonePosition(actorId, Bone.Head);

        //    Vector3 direction = headPos - neckPos;
        //    if (direction.sqrMagnitude == 0)
        //        return Quaternion.identity;

        //    direction = Quaternion.AngleAxis(180, Vector3.up) * direction;
        //    Quaternion neckRotation = Quaternion.LookRotation(direction, Vector3.back);
        //    neckRotation *= Quaternion.AngleAxis(90, Vector3.right);
        //    return neckRotation;
        //}

        //public override float GetBoneConfidence(uint actorId, Bone boneId) {
        //    return Kinect_GetBoneConfidence(pKinect, actorId, boneId);
        //}
        //[DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        //private static extern float Kinect_GetBoneConfidence(IntPtr pKinect, uint actorId, Bone boneId);

        //public override float GetBoneConfidence(uint actorId, Side side, SideBone boneId) {
        //    return Kinect_GetSideBoneConfidence(pKinect, actorId, side, boneId);
        //}
        //[DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        //private static extern float Kinect_GetSideBoneConfidence(IntPtr pKinect, uint actorId, Side side, SideBone boneId);

#if hUNSAFE
        unsafe public override SensorBone GetBone(uint actorId, Bone boneId) {
            SensorTransformC* pTargetTransform = Kinect_GetBone(actorId, boneId);
            return new SensorBone(pTargetTransform);
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        unsafe private static extern SensorTransformC* Kinect_GetBone(uint actorId, Bone boneId);
#else
        public override SensorTransformC GetBoneData(uint actorId, Bone boneId) {
            SensorTransformC sensorTransform = Kinect_GetBoneData(actorId, boneId);
            return sensorTransform;
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern SensorTransformC Kinect_GetBoneData(uint actorId, Bone boneId);
#endif

#if hUNSAFE
        unsafe public override SensorBone GetBone(uint actorId, Side side, SideBone boneId) {
            SensorTransformC* pTargetTransform = Kinect_GetSideBone(actorId, side, boneId);
            return new SensorBone(pTargetTransform);
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        unsafe private static extern SensorTransformC* Kinect_GetSideBone(uint actorId, Side side, SideBone boneId);
#else
        public override SensorTransformC GetBoneData(uint actorId, Side side, SideBone boneId) {
            SensorTransformC sensorTransform = Kinect_GetSideBoneData(actorId, side, boneId);
            return sensorTransform;
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern SensorTransformC Kinect_GetSideBoneData(uint actorId, Side side, SideBone boneId);
#endif
        #endregion

        #region Hands
        public enum HandPose {
            Unknown,
            Open,
            Closed,
            Lasso,
        }

        public HandPose GetHandPose(uint actorId, bool isLeft) {
            return Kinect_GetHandPose(pKinect, actorId, isLeft);
        }
        [DllImport("HumanoidKinect", CallingConvention = CallingConvention.Cdecl)]
        private static extern HandPose Kinect_GetHandPose(IntPtr pKinect, uint actorId, bool isLeft);
        #endregion
    }
}
#endif