using RootSystem = System;
using System.Linq;
using System.Collections.Generic;
namespace Windows.Kinect
{
    //
    // Windows.Kinect.Joint
    //
    [RootSystem.Runtime.InteropServices.StructLayout(RootSystem.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct KinectJoint
    {
        public Windows.Kinect.JointType JointType { get; set; }
        public Windows.Kinect.CameraSpacePoint Position { get; set; }
        public Windows.Kinect.TrackingState TrackingState { get; set; }

        public override int GetHashCode()
        {
            return JointType.GetHashCode() ^ Position.GetHashCode() ^ TrackingState.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is KinectJoint))
            {
                return false;
            }

            return this.Equals((KinectJoint)obj);
        }

        public bool Equals(KinectJoint obj)
        {
            return JointType.Equals(obj.JointType) && Position.Equals(obj.Position) && TrackingState.Equals(obj.TrackingState);
        }

        public static bool operator ==(KinectJoint a, KinectJoint b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(KinectJoint a, KinectJoint b)
        {
            return !(a.Equals(b));
        }
    }

}
