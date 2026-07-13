using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct VideoPoint2fNative
        {
            internal float X;
            internal float Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct VideoRectNative
        {
            internal int X;
            internal int Y;
            internal int Width;
            internal int Height;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct VideoRotatedRectNative
        {
            internal float CenterX;
            internal float CenterY;
            internal float Width;
            internal float Height;
            internal float Angle;
        }
    }
}
