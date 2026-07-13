using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    internal static class NativeXImgProcConvert
    {
        internal static NativeMethods.XImgProcRectNative ToNative(Rect rect)
        {
            return new NativeMethods.XImgProcRectNative
            {
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height
            };
        }

        internal static Rect ToRect(NativeMethods.XImgProcRectNative rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
