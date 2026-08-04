using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSurfaceMatchingPose3DResult
    {
        public double Alpha;
        public double Residual;
        public ulong ModelIndex;
        public ulong NumVotes;
        public double Angle;
        public double T0;
        public double T1;
        public double T2;
        public double Q0;
        public double Q1;
        public double Q2;
        public double Q3;
        public double Pose00;
        public double Pose01;
        public double Pose02;
        public double Pose03;
        public double Pose10;
        public double Pose11;
        public double Pose12;
        public double Pose13;
        public double Pose20;
        public double Pose21;
        public double Pose22;
        public double Pose23;
        public double Pose30;
        public double Pose31;
        public double Pose32;
        public double Pose33;
    }
}
