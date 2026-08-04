using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeLineDescriptorKeyLine
    {
        public float Angle;
        public int ClassId;
        public int Octave;
        public float PtX;
        public float PtY;
        public float Response;
        public float Size;
        public float StartPointX;
        public float StartPointY;
        public float EndPointX;
        public float EndPointY;
        public float StartPointInOctaveX;
        public float StartPointInOctaveY;
        public float EndPointInOctaveX;
        public float EndPointInOctaveY;
        public float LineLength;
        public int NumOfPixels;
    }
}
