using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Point2fNative
        {
            internal float X;
            internal float Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Point3fNative
        {
            internal float X;
            internal float Y;
            internal float Z;
        }
    }
}
