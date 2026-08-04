using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct OptFlowRectNative
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }
}
