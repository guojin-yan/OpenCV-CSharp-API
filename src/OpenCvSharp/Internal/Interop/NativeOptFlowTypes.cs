using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
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
