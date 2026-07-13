using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeKeyPoint
    {
        public float X;
        public float Y;
        public float Size;
        public float Angle;
        public float Response;
        public int Octave;
        public int ClassId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeDMatch
    {
        public int QueryIdx;
        public int TrainIdx;
        public int ImgIdx;
        public float Distance;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSimpleBlobParams
    {
        public int Size;
        public float ThresholdStep;
        public float MinThreshold;
        public float MaxThreshold;
        public int MinRepeatability;
        public float MinDistBetweenBlobs;
        public int FilterByColor;
        public int BlobColor;
        public int FilterByArea;
        public float MinArea;
        public float MaxArea;
        public int FilterByCircularity;
        public float MinCircularity;
        public float MaxCircularity;
        public int FilterByInertia;
        public float MinInertiaRatio;
        public float MaxInertiaRatio;
        public int FilterByConvexity;
        public float MinConvexity;
        public float MaxConvexity;
        public int CollectContours;
    }
}
