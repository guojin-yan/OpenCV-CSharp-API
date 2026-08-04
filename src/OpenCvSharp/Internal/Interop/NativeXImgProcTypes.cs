using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct XImgProcRectNative
        {
            internal int X;
            internal int Y;
            internal int Width;
            internal int Height;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XImgProcPointNative
        {
            internal int X;
            internal int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XImgProcPoint3iNative
        {
            internal int X;
            internal int Y;
            internal int Z;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XImgProcEdgeBoxNative
        {
            internal int X;
            internal int Y;
            internal int Width;
            internal int Height;
            internal float Score;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XImgProcEdgeDrawingParamsNative
        {
            internal int PfMode;
            internal int EdgeDetectionOperator;
            internal int GradientThresholdValue;
            internal int AnchorThresholdValue;
            internal int ScanInterval;
            internal int MinPathLength;
            internal float Sigma;
            internal int SumFlag;
            internal int NfaValidation;
            internal int MinLineLength;
            internal double MaxDistanceBetweenTwoLines;
            internal double LineFitErrorThreshold;
            internal double MaxErrorThreshold;
        }
    }
}
